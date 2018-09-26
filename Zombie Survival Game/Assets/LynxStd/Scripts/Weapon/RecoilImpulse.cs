using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    public struct RecoilImpulse
    {
        public Vector3 Direction;

        public float Progress;

        public RecoilImpulse(Vector3 direction)
        {
            Direction = direction;
            Progress = 0.0f;
        }
    }
}
