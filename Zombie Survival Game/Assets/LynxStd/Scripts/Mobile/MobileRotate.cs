using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LynxStd
{
    public class MobileRotate : LynxTouchBase
    {
        private PlayerInput playerInput;
        public GameObject rootPanel;

        void Awake()
        {
            playerInput = GameObject.FindObjectOfType<PlayerInput>();
            if (!SettingManager.instance.useMobileConsole)
                rootPanel.SetActive(false);
        }

        protected override void Update()
        {
            base.Update();

            if (IsPointerOverGameObject())
            {
                playerInput.pointer_x = 0;
                playerInput.pointer_y = 0;
                return;
            }


            if (Delta.sqrMagnitude > float.Epsilon)
            {
                playerInput.pointer_x = Delta.x;
                playerInput.pointer_y = Delta.y;
            }
            else
            {
                playerInput.pointer_x = 0;
                playerInput.pointer_y = 0;
            }
        }

        public static bool IsPointerOverGameObject()
        {
            //check mouse
            if (EventSystem.current.IsPointerOverGameObject())
                return true;

            //check touch
            if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
            {
                if (EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId))
                    return true;
            }

            return false;
        }
    }
}
