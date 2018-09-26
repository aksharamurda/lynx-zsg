using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    [Serializable]
    public struct HandOverwrite
    {
        [Tooltip("Marker to use when the character is aiming.")]
        public Transform Aim;

        [Tooltip("Marker to use when the character is standing in a low cover facing left.")]
        public Transform LowCoverLeft;

        [Tooltip("Marker to use when the character is standing in a low cover facing right.")]
        public Transform LowCoverRight;

        [Tooltip("Marker to use when the character is standing in a tall cover facing left.")]
        public Transform TallCoverLeft;

        [Tooltip("Marker to use when the character is standing in a tall cover facing right.")]
        public Transform TallCoverRight;
    }
}
