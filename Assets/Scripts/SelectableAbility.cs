﻿using System;
using System.Collections.Generic;
using DarkTonic.MasterAudio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace DefaultNamespace {

    public struct AbilityNameDisplay {
        public AbilityNameDisplay(string name, float fontSize, string tooltip) {
            this.name = name;
            this.fontSize = fontSize;
            this.tooltip = tooltip;
        }

        public string name { get; }
        public string tooltip { get; }
        public float fontSize { get; }
    }

    public enum SelectableState {
        Unselected,
        Selecting,
        Selected,
    }
    
    [ExecuteAlways]
    public class SelectableAbility : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler, IPointerDownHandler, IPointerUpHandler {
        
        public Ability ability;
        public SelectableState state = SelectableState.Unselected;

        private SelectableAbility _selectingAbility;
        
        private int scaleTweenId;

        private Canvas _iconCanvas;
        private Canvas _tooltipCanvas;
        private Image _icon;

        public AbilityDeckSpace tmpSpace2;
        public AbilityDeckSpace selectedSpace;

        private int tooltipShowTweenId;
        private bool tooltipShowing;

        public static Dictionary<Ability, AbilityNameDisplay> AbilityNameMap = new() {
            { Ability.EMPTY, new AbilityNameDisplay("N/A", 0.3f, "") },
            { Ability.Attack, new AbilityNameDisplay("ATK", 0.3f, "Attack enemy") },
            { Ability.AttackUp, new AbilityNameDisplay("ATK+", 0.25f, "Increase attack by 1") },
            { Ability.DefenseUp, new AbilityNameDisplay("DEF+", 0.25f, "Increase defense by 1") },
            { Ability.HealthUp, new AbilityNameDisplay("HLT+", 0.25f, "Increase health by 1") },
            { Ability.FoeAttackDown, new AbilityNameDisplay("FATK-", 0.2f, "Decrease enemy attack by 1") },
            { Ability.FoeDefenseDown, new AbilityNameDisplay("FDEF-", 0.2f, "Decrease enemy defense by 1") },
        };

        private GameConfig Config => CombatManager.Instance.Config;

        private void Awake() {
            _iconCanvas = transform.Find("IconCanvas").GetComponent<Canvas>();
            _tooltipCanvas = transform.Find("TooltipCanvas").GetComponent<Canvas>();
            _icon = _iconCanvas.transform.Find("Icon").GetComponent<Image>();
        }

        private void OnValidate() {
            _icon = GetComponentInChildren<Image>();
            _tooltipCanvas = transform.Find("TooltipCanvas").GetComponent<Canvas>();

            string abilityName = AbilityNameMap[ability].name;
            
            // string filename = $"Assets/Sprites/ICON_{abilityName}.png";
            // var rawData = System.IO.File.ReadAllBytes(filename);
            // Texture2D tex = new Texture2D(2, 2);
            // tex.LoadImage(rawData);
            _icon.sprite = Resources.Load<Sprite>($"Sprites/ICON_{abilityName}");

            var tooltipText = _tooltipCanvas.transform.Find("TooltipText").GetComponent<TMP_Text>();
            tooltipText.text = AbilityNameMap[ability].tooltip;

            transform.name = $"SelectableAbility ({Enum.GetName(typeof(Ability), ability)})";
        }

        private void Update() {
            if (state != SelectableState.Selecting) return;
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector2(worldPos.x, worldPos.y);
            
            _iconCanvas.sortingOrder = 5;
        }

        private void OnTriggerEnter2D(Collider2D col) {
            if (!CanEdit()) return;
            if (!col.CompareTag("DeckSpace")) return;

            var deckSpace = col.GetComponent<AbilityDeckSpace>();

            tmpSpace2 = deckSpace;
            DeckSpaceHoverScale();
        }
        
        private void OnTriggerStay2D(Collider2D other) {
            if (!CanEdit()) return;
            if (!other.CompareTag("DeckSpace")) return;
            
            // handle in between spaces, ability hasn't exited current tmpSpace
            // but triggered new space enter
            var deckSpace = other.GetComponent<AbilityDeckSpace>();

            if (!deckSpace || !tmpSpace2) return;

            if (deckSpace == tmpSpace2) return;

            var distToNewSpace = Vector2.Distance(transform.position, deckSpace.transform.position);
            var distToCurSpace = Vector2.Distance(transform.position, tmpSpace2.transform.position);

            // set new space if closer than current space
            if (distToNewSpace < distToCurSpace) {
                tmpSpace2 = deckSpace;
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (!CanEdit()) return;
            if (!other.CompareTag("DeckSpace")) return;
            var deckSpace = other.GetComponent<AbilityDeckSpace>();

            if (deckSpace != tmpSpace2) return;
            
            // exited all possible spaces
            HoverScale();
            tmpSpace2 = null;
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (!CanEdit()) return;
            
            HideTooltip();
            
            if (state == SelectableState.Selected) {

                if (tmpSpace2) {
                    tmpSpace2.ClearAbility();
                }

                selectedSpace = null;
                state = SelectableState.Selecting;
                
                DeckSpaceHoverScale();
                
                return;
            }
            
            var go = Instantiate(this);
            SelectableAbility selectingAbility = go.GetComponent<SelectableAbility>();
            selectingAbility.state = SelectableState.Selecting;
            _selectingAbility = selectingAbility;
            _selectingAbility.HoverScale();
        }


        public void OnPointerUp(PointerEventData eventData) {
            if (!CanEdit()) return;

            // mouse up when selecting from unselected abilities
            var selectingAbility = _selectingAbility;

            // mouse up when selecting from selected abilities
            if (state == SelectableState.Selecting) {
                selectingAbility = this;
            }
            
            if (!selectingAbility) return;
            
            if (!selectingAbility.tmpSpace2) {
                if (Config.debugKeepUnselectedAbility) {
                    selectingAbility.state = SelectableState.Selected;
                }
                else {
                    LeanTween.cancel(tooltipShowTweenId);
                    LeanTween.cancel(scaleTweenId);
                    Destroy(selectingAbility.gameObject);
                    MasterAudio.PlaySoundAndForget("negative_alert_1");
                }
                return;
            }

            selectingAbility.SelectDeckSpace();
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (state == SelectableState.Selected || tmpSpace2) return;

            MasterAudio.PlaySoundAndForget("Select_Tap_UI_Sound_1");
            HoverScale();
        }
        
        public void OnPointerExit(PointerEventData eventData) {
            print($"OnPointerExit, {eventData.pointerEnter.name}");
            SelectedScale();
            LeanTween.cancel(tooltipShowTweenId);
            HideTooltip();
        }

        public void OnPointerMove(PointerEventData eventData) {
            print("OnPointerMove");
            LeanTween.cancel(tooltipShowTweenId);
            if (tooltipShowing || state == SelectableState.Selecting) return;
            tooltipShowTweenId = LeanTween.delayedCall(Config.tooltipHoverDelay, ShowTooltip).id;
        }

        private void ShowTooltip() {
            LeanTween.scale(_tooltipCanvas.gameObject, Vector3.one, Config.tooltipEnterTime);
            tooltipShowing = true;
        }

        private void HideTooltip() {
            // LeanTween.scale(_iconCanvas.gameObject, Vector3.zero, Config.tooltipEnterTime);
            _tooltipCanvas.transform.localScale = Vector3.zero;
            tooltipShowing = false;
        }

        private bool CanEdit() => CombatManager.Instance.CanEditDeck();

        public void SelectDeckSpace() {
            SelectedScale();
            state = SelectableState.Selected;
            transform.position = tmpSpace2.transform.position;

            selectedSpace = tmpSpace2;

            if (tmpSpace2.selectedAbility) {
                Destroy(tmpSpace2.selectedAbility.gameObject);
                tmpSpace2.ClearAbility();
            }

            MasterAudio.PlaySoundAndForget("ability_place");
            
            tmpSpace2.SetAbility(this);
            
            _iconCanvas.sortingOrder = 3;
        }

        public void SelectedScale() {
            LeanTween.cancel(scaleTweenId);
            scaleTweenId = LeanTween.scale(
                gameObject, Vector3.one * 0.9f, Config.abilityHoverTime
            ).id;
        }
        

        public void DeckSpaceHoverScale() {
            LeanTween.cancel(scaleTweenId);
            scaleTweenId = LeanTween.scale(
                gameObject, Vector3.one * Config.deckSpaceAbilityHoverScale, Config.abilityHoverTime
            ).id;
        }

        public void HoverScale() {
            LeanTween.cancel(scaleTweenId);
            scaleTweenId = LeanTween.scale(
                gameObject, Vector3.one * Config.abilityHoverScale, Config.abilityHoverTime
            ).id;
        }
    }
    
}