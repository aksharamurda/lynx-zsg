using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    public class Projectile : MonoBehaviour
    {
        [Tooltip("Speed of the projectile in meters per second.")]
        public float Speed = 10;

        [HideInInspector]
        public float Distance = 1;

        [HideInInspector]
        public Vector3 Direction;

        [HideInInspector]
        public GameObject Target;

        [HideInInspector]
        public Hit Hit;

        private float _path = 0;

        private void OnEnable()
        {
            _path = 0;
        }

        private void Update()
        {
            transform.position += Direction * Speed * Time.deltaTime;
            _path += Speed * Time.deltaTime;

            if (_path >= Distance)
            {
                if (Target != null)
                    Target.SendMessage("OnHit", Hit, SendMessageOptions.DontRequireReceiver);

                GameObject.Destroy(gameObject);
            }
        }
    }
}
