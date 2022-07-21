using System;
using UnityEngine;

namespace DefaultNamespace {
    public class PlayerAbilityDeck : AbilityDeck {
        
        private AbilityDeckSpace[] _abilityDeckSpaces;

        private AnimConfig AnimConfig => CombatManager.Instance.AnimConfig;
        private Vector3 _originalPos;
        
        private void Awake() {
            _abilityDeckSpaces = GetComponentsInChildren<AbilityDeckSpace>();
            
            for (int i = 0; i < _abilityDeckSpaces.Length; i++) {
                var deckSpace = _abilityDeckSpaces[i];
                var i1 = i;
                deckSpace.OnAbilityUpdated += (ability) => {
                    SetAbilityAtIndex(i1, ability);
                };
            }

            _originalPos = transform.position;
        }

        public override void ResetDeck() {
            for (int i = 0; i < _abilityDeckSpaces.Length; i++) {
                var selectedAbility = _abilityDeckSpaces[i].selectedAbility;
                if (selectedAbility) {
                    CombatManager.Instance.playerAbilityManager.EnableSelectableAbility(selectedAbility.ability);
                    Destroy(selectedAbility.gameObject);
                }
                
                _abilityDeckSpaces[i].ClearAbility();
            }

            transform.position = _originalPos;
        }

        public void UnlockDeckAnim(Action onComplete) {
            var pos = transform.position;
            var origYPos = pos.y;
            LeanTween.value(0f, 1f, AnimConfig.deckUnlockAnimTime).setOnUpdate(value => {
                float yOffset = AnimConfig.deckUnlockAnimCurve.Evaluate(value) * AnimConfig.deckLockDist;
                float yPos = origYPos + yOffset;
                transform.position = new Vector3(pos.x, yPos, pos.z);
            }).setOnComplete(onComplete);
        }

        public void LockDeckAnim(Action onComplete) {
            var pos = transform.position;
            var startYPos = pos.y;
            var dist = Vector2.Distance(pos, _originalPos);
            LeanTween.value(0f, 1f, AnimConfig.deckUnlockAnimTime).setOnUpdate(value => {
                float yOffset = AnimConfig.deckUnlockAnimCurve.Evaluate(value) * -dist;
                float yPos = startYPos + yOffset;
                transform.position = new Vector3(pos.x, yPos, pos.z);
            }).setOnComplete(onComplete);
        }

    }
}