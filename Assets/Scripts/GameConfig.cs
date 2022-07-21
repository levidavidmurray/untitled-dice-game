﻿using UnityEngine;

namespace DefaultNamespace {
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Custom/Game Config", order = 0)]
    public class GameConfig : ScriptableObject {

        public Ability enemyAbilityOverride = Ability.EMPTY;
        
        public int maxLevel = 3;
        public float turnDelay = 1f;
        public float attackHomeDelay = 2f;
        public float gameOverDelay = 2f;
        
        public int dupeAbilityLimit = 3;
        public int numRollsInTurn = 3;
        public float autoTurnDelay = 1f;

        public float tooltipHoverDelay = 2f;
        public float tooltipEnterTime = 0.2f;
        public float tooltipExitTime = 0f;
        
        public int defaultHealth = 3;
        public int defaultAttack = 1;
        public int defaultDefense = 0;

        public int maxHealth = 6;
        public int maxAttack = 5;
        public int maxDefense = 3;

        public int minAttack = 1;
        public int minDefense = -1;

        public float deathOpacity = 0.5f;

        public int defaultHomeHealth = 20;

        public float abilityHoverScale = 1.00f;
        public float abilityHoverTime = 0.15f;

        public float deckEditDisabledOpacity = 0.3f;

        public float deckSpaceAbilityHoverScale = 0.80f;

        public Color rollTextDisabledColor;
        public Color rollTextUnhoveredColor;
        public Color rollTextHoveredColor;

        public float showAbilityAlphaTime = 1f;
        public float showAbilityTranslateTime = 1f;
        public float showAbilityTranslateDist = 1f;

        public int damageIndicatorFlickerCount = 4;
        public float damageIndicatorFlickerTime = 0.6f;
        public float damageIndicatorMinAlpha = 0f;

        public float fightDelay = 0.25f;
        public float attackDelayForBuff = 0.3f;

        public float statChangeAnimTime = 0.3f;
        public AnimationCurve statChangeAnimCurve;

        public float attackAnimTime = 0.3f;
        public AnimationCurve attackFullAnimCurve;
        public AnimationCurve attackHalfAnimCurve;

        public int maxDuplicateRolls = 2;

        // debug
        public bool debugNoDamage = false;
        public bool debugKeepUnselectedAbility = false;
    }
}