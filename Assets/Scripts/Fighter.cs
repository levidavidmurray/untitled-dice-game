using System;
using System.Collections;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace {
    
    public class Fighter : MonoBehaviour {
        
        [SerializeField] private int _Health = 3;
        [SerializeField] private int _Attack = 1;
        [SerializeField] private int _Defense = 0;
        [SerializeField] private AbilityDeck abilityDeck;
        
        private int _homeHealth;

        public TMP_Text HealthDisplay;
        public TMP_Text AttackDisplay;
        public TMP_Text DefenseDisplay;
        
        public UsedAbility usedAbility;

        public TMP_Text HomeHealthDisplay;

        public Fighter Foe;
        
        private CanvasGroup _canvasGroup;
        public CanvasGroup homeCanvasGroup;

        public RawImage pawnImage;

        private void Awake() {
            // for (int i = 0; i < _Abilities.Length; i++) {
            //     _Abilities[i] = Ability.EMPTY;
            // }

            _canvasGroup = transform.Find("Canvas").GetComponent<CanvasGroup>();
        }

        private void Start() {
            SetupFighter();
        }

        public void SetupFighter() {
            _homeHealth = Config.defaultHomeHealth;
            abilityDeck.ResetDeck();
            
            Revive();
        }

        public Ability GetAbilityAtIndex(int index) => abilityDeck.GetAbilityAtIndex(index);

        public void SetAbilityAtIndex(int index, Ability ability) => abilityDeck.SetAbilityAtIndex(index, ability);

        public int Health => _Health;

        public int Attack => _Attack;

        public bool IsDead => Health == 0;

        public bool IsHomeDead => _homeHealth == 0;

        public void Revive() {
            _Health = Config.defaultHealth;
            _Attack = Config.defaultAttack;
            _Defense = Config.defaultDefense;
            
            UpdateStatsDisplay();
            SetOpacity(1);
        }

        public int TakeDamage(int dmg) {
            if (dmg <= _Defense) {
                MasterAudio.PlaySoundAndForget("Negative3");
                return _Health;
            }
            
            int dmgApplied = dmg - _Defense;
            _Health = Mathf.Max(0, _Health - dmgApplied);

            if (dmgApplied > 0) {
                DamageFlicker((opacity) => _canvasGroup.alpha = opacity);
                MasterAudio.PlaySoundAndForget("Sword_Kill");
            }
            else {
                MasterAudio.PlaySoundAndForget("Negative3");
            }
            
            return _Health;
        }

        private void DamageFlicker(Action<float> setOpacity) {
            LeanTween.value(1f, Config.damageIndicatorMinAlpha, Config.damageIndicatorFlickerTime)
                .setOnUpdate(setOpacity).setLoopPingPong(Config.damageIndicatorFlickerCount)
                .setOnComplete(() => {
                    if (IsDead) {
                        SetOpacity(Config.deathOpacity);
                    }
                });
        }

        public int TakeHomeDamage(int dmg) {
            _homeHealth = Mathf.Max(0, _homeHealth - dmg);
            UpdateStatsDisplay();
            DamageFlicker(opacity => homeCanvasGroup.alpha = opacity);
            MasterAudio.PlaySoundAndForget("Sword_Slash");
            
            Foe.usedAbility.ShowAbility(Ability.Attack);
            
            return _homeHealth;
        }

        public void SetOpacity(float opacity) {
            _canvasGroup.alpha = opacity;
        }

        public bool IsReady() => abilityDeck.IsReady();

        public void DecreaseAttack() => _Attack = Mathf.Max(Config.minAttack, _Attack - 1);

        public void DecreaseDefense() => _Defense = Mathf.Max(Config.minDefense, _Defense - 1);

        public void ApplyAbility(Ability ability) {
            
            usedAbility.ShowAbility(ability);
            
            PlayAbilitySound(ability);
            
            switch (ability) {
                case Ability.Attack:
                    Foe.TakeDamage(_Attack);
                    break;
                case Ability.AttackUp:
                    _Attack = Mathf.Min(Config.maxAttack, _Attack + 1);
                    break;
                case Ability.DefenseUp:
                    _Defense = Mathf.Min(Config.maxDefense, _Defense + 1);
                    break;
                case Ability.HealthUp:
                    _Health = Mathf.Min(Config.maxHealth, _Health + 1);
                    break;
                case Ability.FoeAttackDown:
                    Foe.DecreaseAttack();
                    break;
                case Ability.FoeDefenseDown:
                    Foe.DecreaseDefense();
                    break;
            }
            
            UpdateStatsDisplay();
            Foe.UpdateStatsDisplay();
        }

        void PlayAbilitySound(Ability ability) {
            if (this != CombatManager.Instance.PlayerFighter) return;
            
            switch (ability) {
                case Ability.AttackUp:
                    MasterAudio.PlaySoundAndForget("Sword_Draw");
                    break;
                case Ability.DefenseUp:
                    MasterAudio.PlaySoundAndForget("Collect_Game_Material_12");
                    break;
                case Ability.HealthUp:
                    MasterAudio.PlaySoundAndForget("Collect_Game_Material_12");
                    break;
                case Ability.FoeAttackDown:
                    MasterAudio.PlaySoundAndForget("Negative_Game_Hit_3");
                    break;
                case Ability.FoeDefenseDown:
                    MasterAudio.PlaySoundAndForget("Negative_Game_Hit_3");
                    break;
            }
        }

        public void UpdateStatsDisplay() {
            HealthDisplay.text = _Health.ToString();
            AttackDisplay.text = _Attack.ToString();
            DefenseDisplay.text = _Defense.ToString();
            HomeHealthDisplay.text = _homeHealth.ToString();
        }

        public override string ToString() {
            return $"[{transform.name}] Fighter {{Health: {_Health}, Attack: {_Attack}, Defense: {_Defense}}}";
        }

        private GameConfig Config => CombatManager.Instance.Config;


    }
    
}
