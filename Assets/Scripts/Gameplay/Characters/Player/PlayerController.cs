using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerRaycastsCheck))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    private InputManager _inputManager;
    private Rigidbody2D _rb;
    private PlayerRaycastsCheck _playerRaycastsCheck;
    private PlayerStealthCheck _stealthCheck;
    private SpriteRenderer _spriteRenderer;

    [Header("Abilities")]
    [SerializeField] private bool _canMove;
    [SerializeField] private bool _canJump;
    [SerializeField] private bool _canDuck; //Some additional checks should be added next (if player can Use abilities or interact etc.)

    //Vector2 frameVelocity - velocity of a player in any given time. Base velocity is calculated by sprinting and walking +
    //some calculations from Jumping/Using platforms and dashing
    [Header("General")]
    [SerializeField] private Vector2 _frameVelocity;
    [SerializeField] bool _isGrounded;

    [Header("Walking")]
    [SerializeField] float _walkMoveSpeed;
    [SerializeField] float _walkAcceleration;

    [Header("Sprinting")]
    [SerializeField] float _sprintMoveSpeed;
    [SerializeField] float _sprintAcceleration;
    float _sprintOffTimer;
    [SerializeField] bool _isSprinting;

    float _groundDeceleration = 40f;
    float _airDeceleration = 25f;
    float _movementDirection;

    [Header("Ducking")]
    [SerializeField] float _duckMoveSpeed;
    [SerializeField] bool _isDucking;
    [SerializeField] bool _isCeilingHit;
    [SerializeField] bool _isOnPlatform;
    PlatformEffector2D _platform;
    private Vector3 _baseScaling;
    [SerializeField] float _duckScaleSize; // change later

    [Header("Stealth")]
    [SerializeField] bool _isInStealth;
    bool _canBeHidden;
    Color _baseColor; // for player transparency

    [Header("Jump")]
    [SerializeField] float _maxJumpPower;
    private bool _isJumpPressed;
    private bool _isJumpHeld;
    private bool _isJumpEndedEarly;
    private float _timeJumpWasPressed;

    [Header("Jump Extras")]
    [SerializeField, Range(0.01f, 1f)] float _bufferTime = 0.1f;
    [SerializeField, Range(0.01f, 1f)] float _coyoteTime = 0.1f;
    bool _bufferedJumpUsable;
    bool _coyoteUsable;
    private bool JumpBuffered => _bufferedJumpUsable && _time < _timeJumpWasPressed + _bufferTime;
    private bool CoyoteJumpPressed => _coyoteUsable && !_isGrounded && _time < _frameLeftGround + _coyoteTime;

    [Header("Ledge Detection")]
    [SerializeField] Vector2 _ledgeOffset;
    bool _isLedgeDetected;
    bool _canClimb = true;
    bool _isRightLedge;

    //GroundCollisions
    float _frameLeftGround = float.MinValue;
    

    //Gravity
    private float _fallGravityMultiplier = 2.5f;

    //Timer
    float _time;
    float _offGroundTimer;

    private void Awake()
    {
        Instance = this;
        _inputManager = GetComponent<InputManager>();
        _rb = GetComponent<Rigidbody2D>();
        _stealthCheck = GetComponent<PlayerStealthCheck>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerRaycastsCheck = GetComponent<PlayerRaycastsCheck>();

        _playerRaycastsCheck.OnGroundHitChanged += OnGroundChanged;
        _playerRaycastsCheck.OnCeilingHitChanged += OnCeilingHit;
        _playerRaycastsCheck.OnPlatformStandingChanged += OnPlatformStanding;
        _playerRaycastsCheck.OnLedgeDetected += OnLedgeDetection;
        _stealthCheck.OnStealthChange += OnStealthChanged;

        _baseScaling = transform.localScale;
        _baseColor = _spriteRenderer.color;

        PlayerEnable();
    }

    private void Update()
    {
        _time += Time.deltaTime;

        GetAllLocomotion();
        CheckStealth();
    }

    private void FixedUpdate()
    {
        Move(_movementDirection);
        CheckLedgeClimb();
        GravityCheck();
        CheckJump();

        ApplyMovement();
    }

    private void GetAllLocomotion()
    {
        _movementDirection = _inputManager.GetPlayerMovement();

        if (_inputManager.GetPlayerDuck() && _canDuck)
        {
            _isDucking = !_isDucking;
            if (_isDucking)
            {
                SetDucking(true);
            }
            else if (!_isDucking && CheckAbilityToUnduck())
            {
                SetDucking(false);
            }
            else if (!_isDucking && !CheckAbilityToUnduck())
            {
                _isDucking = true;
            }
        }

        if (!_isGrounded)
        {
            _offGroundTimer += Time.deltaTime;
            if (_offGroundTimer >= 0.6f && _isDucking)
            {
                _isDucking = false;
                SetDucking(false);
            }
        }
        else
        {
            _offGroundTimer = 0;
        }

        _isSprinting |= _inputManager.GetPlayerSprint();

        _isJumpHeld = _inputManager.GetPlayerJumpHeld();

        if (_inputManager.GetPlayerJump())
        {
            _isJumpPressed |= true;
            _timeJumpWasPressed = _time;
        }
    }

    private void CheckLedgeClimb()
    {
        if (_isLedgeDetected && _canClimb && !_isOnPlatform)
        {
            _canClimb = false;

            if (_isRightLedge)
            {
                transform.position = new Vector2(transform.position.x + _ledgeOffset.x, transform.position.y + _ledgeOffset.y);
            }
            else
            {
                transform.position = new Vector2(transform.position.x - _ledgeOffset.x, transform.position.y + _ledgeOffset.y);
            }
            
            Invoke("UpdateClimb", 0.1f);
        }
    }

    private void UpdateClimb()
    {
        _canClimb = true;
    }

    private void CheckStealth()
    {
        if (_isDucking && _canBeHidden)
        {
            _spriteRenderer.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0.45f);
            _isInStealth = true;
        }
        else
        {
            _spriteRenderer.color = _baseColor;
            _isInStealth = false;
        }
    }

    private void CheckJump()
    {
        if (!_isJumpEndedEarly && !_isGrounded && !_isJumpHeld && _rb.linearVelocity.y > 0)
        {
            _isJumpEndedEarly = true;
        }

        if (!_isJumpPressed && !JumpBuffered)
        {
            return;
        }

        if ((_isGrounded && _isJumpPressed) || CoyoteJumpPressed)
        {
            if (_canJump)
            {
                ExecuteJump();
            }
        }

        _isJumpPressed = false;
    }

    private void GravityCheck()
    {
        if (_isGrounded && _rb.linearVelocity.y <= 0 && _isOnPlatform && _isDucking)
        {
            _frameVelocity.y = -10f;
            Invoke("PassPlatform", 0.45f);
        }
        else if (_isGrounded && _rb.linearVelocity.y <= 0 && !_isDucking)
        {
            _frameVelocity.y = -1.5f;
        }
        else if (!_isGrounded)
        {
            var inAirGravity = 50f;
            if (_isJumpEndedEarly && _frameVelocity.y > 0)
            {
                inAirGravity *= _fallGravityMultiplier;
            }
            inAirGravity = Mathf.MoveTowards(inAirGravity, 50f, 25f * Time.fixedDeltaTime);
            _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -20, inAirGravity * Time.fixedDeltaTime);
        }
    }

    private void Move(float movement)
    {
        var vector = movement > 0 ? 1 : -1;

        if (movement > 0)
        {
            _spriteRenderer.flipX = false;
        }
        else if (movement < 0)
        {
            _spriteRenderer.flipX = true;
        }
        
        if (movement == 0)
        {
            var deceleration = _isGrounded ? _groundDeceleration : _airDeceleration;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);

            _sprintOffTimer += Time.deltaTime;

            if (_sprintOffTimer >= 0.15f)
            {
                _isSprinting = false;
            }
        }
        else if (_canMove && _isDucking)
        {
            _sprintOffTimer = 0;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, vector * _duckMoveSpeed, _walkAcceleration * Time.fixedDeltaTime);
        }
        else if (_canMove && _isSprinting)
        {
            if (!_isDucking)
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, vector * _sprintMoveSpeed, _sprintAcceleration * Time.fixedDeltaTime);
            }
            else if (_isDucking && CheckAbilityToUnduck())
            {
                _isDucking = false;
                SetDucking(false);
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, vector * _sprintMoveSpeed, _sprintAcceleration * Time.fixedDeltaTime);
            }
        }
        else
        {
            _sprintOffTimer = 0;
            _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, vector * _walkMoveSpeed, _walkAcceleration * Time.fixedDeltaTime);
        }
    }

    private void SetDucking(bool isDucking)
    {
        if (isDucking)
        {
            if (!_isOnPlatform)
            {
                transform.localScale = new Vector3(transform.localScale.x, _duckScaleSize, transform.localScale.z);
            }
            else
            {
                _platform.GetComponent<PassablePlatform>().ActivateTrap();
            }
        }
        else
        {
            transform.localScale = _baseScaling;
        }
    }

    private bool CheckAbilityToUnduck()
    {
        if (_isCeilingHit)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void ExecuteJump()
    {
        _timeJumpWasPressed = 0;
        if (!_isDucking)
        {
            _frameVelocity.y = _maxJumpPower;
        }
        else if ((_isDucking && CheckAbilityToUnduck()))
        {
            SetDucking(false);
            _isDucking = false;
            _frameVelocity.y = _maxJumpPower;
        }
    }

    private void PassPlatform()
    {
        _isDucking = false;
        SetDucking(_isDucking);
    }

    private void ApplyMovement()
    {
        _rb.linearVelocity = _frameVelocity;
    }

    public void PlayerEnable()
    {
        _canMove = true;
        _canJump = true;
        _canDuck = true;
    }

    public void PlayerDisable()
    {
        _canMove = false;
        _canJump = false;
        _canDuck = false;
    }

    private void OnCeilingHit(bool isHit)
    {
        if (isHit)
        {
            _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);
            _isSprinting = false;

            _isCeilingHit = true;
        }
        else
        {
            _isCeilingHit = false;
        }
    }

    private void OnGroundChanged(bool isGrounded)
    {
        if (!_isGrounded && isGrounded)
        {
            _isGrounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _isJumpEndedEarly = false;
        }
        else if (_isGrounded && !isGrounded)
        {
            _isGrounded = false;
            _frameLeftGround = _time;
        }
    }

    private void OnStealthChanged(bool canBeHidden)
    {
        if (canBeHidden)
        {
            _canBeHidden = true;
        }
        else
        {
            _canBeHidden = false;
        }
    }

    private void OnPlatformStanding(bool isOnPlatform, PlatformEffector2D platform = default)
    {
        if (isOnPlatform)
        {
            _isOnPlatform = true;
            _platform = platform;
        }
        else
        {
            _isOnPlatform = false;
            _platform = null;
        }
    }

    private void OnLedgeDetection(bool isLedgeDetected, bool isRightLedge)
    {
        if (isLedgeDetected)
        {
            if (isRightLedge)
            {
                _isRightLedge = true;
            }
            else
            {
                _isRightLedge = false;
            }

            _isLedgeDetected = true;
        }
        else
        {
            _isLedgeDetected = false;
        }
    }

    private void OnDestroy()
    {
        _playerRaycastsCheck.OnGroundHitChanged -= OnGroundChanged;
        _playerRaycastsCheck.OnCeilingHitChanged -= OnCeilingHit;
        _playerRaycastsCheck.OnPlatformStandingChanged -= OnPlatformStanding;
        _stealthCheck.OnStealthChange -= OnStealthChanged;
    }
}
