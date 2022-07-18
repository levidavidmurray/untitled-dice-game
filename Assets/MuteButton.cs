using DarkTonic.MasterAudio;
using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour {

    private bool isMuted = false;
    private Image buttonImage;

    private void Start() {
        buttonImage = GetComponent<Image>();
    }

    public void ToggleMute() {
        isMuted = !isMuted;

        var suffix = isMuted ? "muted" : "volume";
        buttonImage.sprite = Resources.Load<Sprite>($"Sprites/ICON_{suffix}");

        if (isMuted) {
            MasterAudio.MuteEverything();
        }
        else {
            MasterAudio.UnmuteEverything();
        }

    }
    
    
}
