using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{

    public struct PlayerMovement{

        public Vector3 Value { get { return Direction * Magnitude; } }

        public bool IsMoving { get { return Value.sqrMagnitude > 0.1f; } }

        public bool IsRunning { get { return Magnitude > 0.6f & IsMoving && !IsSprinting; } }

        public bool IsSprinting { get { return Magnitude > 1.1f && IsMoving; } }

        public Vector3 Direction;

        public float Magnitude;

        public PlayerMovement(Vector3 direction, float magnitude)
        {
            Direction = direction;
            Magnitude = magnitude;
        }
    }
}
