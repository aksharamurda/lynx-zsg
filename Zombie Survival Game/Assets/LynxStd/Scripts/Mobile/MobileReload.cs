using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LynxStd
{
    public class MobileReload : MonoBehaviour, IPointerDownHandler
    {
        private UnityEvent reloadEvent;
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
            reloadEvent.AddListener(OnReload);
        }


        void OnReload()
        {
            playerInput.OnMobileReload();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnReload();
        }

    }
}
