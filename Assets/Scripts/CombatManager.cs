using System;
using DarkTonic.MasterAudio;
using TMPro;
using UnityEngine;

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

        // Number of abilities to complete before end of turn
        public static int ABILITIES_TO_COMPLETE = 2;

        public GameConfig Config;
        public AnimConfig AnimConfig;

        public Fighter PlayerFighter;
        public Fighter FoeFighter;
        public DiceRoller playerDiceRoller;
        public DiceRoller enemyDiceRoller;
        public PlayerAbilityDeck playerAbilityDeck;
        public AbilityManager playerAbilityManager;
        public CanvasGroup abilitiesCanvasGroup;
        public TMP_Text rollText;
        public TMP_Text levelText;
        public TMP_Text rollsToNextTurnText;

        public bool gameBeaten;

        private int _RollsToNextTurn;
        private int _abilitiesWaitingToComplete = ABILITIES_TO_COMPLETE;
        
        private bool rollIsHovered;

        public Action<int> OnLevelChange;

        private int _currentLevel = 1;

        public GameState CurState { get; private set; }

        public int CurrentLevel => _currentLevel;

        private void Awake() {
            PlayerFighter.Foe = FoeFighter;
            FoeFighter.Foe = PlayerFighter;
            CurState = GameState.WaitingReady;

            playerDiceRoller.HandleMouseUp = HandleDiceRoll;
            playerDiceRoller.HandleMouseEnter += () => {
                rollIsHovered = true;
            };
            playerDiceRoller.HandleMouseExit += () => {
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

            LeanTween.delayedCall(AnimConfig.deckLockFightDelay, RollDice);
        }

        public void HandleDiceRoll() {
            if (!CanRoll()) return;

            if (CurState == GameState.WaitingTurn) {
                playerAbilityDeck.LockDeckAnim(() => {
                    ShowTurnCounter();
                    NextTurn();
                });
            }
            
            CurState = GameState.Playing;
        }

        public void RollDice() {
            
            int playerDiceRoll = playerDiceRoller.RollDice();
            MasterAudio.PlaySoundAndForget("dice_roll");
            
            int enemyDiceRoll = enemyDiceRoller.RollDice();
            MasterAudio.PlaySoundAndForget("dice_roll");

            LeanTween.delayedCall(Config.fightDelay, () => {
                Fight(playerDiceRoll, enemyDiceRoll);
                _RollsToNextTurn--;
                UpdateTurnCounter();
            });
        }

        public void Fight(int playerDiceRoll, int enemyDiceRoll) {
            
            PlayerFighter.GetAbilityAtIndex(playerDiceRoll);
            FoeFighter.GetAbilityAtIndex(enemyDiceRoll);
            
            PlayerFighter.ShowActiveAbility();
            FoeFighter.ShowActiveAbility();

            // Both attacking or both buffing
            if ((PlayerFighter.IsAttacking && FoeFighter.IsAttacking) || (!PlayerFighter.IsAttacking && !FoeFighter.IsAttacking)) {
                PlayerFighter.UseActiveAbility();
                FoeFighter.UseActiveAbility();
                return;
            }

            Fighter firstFighter = PlayerFighter;
            Fighter secondFighter = FoeFighter;

            // Player attacking, Foe buffing
            // Player waits for Foe buff
            if (PlayerFighter.IsAttacking) {
                firstFighter = FoeFighter;
                secondFighter = PlayerFighter;
            }
            
            firstFighter.UseActiveAbility();
            
            // Fighter who is attacking waits for opponent buff
            LeanTween.delayedCall(Config.attackDelayForBuff, secondFighter.UseActiveAbility);
            
        }

        public void OnAbilityComplete() {

            _abilitiesWaitingToComplete--;
            if (_abilitiesWaitingToComplete > 0) return;

            _abilitiesWaitingToComplete = ABILITIES_TO_COMPLETE;
            
            var didDie = false;

            Fighter attacker = null;
            
            if (PlayerFighter.IsDead) {
                didDie = true;
                PlayerFighter.Die();
                
                if (!FoeFighter.IsDead) {
                    attacker = FoeFighter;
                }
            }

            if (FoeFighter.IsDead) {
                didDie = true;
                FoeFighter.Die();

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
                    
                    // allow abilities change after 3 turns
                    if (_RollsToNextTurn <= 0 || didDie) {
                        HideTurnCounter();
                        playerAbilityDeck.UnlockDeckAnim(() => {
                            CurState = GameState.WaitingTurn;
                            _RollsToNextTurn = Config.numRollsInTurn;
                            UpdateTurnCounter();
                        });
                        return;
                    }

                    LeanTween.delayedCall(Config.autoTurnDelay, RollDice);
                });
            };
            
            if (attacker) {
                LeanTween.delayedCall(Config.attackHomeDelay, () => {
                    attacker.AttackHome(() => {
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
                });
                return;
            }
            
            onTurnComplete();
        }
        
        private void UpdateTurnCounter() {
            rollsToNextTurnText.text = $"{_RollsToNextTurn} TURNS LOCKED";
        }

        private void ShowTurnCounter() {
            LeanTween.value(0f, 1f, AnimConfig.turnCounterFadeTime).setOnUpdate(value => {
                rollsToNextTurnText.alpha = value;
            });
        }
        
        private void HideTurnCounter() {
            LeanTween.value(1f, 0f, AnimConfig.turnCounterFadeTime).setOnUpdate(value => {
                rollsToNextTurnText.alpha = value;
            });
        }
        

        private void UpdateLevelCounter() {
            levelText.text = $"LEVEL {_currentLevel}/{Config.maxLevel}";
            OnLevelChange?.Invoke(_currentLevel);
        }

        private void NextLevel() {
            _currentLevel++;
            ResetState();
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
            rollsToNextTurnText.alpha = 0;
            _RollsToNextTurn = Config.numRollsInTurn;
            UpdateTurnCounter();
            UpdateLevelCounter();

            LeanTween.delayedCall(AnimConfig.startingDeckUnlockDelay, () => {
                playerAbilityDeck.UnlockDeckAnim(() => { });
            });
        }

        private void RestartGame() {
            _currentLevel = 1;
            ResetState();
        }
        
    }
}