using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    [Serializable]
    public struct DistanceRange
    {
        [Tooltip("Minimum interpolated value.")]
        public float Min;

        [Tooltip("Maximum interpolated value. ")]
        public float Max;

        [Tooltip("Delay is set to MinDelay when distance to camera is lower than this value.")]
        public float MinDistance;

        [Tooltip("Delay is set to MaxDelay when distance to camera is greater than this value.")]
        public float MaxDistance;

        public DistanceRange(float min, float max, float minDistance, float maxDistance)
        {
            Min = min;
            Max = max;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
        }

        public float Get(Vector3 position)
        {
            if (CameraManager.Main == null || CameraManager.Main.transform == null)
                return Min;
            else
                return Get(Vector3.Distance(CameraManager.Main.transform.position, position));
        }

        public float Get(float distance)
        {
            float t = Mathf.Clamp01((distance - MinDistance) / (MaxDistance - MinDistance));
            return Mathf.Lerp(Min, Max, t);
        }
    }
}
