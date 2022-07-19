using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace {
    public class Player : MonoBehaviour {
        private Fighter _Fighter;

        private void Awake() {
            _Fighter = GetComponent<Fighter>();
        }

        private void Update() {
            if (Input.GetKeyUp(KeyCode.R)) {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}