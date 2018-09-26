using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    [Serializable]
    public struct GrenadeSettings
    {
        [Tooltip("Grenade to turn on and off and clone when throwing from left hand.")]
        public GameObject Left;

        [Tooltip("Grenade to turn on and off and clone when throwing from right hand.")]
        public GameObject Right;

        [Tooltip("Maximum allowed initial grenade velocity.")]
        public float MaxVelocity;

        [Tooltip("Gravity applied to the grenade. Positive values point down.")]
        public float Gravity;

        [Tooltip("Time in seconds between each calculated point in the arc.")]
        public float Step;

        [Tooltip("Origin of grenade path relative to the feet when standing. Inverted for the left hand.")]
        public Vector3 StandingOrigin;

        [Tooltip("Origin of grenade path relative to the feet when crouching. Inverted for the left hand.")]
        public Vector3 CrouchOrigin;

        public static GrenadeSettings Default()
        {
            var settings = new GrenadeSettings();
            settings.MaxVelocity = 10;
            settings.Gravity = 12.5f;
            settings.Step = 0.05f;
            settings.StandingOrigin = new Vector3(0.33f, 1.88f, 0);
            settings.CrouchOrigin = new Vector3(0.5f, 1.43f, 0);

            return settings;
        }
    }
}
