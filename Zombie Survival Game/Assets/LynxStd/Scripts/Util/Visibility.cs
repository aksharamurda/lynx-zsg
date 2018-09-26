using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    public class Visibility : MonoBehaviour
    {
        public bool IsVisible
        {
            get { return _isVisible; }
        }

        private bool _isVisible;

        private void OnBecameVisible()
        {
            _isVisible = true;
        }

        private void OnBecameInvisible()
        {
            _isVisible = false;
        }
    }
}
