using System;
using System.Collections;
using ECS.Entities.AI;
using FMOD.Studio;
using FMODUnity;
using Managers;
using Player.Abilities;
using Player.Camera;
using Player.Effects;
using Player.Movement;
using TMPro;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Player
{
    public class PlayerCharacterController : MonoBehaviour
    {
        [SerializeField] private Transform _orientation;
        [SerializeField] private LayerMask _groundMask;
        private RaycastHit _groundHit;

        [Header("Properties")]
        [Range(1,3)]
        [SerializeField] private int _jumps;
        [Range(0,3)]
        [SerializeField] private float _moveSpeedMultiplier;
        private bool pressToHover;

        [Header("Cooldowns")]
        [SerializeField] private float _ability1Cooldown;
        [SerializeField] private float _ability2Cooldown;

        [Header("Vertical Forces")]
        [SerializeField] private float _gravityStrength;
        [SerializeField] private float _hoverHeight;
        [SerializeField] private float _hoverStrength;
        [SerializeField] private float _hoverDamp;

        [Header("Rotation Correction")]
        [SerializeField] private float _rotationCorrectionStrength;
        [SerializeField] private float _rotationCorrectionDamp;

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private CapsuleCollider _capsuleCollider;
        [SerializeField] private FirstPersonCamera _camera;
        [SerializeField] private Animator _animator;
        
        private bool _onGround;
        private float _xInput;
        private float _yInput;
        private bool _jumpInput;
        private bool _ability1Input;
        private bool _ability2Input;
        private bool _inCombat;
        private bool _canBeDisplaced = false;

        private float _ability1Time;
        private float _ability2Time;

        private IMovementHandler _movementHandler;

        private GroundedMovementHandler _groundedMovementHandler = new GroundedMovementHandler(); 
        private AirborneMovementHandler _airborneMovementHandler = new AirborneMovementHandler();
        private ChargeMovementHandler _chargeMovementHandler;
        private DashMovementHandler _dashMovementHandler = new DashMovementHandler();
        private HookMovementHandler _hookMovementHandler;
        private WallClimbMovementHandler _wallClimbMovementHandler = new WallClimbMovementHandler();

        [SerializeField] private Transform _hand;
        [SerializeField] private GameObject _display;
        [SerializeField] private Canvas _uiCanvas;
        [SerializeField] private Canvas _uiCrosshairCanvas;
        [SerializeField] private Canvas _hookCanvas;
        [SerializeField] private TextMeshProUGUI _crosshair;
        [SerializeField] private TextMeshProUGUI _controlPrompt;

        [SerializeField] private float _airTime;
        private Vector3 _startPos;
        private Vector3 _respawnPos;
        private EventInstance _footstepSound;

        [SerializeField] private DashAbility _dashAbility;
        [SerializeField] private ChargeAbility _chargeAbility;
        [SerializeField] private WallClimbAbility _wallClimbAbility;
        [SerializeField] private HookAbility _hookAbility;
        [SerializeField] private LineRenderer _hookLineRenderer;
        [SerializeField] private Transform _hookParticleTransform;
        [SerializeField] private ParticleSystem _trailParticle;
        [SerializeField] private ParticleSystem _smashParticle;
        [SerializeField] private Transform _handBone;
        [SerializeField] HookUI _hookUI;

        [Range(1, 7)]
        [SerializeField] private int _maxHookCharges;
        [Range(0.5f, 20f)]
        [SerializeField] private float _outOfCombatRechargeDuration;
        [Range(0.5f, 20f)]
        [SerializeField] private float _inCombatRechargeDuration;

        private int _hookCharges;
        private float _rechargeTime;

        private float _contactDamageTime = 0f;
        private uint _contactDamageAmount = 1;
        private float _contactDamageHitstop = .1f;

        public bool _isDashUnlocked = false;
        public bool _isChargeUnlocked = false;
        public bool _isHookUnlocked = false;
        public bool _isWallClimbUnlocked = false;

        private bool _canMove = true;

        private void Start()
        {
            _camera.Setup();
            _display.SetActive(false);
            _movementHandler = new GroundedMovementHandler();
            _uiCrosshairCanvas.gameObject.SetActive(true);
            _uiCanvas.gameObject.SetActive(true);
            _respawnPos = _startPos = transform.position;

            _footstepSound = AudioManager.instance.CreateInstance(FMODEvents.instance.playerFootsteps);
            _footstepSound.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));

            _chargeMovementHandler = new ChargeMovementHandler(GetComponent<Hitstop>());
            _hookMovementHandler = new HookMovementHandler(_hookLineRenderer, _smashParticle, _hookParticleTransform, GetComponent<Hitstop>());
            _hookAbility.Setup(_hookLineRenderer);

            _hookCharges = _maxHookCharges;
            _hookUI.SetMaxCharges(_maxHookCharges);
        }

        private void Update()
        {
            CheckGround();
            CheckInput();
            HandUpdate();
            CalculateAirTime();
            CalculateCooldowns();
            UpdateCharges();
            CheckContactDamage();
        }

        private void FixedUpdate()
        {        
            if (_movementHandler.ShouldGravityApply(this))
            {
                ApplyGravity();
            }

            if (_movementHandler.ShouldHoverApply(this))
            {
                Hover();
            }

            CorrectRotation();

            if (_canMove)
            {
                _movementHandler.Move(this);
            }
            VoidReturn();

            SoundUpdate();
        }

        private void ApplyGravity()
        {
            float gravity = _jumpInput && pressToHover && _rigidbody.velocity.y < -4f ? _gravityStrength * 0.2f : _gravityStrength;
            _rigidbody.AddForce(Vector3.down * gravity , ForceMode.Acceleration);
        }

        private void Hover()
        {
            if (!_onGround) return;
            float heightDiff = _groundHit.distance - _hoverHeight;
            _rigidbody.AddForce(Vector3.down * (heightDiff * _hoverStrength + _rigidbody.velocity.y * _hoverDamp), ForceMode.Acceleration);
        }

        private void CorrectRotation()
        {
            Quaternion correction = Quaternion.FromToRotation(transform.up, Vector3.up);
            correction.ToAngleAxis(out float alpha, out Vector3 axis);
            
            _rigidbody.AddTorque(
                axis.normalized * (alpha * Mathf.Deg2Rad * _rotationCorrectionStrength) -
                _rigidbody.angularVelocity * _rotationCorrectionDamp, ForceMode.Acceleration);
            
            Quaternion correction2 = Quaternion.FromToRotation(transform.forward, Vector3.forward);
            correction2.ToAngleAxis(out float alpha2, out Vector3 axis2);
            
            _rigidbody.AddTorque(
                axis2.normalized * (alpha2 * Mathf.Deg2Rad * _rotationCorrectionStrength) -
                _rigidbody.angularVelocity * _rotationCorrectionDamp, ForceMode.Acceleration);
        }

        public void Respawn()
        {
            StartCoroutine(RespawnCoroutine(0.3f));
        }

        private IEnumerator RespawnCoroutine(float dur)
        {
            _canMove = false;
            float t = 0f;
            PostProcessingManager.Instance.BraincellSwitchTransition(dur);
            transform.position = _respawnPos;

            AudioManager.instance.PlayOneShot(FMODEvents.instance.teleportOut, transform.position);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.catDamage, transform.position);
            AudioManager.instance.PlayOneShot(FMODEvents.instance.wandAttack, transform.position);

            while (t < dur)
            {
                transform.position = _respawnPos;
                yield return new WaitForFixedUpdate();
                t += Time.fixedDeltaTime;
            }
            _canMove = true;
        }

        public void SetRespawn(Vector3 newPos)
        {
            _respawnPos = newPos;
        }

        private void VoidReturn()
        {
            if (transform.position.y < -100f)
            {
                transform.position = _startPos;
            }
        }

        private void ApplyTerminalVelocity()
        {
            float tv = 12.5f;
            if (_rigidbody.velocity.y <= tv || _rigidbody.velocity.y > 0) return;
            Vector3 terminalVelocity = _rigidbody.velocity;
            terminalVelocity.y = tv;
            _rigidbody.velocity = terminalVelocity;
        }
        
        private void CheckGround()
        {
            float raycastMargin = 0.5f;
            
            _onGround = Physics.Raycast(transform.position + Vector3.up * (raycastMargin + 0.05f), Vector3.down,
                out _groundHit, _hoverHeight * 5.0f, _groundMask);
            
            if (!_onGround) _groundHit.distance = float.PositiveInfinity;
            _onGround &= _groundHit.distance < _hoverHeight + raycastMargin && Vector3.Angle(_groundHit.normal, Vector3.up) < 37.5f;

            if (!_onGround)
            {
                return;
            }
            
            if (_dashAbility != null)
            {
                _dashAbility.ResetTimesDashed();
            }

        }

        private void CheckInput()
        {
            //TODO ADRI AIXÓ HAURIA D'ANAR A ONGUI()
            _xInput = Input.GetAxis("Horizontal");
            _yInput = Input.GetAxis("Vertical");
            _jumpInput = Input.GetButton("Jump");
            _ability1Input = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            _ability2Input = Input.GetMouseButton(1) || Input.GetKey(KeyCode.Return);

            if (_isDashUnlocked)
            {
                _dashAbility.CheckDash(this, _ability1Input);
            }

            /*if (_isChargeUnlocked)
            {
                _chargeAbility.CheckCharge(this, _ability1Input);
            }*/

            if (_isHookUnlocked)
            {
                _hookAbility.FakeUpdate(this);
            }

            if (_isWallClimbUnlocked)
            {
                _wallClimbAbility.CheckWallClimb(this);
            }
            
            //
        }

        private void HandUpdate()
        {
            if (_movementHandler is not HookMovementHandler)
            {
                Vector3 particlePos = GetHandBone().position;
                _hookParticleTransform.position = particlePos;
            }

            if (!_onGround) return;
            Vector3 wandPos = new Vector3(Mathf.Sin(Time.time * 5f), Mathf.Sin(Time.time * 6f));
            float wobbleMagnitude = Mathf.Clamp(_rigidbody.velocity.magnitude / 25f, 0f, 1f);
            wandPos *= wobbleMagnitude * 0.05f;
            _hand.localPosition = wandPos;
        }

        private void CalculateAirTime()
        {            
            if (!_onGround)
            {
                _airTime += Time.deltaTime;
                if (_airTime > 5f)
                {
                }
            }
            else
            {
                _airTime = 0f;
            }

            var em = _trailParticle.emission;
            em.rateOverDistance = Mathf.Max(0f, 12 - _airTime * 50f);
        }

        private void CalculateCooldowns()
        {
            _ability1Time = Mathf.Max(0f, _ability1Time - Time.deltaTime);
            _ability2Time = Mathf.Max(0f, _ability2Time - Time.deltaTime);
        }

        public void AddHookCharge(int amount = 1)
        {
            _hookCharges = Math.Min(_hookCharges + amount, _maxHookCharges);
        }

        private void UpdateCharges()
        {
            if (IsInCombat() &&
                _rechargeTime > _inCombatRechargeDuration ||
                !IsInCombat() &&
                _rechargeTime > _outOfCombatRechargeDuration)
            {
                _hookCharges++;
                _rechargeTime = 0f;
            }

            _hookUI.UpdateUI(_hookCharges, _rechargeTime,
                IsInCombat() ? _inCombatRechargeDuration : _outOfCombatRechargeDuration);

            if (_hookCharges < _maxHookCharges)
            {
                _rechargeTime += Time.deltaTime;
            }
        }

        private void CheckContactDamage()
        {
            if (_contactDamageTime <= 0f) return;

            RaycastHit hit;
            /*Vector3 p1 = transform.position + _capsuleCollider.center + Vector3.up * (-_capsuleCollider.height * 0.5f);
            Vector3 p2 = p1 + Vector3.up * _capsuleCollider.height;*/
            //if (Physics.CapsuleCast(p1, p2, _capsuleCollider.radius * 1.3f, _rigidbody.velocity.normalized, out hit, 1f,
            if (Physics.SphereCast(transform.position + Vector3.up, _capsuleCollider.radius * 2f, _rigidbody.velocity.normalized, out hit, 1.5f,
                GameManager.Instance.GetRaycastLayersWithoutAlly(), QueryTriggerInteraction.Ignore))
            {
                AgentEntity entity = hit.collider.GetComponent<AgentEntity>();
                if (entity != null)
                {
                    AudioManager.instance.PlayOneShot(FMODEvents.instance.charge, transform.position);
                    entity.OnReceiveDamage(_contactDamageAmount, hit.point, transform.position);
                    GetComponent<Hitstop>().Add(_contactDamageHitstop);
                    _contactDamageTime = 0f;

                    float bounceStrength = 100f;
                    float bounceVerticalRatio = .45f;
                    
                    GetCamera().ScreenShake(.25f, 1.3f);
                    _rigidbody.velocity = Vector3.zero;
                    _rigidbody.AddForce(
                    (new Vector3(hit.normal.x, Mathf.Max(0f, hit.normal.y), hit.normal.z).normalized +
                    new Vector3(0f, bounceVerticalRatio * 1.2f, 0f)).normalized *
                        (bounceStrength * (1.4f)), ForceMode.Impulse);

                    ChangeMovementHandlerToAirborne();
                }
            }

            _contactDamageTime = Mathf.Max(0f, _contactDamageTime - Time.deltaTime);
        }

        private void SoundUpdate()
        {
            //Walk sound
            _footstepSound.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
            PLAYBACK_STATE playbackState;
            _footstepSound.getPlaybackState(out playbackState);
            if (_onGround && (_xInput != 0f || _yInput != 0f) && _rigidbody.velocity != Vector3.zero)
            {
                if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
                {
                    _footstepSound.start();
                }
            }
            else if (playbackState.Equals(PLAYBACK_STATE.PLAYING))
            {
                _footstepSound.stop(STOP_MODE.ALLOWFADEOUT);
            }
        }

        public void SetCrosshairColor(Color color)
        {
            _crosshair.color = color;
        }

        public Rigidbody GetRigidbody()
        {
            return _rigidbody;
        }

        public Transform GetOrientation()
        {
            return _orientation;
        }

        public CapsuleCollider GetCapsuleCollider()
        {
            return _capsuleCollider;
        }

        public FirstPersonCamera GetCamera()
        {
            return _camera;
        }

        public Animator GetAnimator()
        {
            return _animator;
        }

        public float GetXInput()
        {
            return _xInput;
        }

        public float GetYInput()
        {
            return _yInput;
        }

        public float GetMoveSpeedMultiplier()
        {
            return _moveSpeedMultiplier;
        }

        public bool IsOnTheGround()
        {
            return _onGround;
        }

        public float GetGravityStrength()
        {
            return _gravityStrength;
        }

        public int GetJumps()
        {
            return _jumps;
        }

        public bool HasPressedJump()
        {
            return _jumpInput;
        }

        public RaycastHit GetGroundHit()
        {
            return _groundHit;
        }

        public float GetHoverHeight()
        {
            return _hoverHeight;
        }

        public float GetAbility1Time()
        {
            return _ability1Time;
        }

        public float GetAbility2Time()
        {
            return _ability2Time;
        }

        //TODO ADRI AIXÓ NO HAURIA D'ESTAR, NOMÉS ESTÀ PEL HOOK I ES TRIGGEREJA AMB UN INPUT AIXÍ QUE PASA_HO PER ONGUI()
        public bool GetAbility2Input()
        {
            return _ability2Input;
        }
        //

        public void ResetAbility1Cooldown()
        {
            _ability1Time = _ability1Cooldown;
        }

        public void ResetAbility2Cooldown()
        {
            _ability2Time = _ability2Cooldown;
        }

        public void SetInCombat(bool c)
        {
            _inCombat = c;
        }
        public bool IsInCombat()
        {
            return _inCombat;
        }

        public void UnlockDash()
        {
            _isDashUnlocked = true;
            _hookCanvas.gameObject.SetActive(true);
            StartCoroutine(SetControlText("<color=#77f0d8><size=43><b>[Shift]</b> <color=white> <size=40>Dash", 0.5f, _dashMovementHandler));
        }

        public void UnlockCharge()
        {
            _isChargeUnlocked = true;
            StartCoroutine(SetControlText("<color=#77f0d8><size=43><b>[Hook Downwards]</b> <color=white> <size=40>Smash", 0.5f, _hookMovementHandler));
        }

        public void UnlockHook()
        {
            _isHookUnlocked = true;
            _hookCanvas.gameObject.SetActive(true);
            StartCoroutine(SetControlText("<color=#77f0d8><size=43><b>[Right Click]</b> <color=white> <size=40>Hook", 0.5f, _hookMovementHandler));
        }

        private IEnumerator SetControlText(string text, float dur = 0.5f, IMovementHandler movementHandler = null)
        {
            _controlPrompt.text = text;
            _controlPrompt.color = new Color(1f, 1f, 1f, 1f);
            if (movementHandler != null)
            {
                yield return new WaitUntil(() => _movementHandler == movementHandler);
            }
            else
            {
                yield return new WaitForSeconds(5);
            }
            float t = 0f;
            float a = 1f;
            while (t < dur)
            {
                a = 1f - t / dur;
                _controlPrompt.color = new Color(1f, 1f, 1f, a);
                yield return new WaitForFixedUpdate();
                t += Time.fixedDeltaTime;
            }
            _controlPrompt.text = "";
        }

        public void UnlockWallClimb()
        {
            _isWallClimbUnlocked = true;
        }

        public void ChangeMovementHandlerToGrounded()
        {
            _groundedMovementHandler.ResetValues();
            _movementHandler = _groundedMovementHandler;
            _canBeDisplaced = true;
        }

        public void ChangeMovementHandlerToAirborne()
        {
            _airborneMovementHandler.ResetValues();
            _movementHandler = _airborneMovementHandler;
            _hookLineRenderer.enabled = false;
            _hookParticleTransform.gameObject.SetActive(false);
            //_movementHandler = new AirborneMovementHandler();
            _canBeDisplaced = true;
        }

        public void SetHorizontalDrag(float drag)
        {
            _airborneMovementHandler.horizontalDrag = drag;
        }

        public void ChangeMovementHandlerToCharge()
        {
            _chargeMovementHandler.Setup(this, _orientation.forward);
            _chargeMovementHandler.ResetValues();
            _movementHandler = _chargeMovementHandler;
            _canBeDisplaced = false;
        }

        public void ChangeMovementHandlerToHook(Vector3 startPos, Vector3 endPos, Vector3 endVisualPos, bool snap, bool smash)
        {
            _hookMovementHandler.Setup(this, startPos, endPos, endVisualPos, snap, smash);
            _hookMovementHandler.ResetValues();
            _movementHandler = _hookMovementHandler;
            _canBeDisplaced = false;
        }

        public void ChangeMovementHandlerToDash(Vector3 dashDirection)
        {
            _dashMovementHandler.Setup(this, dashDirection);
            _dashMovementHandler.ResetValues();
            _movementHandler = _dashMovementHandler;
        }

        public void ChangeMovementHandlerToWallClimb()
        {
            _wallClimbMovementHandler.ResetValues();
            _movementHandler = _wallClimbMovementHandler;
        }

        public IMovementHandler GetMovementHandler()
        {
            return _movementHandler;
        }

        public void SetJumpsAmount(int jumps)
        {
            _jumps = jumps;
            StartCoroutine(SetControlText("<color=#77f0d8><size=43><b>[Space] Mid-air</b> <color=white> <size=40>Double Jump", 0.5f, _airborneMovementHandler));
        }

        public void SetContactDamage(float t, int damage = -1)
        {
            _contactDamageTime = t;
            if (damage > 0)
            {
                _contactDamageAmount = (uint)damage;
            }
        }

        public int GetCharges()
        {
            return _hookCharges;
        }

        public void ConsumeCharge()
        {
            _hookCharges--;
        }

        public void SetMoveSpeedMultiplier(float moveSpeedMultiplier)
        {
            _moveSpeedMultiplier = moveSpeedMultiplier;
        }

        public void SetDisplaceability(bool d)
        {
            _canBeDisplaced = d;
        }

        public bool CanBeDisplaced()
        {
            return _canBeDisplaced;
        }

        public Transform GetHandBone()
        {
            return _handBone;
        }
    }
}
