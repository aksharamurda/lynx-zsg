using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LynxStd
{
    public class BloodSplat : MonoBehaviour
    {

        private Image splatBlood;
        public Color startColor;
        public Color endColor;
        public float speed = 1.0f;

        float startTime;

        void Start()
        {
            startTime = Time.time;
            splatBlood = GetComponent<Image>();
            splatBlood.enabled = false;
        }

        public void OnHit()
        {
            splatBlood.enabled = true;
            startTime = Time.time;
        }

        void Update()
        {
            float t = (Time.time - startTime) * speed;
            splatBlood.color = Color.Lerp(startColor, endColor, t);
        }
    }
}
