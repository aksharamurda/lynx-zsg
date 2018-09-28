using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LynxStd
{
    public class MobileFire : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private UnityEvent<bool> fireEvent;

        private bool pointerDown;
        private PlayerInput playerInput;
        public GameObject rootPanel;

        void Awake()
        {
            playerInput = GameObject.FindObjectOfType<PlayerInput>();
            if (!SettingManager.instance.useMobileConsole)
                rootPanel.SetActive(false);
        }

        void Start()
        {
            fireEvent.AddListener(OnFire);
        }

        private void Update()
        {
            if (fireEvent != null)
                fireEvent.Invoke(pointerDown);
        }

        void OnFire(bool i)
        {
            playerInput.OnMobileFire(i);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pointerDown = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            pointerDown = false;
        }
    }
}
