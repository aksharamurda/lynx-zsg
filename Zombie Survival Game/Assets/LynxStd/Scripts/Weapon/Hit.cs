using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    public struct Hit
    {
        public Vector3 Position;

        public Vector3 Normal;

        public float Damage;

        public GameObject Attacker;

        public GameObject Target;

        public Hit(Vector3 position, Vector3 normal, float damage, GameObject attacker, GameObject target)
        {
            Position = position;
            Normal = normal;
            Damage = damage;
            Attacker = attacker;
            Target = target;
        }
    }
}