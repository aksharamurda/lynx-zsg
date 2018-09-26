using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    public class Gun : MonoBehaviour
    {
        public Vector3 Origin { get { return _lastAimOrigin; } }

        public Vector3 Direction
        {
            get { return getRecoiledDirectionFrom(Origin); }
        }

        public Vector3 TargetDirection
        {
            get { return getTargetDirectionFrom(Origin); }
        }

        public float RecoilIntensity
        {
            get { return _recoilIntensity; }
        }

        public Vector3 RecoilShift
        {
            get { return _recoil; }
        }

        public bool IsClipEmpty
        {
            get { return Clip <= 0; }
        }

        public bool CanHit
        {
            get { return _hitWait <= 0; }
        }

        public bool HasJustFired
        {
            get { return _hasJustFired; }
        }

        public bool IsAllowed
        {
            get { return _isAllowed; }
        }

        public Renderer Renderer
        {
            get { return _renderer; }
        }

        public GunStats gunStats;

        [Tooltip("Name of the gun to be display on the HUD.")]
        public string Name = "Gun";

        [Tooltip("How many degrees should the camera FOV be reduced when zooming with the gun.")]
        public float Zoom = 0;

        public Sprite iconGun;

        [Tooltip("Rate of fire in bullets per second.")]
        [Range(0, 1000)]
        public float Rate = 7;

        [Tooltip("Maximum distance of a bullet hit. Objects further than this value are ignored.")]
        public float Distance = 50;

        [Tooltip("Damage dealt by a single bullet.")]
        [Range(0, 1000)]
        public float Damage = 10;

        [Tooltip("Size of a clip. Clip is set to this value on reload.")]
        [Range(0, 1000)]
        public int ClipSize = 10;

        [Tooltip("Current number of bullets in a clip.")]
        [Range(0, 1000)]
        public int Clip = 10;

        [Tooltip("Should the gun be reloaded automatically when the clip is empty.")]
        public bool AutoReload = false;

        [Tooltip("Damage done by a melee attack.")]
        public float MeleeDamage = 20;

        [Tooltip("Distance of a sphere that checks for melee targets in front of the character.")]
        public float MeleeDistance = 1.5f;

        [Tooltip("Radius of a sphere that checks for melee targets in front of the character.")]
        public float MeleeRadius = 1.0f;

        [Tooltip("Height of a sphere that checks for melee targets in front of the character.")]
        public float MeleeHeight = 0.5f;

        [Tooltip("Time in seconds for to wait for another melee hit.")]
        public float HitCooldown = 0.4f;

        [Tooltip("Will the character fire by just aiming the mobile controller.")]
        public bool FireOnMobileAim = true;

        [Tooltip("Should the gun modify the transparency of the attached laser.")]
        public bool ManageLaserAlpha = true;

        [Tooltip("Should a debug ray be displayed.")]
        public bool DebugAim = false;

        [Tooltip("Link to the object that controls the aiming direction.")]
        public GameObject Aim;

        [Tooltip("Object to be instantiated as a bullet.")]
        public GameObject Bullet;

        [Tooltip("Settings that manage gun's recoil behaviour.")]
        public RecoilSettings RecoilSettings = RecoilSettings.Default();

        [Tooltip("Link to the object that controls the position of character's left hand relative to the weapon.")]
        public Transform LeftHandDefault;

        [Tooltip("Links to objects that overwrite the value in LeftHand based on the gameplay situation.")]
        public HandOverwrite LeftHandOverwrite;

        [HideInInspector]
        public PlayerMotor Character;

        [HideInInspector]
        public Vector3 Target;

        private Renderer _renderer;

        private float _recoilIntensity;

        private bool _hasJustFired;

        private bool _isUsingCustomRaycastOrigin;
        private Vector3 _customRaycastOrigin;

        private float _fireWait = 0;
        private bool _isGoingToFire;
        private bool _isFiringOnNextUpdate;
        private bool _isAllowed;
        private bool _wasAllowedAndFiring;

        private Vector3 _lastAimOrigin;

        private Vector3 _intendedForward;

        private Vector3 _recoil;

        private float _hitWait = 0;

        private List<RecoilImpulse> _recoilImpulses = new List<RecoilImpulse>();

        private RaycastHit[] _hits = new RaycastHit[16];

        private LineRenderer _laser;
        private float _laserIntensity;

        private bool _isIgnoringSelf = true;
        private bool _hasFireCondition;
        private int _fireConditionSide;

        public void ToUse()
        {
            TryFireNow();
        }

        private void Start()
        {
            _laser = GetComponent<LineRenderer>();

            if (_laser != null)
            {
                var material = _laser.material == null ? null : Material.Instantiate(_laser.material);
                _laser.material = material;
            }
        }

        public void IgnoreSelf(bool value = true)
        {
            _isIgnoringSelf = value;
        }

        public void SetFireCondition(int side)
        {
            _hasFireCondition = true;
            _fireConditionSide = side;
        }

        public void CancelFireCondition()
        {
            _hasFireCondition = false;
        }

        public GameObject FindCurrentAimedTarget()
        {
            var hit = Raycast();

            if (hit.collider != null)
                return hit.collider.gameObject;
            else
                return null;
        }

        public GameObject FindCurrentAimedHealthTarget()
        {
            return getHealthTarget(FindCurrentAimedTarget());
        }

        private GameObject getHealthTarget(GameObject target)
        {
            while (target != null)
            {
                var health = target.GetComponent<PlayerHealth>();

                if (health != null)
                {
                    if (health.Health <= float.Epsilon)
                        target = null;

                    break;
                }

                var parent = target.transform.parent;

                if (parent != null)
                    target = parent.gameObject;
                else
                    target = null;
            }

            return target;
        }

        private void OnValidate()
        {
            Distance = Mathf.Max(0, Distance);
        }

        private Vector3 getRecoiledDirectionFrom(Vector3 origin)
        {
            var rotation = Quaternion.FromToRotation(_intendedForward, transform.forward);
            var vector = getTargetDirectionFrom(origin);
            vector.y = (rotation * vector).y;

            return vector.normalized;
        }

        private Vector3 getTargetDirectionFrom(Vector3 origin)
        {
            var value = Target - origin;

            if (value.magnitude > float.Epsilon)
                value.Normalize();

            return value;
        }

        private void Awake()
        {
            Name = gunStats.nameGun;
            Rate = gunStats.fireRateGun;
            Damage = gunStats.damageGun;
            ClipSize = gunStats.clipSizeGun;
            Clip = gunStats.clipGun;

            RecoilSettings.Strength = gunStats.recoilStrength;
            RecoilSettings.AttackTime = gunStats.recoilAttackTime;
            RecoilSettings.DecayTime = gunStats.recoilDecayTime;
            RecoilSettings.Limit = gunStats.recoilLimit;

            _renderer = GetComponent<Renderer>();
        }

        private void OnDisable()
        {
            _recoilImpulses.Clear();
        }

        public void Reload()
        {
            Clip = ClipSize;
        }

        public void Hit()
        {
            if (Character == null || _hitWait > 0)
                return;

            _hitWait = HitCooldown;

            var position = Character.transform.position + Vector3.up * MeleeHeight;

            var bestHit = new RaycastHit();
            var bestHitHasHealth = false;

            for (int i = 0; i < Physics.SphereCastNonAlloc(position, MeleeRadius, Character.transform.forward, _hits, MeleeDistance); i++)
                if (_hits[i].collider != null && _hits[i].collider.gameObject != null && _hits[i].collider.gameObject != Character)
                {
                    var hit = _hits[i];
                    var hasHealth = hit.collider.GetComponent<PlayerHealth>() != null;

                    if (bestHit.collider == null ||
                        (hasHealth && !bestHitHasHealth) ||
                        (hit.distance < bestHit.distance && (!bestHitHasHealth || hasHealth)))
                    {
                        bestHit = hit;
                        bestHitHasHealth = hasHealth;
                    }
                }

            if (bestHit.collider != null)
                bestHit.collider.SendMessage("OnHit",
                                             new Hit(bestHit.point, bestHit.normal, MeleeDamage, Character.gameObject, bestHit.collider.gameObject),
                                             SendMessageOptions.DontRequireReceiver);
        }

        public void TryFireNow()
        {
            _isFiringOnNextUpdate = true;
        }

        public void FireWhenReady()
        {
            _isGoingToFire = true;
        }

        public void CancelFire()
        {
            _isGoingToFire = false;
        }

        public void Allow(bool value)
        {
            _isAllowed = value;
        }

        private Vector3 raycastOrigin
        {
            get { return _isUsingCustomRaycastOrigin ? _customRaycastOrigin : Origin; }
        }

        public void SetFireFrom(Vector3 point)
        {
            _isUsingCustomRaycastOrigin = true;
            _customRaycastOrigin = point;
        }

        public void StopFiringFromCustom()
        {
            _isUsingCustomRaycastOrigin = false;
        }

        public void UpdateAimOrigin()
        {
            _lastAimOrigin = Aim == null ? transform.position : Aim.transform.position;
        }

        public void UpdateIntendedRotation()
        {
            _intendedForward = transform.forward;
        }

        private void LateUpdate()
        {
            _hasJustFired = false;

            if (_isGoingToFire)
                _isFiringOnNextUpdate = true;

            if (_hitWait >= 0)
                _hitWait -= Time.deltaTime;

            if (DebugAim)
            {
                Debug.DrawLine(raycastOrigin, raycastOrigin + getRecoiledDirectionFrom(raycastOrigin) * Distance, Color.red);
                Debug.DrawLine(_customRaycastOrigin, Target, Color.green);
            }

            // Notify character if the trigger is pressed. Used to make faces.
            {
                var isAllowedAndFiring = _isGoingToFire && _isAllowed;

                if (Character != null)
                {
                    if (isAllowedAndFiring && !_wasAllowedAndFiring) Character.gameObject.SendMessage("OnStartGunFire", SendMessageOptions.DontRequireReceiver);
                    if (!isAllowedAndFiring && _wasAllowedAndFiring) Character.gameObject.SendMessage("OnStopGunFire", SendMessageOptions.DontRequireReceiver);
                }

                _wasAllowedAndFiring = isAllowedAndFiring;
            }

            // Update recoil.
            {
                // Starts from 1 when firing a bullet and decays to 0.
                _recoilIntensity -= Time.deltaTime * 10;

                // Decay the recoil.

                if (RecoilSettings.DecayTime <= float.Epsilon)
                {
                    // If DecayTime is zero or less clear _recoil immediately.

                    _recoil = Vector3.zero;
                }
                else
                {
                    var recoilLeft = _recoil.magnitude;

                    if (recoilLeft > float.Epsilon)
                    {
                        var value = _recoil / recoilLeft;
                        var decrease = RecoilSettings.Strength * Time.deltaTime / RecoilSettings.DecayTime;

                        if (decrease >= recoilLeft)
                            _recoil = Vector3.zero;
                        else
                            _recoil -= value * decrease;
                    }
                }

                // Sum all currently acting recoil impulses and add it to the recoil shift.

                if (RecoilSettings.AttackTime <= float.Epsilon)
                {
                    // If AttackTime is zero or leso sum up all impulses immediately.

                    foreach (var recoil in _recoilImpulses)
                        _recoil += recoil.Direction * RecoilSettings.Strength;

                    _recoilImpulses.Clear();
                }
                else
                    for (int index = _recoilImpulses.Count - 1; index >= 0; index--)
                    {
                        var recoil = _recoilImpulses[index];
                        recoil.Progress += Time.deltaTime / RecoilSettings.AttackTime;

                        if (recoil.Progress >= 1)
                            _recoilImpulses.RemoveAt(index);
                        else
                        {
                            _recoilImpulses[index] = recoil;
                            _recoil += recoil.Direction * RecoilSettings.Strength * Mathf.Clamp01(Time.deltaTime / RecoilSettings.AttackTime);
                        }
                    }

                if (_recoil.magnitude > RecoilSettings.Limit)
                    _recoil = _recoil.normalized * RecoilSettings.Limit;
            }

            _fireWait -= Time.deltaTime;

            // Check if the trigger is pressed.
            if (_isFiringOnNextUpdate && _isAllowed)
            {
                // Time in seconds between bullets.
                var fireDelay = 1.0f / Rate;

                var delay = 0f;

                if (_fireWait < 0.5f * fireDelay && fireDelay < RecoilSettings.DecayTime + RecoilSettings.AttackTime)
                    _recoilIntensity += Time.deltaTime * 20;

                // Fire all bullets in this frame.
                while (_fireWait < 0)
                {
                    if (!IsClipEmpty)
                        fire(delay);

                    delay += fireDelay;
                    _fireWait += fireDelay;
                    _isGoingToFire = false;
                }
            }

            _isFiringOnNextUpdate = false;

            // Clamp recoil intensity to 0 and 1.
            _recoilIntensity = Mathf.Clamp01(_recoilIntensity);

            // Clamp fire delay timer.
            if (_fireWait < 0) _fireWait = 0;

            // Adjust the laser.
            if (_laser != null)
            {
                var current = (Aim == null ? transform.forward : Aim.transform.forward).normalized;
                var perfect = TargetDirection.normalized;

                var targetAlpha = 0f;

                if (Character != null)
                    targetAlpha = (Character.IsAlive && Character.IsAimingGun) ? 1 : 0;

                if (targetAlpha < _laserIntensity)
                {
                    _laserIntensity -= Time.deltaTime * 8;
                    _laserIntensity = Mathf.Clamp(_laserIntensity, targetAlpha, 1);
                }
                else
                {
                    _laserIntensity += Time.deltaTime * 3;
                    _laserIntensity = Mathf.Clamp(_laserIntensity, 0, targetAlpha);
                }

                if (_laser.material != null && ManageLaserAlpha)
                {
                    var color = _laser.material.color;
                    color.a = _laserIntensity;
                    _laser.material.color = color;
                }

                var direction = Vector3.Dot(current, perfect) > 0.75f ? perfect : current;

                bool isFriend;
                var hit = Raycast(Origin, direction, out isFriend, false);

                _laser.SetPosition(0, Origin);

                if (hit.collider == null)
                    _laser.SetPosition(1, Origin + direction * Distance);
                else
                    _laser.SetPosition(1, hit.point);
            }
        }

        private void fire(float delay = 0)
        {
            bool isFriend;
            var hit = Raycast(raycastOrigin, getRecoiledDirectionFrom(raycastOrigin), out isFriend, true);

            if (!isFriend)
            {
                var end = hit.point;

                SendMessage("OnFire", delay, SendMessageOptions.DontRequireReceiver);
                _recoilImpulses.Add(new RecoilImpulse(Vector3.Lerp(-Direction, Vector3.up, RecoilSettings.UpForce)));

                Clip--;

                if (hit.collider == null)
                    end = raycastOrigin + Distance * Direction;

                if (Bullet != null)
                {
                    var bullet = GameObject.Instantiate(Bullet);
                    bullet.transform.position = Origin;
                    bullet.transform.parent = null;
                    bullet.transform.LookAt(end);

                    var projectile = bullet.GetComponent<Projectile>();
                    var vector = end - Origin;

                    if (projectile != null)
                    {
                        projectile.Distance = vector.magnitude;
                        projectile.Direction = vector.normalized;

                        if (hit.collider != null)
                        {
                            projectile.Target = hit.collider.gameObject;
                            projectile.Hit = new Hit(hit.point, -Direction, Damage, Character.gameObject, hit.collider.gameObject);
                        }

                    }
                    else if (hit.collider != null)
                        hit.collider.SendMessage("OnHit",
                                             new Hit(hit.point, -Direction, Damage, Character.gameObject, hit.collider.gameObject),
                                             SendMessageOptions.DontRequireReceiver);

                    bullet.SetActive(true);
                }
                else if (hit.collider != null)
                    hit.collider.SendMessage("OnHit",
                                             new Hit(hit.point, -Direction, Damage, Character.gameObject, hit.collider.gameObject),
                                             SendMessageOptions.DontRequireReceiver);

                if (hit.collider != null && Character != null)
                    Character.SendMessage("OnSuccessfulHit", new Hit(hit.point, -Direction, Damage, Character.gameObject, hit.collider.gameObject), SendMessageOptions.DontRequireReceiver);
            }

            _hasJustFired = true;
        }

        public RaycastHit Raycast()
        {
            bool isFriend;
            return Raycast(raycastOrigin, getRecoiledDirectionFrom(raycastOrigin), out isFriend, false);
        }

        public RaycastHit Raycast(Vector3 origin, Vector3 direction, out bool isFriend, bool friendCheck)
        {
            RaycastHit closestHit = new RaycastHit();
            float closestDistance = Distance * 10;

            var minDistance = 0f;

            if (_isUsingCustomRaycastOrigin)
                minDistance = Vector3.Distance(Origin, raycastOrigin);

            isFriend = false;

            for (int i = 0; i < Physics.RaycastNonAlloc(origin, direction, _hits, Distance); i++)
            {
                var hit = _hits[i];

                if (Character != null && Util.InHiearchyOf(hit.collider.gameObject, Character.gameObject))
                    continue;

                if (hit.distance < closestDistance && hit.distance > minDistance)
                {
                    var isOk = true;

                    if (hit.collider.isTrigger)
                        isOk = hit.collider.GetComponent<BodyPartHealth>() != null;
                    else
                    {
                        var health = hit.collider.GetComponent<PlayerHealth>();

                        if (health != null)
                            isOk = health.IsRegisteringHits;
                    }

                    if (isOk)
                    {
                        if ((_isIgnoringSelf || _hasFireCondition) && friendCheck)
                        {
                            var root = getHealthTarget(hit.collider.gameObject);

                            if (root != null)
                            {
                                if (_isIgnoringSelf && Character != null && root == Character.gameObject)
                                    isFriend = true;
                                else if (_hasFireCondition)
                                {
                                    isFriend = false;
                                }
                                else
                                    isFriend = false;
                            }
                            else
                                isFriend = false;
                        }

                        closestHit = hit;
                        closestDistance = hit.distance;
                    }
                }
            }

            return closestHit;
        }
    }
}
