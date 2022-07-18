using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace DefaultNamespace {
    public class GameUI : MonoBehaviour {
        
        public static GameUI Instance;

        private VisualElement root;

        public VisualElement gameOverLightbox;
        public Label resultLabel;
        public Button playAgainButton;
        public Button quitButton;

        public VisualElement levelCompleteOverlay;
        public Button nextLevelButton;
        
        public VisualElement tutorialOverlay;
        public Button playButton;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(this);
            }
            else {
                Instance = this;
            }
        }

        private void OnEnable() {
            root = GetComponent<UIDocument>().rootVisualElement;

            gameOverLightbox = root.Q<VisualElement>("GameOverLightbox");
            resultLabel = root.Q<Label>("ResultLabel");
            playAgainButton = root.Q<Button>("PlayAgainButton");
            quitButton = root.Q<Button>("QuitButton");

            levelCompleteOverlay = root.Q<VisualElement>("LevelCompleteOverlay");
            nextLevelButton = root.Q<Button>("NextLevelButton");

            tutorialOverlay = root.Q<VisualElement>("Tutorial");
            playButton = root.Q<Button>("StartGame");

            playButton.clicked += HideTutorial;
        }

        public void ShowMenu() {
            gameOverLightbox.style.display = DisplayStyle.Flex;
        }

        public void HideMenu() {
            gameOverLightbox.style.display = DisplayStyle.None;
        }

        public void HideTutorial() {
            tutorialOverlay.style.display = DisplayStyle.None;
        }

        public void ShowLevelComplete() {
            levelCompleteOverlay.style.display = DisplayStyle.Flex;
        }
        
        public void HideLevelComplete() {
            levelCompleteOverlay.style.display = DisplayStyle.None;
        }

        public bool MenuIsOpen => gameOverLightbox.style.display == DisplayStyle.Flex;
        
        public bool TutorialIsOpen => tutorialOverlay.style.display == DisplayStyle.Flex;

        public bool LevelCompleteIsOpen => levelCompleteOverlay.style.display == DisplayStyle.Flex;
    }
}