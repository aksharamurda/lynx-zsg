using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    public class SettingManager : MonoBehaviour
    {
        public static SettingManager instance;

        [Header("Control")]
        public bool useMobileConsole;

        void Awake()
        {
            instance = this;
        }
    }
}
