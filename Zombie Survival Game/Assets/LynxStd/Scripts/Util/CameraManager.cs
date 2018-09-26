using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    public static class CameraManager
    {
        public static Camera Main
        {
            get
            {
                if (_camera == null)
                    _camera = GameObject.FindObjectOfType<PlayerCamera>()._camera;

                return _camera;
            }
        }

        private static Camera _camera;

        public static void Update()
        {
            _camera = GameObject.FindObjectOfType<PlayerCamera>()._camera;
        }
    }
}
