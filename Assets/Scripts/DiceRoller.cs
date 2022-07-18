using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DefaultNamespace {
    public class DiceRoller : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        public Action HandleMouseUp;
        public Action HandleMouseEnter;
        public Action HandleMouseExit;
        
        public SpriteRenderer diceSpriteRenderer;

        public void UpdateDice(int faceNum) {
            diceSpriteRenderer.sprite = Resources.Load<Sprite>($"Sprites/dice_{faceNum}");
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