using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LynxStd
{
    public class PlayerController : MonoBehaviour
    {
        public bool IsActivelyFacing
        {
            get { return _isActivelyFacing; }
        }

        public bool IsZooming
        {
            get { return _motor.IsAlive && ZoomInput && _motor.IsAiming; }
        }

        public bool IsScoped
        {
            get { return IsZooming && _motor.Gun != null && _motor.Gun.Scope != null; }
        }

        [Tooltip("Is the character always aiming in camera direction when not in cover.")]
        public bool AlwaysAim = false;

        [Tooltip("How long to continue aiming after no longer needed.")]
        public float AimSustain = 0.4f;

        [Tooltip("Time in seconds to keep the gun down when starting to move.")]
        public float NoAimSustain = 0.14f;

        [Tooltip("Degrees to add when aiming a grenade vertically.")]
        public float ThrowAngleOffset = 30;

        [Tooltip("How high can the player throw the grenade.")]
        public float MaxThrowAngle = 45;

        [Tooltip("Prefab to instantiate to display grenade explosion preview.")]
        public GameObject ExplosionPreview;

        [Tooltip("Prefab to instantiate to display grenade path preview.")]
        public GameObject PathPreview;

        [Tooltip("Scope object and component that's enabled and maintained when using scope.")]
        public Image Scope;

        [HideInInspector]
        public bool FireInput;

        [HideInInspector]
        public bool ZoomInput;

        [HideInInspector]
        public Vector3 LookTargetInput;

        [HideInInspector]
        public Vector3 FireTargetInput;

        [HideInInspector]
        public float GrenadeHorizontalAngleInput;

        [HideInInspector]
        public float GrenadeVerticalAngleInput;

        [HideInInspector]
        public PlayerMovement MovementInput;

        private PlayerMotor _motor;

        private GameObject _explosionPreview;
        private GameObject _pathPreview;

        private bool _isSprinting;
        private bool _isAiming;
        private bool _isActivelyFacing;

        private float _noAimSustain;
        private float _aimSustain;
        private float _postSprintNoAutoAim;

        private bool _wasZooming;

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
        }


        private void Update()
        {
            _isActivelyFacing = AlwaysAim && !_isSprinting;

            updateMovement();

            {
                if (!_isSprinting)
                {
                    if (_motor.IsWeaponReady && FireInput)
                    {
                        if (_motor.Gun != null && _motor.Gun.IsClipEmpty)
                            _motor.InputReload();
                        else
                            _motor.InputFire();

                        _isActivelyFacing = true;
                    }

                    if (_motor.IsGunScopeReady && ZoomInput)
                    {
                        _motor.InputAim();
                        _motor.InputZoom();
                        _isActivelyFacing = true;
                    }
                }
            }

            if (_isSprinting)
            {
                _isAiming = false;
                _isActivelyFacing = false;
                FireInput = false;
                ZoomInput = false;
            }

            if (_isAiming && _aimSustain >= 0)
                _aimSustain -= Time.deltaTime;

            if (_noAimSustain >= 0)
                _noAimSustain -= Time.deltaTime;

            if (!FireInput && !ZoomInput)
            {
                if (_postSprintNoAutoAim >= 0)
                    _postSprintNoAutoAim -= Time.deltaTime;
            }
            else
            {
                _postSprintNoAutoAim = 0;
                _noAimSustain = 0;
            }

            if (((AlwaysAim || _isActivelyFacing) && _postSprintNoAutoAim <= float.Epsilon) ||
                 FireInput ||
                 ZoomInput)
            {
                _isAiming = true;
                _aimSustain = AimSustain;
            }
            else if (!_isAiming)
                _noAimSustain = NoAimSustain;

            if (!AlwaysAim)
                if (_aimSustain <= float.Epsilon || _noAimSustain > float.Epsilon)
                    _isAiming = false;

            if (_isAiming && _motor.Gun != null)
            {
                    _motor.InputAim();
            }

            if (FireInput || ZoomInput)
                _motor.InputPossibleImmediateTurn();

            if (_isActivelyFacing || _motor.IsAiming )
            {
                if (!_isSprinting)
                    _motor.SetBodyLookTarget(LookTargetInput);

                _motor.SetLookTarget(LookTargetInput);
                _motor.SetFireTarget(FireTargetInput);
            }

            if (ZoomInput && !_wasZooming)
                SendMessage("OnZoom", SendMessageOptions.DontRequireReceiver);
            else if (!ZoomInput && _wasZooming)
                SendMessage("OnUnzoom", SendMessageOptions.DontRequireReceiver);

            _wasZooming = ZoomInput;

            if (Scope != null)
            {
                if (Scope.gameObject.activeSelf != IsScoped)
                {
                    Scope.gameObject.SetActive(IsScoped);

                    if (Scope.gameObject.activeSelf)
                        Scope.sprite = _motor.Gun.Scope;
                }
            }
        }

        private void updateMovement()
        {
            var movement = MovementInput;

            var wasSprinting = _isSprinting;
            _isSprinting = false;

            if (movement.IsMoving)
            {
                _isActivelyFacing = true;

                // Smooth sprinting turns
                if (movement.Magnitude > 1.1f)
                {
                    var lookAngle = Util.AngleOfVector(LookTargetInput - _motor.transform.position);

                    // Don't allow sprinting backwards
                    if (Mathf.Abs(Mathf.DeltaAngle(lookAngle, Util.AngleOfVector(movement.Direction))) < 100)
                    {
                        var wantedAngle = Util.AngleOfVector(movement.Direction);
                        var bodyAngle = _motor.transform.eulerAngles.y;
                        var delta = Mathf.DeltaAngle(bodyAngle, wantedAngle);

                        const float MaxSprintTurn = 60;

                        if (delta > MaxSprintTurn)
                            movement.Direction = Quaternion.AngleAxis(bodyAngle + MaxSprintTurn, Vector3.up) * Vector3.forward;
                        else if (delta < -MaxSprintTurn)
                            movement.Direction = Quaternion.AngleAxis(bodyAngle - MaxSprintTurn, Vector3.up) * Vector3.forward;

                        _motor.SetBodyLookTarget(_motor.transform.position + movement.Direction * 100);
                        _motor.InputPossibleImmediateTurn(false);

                        _isSprinting = true;
                    }
                    else
                        movement.Magnitude = 1.0f;
                }

                if (!_isSprinting && wasSprinting)
                    _postSprintNoAutoAim = 0.0f;
            }
            else if (wasSprinting)
                _postSprintNoAutoAim = 0.3f;

            _motor.InputMovement(movement);
        }

    }
}
