using System;
using UnityEngine;

namespace DefaultNamespace {
    public class AbilityDeck : MonoBehaviour {

        private Ability[] _abilities = new Ability[6];

        public virtual void ResetDeck() {
            throw new NotImplementedException();
        }

        public virtual Ability GetAbilityAtIndex(int index) {
            return _abilities[index];
        }

        public void SetAbilityAtIndex(int index, Ability ability) {
            _abilities[index] = ability;
        }

        public bool IsReady() {
            foreach (Ability ability in _abilities) {
                if (ability == Ability.EMPTY) return false;
            }

            return true;
        }
        
    }
}