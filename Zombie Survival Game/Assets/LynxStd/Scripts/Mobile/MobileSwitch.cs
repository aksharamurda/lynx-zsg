using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LynxStd
{
    public class MobileSwitch : MonoBehaviour, IPointerDownHandler
    {
        private UnityEvent switchEvent;

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
            switchEvent.AddListener(OnSwitch);
        }

        void OnSwitch()
        {
            playerInput.OnMobileSwitch();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnSwitch();
        }

    }
}
