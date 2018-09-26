using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    public class PlayerMotor : MonoBehaviour
    {
        public const int FirstWeaponLayer = 2;

        #region Properties

        public GameObject Target
        {
            get { return _target; }
        }

        public Vector3 BodyLookTarget
        {
            get { return _currentBodyLookTarget; }
        }

        public Vector3 LookTarget
        {
            get { return _lookTarget; }
        }

        public Vector3 FireTarget
        {
            get { return _fireTarget; }
        }

        public Vector3 Neck
        {
            get { return _neck.position; }
        }

        public Vector3 HeadForward
        {
            get
            {
                var vec = HeadLookTarget - transform.position;
                vec.y = 0;

                return vec.normalized;
            }
        }

        public Vector3 HeadLookTarget
        {
            get { return _isHeadLookTargetOverriden ? _headLookTargetOverride : _lookTarget; }
        }

        public bool IsCrouching
        {
            get { return _isCrouching; }
        }

        public float LookAngle
        {
            get { return _horizontalLookAngle; }
        }

        public float VerticalAngle
        {
            get { return _verticalLookAngle; }
        }
        
        public Gun Gun
        {
            get { return _gun; }
            set
            {
                for (int i = 0; i < Weapons.Length; i++)
                    if (Weapons[i].Gun == value)
                    {
                        InputWeapon(i + 1);
                        break;
                    }
            }
        }

        public bool IsInAimableState
        {
            get
            {
                if (CurrentWeapon <= 0 || CurrentWeapon > Weapons.Length)
                    return false;

                if (_isChangingWeapon ||  _isFalling || _isJumping || IsSprinting || _localMovement.z >= 1.1f)
                    return false;

                return true;
            }
        }

        public bool IsZooming
        {
            get { return IsAimingGun && (_wantsToZoom || _wantedToZoom); }
        }

        public bool IsWeaponReady
        {
            get
            {
                if (!IsInAimableState || _isReloading)
                    return false;

                return true;
            }
        }

        public bool IsWeaponScopeReady
        {
            get
            {
                if (!IsInAimableState || (_isReloading && !canAimOnReload))
                    return false;

                return true;
            }
        }

        public bool IsGunReady
        {
            get
            {
                if (!IsWeaponReady)
                    return false;

                if (Weapons[CurrentWeapon - 1].Gun != null)
                    return _gun != null;

                return false;
            }
        }

        public bool IsGunScopeReady
        {
            get
            {
                if (!IsWeaponScopeReady)
                    return false;

                if (Weapons[CurrentWeapon - 1].Gun != null)
                    return _gun != null;

                return false;
            }
        }

        public bool IsGoingToSprint
        {
            get { return _isGrounded && _inputMovement.Magnitude > 1.1f; }
        }

        public bool IsSprinting
        {
            get { return _isSprinting; }
        }

        public bool IsReloading
        {
            get { return _isReloading; }
        }

        public Vector3 GunOrigin
        {
            get { return _gun == null ? transform.position : _gun.Origin; }
        }

        public Vector3 GunDirection
        {
            get { return _gun == null ? transform.forward : _gun.Direction; }
        }

        public Vector3 GunTargetDirection
        {
            get { return _gun == null ? transform.forward : _gun.TargetDirection; }
        }

        public Vector3 Top
        {
            get { return transform.position + Vector3.up * _defaultCapsuleHeight; }
        }

        public float RecoilIntensity
        {
            get { return _gun == null ? 0 : _gun.RecoilIntensity; }
        }

        public bool WouldAim
        {
            get
            {
                return CurrentWeapon > 0;
            }
        }

        public bool IsAimingTool
        {
            get
            {
                return _isUsingWeapon && CurrentWeapon > 0 && Weapons[CurrentWeapon - 1].IsAnAimableTool(_isUsingWeaponAlternate);
            }
        }

        public bool IsAimingGun
        {
            get
            {
                if (!_isFalling &&
                    !_isJumping &&
                    _canAim &&
                    IsGunScopeReady)
                {
                    if ((wantsToAim ))
                        return true;
                    else if (CurrentWeapon > 0 && (wantsToAim))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        public bool IsAiming
        {
            get { return IsAimingGun || IsAimingTool; }
        }

        public float StandingHeight
        {
            get { return _defaultCapsuleHeight; }
        }

        public float CurrentHeight
        {
            get { return _capsule.height; }
        }

        public float TargetHeight
        {
            get
            {
                var targetHeight = _defaultCapsuleHeight;

                if (_isJumping && _jumpTimer < JumpSettings.HeightDuration)
                    targetHeight = JumpSettings.CapsuleHeight;
                else if (_isCrouching)
                    targetHeight = CrouchHeight;

                return targetHeight;
            }
        }

        public int NextWeapon
        {
            get { return _weaponToChangeTo; }
        }

        #endregion

        #region Public fields

        [Tooltip("Controls wheter the character is in a state of death.")]
        public bool IsAlive = true;

        [Tooltip("Speed multiplier for the movement speed. Adjusts animations.")]
        public float Speed = 1.0f;

        [Tooltip("Distance below feet to check for ground.")]
        [Range(0, 1)]
        public float GroundThreshold = 0.3f;

        [Tooltip("Minimal height to trigger state of falling. It’s ignored when jumping over gaps.")]
        [Range(0, 10)]
        public float FallThreshold = 2.0f;

        [Tooltip("Movement to obstacles closer than this is ignored.")]
        [Range(0, 2)]
        public float ObstacleDistance = 0.05f;

        [Tooltip("Aiming in front of obstacles closer than this distance is ignored.")]
        [Range(0, 5)]
        public float AimObstacleDistance = 0.4f;

        [Tooltip("Gravity force applied to this character.")]
        public float Gravity = 10;

        [Tooltip("Sets the origin of bullet raycasts, either a camera or an end of a gun.")]
        public bool IsFiringFromCamera = true;

        [Tooltip("ID of the currently held weapon. Index starts from 1. Value of 0 means unarmed.")]
        public int CurrentWeapon = 0;

        [Tooltip("Capsule height when crouching.")]
        public float CrouchHeight = 1.5f;

        [Tooltip("Descriptions of currently held weapons.")]
        public WeaponDescription[] Weapons;

        [Tooltip("IK settings for the character.")]
        public IKSettings IK = IKSettings.Default();

        [Tooltip("Settings for jumping.")]
        public JumpSettings JumpSettings = JumpSettings.Default();

        [Tooltip("Settings for turning and aiming.")]
        public TurnSettings TurnSettings = TurnSettings.Default();

        #endregion

        #region Private fields

        private bool _hasRegistered;

        private CapsuleCollider _capsule;
        private Rigidbody _body;
        private Animator _animator;
        private SkinnedMeshRenderer _renderer;
        private Visibility _visibility;
        private Transform _neck;
        private Renderer[] _renderers;
        private int _targetLayer;

        private Vector3 _lastKnownPosition;
        private float _previousCapsuleHeight;


        private bool _isGrounded = true;
        private bool _wasGrounded;
        private bool _isFalling;

        private bool _wasAimingGun;

        private bool _wantsToZoom;
        private bool _wantedToZoom;

        private bool _hasLookTarget;
        private float _bodyTurnSpeed = 10;
        private Vector3 _bodyLookTarget;
        private Vector3 _currentBodyLookTarget;
        private Vector3 _lookTarget;

        private Vector3 _fireTarget;
        private Vector3 _currentFireTarget;

        private Vector3 _headLookTargetOverride;
        private Vector3 _headLookTargetOverrideTarget;
        private bool _isHeadLookTargetOverriden;
        private float _headTurnSpeed = 10;

        private float _horizontalBodyAngle;
        private float _currentHorizontalBodyAngle;
        private float _horizontalLookAngle;
        private float _verticalLookAngle;
        private float _bodyAngleDiff;
        private bool _wouldTurnImmediately;

        private float _currentAnimatedAngle;
        private float _requiredAnimationRotation;

        private float _leftHandIntensity = 0;
        private float _armAimIntensity = 0;
        private float _previousArmAimTargetIntensity = 0;
        private float _throwAimIntensity = 0;
        private float _headAimIntensity = 0;
        private float _aimPivotIntensity = 0;

        private bool _isChangingWeapon = false;
        private int _weaponToChangeTo = 0;
        private bool _isPreviousWeaponHidden = false;
        private Gun _gun;
        private GameObject _weapon;
        private float _armTimer = 0;

        private float _movementInput = 0;

        private bool _isSprinting = false;
        private bool _wantsToSprint = false;

        private bool _useGravity = true;
        private float _vaultHeight;

        private bool _isUsingWeapon = false;
        private bool _isUsingWeaponAlternate = false;
        private bool _wasWeaponUsed = false;
        private bool _keepUsingWeapon = false;
        private bool _wantedToKeepUsingWeapon = false;
        private bool _hasBeganUsingWeapon = false;
        private float _normalizedWeaponUseTime = 0;

        private bool _isReloading = false;
        private bool _hasBeganReloading = false;
        private float _reloadTime = 0;
        private float _normalizedReloadTime = 0;

        private bool _canAim = true;
        private float _canAimLerp = 1;

        private bool _isJumping = false;
        private float _jumpAngle;
        private bool _isIntendingToJump = false;
        private bool _wantsToJump = false;
        private float _nextJumpTimer = 0;
        private float _jumpLegTimer = 0;
        private float _jumpTimer = 0;
        private float _normalizedJumpTime = 0;

        private float _defaultCapsuleHeight = 2.0f;
        private float _defaultCapsuleCenter = 1.0f;

        private float _leftMoveIntensity = 1;
        private float _rightMoveIntensity = 1;
        private float _backMoveIntensity = 1;
        private float _frontMoveIntensity = 1;

        private IK _aimIK = new IK();
        private IK _recoilIK = new IK();
        private IK _leftHandIK = new IK();
        private IK _sightIK = new IK();
        private IK _throwIK = new IK();

        private Vector3 _localMovement = new Vector3(0, 0, 0);

        private bool _isCrouching = false;
        private bool _wantsToCrouch = false;
        private PlayerMovement _inputMovement;
        private PlayerMovement _currentMovement;
        private bool _wantsToAim;
        private bool _wantsToFire;
        private bool _hasFireCondition;
        private int _fireConditionSide;
        private bool _dontChangeArmAimingJustYet = false;
        private bool _wantsToFaceInADirection;

        private Quaternion _lastHit = Quaternion.identity;
        private float _lastHitStrength;

        private int _lastFoot;

        private Vector3 _positionToSnap;
        private Vector3 _positionToSnapStart;
        private float _positionSnapTimer;
        private float _positionSnapTimerStart;

        private float _directionChangeDelay;
        private bool _wasAlive = true;

        private Transform _lastAimTransform;

        private Collider[] _colliderCache = new Collider[16];
        private RaycastHit[] _raycastHits = new RaycastHit[16];

        private WeaponAnimationStates _weaponStates = WeaponAnimationStates.Default();

        private GameObject _target;

        private bool _isOnSlope = false;
        private Vector3 _groundNormal;
        private float _slope;

        private float _noMovementTimer;
        private float _groundTimer;

        #endregion

        #region Public methods

        public void SetHeadLookTargetOverride(Vector3 target, float speed = 8f)
        {
            if (!_isHeadLookTargetOverriden)
                _headLookTargetOverride = TurnSettings.IsAimingPrecisely ? _currentFireTarget : _lookTarget;

            _headTurnSpeed = speed;
            _headLookTargetOverrideTarget = target;
            _isHeadLookTargetOverriden = true;
        }

        public void SetBodyLookTarget(Vector3 target, float speed = 8f)
        {
            _bodyLookTarget = target;

            if (!_hasLookTarget)
            {
                _lookTarget = _bodyLookTarget;
                _hasLookTarget = true;
            }

            _bodyTurnSpeed = speed;

            calculateBodyTurn();
        }

        public void SetLookTarget(Vector3 target)
        {
            if (_gun != null)
            {
                var vector = target - _gun.transform.position;
                var distance = vector.magnitude;

                if (distance < 2 && distance > 0.01f)
                    _lookTarget = _gun.transform.position + vector.normalized * 2;
                else
                    _lookTarget = target;
            }
            else
                _lookTarget = target;

            _hasLookTarget = true;
            _isHeadLookTargetOverriden = false;
        }

        public void FireFrom(Vector3 position)
        {
            if (_gun != null)
                _gun.SetFireFrom(position);
        }

        public void SetDefaultFireOrigin()
        {
            if (_gun != null)
                _gun.StopFiringFromCustom();
        }

        public void SetFireTarget(Vector3 target)
        {
            _fireTarget = target;
        }

        #endregion

        #region Commands

        public void OnUseWeapon()
        {
            useWeapon();
        }

        #endregion

        #region Events

        public void OnDead()
        {
            IsAlive = false;
            StartCoroutine(SlowMotionDead());
        }

        IEnumerator SlowMotionDead()
        {
            yield return new WaitForSeconds(0.75f);
            Time.timeScale = 0.2f;
        }


        public void OnHit(Hit hit)
        {

            GameObject.FindObjectOfType<BloodSplat>().OnHit();

            EZCameraShake.CameraShaker.Instance.ShakeOnce(0.2f, 5f, 0.5f, 0.5f);
            if (IK.HitBone == null)
                return;

            var forwardDot = Vector3.Dot(-IK.HitBone.transform.forward, hit.Normal);
            var rightDot = Vector3.Dot(IK.HitBone.transform.right, hit.Normal);

            _lastHit = Quaternion.Euler(forwardDot * 40, 0, rightDot * 40);
            _lastHitStrength = 1.0f;




            //Debug.Log(CameraManager.Main.transform.name);

        }

        public Quaternion GetPivotRotation()
        {
            Quaternion body;

            body = transform.rotation;

            return Quaternion.Lerp(body, body * Quaternion.Euler(0, _horizontalLookAngle - transform.eulerAngles.y, 0), _aimPivotIntensity);
        }

        public Vector3 GetPivotPosition()
        {
            return transform.position;
        }

        #endregion

        #region Input

        public void InputLayer(int value)
        {
            _targetLayer = value;
        }

        public void InputJump()
        {
            if (_isIntendingToJump )
                return;

            if (_inputMovement.IsMoving)
                _jumpAngle = Util.AngleOfVector(_inputMovement.Direction);
            else
                _jumpAngle = _horizontalLookAngle;

            _wantsToJump = true;
        }

        public void InputMovement(PlayerMovement movement)
        {
            _inputMovement = movement;
            _wantsToSprint = movement.IsSprinting;
        }

        public void InputMoveForward(float strength = 1)
        {
            InputMovement(new PlayerMovement(Quaternion.AngleAxis(_horizontalLookAngle, Vector3.up) * Vector3.forward, 1));
        }

        public void InputMoveBack(float strength = 1)
        {
            InputMovement(new PlayerMovement(Quaternion.AngleAxis(_horizontalLookAngle - 180, Vector3.up) * Vector3.forward, 1));
        }

        public void InputMoveLeft(float strength = 1)
        {
            InputMovement(new PlayerMovement(Quaternion.AngleAxis(_horizontalLookAngle - 90, Vector3.up) * Vector3.forward, 1));
        }

        public void InputMoveRight(float strength = 1)
        {
            InputMovement(new PlayerMovement(Quaternion.AngleAxis(_horizontalLookAngle + 90, Vector3.up) * Vector3.forward, 1));
        }

        public void InputCrouch()
        {
            _wantsToCrouch = true;
        }

        public void InputPossibleImmediateTurn(bool value = true)
        {
            _wouldTurnImmediately = value;
        }

        public void InputAim()
        {
            _wantsToAim = true;
        }

        public void InputZoom()
        {
            _wantsToZoom = true;
        }

        public void InputUseToolAlternate()
        {
            InputUseTool(true);
        }

        public void InputUseTool(bool isAlternate = false)
        {
            if (_isUsingWeapon)
            {
                _keepUsingWeapon = isAlternate == _isUsingWeaponAlternate;
                return;
            }

            if (CurrentWeapon <= 0)
                return;

            if (Weapons[CurrentWeapon - 1].Gun != null)
            {
                InputFire();
                return;
            }

            _isUsingWeaponAlternate = isAlternate;
            _isUsingWeapon = true;
            _wasWeaponUsed = false;
            _keepUsingWeapon = true;
            _hasBeganUsingWeapon = false;
            _normalizedWeaponUseTime = 0;

            if (_weapon != null)
            {
                if (_isUsingWeaponAlternate)
                    _weapon.SendMessage("OnStartUsingAlternate", SendMessageOptions.DontRequireReceiver);
                else
                    _weapon.SendMessage("OnStartUsing", SendMessageOptions.DontRequireReceiver);
            }
        }

        public void InputFire()
        {
            if (CurrentWeapon <= 0)
                return;

            if (Weapons[CurrentWeapon - 1].Gun == null)
            {
                InputUseTool();
                return;
            }

            _wantsToFire = true;
            _hasFireCondition = false;
            InputAim();
        }

        public void InputFireOnCondition(int ignoreSide)
        {
            if (CurrentWeapon <= 0)
                return;

            if (Weapons[CurrentWeapon - 1].Gun == null)
            {
                InputUseTool();
                return;
            }

            _hasFireCondition = true;
            _fireConditionSide = ignoreSide;
            _wantsToFire = true;
            InputAim();
        }

        public void InputReload()
        {
            if (_isReloading || _gun == null)
                return;

            _isReloading = true;
            _hasBeganReloading = false;
            _reloadTime = 0;
            _normalizedReloadTime = 0;

            _gun.SendMessage("OnReloadStart", SendMessageOptions.DontRequireReceiver);
        }

        public void InputWeapon(int id)
        {
            if (id < 0)
                id = 0;
            else if (id > Weapons.Length)
                id = Weapons.Length;

            if (id != _weaponToChangeTo)
                _weaponToChangeTo = id;
        }

        public void InputStandLeft()
        {
            _wantsToFaceInADirection = true;
        }

        public void InputStandRight()
        {
            _wantsToFaceInADirection = true;
        }

        #endregion

        #region Behaviour

        private void OnEnable()
        {
            if (IsAlive)
            {
                Characters.Register(this);
                _hasRegistered = true;
            }
        }

        private void OnDisable()
        {
            Characters.Unregister(this);
            _hasRegistered = false;
        }

        private void Awake()
        {
            _capsule = GetComponent<CapsuleCollider>();
            _body = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();
            _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            _neck = _animator.GetBoneTransform(HumanBodyBones.Neck);

            _renderers = GetComponentsInChildren<Renderer>();

            if (_renderer != null)
            {
                _visibility = _renderer.GetComponent<Visibility>();

                if (_visibility == null)
                    _visibility = _renderer.gameObject.AddComponent<Visibility>();
            }

            _weaponToChangeTo = CurrentWeapon;

            _defaultCapsuleHeight = _capsule.height;
            _defaultCapsuleCenter = _capsule.center.y;
            _previousCapsuleHeight = _defaultCapsuleHeight;

            SetLookTarget(transform.position + transform.forward * 1000);
            SetFireTarget(transform.position + transform.forward * 1000);

            SendMessage("OnStandingHeight", _defaultCapsuleHeight, SendMessageOptions.DontRequireReceiver);
            SendMessage("OnCurrentHeight", _defaultCapsuleHeight, SendMessageOptions.DontRequireReceiver);
        }

        private void LateUpdate()
        {
            _target = null;

            if (IsAlive && !_hasRegistered)
            {
                _hasRegistered = true;
                Characters.Register(this);
            }
            else if (!IsAlive && _hasRegistered)
            {
                _hasRegistered = false;
                Characters.Unregister(this);
            }

            if (IsAlive)
            {
                _isCrouching = _wantsToCrouch;

                {
                    var distance = Vector3.Distance(_fireTarget, Top);
                    var dir = Vector3.Lerp((_currentFireTarget - Top).normalized, (_fireTarget - Top).normalized, Time.deltaTime * 16);

                    _currentFireTarget = Top + dir * distance;
                }

                calculateBodyTurn();
                calculateLookAngle();

                if (_useGravity && IsAlive)
                {
                    var force = new Vector3(0, Gravity, 0) * Time.deltaTime;

                    if (_noMovementTimer < 0.2f || !_isGrounded || _isOnSlope || _groundTimer < 0.2f)
                    {
                        if (_isOnSlope && _noMovementTimer > float.Epsilon && !_isJumping)
                            _body.velocity -= force * 10;
                        else if (_isGrounded && _jumpTimer < 0.1f && !_isOnSlope)
                            _body.velocity -= force * 2;
                        else
                            _body.velocity -= force;
                    }
                }

                updateCanAim();
                updateHeadAimIntennsity();
                updateThrowAimIntensity();
                updateArmAimIntennsity();
                updateLeftHandIntensity();
                updateAimPivotIntensity();
                updateWeapons();
                updateReload();
                updateSprinting();

                updateCommon();

                if (_visibility == null || _visibility.IsVisible)
                    updateIK();
                else if (_gun != null)
                    _gun.UpdateIntendedRotation();

                if (Mathf.Abs(_movementInput) > float.Epsilon)
                    _noMovementTimer = 0;
                else if (_noMovementTimer < 1)
                    _noMovementTimer += Time.deltaTime;

                if (!_isGrounded)
                    _groundTimer = 0;
                else if (_groundTimer < 1)
                    _groundTimer += Time.deltaTime;

            }
            else
            {
                _isCrouching = false;

                _body.velocity = Vector3.zero;
                updateGround();
            }

            updateCapsule();
            updateAnimator();

            _wantedToZoom = _wantsToZoom;

            _wantedToKeepUsingWeapon = _keepUsingWeapon;
            _keepUsingWeapon = false;

            //foreach (var renderer in _renderers)
            //if (renderer.gameObject.layer != _targetLayer)
            //renderer.gameObject.layer = _targetLayer;

            _wasAimingGun = IsAimingGun;
            _targetLayer = 0;
            _wantsToAim = false;
            _wouldTurnImmediately = false;
            _wantsToJump = false;
            _inputMovement = new PlayerMovement();
            _wantsToSprint = false;
            _wantsToCrouch = false;
            _wantsToFire = false;
            _wantsToFaceInADirection = false;
            _wantsToZoom = false;

            {
                var isAlive = IsAlive;
                if (isAlive && !_wasAlive) SendMessage("OnAlive", SendMessageOptions.DontRequireReceiver);
                if (!isAlive && _wasAlive) SendMessage("OnDead", SendMessageOptions.DontRequireReceiver);
                _wasAlive = isAlive;
            }
        }

        #endregion

        #region Private methods

        private void useWeapon()
        {
            if (!_wasWeaponUsed)
            {
                _wasWeaponUsed = true;

                if (_weapon != null)
                {
                    if (_isUsingWeaponAlternate)
                        _weapon.SendMessage("OnUsedAlternate", SendMessageOptions.DontRequireReceiver);
                    else
                        _weapon.SendMessage("OnUsed", SendMessageOptions.DontRequireReceiver);
                }

                if (_isUsingWeaponAlternate)
                    SendMessage("OnToolUsedAlternate", SendMessageOptions.DontRequireReceiver);
                else
                    SendMessage("OnToolUsed", SendMessageOptions.DontRequireReceiver);
            }
        }

        private void snapToPosition(Vector3 value, float time)
        {
            _positionToSnap = value;
            _positionToSnapStart = transform.position;
            _positionSnapTimer = time;
            _positionSnapTimerStart = time;
        }

        private void calculateLookAngle()
        {
            {
                var diff = _bodyLookTarget - transform.position;

                if (diff.magnitude > 0)
                {
                    diff.Normalize();

                    _verticalLookAngle = Mathf.Asin(diff.y) * 180f / Mathf.PI;
                    _horizontalLookAngle = Util.AngleOfVector(diff);
                }
            }

            if (TurnSettings.ImmediateAim && _wouldTurnImmediately)
                _currentHorizontalBodyAngle = _horizontalLookAngle;
            else
                _currentHorizontalBodyAngle = Util.AngleOfVector(_currentBodyLookTarget - transform.position);
        }

        private void calculateBodyTurn()
        {
            _horizontalBodyAngle = Util.AngleOfVector(_bodyLookTarget - transform.position);

            if (TurnSettings.ImmediateAim && _wouldTurnImmediately)
                _currentHorizontalBodyAngle = _horizontalLookAngle;
            else
                _currentHorizontalBodyAngle = Util.AngleOfVector(_currentBodyLookTarget - transform.position);
        }

        private void updateCapsule()
        {
            if (IsAlive)
            {
                {
                    if (!_capsule.enabled)
                        _groundTimer = 0;

                    _capsule.enabled = true;
                }

                _capsule.height = Mathf.Lerp(_capsule.height, TargetHeight, Time.deltaTime * 10);
                _capsule.center = new Vector3(_capsule.center.x, _defaultCapsuleCenter - (_defaultCapsuleHeight - _capsule.height) * 0.5f, _capsule.center.z);

                if (_previousCapsuleHeight != _capsule.height)
                    SendMessage("OnCurrentHeight", _capsule.height, SendMessageOptions.DontRequireReceiver);
            }
            else
                _capsule.enabled = false;
        }

        private void updateCanAim()
        {
            _canAimLerp = Mathf.Lerp(_canAimLerp, IsFree(transform.forward, AimObstacleDistance, 0.7f) ? 1 : 0, Time.deltaTime * 4);

            _canAim = _canAimLerp > 0.5f;
        }

        private void updateThrowAimIntensity()
        {
            float targetIntensity = 0;
            _throwAimIntensity = Mathf.Lerp(_throwAimIntensity, targetIntensity, Mathf.Clamp01(Time.deltaTime * 6));
        }

        private void updateArmAimIntennsity()
        {
            var targetIntensity = 0f;

            if (Vector3.Dot(transform.forward, (_lookTarget - transform.position).normalized) > 0 && _wasAimingGun && IsAimingGun)
                targetIntensity = 1;

            if (_dontChangeArmAimingJustYet && _previousArmAimTargetIntensity < targetIntensity)
                targetIntensity = 0.0f;
            else
                _previousArmAimTargetIntensity = targetIntensity;

            if (targetIntensity > _armAimIntensity)
                Util.Lerp(ref _armAimIntensity, targetIntensity, Time.deltaTime * 3);
            else
                Util.Lerp(ref _armAimIntensity, targetIntensity, Time.deltaTime * 10);
        }

        private void updateLeftHandIntensity()
        {
            float targetIntensity = 0f;

            if (IsGunReady && !_isFalling && !IsSprinting)
            {
                if (Weapons[CurrentWeapon - 1].Type == WeaponType.Pistol)
                {
                    if (IsAimingGun)
                        targetIntensity = 1;
                }
                else
                    targetIntensity = 1f;
            }

            _leftHandIntensity = Mathf.Lerp(_leftHandIntensity, targetIntensity, Mathf.Clamp01(Time.deltaTime * 15));
        }

        private void updateHeadAimIntennsity()
        {
            float targetIntensity = 0f;

            if ((IsAiming) || _isHeadLookTargetOverriden)
                targetIntensity = 1;

            if (targetIntensity > _headAimIntensity)
                _headAimIntensity = Mathf.Lerp(_headAimIntensity, targetIntensity, Time.deltaTime * 2);
            else
                _headAimIntensity = Mathf.Lerp(_headAimIntensity, targetIntensity, Time.deltaTime * 15);
        }

        private void updateAimPivotIntensity()
        {
            float targetIntensity = 0f;

            if (_isFalling)
                targetIntensity = 0;
            else
                if (CurrentWeapon > 0)
                    targetIntensity = 1;

                _aimPivotIntensity = Mathf.Lerp(_aimPivotIntensity, targetIntensity, Mathf.Clamp01(Time.deltaTime * 8));
        }

        private void updateSprinting()
        {
            var state = _animator.GetCurrentAnimatorStateInfo(0);

            _isSprinting = state.IsName("Sprint") || state.IsName("Sprint Rifle");

            if (_isSprinting)
            {
                var next = _animator.GetNextAnimatorStateInfo(0);

                if (next.shortNameHash != 0 && !(next.IsName("Sprint") || next.IsName("Sprint Rifle")))
                    _isSprinting = false;
            }
        }


        private void updateUse()
        {
            if (_isUsingWeapon)
            {
                _reloadTime += Time.deltaTime;

                var info = _animator.GetCurrentAnimatorStateInfo(FirstWeaponLayer + weaponType);

                var isInState = false;

                if (_isUsingWeaponAlternate)
                    isInState = info.IsName(_weaponStates.AlternateUse);
                else
                    isInState = info.IsName(_weaponStates.Use);

                if (isInState)
                    _hasBeganUsingWeapon = true;
                else if (_hasBeganUsingWeapon)
                    _isUsingWeapon = false;

                var wasUsed = _isUsingWeapon;
                var isContinuous = Weapons[CurrentWeapon - 1].IsAContinuousTool(_isUsingWeaponAlternate);

                if (_hasBeganUsingWeapon)
                {
                    if (info.normalizedTime > _normalizedWeaponUseTime)
                        _normalizedWeaponUseTime = info.normalizedTime;

                    if (_normalizedWeaponUseTime > 0.8f && !isContinuous)
                        _isUsingWeapon = false;
                }

                if (isContinuous)
                {
                    if (!_keepUsingWeapon && !_wantedToKeepUsingWeapon)
                        _isUsingWeapon = false;
                    else if (_weaponToChangeTo != CurrentWeapon)
                        _isUsingWeapon = false;
                }

                if (wasUsed && !_isUsingWeapon)
                    useWeapon();
            }

        }

        private void updateReload()
        {
            if (Gun != null && Gun.AutoReload && Gun.Clip <= 0)
                InputReload();

            if (_isReloading)
            {
                _reloadTime += Time.deltaTime;

                var info = _animator.GetCurrentAnimatorStateInfo(FirstWeaponLayer + weaponType);

                var isInState = false;

                if (info.IsName(_weaponStates.Reload))
                    isInState = true;

                if (isInState)
                    _hasBeganReloading = true;
                else if (_hasBeganReloading)
                    _isReloading = false;

                if (_hasBeganReloading)
                {
                    if (info.normalizedTime >= 0.2f && _gun != null)
                        _gun.Reload();

                    if (info.normalizedTime > _normalizedReloadTime)
                        _normalizedReloadTime = info.normalizedTime;

                    if (_normalizedReloadTime > 0.8f)
                        _isReloading = false;
                }

                if (_reloadTime > 10)
                    _isReloading = false;
            }
            else
                _reloadTime = 0;
        }

        private Vector3 lerpRelativePosition(Vector3 from, Vector3 to, float speed)
        {
            var current = from - transform.position;
            var next = to - transform.position;

            var currentLength = current.magnitude;
            var nextLength = next.magnitude;

            if (currentLength > float.Epsilon) current.Normalize();
            if (nextLength > float.Epsilon) next.Normalize();

            var vector = Vector3.Lerp(current, next, Time.deltaTime * speed);
            var length = Mathf.Lerp(currentLength, nextLength, Time.deltaTime * speed);

            return transform.position + vector * length;
        }

        private void OnAnimatorMove()
        {
            if (_isHeadLookTargetOverriden)
            {
                _headLookTargetOverride = lerpRelativePosition(_headLookTargetOverride, _headLookTargetOverrideTarget, _headTurnSpeed);

                var angle0 = Util.AngleOfVector(transform.forward);
                var angle1 = Util.AngleOfVector(_headLookTargetOverride - transform.position);
                var delta = Mathf.DeltaAngle(angle0, angle1);

                const float limit = 70f;

                if (Mathf.Abs(delta) > limit)
                {
                    var vector = _headLookTargetOverride - transform.position;
                    var dist = vector.magnitude;
                    if (dist > float.Epsilon) vector /= dist;

                    if (delta < 0)
                        vector = Quaternion.AngleAxis(angle0 - limit, Vector3.up) * Vector3.forward;
                    else
                        vector = Quaternion.AngleAxis(angle0 + limit, Vector3.up) * Vector3.forward;

                    _headLookTargetOverride = transform.position + vector * dist;
                }
            }

            _currentBodyLookTarget = lerpRelativePosition(_currentBodyLookTarget, _bodyLookTarget, _bodyTurnSpeed);

            if (_positionSnapTimer > float.Epsilon)
            {
                _body.velocity = Vector3.zero;
                transform.position = Vector3.Lerp(_positionToSnapStart, _positionToSnap, 1.0f - _positionSnapTimer / _positionSnapTimerStart);
                _positionSnapTimer -= Time.deltaTime;
            }
            else
            {
                var animatorMovement = _animator.deltaPosition / Time.deltaTime;
                var animatorSpeed = animatorMovement.magnitude;

                if (!IsAlive)
                {
                }
                else if ((_isJumping && _normalizedJumpTime < JumpSettings.AnimationLock) || _isIntendingToJump)
                {
                    smoothTurn(_bodyAngleDiff, JumpSettings.RotationSpeed);
                }
                else if (!_isGrounded)
                {
                    smoothTurn(_bodyAngleDiff, JumpSettings.RotationSpeed);
                }
                else
                {
                    if (_wouldTurnImmediately && TurnSettings.ImmediateAim)
                    {
                        transform.Rotate(0, _bodyAngleDiff, 0);
                        _bodyAngleDiff = 0;
                    }
                    else
                    {
                        float minThreshold = -1.0f;
                        float maxThreshold = 1.0f;

                        float manualSpeed;
                        float manualInfluence;

                        float rootMovement = _animator.deltaPosition.magnitude / Time.deltaTime;

                        if (rootMovement >= minThreshold || IsAiming)
                        {
                            manualInfluence = Mathf.Clamp01((rootMovement - minThreshold) / (maxThreshold - minThreshold));

                            if (IsSprinting)
                            {
                                manualSpeed = TurnSettings.SprintingRotationSpeed;
                                manualInfluence = 1.0f;
                            }
                            else
                                manualSpeed = TurnSettings.RunningRotationSpeed;

                            if (manualInfluence < 1.0f)
                                manualInfluence = 1.0f;
                        }
                        else
                        {
                            manualSpeed = 0.0f;
                            manualInfluence = 1.0f;
                        }

                        var anim = _animator.deltaRotation.eulerAngles.y;

                        if (anim > 0)
                            anim = Mathf.Clamp(anim, 0, _bodyAngleDiff);
                        else
                            anim = Mathf.Clamp(anim, _bodyAngleDiff, 0);

                        var turn = Mathf.LerpAngle(anim,
                                                   _bodyAngleDiff * Mathf.Clamp01(Time.deltaTime * manualSpeed),
                                                   manualInfluence);

                        transform.Rotate(0, turn, 0);
                    }

                    
                        applyVelocityToTheGround(animatorMovement * _movementInput);
                }
            }

            var targetAngle = transform.eulerAngles.y + _bodyAngleDiff;

            var oldDelta = Mathf.Abs(_requiredAnimationRotation);
            var calculatedDelta = Mathf.DeltaAngle(_currentAnimatedAngle, targetAngle);
            var newDelta = Mathf.Abs(calculatedDelta);

            if (newDelta < 30 && oldDelta < float.Epsilon)
                Util.LerpAngle(ref _requiredAnimationRotation, 0, Time.deltaTime * 180);
            else
            {
                _requiredAnimationRotation = calculatedDelta;
                Util.LerpAngle(ref _currentAnimatedAngle, targetAngle, Time.deltaTime * 180);
            }
        }

        private void applyVelocityToTheGround(Vector3 velocity)
        {
            velocity.y = 0;

            if (_isOnSlope && _isGrounded)
            {
                var right = Vector3.Cross(_groundNormal, Vector3.up);
                right.y = 0;

                if (right.sqrMagnitude > float.Epsilon)
                    right.Normalize();

                _body.velocity = Quaternion.AngleAxis(-Mathf.Clamp(_slope, -45f, 45f), right) * velocity;

            }
            else
                _body.velocity = velocity;
        }

        private void smoothTurn(float angle, float speed)
        {
            var initialAngle = angle;

            angle *= Time.deltaTime * speed;
            var clamp = Time.deltaTime * 720 * speed;

            if (angle > clamp)
                angle = clamp;
            else if (angle < -clamp)
                angle = -clamp;

            if (initialAngle > 0 && angle > initialAngle)
                angle = initialAngle;
            else if (initialAngle < 0 && angle < initialAngle)
                angle = initialAngle;

            transform.Rotate(0, angle, 0);
        }

        private bool canAimOnReload
        {
            get { return Gun != null && Gun.AutoReload; }
        }

        private bool wantsToAim
        {
            get { return (_wantsToFire || _wantedToZoom || _wantsToAim); }
        }

        private bool canFire
        {
            get
            {
                return !_isReloading && !_isChangingWeapon && IsGunReady && !_gun.IsClipEmpty;
            }
        }
        
        private int weaponType
        {
            get
            {
                if (_weapon != null)
                {
                    for (int i = 0; i < Weapons.Length; i++)
                        if (Weapons[i].Item == _weapon)
                            return (int)Weapons[i].Type + 1;

                    return 0;
                }

                if (CurrentWeapon > 0 && CurrentWeapon <= Weapons.Length)
                    return (int)Weapons[CurrentWeapon - 1].Type + 1;

                return 0;
            }
        }

        private void updateWeapons()
        {
            var weaponToShow = 0;

            if (_weaponToChangeTo < 0)
                _weaponToChangeTo = 0;
            else if (_weaponToChangeTo > Weapons.Length)
                _weaponToChangeTo = CurrentWeapon;

            for (int i = 0; i < Weapons.Length; i++)
                if (_weapon == Weapons[i].Item)
                    weaponToShow = i + 1;

            if (!_isChangingWeapon && !_isReloading && _weaponToChangeTo != CurrentWeapon && _isGrounded && !_isUsingWeapon)
            {
                _isPreviousWeaponHidden = false;
                _isChangingWeapon = true;
                CurrentWeapon = _weaponToChangeTo;
            }

            if (_isChangingWeapon && !_isReloading)
            {
                if (!_isPreviousWeaponHidden)
                {
                    if (_weapon == null)
                        _isPreviousWeaponHidden = true;
                    else
                    {
                        var previousType = weaponType;

                        var next = _animator.GetNextAnimatorStateInfo(FirstWeaponLayer + previousType);
                        var isNextState = next.shortNameHash == 0 || next.IsName("Idle Body") || next.IsName("None");

                        if (isNextState)
                        {
                            var state = _animator.GetCurrentAnimatorStateInfo(FirstWeaponLayer + previousType);

                            if (state.IsName("Idle Body") || state.IsName("None"))
                            {
                                weaponToShow = 0;
                                _isPreviousWeaponHidden = true;
                            }
                            else if (previousType < 2 && _animator.IsInTransition(FirstWeaponLayer + previousType))
                                weaponToShow = 0;
                        }
                    }
                }

                if (_isPreviousWeaponHidden)
                {
                    if (CurrentWeapon == 0)
                    {
                        weaponToShow = 0;
                        _isChangingWeapon = false;
                        _animator.SetLayerWeight(FirstWeaponLayer, 1);
                    }
                    else
                    {
                        weaponToShow = 0;

                        for (int i = 0; i < Weapons.Length; i++)
                            if (CurrentWeapon == i + 1)
                            {
                                int type = (int)Weapons[i].Type + 1;
                                var state = _animator.GetCurrentAnimatorStateInfo(FirstWeaponLayer + type);
                                var next = _animator.GetNextAnimatorStateInfo(FirstWeaponLayer + type);

                                var isTransitional = false;

                                if (state.IsName(_weaponStates.Equip))
                                {
                                    weaponToShow = i + 1;
                                    isTransitional = true;
                                }
                                else if (next.IsName(_weaponStates.Equip))
                                    isTransitional = true;

                                if (!isTransitional)
                                {
                                    foreach (var name in _weaponStates.Common)
                                        if (state.IsName(name) && (next.shortNameHash == 0 || next.IsName(name)))
                                        {
                                            weaponToShow = i + 1;
                                            _isChangingWeapon = false;
                                            break;
                                        }
                                }
                            }
                    }
                }
            }

            if (!_isChangingWeapon)
            {
                var previousWeapon = _weapon;
                var previousGun = _gun;

                if (CurrentWeapon > 0 && CurrentWeapon <= Weapons.Length)
                {
                    _weapon = Weapons[CurrentWeapon - 1].Item;
                    _gun = Weapons[CurrentWeapon - 1].Gun;
                }
                else
                {
                    _weapon = null;
                    _gun = null;
                }

                if (previousWeapon != _weapon && _isUsingWeapon && _weapon != null)
                {
                    if (_isUsingWeaponAlternate)
                        _weapon.SendMessage("OnStartUsingAlternate", SendMessageOptions.DontRequireReceiver);
                    else
                        _weapon.SendMessage("OnStartUsing", SendMessageOptions.DontRequireReceiver);
                }

                if (previousGun != _gun)
                {
                    if (previousGun != null) previousGun.CancelFire();
                    if (_gun != null) _gun.CancelFire();
                }
            }

            for (int i = 0; i < Weapons.Length; i++)
            {
                var show = weaponToShow == i + 1;
                var weapon = Weapons[i];

                if (weapon.Item != null && weapon.Item.activeSelf != show) weapon.Item.SetActive(show);
                if (weapon.Holster != null && weapon.Holster.activeSelf != !show) weapon.Holster.SetActive(!show);
            }

            if (_gun != null)
            {
                _gun.Target = _fireTarget;
                _gun.Character = this;
                _gun.Allow(IsGunReady && !_isFalling);
            }

            if (CurrentWeapon == 0)
                _armTimer = 0.2f;
            else if (_armTimer > 0)
                _armTimer -= Time.deltaTime;
        }

        private void updateAim()
        {
            var wantsToAim = _wantsToAim;

            if (CurrentWeapon > 0 && Weapons[CurrentWeapon - 1].Type == WeaponType.Tool)
                wantsToAim = _isUsingWeapon && Weapons[CurrentWeapon - 1].IsAnAimableTool(_isUsingWeaponAlternate);
        }


        private void updateFire()
        {
            if (wantsToAim)
            {
                if (IsGunReady)
                {
                    var canFire = _canAim;

                    if (_gun == null || _gun.IsClipEmpty)
                        canFire = false;

                    
                        if (canFire)
                        {
                            if (_wantsToFire)
                                fire();
                        }

                }
            }
        }

        private void fire()
        {

            if (_hasFireCondition)
                _gun.SetFireCondition(_fireConditionSide);
            else
                _gun.CancelFireCondition();

            
                _gun.CancelFire();
                _gun.TryFireNow();
        }

        public bool IsFree(Vector3 direction)
        {
            return IsFree(direction, ObstacleDistance, 0.3f);
        }

        public bool IsFree(Vector3 direction, float distance, float height)
        {
            var count = Physics.RaycastNonAlloc(transform.position + new Vector3(0, _capsule.height * height, 0),
                                                direction,
                                                _raycastHits,
                                                _capsule.radius + distance,
                                                1);

            for (int i = 0; i < count; i++)
                if (!_raycastHits[i].collider.isTrigger && !Util.InHiearchyOf(_raycastHits[i].collider.gameObject, gameObject))
                    return false;

            return true;
        }

        private void updateCommon()
        {
            float requiredUpdateDelay;

            if (_inputMovement.IsMoving || Vector3.Distance(_lastKnownPosition, transform.position) > 0.1f)
            {
                _lastKnownPosition = transform.position;
                
            }

            updateLookAngleDiff();

            updateAim();

            if (_isUsingWeapon)
                updateUse();
            else
                updateFire();

            updateWalk();
            updateVertical();
        }

        private void updateWalk()
        {
            Vector3 movement;

            if (_directionChangeDelay > float.Epsilon)
                _directionChangeDelay -= Time.deltaTime;

            _currentMovement = _inputMovement;

            if (_currentMovement.Direction.sqrMagnitude > 0.1f)
            {
                var overallIntensity = 1.0f;

                

                var local = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * _currentMovement.Direction;

                _leftMoveIntensity = Mathf.Lerp(_leftMoveIntensity, IsFree(-transform.right) ? 1.0f : 0.0f, Time.deltaTime * 4);
                _rightMoveIntensity = Mathf.Lerp(_rightMoveIntensity, IsFree(transform.right) ? 1.0f : 0.0f, Time.deltaTime * 4);
                _backMoveIntensity = Mathf.Lerp(_backMoveIntensity, IsFree(-transform.forward) ? 1.0f : 0.0f, Time.deltaTime * 4);
                _frontMoveIntensity = Mathf.Lerp(_frontMoveIntensity, IsFree(transform.forward) ? 1.0f : 0.0f, Time.deltaTime * 4);

                if (local.x < -float.Epsilon) local.x *= _leftMoveIntensity;
                if (local.x > float.Epsilon) local.x *= _rightMoveIntensity;
                if (local.z < -float.Epsilon) local.z *= _backMoveIntensity;
                if (local.z > float.Epsilon) local.z *= _frontMoveIntensity;

                _currentMovement.Direction = Quaternion.Euler(0, transform.eulerAngles.y, 0) * local;
                movement = local * _currentMovement.Magnitude * overallIntensity;
            }
            else
                movement = Vector3.zero;

            _localMovement = Vector3.Lerp(_localMovement, movement, Time.deltaTime * 8);
            _movementInput = Mathf.Clamp(movement.magnitude * 2, 0, 1);
        }

        

        private float deltaAngleToTurnTo(float target)
        {
            var angle = Mathf.DeltaAngle(transform.eulerAngles.y, target);

            if (Mathf.Abs(angle) <= 90)
                return angle;

            return angle;
        }

        private void updateLookAngleDiff()
        {
            if (_isIntendingToJump || (_isJumping && _normalizedJumpTime < JumpSettings.AnimationLock))
                _bodyAngleDiff = deltaAngleToTurnTo(_jumpAngle);
            else
            {
                _bodyAngleDiff = deltaAngleToTurnTo(_currentHorizontalBodyAngle);
            }
        }

        private void updateVertical()
        {
            if (_jumpTimer < 999) _jumpTimer += Time.deltaTime;

            updateGround();

            if (_isGrounded)
            {
                if (_nextJumpTimer > -float.Epsilon) _nextJumpTimer -= Time.deltaTime;

                if (!_isJumping && _nextJumpTimer < float.Epsilon && _wantsToJump)
                    _isIntendingToJump = true;
            }
            else if (_body.velocity.y < -5)
                _isJumping = false;

            if (_isGrounded)
            {
                if (_isIntendingToJump && Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, _jumpAngle)) < 10)
                {
                    if (!_isJumping)
                    {
                        _animator.SetTrigger("Jump");
                        _isJumping = true;
                        _jumpTimer = 0;

                        SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);
                    }

                    var direction = _localMovement;

                    if (direction.magnitude > 0.1f)
                        direction.Normalize();

                    var v = transform.rotation * direction * JumpSettings.Speed;
                    v.y = JumpSettings.Strength;
                    _body.velocity = v;
                }
                else if (_isJumping)
                    _isJumping = false;
            }
            else
                _isIntendingToJump = false;

            if (_isJumping)
            {
                var info = _animator.GetCurrentAnimatorStateInfo(0);

                if (info.IsName("Jump")) _normalizedJumpTime = info.normalizedTime;
                else if (info.IsName("Jump Land")) _normalizedJumpTime = 1;
                else
                {
                    info = _animator.GetNextAnimatorStateInfo(0);

                    if (info.IsName("Jump")) _normalizedJumpTime = info.normalizedTime;
                    else if (info.IsName("Jump Land")) _normalizedJumpTime = 1;
                }
            }

            _isFalling = false;

            if (_isFalling)
            {
                Vector3 edge;
                if (findEdge(out edge, 0.1f))
                {
                    var offset = transform.position - edge;
                    offset.y = 0;
                    var distance = offset.magnitude;

                    if (distance > 0.01f)
                    {
                        offset /= distance;
                        transform.position += offset * Mathf.Clamp(Time.deltaTime * 3, 0, distance);
                    }
                }
            }
        }

        private void updateGround()
        {

            _isGrounded = true;

            if (_isGrounded && !_wasGrounded && IsAlive)
            {
                SendMessage("OnLand", SendMessageOptions.DontRequireReceiver);
                _nextJumpTimer = 0.2f;
            }

            _wasGrounded = _isGrounded;
        }

        private void updateAnimator()
        {
            if (IsAlive)
            {
                var state = _animator.GetCurrentAnimatorStateInfo(0);

                float runCycle = Mathf.Repeat(state.normalizedTime, 1);
                float jumpLeg = (runCycle < 0.5f ? 1 : -1) * _movementInput;
                if (_isGrounded)
                {
                    if (_jumpLegTimer > 0)
                        _jumpLegTimer -= Time.deltaTime;
                    else
                        _animator.SetFloat("JumpLeg", jumpLeg);
                }
                else
                    _jumpLegTimer = 0.5f;

                if (IsAlive &&
                    (state.IsName("Walk Armed") || state.IsName("Walk Unarmed") || state.IsName("Sprint") || state.IsName("Sprint Rifle")))
                {
                    if (runCycle > 0.6f)
                    {
                        if (_lastFoot != 1)
                        {
                            _lastFoot = 1;
                            SendMessage("OnFootstep", _animator.GetBoneTransform(HumanBodyBones.LeftFoot).position, SendMessageOptions.DontRequireReceiver);
                        }
                    }
                    else if (runCycle > 0.1f)
                    {
                        if (_lastFoot != 0)
                        {
                            _lastFoot = 0;
                            SendMessage("OnFootstep", _animator.GetBoneTransform(HumanBodyBones.RightFoot).position, SendMessageOptions.DontRequireReceiver);
                        }
                    }
                }
                else
                    _lastFoot = -1;

                _animator.SetFloat("Speed", Speed);

                _animator.SetBool("IsDead", false);
                _animator.SetBool("IsJumping", _isJumping);
                _animator.SetFloat("Rotation", _requiredAnimationRotation);

                _animator.SetFloat("MovementX", _localMovement.x);
                _animator.SetFloat("MovementZ", _localMovement.z);
                _animator.SetFloat("MovementInput", _movementInput);
                _animator.SetBool("IsFalling", _isFalling && !_isJumping);
                _animator.SetBool("IsGrounded", _isGrounded);
                _animator.SetBool("IsSprinting", _wantsToSprint);

                int weaponType;

                if (_isChangingWeapon && !_isPreviousWeaponHidden)
                    weaponType = 0;
                else
                {
                    if (CurrentWeapon > 0 && CurrentWeapon <= Weapons.Length)
                        weaponType = 1 + (int)Weapons[CurrentWeapon - 1].Type;
                    else
                        weaponType = 0;
                }

                _animator.SetInteger("WeaponType", weaponType);
                _animator.SetBool("IsArmed", CurrentWeapon > 0 && _armTimer <= float.Epsilon);

                // Small hacks. Better animation transitions when rolling.
                var isWeaponReady = IsWeaponReady;

                if (isWeaponReady && Weapons[CurrentWeapon - 1].Gun != null)
                    isWeaponReady = _gun != null;

                _animator.SetBool("IsWeaponReady", isWeaponReady);

                _dontChangeArmAimingJustYet = false;

                if (CurrentWeapon > 0 && Weapons[CurrentWeapon - 1].Gun != null)
                {
                    if (_gun != null)
                    {
                        var type = this.weaponType;

                        if (_animator.GetCurrentAnimatorStateInfo(FirstWeaponLayer + type).IsName("Aim"))
                            if (_animator.IsInTransition(FirstWeaponLayer + type))
                                _dontChangeArmAimingJustYet = true;
                    }

                    _animator.SetFloat("GunVariant", _wantsToZoom ? 1 : 0, 0.1f, Time.deltaTime);
                    _animator.SetFloat("Crouch", IsCrouching ? 1 : 0, 0.1f, Time.deltaTime);

                    if (!_dontChangeArmAimingJustYet)
                    {
                        _animator.SetBool("IsUsingWeapon", !IsGoingToSprint &&
                                                           (IsAiming ||
                                                            (_isChangingWeapon && _wantsToAim && CurrentWeapon > 0)));
                    }
                }
                else
                {
                    if (CurrentWeapon > 0)
                        _animator.SetFloat("Tool", (int)Weapons[CurrentWeapon - 1].Tool - 1);

                    _animator.SetBool("IsAlternateUse", _isUsingWeaponAlternate);
                    _animator.SetBool("IsUsingWeapon", _isUsingWeapon);
                }

                _animator.SetFloat("VerticalVelocity", _body.velocity.y);
                _animator.SetBool("IsCrouching", _isCrouching);
                _animator.SetBool("IsReloading", _isReloading);

                
                _animator.SetFloat("LowAim", 0, 0.1f, Time.deltaTime);

                if (_verticalLookAngle < 0f)
                    _animator.SetFloat("LookHeight", Mathf.Clamp(_verticalLookAngle / 55f, -1, 1));
                else
                    _animator.SetFloat("LookHeight", Mathf.Clamp(_verticalLookAngle / 40f, -1, 1));
            }
            else
            {
                _animator.SetBool("IsDead", true);
                _animator.SetBool("IsUsingWeapon", false);
            }
        }

        private void updateIK()
        {
            if (!IsAlive)
                return;


            if (_lastHitStrength > float.Epsilon)
            {
                if (IK.HitBone != null)
                    IK.HitBone.localRotation *= Quaternion.Lerp(Quaternion.identity, _lastHit, _lastHitStrength);

                _lastHitStrength -= Time.deltaTime * 5.0f;
            }


            if (_gun != null)
                _lastAimTransform = _gun.transform.Find("Aim");

            var distance = 0f;

            if (CameraManager.Main != null && CameraManager.Main.transform != null)
                distance = Vector3.Distance(transform.position, CameraManager.Main.transform.position);

            var lookTarget = _lookTarget;

            {
                var groundTarget = _lookTarget;
                groundTarget.y = transform.position.y;

                var lookDistance = Mathf.Max((groundTarget - transform.position).magnitude, 3);
                var lookDirection = (groundTarget - transform.position).normalized;

                var currentTarget = transform.position + transform.forward * lookDistance;
                currentTarget.y = groundTarget.y;
                var currentDirection = (currentTarget - transform.position).normalized;

                var lookAngle = Mathf.Atan2(lookDirection.z, lookDirection.x) * Mathf.Rad2Deg;
                var currentAngle = Mathf.Atan2(currentDirection.z, currentDirection.x) * Mathf.Rad2Deg;
                var deltaAngle = Mathf.DeltaAngle(currentAngle, lookAngle);

                var direction = lookDirection;

                if (deltaAngle < -TurnSettings.MaxAimAngle)
                    direction = new Vector3(Mathf.Cos((currentAngle - TurnSettings.MaxAimAngle) * Mathf.Deg2Rad), 0, Mathf.Sin((currentAngle - TurnSettings.MaxAimAngle) * Mathf.Deg2Rad)).normalized;
                else if (deltaAngle > TurnSettings.MaxAimAngle)
                    direction = new Vector3(Mathf.Cos((currentAngle + TurnSettings.MaxAimAngle) * Mathf.Deg2Rad), 0, Mathf.Sin((currentAngle + TurnSettings.MaxAimAngle) * Mathf.Deg2Rad)).normalized;

                lookTarget = transform.position + direction * lookDistance;
                lookTarget.y = _lookTarget.y;
            }

            if (!IK.ThrowChain.IsEmpty && _throwAimIntensity > 0.01f)
            {
                var target = lookTarget;

                _throwIK.Target = IK.Sight;
                _throwIK.Bones = IK.ThrowChain.Bones;
                _throwIK.UpdateAim(target, IK.ThrowChain.Delay.Get(distance), _throwAimIntensity, IK.ThrowChain.Iterations);
            }

            if (_lastAimTransform != null && !IK.AimChain.IsEmpty && _armAimIntensity > 0.01f)
            {
                _aimIK.Target = _lastAimTransform;
                _aimIK.Bones = IK.AimChain.Bones;
                _aimIK.UpdateAim(TurnSettings.IsAimingPrecisely ? _currentFireTarget : lookTarget, IK.AimChain.Delay.Get(distance), _armAimIntensity, IK.AimChain.Iterations);
            }

            if (_gun != null)
                _gun.UpdateIntendedRotation();

            if (_gun != null && IK.RightHand != null && _gun.RecoilShift.magnitude > 0.01f && !IK.RecoilChain.IsEmpty)
            {
                _recoilIK.Target = IK.RightHand;
                _recoilIK.Bones = IK.RecoilChain.Bones;
                _recoilIK.UpdateMove(IK.RightHand.position + _gun.RecoilShift, IK.RecoilChain.Delay.Get(distance), 1.0f, IK.RecoilChain.Iterations);
            }

            if (_gun != null && IK.LeftHand != null && !IK.LeftArmChain.IsEmpty && _leftHandIntensity > 0.01f)
            {
                Transform hand = null;

                if (IsAimingGun)
                    hand = _gun.LeftHandOverwrite.Aim;
                
                if (hand == null)
                    hand = _gun.LeftHandDefault;

                if (hand != null)
                {
                    _leftHandIK.Target = IK.LeftHand;
                    _leftHandIK.Bones = IK.LeftArmChain.Bones;
                    _leftHandIK.UpdateMove(hand.position, IK.LeftArmChain.Delay.Get(distance), _leftHandIntensity, IK.LeftArmChain.Iterations);
                }
            }

            if (IK.Sight != null && !IK.SightChain.IsEmpty && _headAimIntensity > 0.01f)
            {
                Vector3 target;

                if (_isHeadLookTargetOverriden)
                    target = _headLookTargetOverride;
                else
                    target = TurnSettings.IsAimingPrecisely ? _currentFireTarget : lookTarget;

                _sightIK.Target = IK.Sight;
                _sightIK.Bones = IK.SightChain.Bones;
                _sightIK.UpdateAim(target, IK.SightChain.Delay.Get(distance), _headAimIntensity, IK.SightChain.Iterations);
            }

            if (_gun != null)
            {
                _gun.UpdateAimOrigin();
                _target = _gun.FindCurrentAimedHealthTarget();
            }
        }
        private bool findGround(float threshold)
        {
            var offset = 0.2f;

            for (int i = 0; i < Physics.RaycastNonAlloc(transform.position + Vector3.up * offset, Vector3.down, _raycastHits, threshold + offset); i++)
            {
                var hit = _raycastHits[i];

                if (!hit.collider.isTrigger)
                    if (hit.collider.gameObject != gameObject)
                        return true;
            }

            return false;
        }

        private void findGroundAndSlope(float threshold)
        {
            _isOnSlope = false;
            _isGrounded = false;

            var offset = 0.2f;

            for (int i = 0; i < Physics.RaycastNonAlloc(transform.position + Vector3.up * offset, Vector3.down, _raycastHits, threshold + offset); i++)
            {
                var hit = _raycastHits[i];

                if (!hit.collider.isTrigger)
                    if (hit.collider.gameObject != gameObject)
                    {
                        var up = Vector3.Dot(Vector3.up, hit.normal);

                        _slope = Mathf.Acos(up) * Mathf.Rad2Deg;

                        if (up > 0.99f) _slope = 0;

                        if (_slope > 20f)
                            _isOnSlope = true;

                        _groundNormal = hit.normal;
                        _isGrounded = true;

                        break;
                    }
            }
        }

        private float getGoundHeight()
        {
            for (int i = 0; i < Physics.RaycastNonAlloc(transform.position + (Vector3.up * (0.1f)), Vector3.down, _raycastHits); i++)
            {
                var hit = _raycastHits[i];

                if (hit.collider.gameObject != gameObject)
                    return hit.point.y;
            }

            return 0;
        }

        private bool findEdge(out Vector3 position, float threshold)
        {
            var bottom = transform.TransformPoint(_capsule.center - new Vector3(0, _capsule.height * 0.5f + _capsule.radius, 0));
            var count = Physics.OverlapSphereNonAlloc(bottom, _capsule.radius + threshold, _colliderCache);

            for (int i = 0; i < count; i++)
                if (_colliderCache[i].gameObject != gameObject)
                {
                    position = _colliderCache[i].ClosestPointOnBounds(bottom);
                    return true;
                }

            position = Vector3.zero;
            return false;
        }

        #endregion 
    }
}
