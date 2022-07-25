using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;

public class UsedAbility : MonoBehaviour {
    
    private SpriteRenderer _spriteRenderer;
    private Vector3 _startPos;

    private GameConfig Config => CombatManager.Instance.Config;

    private void Awake() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _startPos = transform.localPosition;
    }

    public void ShowAbility(Ability ability) {
        transform.localPosition = _startPos;

        Sprite sprite = CombatManager.Instance.abilityUseSpriteMap[ability];
        _spriteRenderer.sprite = sprite;

        transform.LeanMoveLocalY(_startPos.y + Config.showAbilityTranslateDist, Config.showAbilityTranslateTime);
        LeanTween.value(0f, 1f, Config.showAbilityAlphaTime / 2f).setOnUpdate((float value) => {
            _spriteRenderer.color = new Color(1, 1, 1, value);
        }).setLoopPingPong(1);

    }
    
}
