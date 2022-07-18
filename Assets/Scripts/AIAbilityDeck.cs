using UnityEngine;

namespace DefaultNamespace {
    public class AIAbilityDeck : AbilityDeck {
        
        private Ability[][] _Decks = {
            new[] { Ability.Attack, Ability.Attack, Ability.FoeAttackDown, Ability.FoeDefenseDown, Ability.DefenseUp, Ability.HealthUp },
            new[] { Ability.Attack, Ability.AttackUp, Ability.Attack, Ability.AttackUp, Ability.Attack, Ability.AttackUp },
            new[] { Ability.DefenseUp, Ability.DefenseUp, Ability.AttackUp, Ability.AttackUp, Ability.Attack, Ability.Attack },
            new[] { Ability.Attack, Ability.FoeDefenseDown, Ability.AttackUp, Ability.Attack, Ability.Attack, Ability.Attack },
            new[] { Ability.Attack, Ability.Attack, Ability.DefenseUp, Ability.HealthUp, Ability.Attack, Ability.FoeAttackDown },
            new[] { Ability.Attack, Ability.Attack, Ability.Attack, Ability.Attack, Ability.Attack, Ability.Attack },
        };

        private Ability[] _firstLevel = {
            Ability.FoeAttackDown, Ability.FoeAttackDown, Ability.FoeAttackDown, Ability.Attack, Ability.DefenseUp,
            Ability.Attack
        };

        private int _lastDeckIndex;

        public override void ResetDeck() {
            ChooseRandomDeck();
        }

        private void Start() {
            ChooseRandomDeck();
        }

        private void SetDeck(Ability[] deck) {
            for (int i = 0; i < deck.Length; i++) {
                SetAbilityAtIndex(i, deck[i]);
            }
        }

        public void ChooseRandomDeck() {
            var cm = CombatManager.Instance;

            if (cm.CurrentLevel == 1 && !cm.gameBeaten) {
                SetDeck(_firstLevel);
                return;
            }
            
            int index = Random.Range(0, _Decks.Length);
            
            while (index != _lastDeckIndex) {
                index = Random.Range(0, _Decks.Length);
            }
            
            SetDeck(_Decks[index]);
            _lastDeckIndex = index;
        }
        
    }
}