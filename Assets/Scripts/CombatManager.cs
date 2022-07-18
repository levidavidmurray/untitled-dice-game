using System;
using System.Collections;
using DarkTonic.MasterAudio;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DefaultNamespace {

    public enum Ability {
        EMPTY,
        Attack,
        AttackUp,
        DefenseUp,
        FoeAttackDown,
        FoeDefenseDown,
        HealthUp,
    }

    public enum GameState {
        WaitingReady,
        WaitingTurn,
        WaitingRoll,
        Playing,
        GameOver,
    }
    
    public class CombatManager : MonoBehaviour {

        public static CombatManager Instance { get; private set; }

        public GameConfig Config;

        public Fighter PlayerFighter;
        public Fighter FoeFighter;
        public DiceRoller diceRoller;
        public CanvasGroup abilitiesCanvasGroup;
        public TMP_Text rollText;
        public TMP_Text levelText;
        public TMP_Text rollsToNextTurnText;

        public bool gameBeaten;

        private int _RollsToNextTurn;
        
        private bool rollIsHovered;

        public Action OnNewTurn;
        public Action<int> OnLevelChange;

        private int _currentLevel = 1;

        public GameState CurState { get; private set; }

        public int CurrentLevel => _currentLevel;

        private void Awake() {
            PlayerFighter.Foe = FoeFighter;
            FoeFighter.Foe = PlayerFighter;
            CurState = GameState.WaitingReady;

            diceRoller.HandleMouseUp = HandleDiceRoll;
            diceRoller.HandleMouseEnter += () => {
                rollIsHovered = true;
            };
            diceRoller.HandleMouseExit += () => {
                rollIsHovered = false;
            };
            
            _RollsToNextTurn = Config.numRollsInTurn;

            if (Instance != null && Instance != this) {
                Destroy(this);
            }
            else {
                Instance = this;
            }
        }

        private void Start() {
            GameUI.Instance.quitButton.clicked += Application.Quit;
            GameUI.Instance.playAgainButton.clicked += RestartGame;
            GameUI.Instance.nextLevelButton.clicked += NextLevel;

            RestartGame();
        }

        private void Update() {

            if (Input.GetKeyUp(KeyCode.Escape) && !GameUI.Instance.LevelCompleteIsOpen && !GameUI.Instance.TutorialIsOpen) {
                if (GameUI.Instance.MenuIsOpen) {
                    GameUI.Instance.HideMenu();
                    MasterAudio.PlaySoundAndForget("Space_Page_Transition");
                }
                else {
                    GameUI.Instance.ShowMenu();
                    MasterAudio.PlaySoundAndForget("Space_Page_Transition_2");
                }
            }
            
            if (!PlayerFighter.IsReady()) {
                CurState = GameState.WaitingReady;
            } else if (CurState == GameState.WaitingReady) {
                CurState = GameState.WaitingTurn;
            }

            abilitiesCanvasGroup.alpha = CanEditDeck() ? 1f : Config.deckEditDisabledOpacity;

            if (CanRoll()) {
                rollText.color = rollIsHovered ? Config.rollTextHoveredColor : Config.rollTextUnhoveredColor;
            }
            else {
                rollText.color = Config.rollTextDisabledColor;
            }
        }

        public bool CanEditDeck() => CurState == GameState.WaitingReady || CurState == GameState.WaitingTurn;
        
        public bool CanRoll() => CurState == GameState.WaitingRoll || CurState == GameState.WaitingTurn;

        public void NextTurn() {
            if (!PlayerFighter.IsReady() || !FoeFighter.IsReady()) return;
            
            StartCoroutine(RollDice());
        }

        public void HandleDiceRoll() {
            
            var state = CurState;
            
            if (!CanRoll()) return;
            
            if (state == GameState.WaitingRoll) {
                StartCoroutine(RollDice());
                return;
            }

            if (state == GameState.WaitingTurn) {
                NextTurn();
            }
        }

        public IEnumerator RollDice() {
            
            CurState = GameState.Playing;
            
            print(PlayerFighter);
            print(FoeFighter);
            
            int diceRoll = Random.Range(0, 6);
            diceRoller.UpdateDice(diceRoll + 1);
            MasterAudio.PlaySoundAndForget("dice_roll");

            yield return new WaitForSeconds(Config.fightDelay);
            Fight(diceRoll);
            
            print(PlayerFighter);
            print(FoeFighter);
            
            _RollsToNextTurn--;
            UpdateTurnCounter();

            var didDie = false;

            Fighter attacker = null;
            
            if (PlayerFighter.IsDead) {
                didDie = true;
                PlayerFighter.SetOpacity(Config.deathOpacity);
                
                if (!FoeFighter.IsDead) {
                    attacker = FoeFighter;
                }
            }

            if (FoeFighter.IsDead) {
                didDie = true;
                FoeFighter.SetOpacity(Config.deathOpacity);

                if (!PlayerFighter.IsDead) {
                    attacker = PlayerFighter;
                }
            }

            Action onTurnComplete = () => {
                LeanTween.delayedCall(Config.turnDelay, () => {
                    if (FoeFighter.IsDead) {
                        FoeFighter.Revive();
                    }

                    if (PlayerFighter.IsDead) {
                        PlayerFighter.Revive();
                    }
                    
                    // allow change abilities after 3 turns
                    if (_RollsToNextTurn < 0 || didDie) {
                        _RollsToNextTurn = Config.numRollsInTurn;
                        CurState = GameState.WaitingTurn;
                        UpdateTurnCounter();
                        OnNewTurn?.Invoke();
                        return;
                    }
                    
                    CurState = GameState.WaitingRoll;
                });
            };
            
            if (attacker) {
                LeanTween.delayedCall(Config.attackHomeDelay, () => {
                    AttackHome(attacker);

                    if (PlayerFighter.IsHomeDead) {
                        GameOver(false);
                        return;
                    }

                    if (FoeFighter.IsHomeDead) {
                        LevelComplete();
                        return;
                    }
                    
                    onTurnComplete();
                });

                yield break;
            }
            
            onTurnComplete();
        }
        
        private void UpdateTurnCounter() {
            rollsToNextTurnText.text = $"{_RollsToNextTurn} TURNS LOCKED";
        }

        private void UpdateLevelCounter() {
            levelText.text = $"LEVEL {_currentLevel}/{Config.maxLevel}";
            OnLevelChange?.Invoke(_currentLevel);
        }

        private void NextLevel() {
            _currentLevel++;
            ResetState();
        }

        private void AttackHome(Fighter attacker) {
            Fighter foe = attacker.Foe;
            foe.TakeHomeDamage(attacker.Attack);
        }

        private void LevelComplete() {
            if (_currentLevel == Config.maxLevel) {
                GameOver(true);
                gameBeaten = true;
                return;
            }
            
            LeanTween.delayedCall(Config.gameOverDelay, () => {
                MasterAudio.PlaySoundAndForget("Casino_Positive");
                GameUI.Instance.ShowLevelComplete();
            });
        }

        private void GameOver(bool didWin) {
            LeanTween.delayedCall(Config.gameOverDelay, () => {
                MasterAudio.PlaySoundAndForget(didWin ? "Congrats" : "Game_Over");
                GameUI.Instance.resultLabel.text = didWin ? "YOU WON" : "YOU LOST";
                GameUI.Instance.ShowMenu();
            });
        }

        private void ResetState() {
            MasterAudio.PlaySoundAndForget("Level_Unlock");
            GameUI.Instance.HideMenu();
            GameUI.Instance.HideLevelComplete();
            GameUI.Instance.resultLabel.text = "PAUSED";
            PlayerFighter.SetupFighter();
            FoeFighter.SetupFighter();
            _RollsToNextTurn = Config.numRollsInTurn;
            UpdateTurnCounter();
            UpdateLevelCounter();
        }

        private void RestartGame() {
            _currentLevel = 1;
            ResetState();
        }

        public void Fight(int diceRoll) {
            Ability playerAbility = PlayerFighter.GetAbilityAtIndex(diceRoll);
            Ability foeAbility = FoeFighter.GetAbilityAtIndex(diceRoll);
            
            PlayerFighter.ApplyAbility(playerAbility);
            FoeFighter.ApplyAbility(foeAbility);
            
            print($"[{diceRoll}]: Player ({playerAbility}) | Foe ({foeAbility})");
        }
        
    }
}