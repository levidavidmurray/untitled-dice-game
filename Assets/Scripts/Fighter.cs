using System;
using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using DarkTonic.MasterAudio;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace {
    
    public class Fighter : MonoBehaviour {
        
        private int _Health = 3;
        private int _Attack = 1;
        private int _Defense = 0;
        [SerializeField] private AbilityDeck abilityDeck;
        [SerializeField] private ParticleSystem hurtParticles;
        
        private int _homeHealth;

        public TMP_Text HealthDisplay;
        public TMP_Text AttackDisplay;
        public TMP_Text DefenseDisplay;
        
        public UsedAbility usedAbility;

        public TMP_Text HomeHealthDisplay;
        
        public Transform defenseStat;
        public Transform attackStat;
        public Transform healthStat;

        public Fighter Foe;
        
        private CanvasGroup _canvasGroup;
        private Canvas _canvas;
        private CircleCollider2D _collider;
        public CanvasGroup homeCanvasGroup;

        public Ability activeAbility = Ability.EMPTY;

        public Sprite faceDefault;
        public Sprite faceHurt;
        public SpriteRenderer faceSR;

        private SpriteRenderer[] _allSRs;

        private Action onAttackCollision;
        private Vector3 _spawnPos;

        private void Awake() {
            // for (int i = 0; i < _Abilities.Length; i++) {
            //     _Abilities[i] = Ability.EMPTY;
            // }

            // _canvasGroup = transform.Find("Canvas").GetComponent<CanvasGroup>();
            // _canvas = transform.Find("Canvas").GetComponent<Canvas>();
            _collider = GetComponent<CircleCollider2D>();
            SpriteRenderer[] SRs = GetComponentsInChildren<SpriteRenderer>();
            _allSRs = new SpriteRenderer[SRs.Length - 1];
            int i = 0;
            foreach (var sr in SRs) {
                if (sr.transform == usedAbility.transform) continue;

                _allSRs[i] = sr;
                i++;
            }
        }

        private void Start() {
            _spawnPos = transform.position;
            SetupFighter();
        }

        public void SetupFighter() {
            _homeHealth = Config.defaultHomeHealth;
            abilityDeck.ResetDeck();
            
            Revive();
        }

        public Ability GetAbilityAtIndex(int index) {
            activeAbility = abilityDeck.GetAbilityAtIndex(index);
            return activeAbility;
        }

        public void SetAbilityAtIndex(int index, Ability ability) => abilityDeck.SetAbilityAtIndex(index, ability);

        public int Health => _Health;

        public int Attack => _Attack;

        public bool IsDead => Health == 0;

        public bool IsHomeDead => _homeHealth == 0;

        public bool IsAttacking => activeAbility == Ability.Attack;

        public void Revive() {
            _Health = Config.defaultHealth;
            _Attack = Config.defaultAttack;
            _Defense = Config.defaultDefense;
            _collider.enabled = true;
            
            UpdateStatsDisplay();
            SetOpacity(1);
        }

        public void Die() {
            SetOpacity(Config.deathOpacity);
            _collider.enabled = false;
            
            // death sound
            // if (Foe.IsDead) {
            //     if (IsPlayer) MasterAudio.PlaySoundAndForget("Player_Hurt");
            // }
            // else {
            //     MasterAudio.PlaySoundAndForget("Player_Hurt");
            // }
        }

        public void SetOpacity(float opacity) {
            foreach (SpriteRenderer sr in _allSRs) {
                Color c = sr.color;
                sr.color = new Color(c.r, c.g, c.b, opacity);
            }

            HealthDisplay.alpha = opacity;
            AttackDisplay.alpha = opacity;
            DefenseDisplay.alpha = opacity;
        }

        private void OnTriggerEnter2D(Collider2D col) {
            if (!col.CompareTag("Fighter") && !col.CompareTag("Player") && !col.CompareTag("Home")) return;

            if (homeCanvasGroup.transform.parent == col.transform) return;
            
            // Attack collision
            onAttackCollision?.Invoke();
            onAttackCollision = null;
            activeAbility = Ability.EMPTY;

            // Only want to invoke a single camera shake when attacking one another
            if (IsPlayer || col.CompareTag("Home")) {
                ProCamera2DShake.Instance.Shake(0);
            }
        }

        public int TakeDamage(int dmg) {
            if (dmg <= _Defense) {
                MasterAudio.PlaySoundAndForget("Negative3");
                StatChangeEffect(defenseStat);
                return _Health;
            }
            
            int dmgApplied = dmg - _Defense;

            _Health = Mathf.Max(0, _Health - dmgApplied);

            if (Config.debugNoDamage) _Health += dmgApplied;
            
            if (dmgApplied > 0) {
                DamageFlicker(SetOpacity, transform);
                MasterAudio.PlaySoundAndForget("Sword_Kill");
                hurtParticles.Play();
                StatChangeEffect(healthStat);
            }
            
            return _Health;
        }

        public int TakeHomeDamage(int dmg) {
            _homeHealth = Mathf.Max(0, _homeHealth - dmg);
            UpdateStatsDisplay();
            DamageFlicker(opacity => homeCanvasGroup.alpha = opacity, homeCanvasGroup.transform.parent);
            MasterAudio.PlaySoundAndForget("Sword_Slash");
            
            return _homeHealth;
        }

        public void AttackHome(Action onAttackComplete) {
            // _canvas.sortingOrder = 4;
            ChargeAttackAnim(AnimConfig.attackHomeAnimCurve, AnimConfig.attackHomeAnimTime, onAttackComplete);
            usedAbility.ShowAbility(Ability.Attack);

            onAttackCollision = () => {
                Foe.TakeHomeDamage(Attack);
            };
        }

        public void StatChangeEffect(Transform statDisplay) {
            LeanTween.value(0f, 1f, AnimConfig.statChangeAnimTime).setOnUpdate((float value) => {
                statDisplay.localScale = Vector3.one * AnimConfig.statChangeAnimCurve.Evaluate(value);
            });
            UpdateStatsDisplay();
        }

        private void DamageFlicker(Action<float> setOpacity, Transform scaleBody) {
            // alpha flash
            LeanTween.value(1f, Config.damageIndicatorMinAlpha, Config.damageIndicatorFlickerTime)
                .setOnUpdate(setOpacity).setLoopPingPong(Config.damageIndicatorFlickerCount)
                .setOnComplete(() => {
                    if (IsDead) {
                        SetOpacity(Config.deathOpacity);
                    }
                });
            // scale flash
            var origScale = scaleBody.localScale;
            LeanTween.value(0f, 1f, AnimConfig.takeDamageTime).setOnUpdate((float value) => {
                scaleBody.localScale = origScale * AnimConfig.takeDamageScaleAnimCurve.Evaluate(value);
            }).setOnComplete(() => scaleBody.localScale = origScale);
        }

        public bool IsReady() => abilityDeck.IsReady();

        public void DecreaseAttack() {
            _Attack = Mathf.Max(Config.minAttack, _Attack - 1);
            StatChangeEffect(attackStat);
        }

        public void DecreaseDefense() {
            _Defense = Mathf.Max(Config.minDefense, _Defense - 1);
            StatChangeEffect(defenseStat);
        }

        public bool IsPlayer => this == CombatManager.Instance.PlayerFighter;

        private void AttackEnemy(bool forced = false) {
            AnimationCurve attackAnimCurve = AnimConfig.attackFullAnimCurve;
            float attackAnimTime = AnimConfig.attackFullAnimTime;
            // _canvas.sortingOrder = 4;
            
            if (Foe.IsAttacking) {
                attackAnimCurve = AnimConfig.attackHalfAnimCurve;
                attackAnimTime = AnimConfig.attackHalfAnimTime;
                // _canvas.sortingOrder = IsPlayer ? 5 : 4;
            }
            
            ChargeAttackAnim(attackAnimCurve, attackAnimTime, () => {
                if (forced) return;
                CombatManager.Instance.OnAbilityComplete();
            });
            
            onAttackCollision = () => Foe.TakeDamage(_Attack);
        }

        private void ChargeAttackAnim(AnimationCurve attackAnimCurve, float attackAnimTime, Action onAnimComplete = null) {
            var origPos = transform.localPosition;
            int atkDir = IsPlayer ? 1 : -1;
            
            LeanTween.value(0f, 1f, attackAnimTime).setOnUpdate(value => {
                var yPos = origPos.y + (attackAnimCurve.Evaluate(value) * atkDir);
                transform.localPosition = new Vector2(origPos.x, yPos);
            }).setOnComplete(() => {
                transform.position = _spawnPos;
                // _canvas.sortingOrder = 3;
                onAnimComplete?.Invoke();
            });
        }

        public void UseActiveAbility(bool forced = false) {
            ApplyAbility(activeAbility, forced);
        }

        public void ShowActiveAbility() {
            usedAbility.ShowAbility(activeAbility);
        }

        public void ApplyAbility(Ability ability, bool forced = false) {
            
            PlayAbilitySound(ability);
            
            switch (ability) {
                case Ability.Attack:
                    AttackEnemy(forced);
                    break;
                case Ability.AttackUp:
                    _Attack = Mathf.Min(Config.maxAttack, _Attack + 1);
                    StatChangeEffect(attackStat);
                    break;
                case Ability.DefenseUp:
                    _Defense = Mathf.Min(Config.maxDefense, _Defense + 1);
                    StatChangeEffect(defenseStat);
                    break;
                case Ability.HealthUp:
                    _Health = Mathf.Min(Config.maxHealth, _Health + 1);
                    StatChangeEffect(healthStat);
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
            
            if (activeAbility != Ability.Attack && !forced) {
                CombatManager.Instance.OnAbilityComplete();
            }
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

        private AnimConfig AnimConfig => CombatManager.Instance.AnimConfig;


    }
    
}
