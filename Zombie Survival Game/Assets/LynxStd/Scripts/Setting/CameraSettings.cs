using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    [Serializable]
    public struct CameraStates
    {
        [Tooltip("Camera state used when the character is unarmed.")]
        public CameraState Default;

        [Tooltip("Camera state to use when the character is standing and aiming.")]
        public CameraState Aim;

        [Tooltip("Camera state to use when the character is crouching.")]
        public CameraState Crouch;

        [Tooltip("Camera state to use when the character is dead.")]
        public CameraState Dead;

        [Tooltip("Camera state to use when the character is standing and using zoom.")]
        public CameraState Zoom;

        [Tooltip("Camera state to use when the character is crouching and using zoom.")]
        public CameraState CrouchZoom;

        public static CameraStates GetDefault()
        {
            var states = new CameraStates();
            states.Default = CameraState.Default();
            states.Aim = CameraState.Aim();
            states.Crouch = CameraState.Crouch();
            states.Dead = CameraState.Dead();
            states.Zoom = CameraState.Zoom();
            states.CrouchZoom = CameraState.CrouchZoom();

            return states;
        }
    }

    [Serializable]
    public struct CameraState
    {
        [Tooltip("Position around which the camera is rotated.")]
        public Vector3 Pivot;

        [Tooltip("Offset from the pivot. The offset is rotated using camera's Horizontal and Vertical values.")]
        public Vector3 Offset;

        [Tooltip("Final rotation of the camera once it is in position.")]
        public Vector3 Orientation;

        [Tooltip("Field of view.")]
        [Range(0, 360)]
        public float FOV;

        public static CameraState Default()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.64f, 0.1f, -2.5f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState Aim()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.75f, -0.25f, -1.7f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState Crouch()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2, 0);
            settings.Offset = new Vector3(0.75f, -0.8f, -1f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState Dead()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 0, 0);
            settings.Offset = new Vector3(0f, 1f, -2.5f);
            settings.FOV = 60;

            return settings;
        }

        public static CameraState Zoom()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2f, 0);
            settings.Offset = new Vector3(0.75f, -0.25f, -1f);
            settings.FOV = 40;

            return settings;
        }

        public static CameraState CrouchZoom()
        {
            var settings = new CameraState();
            settings.Pivot = new Vector3(0, 2f, 0);
            settings.Offset = new Vector3(0.5f, -0.78f, -1f);
            settings.FOV = 40;

            return settings;
        }
    }
}
