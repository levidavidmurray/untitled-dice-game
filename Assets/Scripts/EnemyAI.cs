using System;
using UnityEngine;

namespace DefaultNamespace {
    public class EnemyAI : MonoBehaviour {
        
        private Fighter _Fighter;

        public GameObject levelOnePawn;
        public GameObject levelTwoPawn;
        public GameObject levelThreePawn;

        private void Awake() {
            _Fighter = GetComponent<Fighter>();
        }

        private void Start() {
            CombatManager.Instance.OnLevelChange += (level) => {
                levelOnePawn.SetActive(level == 1);
                levelTwoPawn.SetActive(level == 2);
                levelThreePawn.SetActive(level == 3);
            };
        }
    }
}