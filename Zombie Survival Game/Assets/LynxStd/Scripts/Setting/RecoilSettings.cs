using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    [Serializable]
    public struct RecoilSettings
    {
        [Tooltip("Strength of a single bullet fire.")]
        [Range(0, 1)]
        public float Strength;

        [Tooltip("Time in seconds it takes to move the right hand by recoil.")]
        [Range(0, 1)]
        public float AttackTime;

        [Tooltip("Time in seconds it takes for the right hand to return to its position.")]
        [Range(0, 1)]
        public float DecayTime;

        [Tooltip("How much the gun is lifted up.")]
        [Range(-5, 5)]
        public float UpForce;

        [Tooltip("Limits the total amount of recoil applied to the gun.")]
        [Range(0, 1)]
        public float Limit;

        public static RecoilSettings Default()
        {
            var settings = new RecoilSettings();
            settings.Strength = 0.1f;
            settings.AttackTime = 0.05f;
            settings.DecayTime = 0.25f;
            settings.UpForce = 0.5f;
            settings.Limit = 0.1f;

            return settings;
        }
    }
}
