using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    [Serializable]
    public struct TurnSettings
    {
        [Tooltip("Maximum allowed angle between aim direction and body direction.")]
        public float MaxAimAngle;

        [Tooltip("Controls whether weapons are aimed at a point in a world (on) or at an infinite distance (off).")]
        public bool IsAimingPrecisely;

        [Tooltip("Should the character turn towards the fire direction immediately instead of animating the turn.")]
        public bool ImmediateAim;

        [Tooltip("How quickly the character model is orientated towards the standing direction.")]
        [Range(0, 50)]
        public float StandingRotationSpeed;

        [Tooltip("How quickly the character model is orientated towards the running direction.")]
        [Range(0, 50)]
        public float RunningRotationSpeed;

        [Tooltip("How quickly the character model is orientated towards the running direction.")]
        [Range(0, 50)]
        public float SprintingRotationSpeed;

        [Tooltip("How quickly the character model is orientated towards the throw direction.")]
        [Range(0, 50)]
        public float GrenadeRotationSpeed;

        public static TurnSettings Default()
        {
            var result = new TurnSettings();
            result.MaxAimAngle = 60;
            result.IsAimingPrecisely = false;
            result.ImmediateAim = false;
            result.StandingRotationSpeed = 5;
            result.RunningRotationSpeed = 5;
            result.SprintingRotationSpeed = 20;
            result.GrenadeRotationSpeed = 20;

            return result;
        }
    }
}
