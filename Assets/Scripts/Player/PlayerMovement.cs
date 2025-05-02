using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Transform _center;

    private Vector2 _movement;
    private Vector2 _lastMovementDirection;
    private float _knockbackVel = 8f;
    private float _knockedTime = 0.5f;
    private bool _knocked = false;

    private float _idleDelay = 0.125f; // 1/8 second delay
    public float _idleTimer = 0f;

    private Vector2 _lockedAimDirection; // Stores attack direction
    public bool _isAttacking = false;   // Prevents aim updates during attack

    public enum PlayerDirection { Up, Down, Left, Right }
    public PlayerDirection _previousDirection = PlayerDirection.Down;

    private Rigidbody2D _rb;
    private Animator _animator;

    private PlayerMelee _playerMelee;

    [Header("Player Audio")]
    [SerializeField] private AudioSource _playerWalkSource;
    [SerializeField] private AudioSource _playerDamageSource;
    [SerializeField] private AudioClip _walkClip;
    [SerializeField] private AudioClip _hurtClip;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _playerMelee = FindObjectOfType<PlayerMelee>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!_knocked)
        {
            HandleMovement();
            HandleAim();
        }
        else
        {
            ApplyKnockbackDeceleration();
        }
    }

    private void HandleMovement()
    {
        _movement.Set(InputManager.Movement.x, InputManager.Movement.y);
        _rb.linearVelocity = _movement * _moveSpeed;

        if (_movement.sqrMagnitude <= 0f)
        {
            _idleTimer -= Time.deltaTime;

            if(_idleTimer <= 0f && !_playerMelee._attackTriggered)
            {
                SetIdleState();
            }
        }
        else
        {
            // Ensure Idle is false when moving
            _idleTimer = _idleDelay;
            _animator.SetBool("Idle", false);

            if (!_playerWalkSource.isPlaying)
            {
                _playerWalkSource.clip = _walkClip;
                _playerWalkSource.loop = true;
                _playerWalkSource.Play();
            }

            _animator.SetBool("WalkDown", _movement.y < 0);
            _animator.SetBool("WalkUp", _movement.y > 0);
            _animator.SetBool("WalkLeft", _movement.x < 0);
            _animator.SetBool("WalkRight", _movement.x > 0);

            if (_movement.y < 0) _previousDirection = PlayerDirection.Down;
            else if (_movement.y > 0) _previousDirection = PlayerDirection.Up;
            else if (_movement.x < 0) _previousDirection = PlayerDirection.Left;
            else if (_movement.x > 0) _previousDirection = PlayerDirection.Right;
        }
    }

    private void SetIdleState()
    {
        // Set Idle to true when not moving
        _animator.SetBool("Idle", true); 
        _animator.SetBool("WalkDown", false);
        _animator.SetBool("WalkLeft", false);
        _animator.SetBool("WalkRight", false);
        _animator.SetBool("WalkUp", false);
        _animator.SetBool("Hurt", false);

        if (_playerWalkSource.isPlaying)
        {
            _playerWalkSource.Stop();
        }
    }

    public Vector2 HandleAim()
    {
        // If attacking, maintain locked aim direction
        if (_isAttacking) return _lockedAimDirection;

        Vector2 moveInput = new Vector2(InputManager.Movement.x, InputManager.Movement.y);
        Vector2 aimDirection = Vector2.zero;

        // Determine primary aim direction based on movement input
        if (moveInput.sqrMagnitude > 0f) // Ensure movement is happening
        {
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                aimDirection = new Vector2(Mathf.Sign(moveInput.x), 0); // Prioritize horizontal movement
            }
            else
            {
                aimDirection = new Vector2(0, Mathf.Sign(moveInput.y)); // Prioritize vertical movement
            }
        
            // Store last valid movement direction
            _lastMovementDirection = aimDirection;
        }
        else
        {
            // If no movement input, use last movement direction
            aimDirection = _lastMovementDirection;
        }

        // If last movement direction is still (0,0), use previous direction enum
        if (aimDirection.sqrMagnitude <= 0f)
        {
            switch (_previousDirection)
            {
                case PlayerDirection.Up: aimDirection = Vector2.up; break;
                case PlayerDirection.Down: aimDirection = Vector2.down; break;
                case PlayerDirection.Left: aimDirection = Vector2.left; break;
                case PlayerDirection.Right: aimDirection = Vector2.right; break;
            }
        }

        // Lock in aim direction if an attack is starting
        if (_playerMelee._attackTriggered && aimDirection.sqrMagnitude > 0f)
        {
            _lockedAimDirection = aimDirection;
            _isAttacking = true;
        }

        return aimDirection;
    }

    private void ApplyKnockbackDeceleration()
    {
        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, Time.deltaTime * 3);
    }

    public void Knockback(Transform t)
    {
        Vector2 direction = _center.position - t.position;
        
        // Enable Hurt state
        _knocked = true;
        PlayHurtSound();
        _animator.SetBool("Hurt", true);

        // Trigger the appropriate Hurt animation based on the last direction
        switch (_previousDirection)
        {
            case PlayerDirection.Down:
                _animator.SetTrigger("HurtDown");
                break;
            case PlayerDirection.Up:
                _animator.SetTrigger("HurtUp");
                break;
            case PlayerDirection.Left:
                _animator.SetTrigger("HurtLeft");
                break;
            case PlayerDirection.Right:
                _animator.SetTrigger("HurtRight");
                break;
        }

        _rb.linearVelocity = direction.normalized * _knockbackVel;

        StartCoroutine(Unknocked());
    }

    private IEnumerator Unknocked()
    {
        // Get the current animation clip length
        float animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;

        // Use the longer time between knockback and animation duration
        float waitTime = Mathf.Max(_knockedTime, animationLength);

        yield return new WaitForSeconds(waitTime);

        _knocked = false;
        _animator.SetBool("Hurt", false);
    }

    private void PlayHurtSound()
    {
        if (_playerDamageSource != null && _hurtClip != null)
        {
            _playerDamageSource.PlayOneShot(_hurtClip);
        }
    }
}