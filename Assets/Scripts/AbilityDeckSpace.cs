using System;
using TMPro;
using UnityEngine;

namespace DefaultNamespace {
    public class AbilityDeckSpace : MonoBehaviour {

        private TMP_Text spaceLabel;
        public SelectableAbility selectedAbility;
        public Action<Ability> OnAbilityUpdated;

        public void SetAbility(SelectableAbility _selectedAbility) {
            selectedAbility = _selectedAbility;
            OnAbilityUpdated?.Invoke(selectedAbility ? selectedAbility.ability : Ability.EMPTY);
        }
        
        public void ClearAbility() => SetAbility(null);

        public bool HasAbility => selectedAbility != null;

    }
}