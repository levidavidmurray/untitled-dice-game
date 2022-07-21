using UnityEngine;

namespace DefaultNamespace {
    [CreateAssetMenu(fileName = "AnimConfig", menuName = "Create/Anim Config", order = 0)]
    public class AnimConfig : ScriptableObject {
        public float statChangeAnimTime = 0.3f;
        public AnimationCurve statChangeAnimCurve;

        public float attackFullAnimTime = 0.4f;
        public AnimationCurve attackFullAnimCurve;

        public float attackHalfAnimTime = 0.4f;
        public AnimationCurve attackHalfAnimCurve;

        public float attackHomeAnimTime = 0.45f;
        public AnimationCurve attackHomeAnimCurve;

        public float takeDamageTime = 0.4f;
        public AnimationCurve takeDamageScaleAnimCurve;

        public float startingDeckUnlockDelay = 1f;
        public float deckLockDist = 1.5f;

        public float deckLockFightDelay = 0.75f;
        public float deckLockAnimTime = 0.4f;
        public AnimationCurve deckLockAnimCurve;
        
        public float deckUnlockAnimTime = 0.4f;
        public AnimationCurve deckUnlockAnimCurve;

        public float turnCounterFadeTime = 0.3f;

    }
}