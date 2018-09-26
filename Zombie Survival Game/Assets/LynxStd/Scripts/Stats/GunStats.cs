using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    [CreateAssetMenu(fileName = "Gun", menuName = "Weapon/Stats")]
    public class GunStats : ScriptableObject
    {
        public string nameGun;

        public float fireRateGun = 13;
        public float damageGun = 5;
        public int clipSizeGun = 30;
        public int clipGun = 30;

        public float recoilStrength = 0.09f;
        public float recoilAttackTime = 0;
        public float recoilDecayTime = 0.1f;
        public float recoilUpForce = 0.02f;
        public float recoilLimit = 0.12f;
    }
}
