using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;

public class UsedAbility : MonoBehaviour {
    
    private Image _abilityImage;
    private Vector3 _startPos;

    private GameConfig Config => CombatManager.Instance.Config;

    private void Awake() {
        _abilityImage = GetComponent<Image>();
        _startPos = transform.localPosition;
    }

    public void ShowAbility(Ability ability) {
        transform.localPosition = _startPos;

        var abilityName = SelectableAbility.AbilityNameMap[ability].name;
        _abilityImage.sprite = Resources.Load<Sprite>($"Sprites/USE_{abilityName}");

        transform.LeanMoveLocalY(_startPos.y + Config.showAbilityTranslateDist, Config.showAbilityTranslateTime);
        LeanTween.value(0f, 1f, Config.showAbilityAlphaTime / 2f).setOnUpdate((float value) => {
            _abilityImage.color = new Color(1, 1, 1, value);
        }).setLoopPingPong(1);

    }
    
}
