using System;
using UnityEngine;

namespace DefaultNamespace {
    public class Player : MonoBehaviour {
        private Fighter _Fighter;

        private void Awake() {
            _Fighter = GetComponent<Fighter>();
        }
        

    }
}