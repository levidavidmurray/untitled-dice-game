using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

namespace DefaultNamespace {
    public class DiceRoller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        public Action HandleMouseUp;
        public Action HandleMouseEnter;
        public Action HandleMouseExit;
        
        public SpriteRenderer diceSpriteRenderer;

        private int _lastRoll = -1;
        private int _dupeRollCount;
        
        private int _currentRoll = -1;
        
        private GameConfig Config => CombatManager.Instance.Config;

        private void UpdateDice(int faceNum) {
            diceSpriteRenderer.sprite = Resources.Load<Sprite>($"Sprites/dice_{faceNum}");
        }

        public int RollDice() {
            _currentRoll = Random.Range(0, 6);
            
            if (_currentRoll == _lastRoll) {
                if (_dupeRollCount >= Config.maxDuplicateRolls) {
                    while (_currentRoll == _lastRoll) {
                        _currentRoll = Random.Range(0, 6);
                    }

                    _dupeRollCount = 0;
                }
                else {
                    _dupeRollCount++;
                }
            }
            else {
                _dupeRollCount = 0;
            }

            _lastRoll = _currentRoll;
                
            UpdateDice(_currentRoll + 1);
            
            return _currentRoll;
        }
        
        private void OnMouseUp() {
            HandleMouseUp?.Invoke();
        }

        private void OnMouseEnter() {
            HandleMouseEnter?.Invoke();
        }

        private void OnMouseExit() {
            HandleMouseExit?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            HandleMouseEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData) {
            HandleMouseExit?.Invoke();
        }
    }
}