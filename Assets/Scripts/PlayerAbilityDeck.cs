using UnityEngine;

namespace DefaultNamespace {
    public class PlayerAbilityDeck : AbilityDeck {
        
        private AbilityDeckSpace[] _abilityDeckSpaces;

        private void Awake() {
            _abilityDeckSpaces = GetComponentsInChildren<AbilityDeckSpace>();
            
            for (int i = 0; i < _abilityDeckSpaces.Length; i++) {
                var deckSpace = _abilityDeckSpaces[i];
                var i1 = i;
                deckSpace.OnAbilityUpdated += (ability) => {
                    SetAbilityAtIndex(i1, ability);
                };
            }
        }

        public override void ResetDeck() {
            for (int i = 0; i < _abilityDeckSpaces.Length; i++) {
                if (_abilityDeckSpaces[i].selectedAbility) {
                    Destroy(_abilityDeckSpaces[i].selectedAbility.gameObject);
                }
                
                _abilityDeckSpaces[i].ClearAbility();
            }
        }

    }
}