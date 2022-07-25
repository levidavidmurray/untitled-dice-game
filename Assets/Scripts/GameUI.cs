using System;
using DarkTonic.MasterAudio;
using UnityEngine;
using UnityEngine.UIElements;

namespace DefaultNamespace {
    public class GameUI : MonoBehaviour {
        
        public static GameUI Instance;
        public GameConfig Config;

        private VisualElement root;

        public VisualElement gameOverLightbox;
        public Label resultLabel;
        public Button restartButton;
        public Button quitButton;

        public VisualElement levelCompleteOverlay;
        public Button nextLevelButton;
        
        public VisualElement tutorialOverlay;
        public Button playButton;
        public Button helpButton;
        public Button muteButton;
        public Label levelText;

        private bool _isMuted;
        private Sprite _mutedSprite;
        private Sprite _unmutedSprite;

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
            restartButton = root.Q<Button>("RestartButton");
            quitButton = root.Q<Button>("QuitButton");
            levelCompleteOverlay = root.Q<VisualElement>("LevelCompleteOverlay");
            nextLevelButton = root.Q<Button>("NextLevelButton");
            tutorialOverlay = root.Q<VisualElement>("Tutorial");
            playButton = root.Q<Button>("StartGame");
            helpButton = root.Q<Button>("HelpButton");
            muteButton = root.Q<Button>("MuteButton");
            levelText = root.Q<Label>("Level");
            
            _mutedSprite = Resources.Load<Sprite>("Sprites/button-volume-muted");
            _unmutedSprite = Resources.Load<Sprite>("Sprites/button-volume-unmuted");
            
            RegisterButtonCallbacks(nextLevelButton);
            RegisterButtonCallbacks(restartButton);
            RegisterButtonCallbacks(quitButton);
            RegisterButtonCallbacks(playButton);
            RegisterButtonCallbacks(helpButton);
            RegisterButtonCallbacks(muteButton);

            playButton.clicked += HideTutorial;

            helpButton.clicked += ShowTutorial;
            muteButton.clicked += () => {
                _isMuted = !_isMuted;
                muteButton.style.backgroundImage = new StyleBackground(_isMuted ? _mutedSprite : _unmutedSprite);
                
                if (_isMuted)
                    MasterAudio.MuteEverything();
                else
                    MasterAudio.UnmuteEverything();
            };
        }

        private void RegisterButtonCallbacks(Button button) {
            button.RegisterCallback<MouseOverEvent>((type) => {
                button.style.scale = new Scale(Vector3.one * Config.buttonHoverScale);
                MasterAudio.PlaySoundAndForget("Select_Tap_UI_Sound_1");
            });
            button.RegisterCallback<MouseOutEvent>((type) => {
                button.style.scale = new Scale(Vector3.one);
            });
        }

        public void ShowMenu() {
            gameOverLightbox.style.display = DisplayStyle.Flex;
        }

        public void HideMenu() {
            gameOverLightbox.style.display = DisplayStyle.None;
        }

        public void ShowTutorial() {
            tutorialOverlay.style.display = DisplayStyle.Flex;
        }

        public void HideTutorial() {
            tutorialOverlay.style.display = DisplayStyle.None;
            playButton.text = "Close";
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