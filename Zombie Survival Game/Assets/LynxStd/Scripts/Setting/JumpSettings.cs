using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    [Serializable]
    public struct JumpSettings
    {
        [Tooltip("Jump up velocity.")]
        [Range(0, 20)]
        public float Strength;

        [Tooltip("Jump forward velocity.")]
        [Range(0, 20)]
        public float Speed;

        [Tooltip("Character's capsule height during a jump.")]
        [Range(0, 10)]
        public float CapsuleHeight;

        [Tooltip("Duration of character's capsule height adjustment.")]
        [Range(0, 10)]
        public float HeightDuration;

        [Tooltip("How fast the character turns towards the jump direction.")]
        public float RotationSpeed;

        [Tooltip("Fraction of the jump animation that has to be played before the character can aim again.")]
        [Range(0, 1)]
        public float AnimationLock;

        public static JumpSettings Default()
        {
            JumpSettings settings;
            settings.Strength = 6;
            settings.Speed = 5;
            settings.CapsuleHeight = 1.0f;
            settings.HeightDuration = 0.75f;
            settings.RotationSpeed = 10;
            settings.AnimationLock = 0.75f;

            return settings;
        }
    }
}
