using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace {
    public class AbilityManager : MonoBehaviour {
        
        private Dictionary<Ability, SelectableAbility> UnselectedAbilityMap = new();

        private void Start() {
            foreach (SelectableAbility selectable in GetComponentsInChildren<SelectableAbility>()) {
                UnselectedAbilityMap.Add(selectable.ability, selectable);
            }
        }

        // Hide unselected ability if last one was selected due to dupe limit
        public void HideIfLastAbility(Ability ability) {
            CombatManager cm = CombatManager.Instance;
            if (cm.playerAbilityDeck.GetAbilityDeckCount(ability) == cm.Config.dupeAbilityLimit - 1) {
                UnselectedAbilityMap[ability].Disable();
            }
        }

        // Enable every time ability is destroyed because only one is needed
        public void EnableSelectableAbility(Ability ability) {
            UnselectedAbilityMap[ability].Enable();
        }
        
    }
}