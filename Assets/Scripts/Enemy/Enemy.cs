using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] Transform _target;
    [SerializeField] private Transform _center;
    [SerializeField] private float _knockbackVel = 8f;
    [SerializeField] float _health = 3f;

    private float _knockTime = 0.5f;
    private bool _knocked = false;
    private bool _playerKnockedBack = false;

    private enum EnemyDirection { Up, Down, Left, Right }
    private EnemyDirection _currentDirection;

    private enum EnemyState { Idle, Walking, Knocked, Dead }
    private EnemyState _currentState = EnemyState.Idle;

    NavMeshAgent _agent;
    Animator _animator;
    private Vector3 _previousPosition;

    [Header("Enemy Audio")]
    [SerializeField] private AudioSource _enemyWalkSource;
    [SerializeField] private AudioSource _enemyDamageSource;
    [SerializeField] private AudioClip _walkClip;
    [SerializeField] private AudioClip _hurtClip;
    [SerializeField] private AudioClip _deathClip;

    private void Awake()
    {
        _target = GameObject.FindWithTag("Player")?.transform;
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;

        _previousPosition = transform.position;

        if (_enemyWalkSource == null) 
        {
            _enemyWalkSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        // If the enemy is dead, do not update animations or movement
        if (_currentState == EnemyState.Dead)
        {
            return;
        } 

        if (_playerKnockedBack)
        {
            _agent.isStopped = true;
            return;
        }

        if (!_knocked && _target != null && IsPlayerOnSameNavMesh())
        {
            _agent.SetDestination(_target.position);
        }
        else
        {
            _agent.ResetPath(); // Stop moving if the player is not reachable
        }

        // Calculate movement delta
        Vector3 movementDelta = transform.position - _previousPosition;
        _previousPosition = transform.position;

        float sensitivity = 0.005f; // Sensitivity threshold to detect movement

        if (movementDelta.sqrMagnitude < sensitivity * sensitivity)
        {
            SetIdleState();
            return;
        }

        SetWalkingState(movementDelta);
    }

    public void SetPlayerKnockbackState(bool state)
    {
        _playerKnockedBack = state;
    }

    private void SetIdleState()
    {
        _currentState = EnemyState.Idle;

        if (_enemyWalkSource.isPlaying)
        {
            _enemyWalkSource.Stop();
        }

        // Disable all movement animations
        _animator.SetBool("WalkUp", false);
        _animator.SetBool("WalkDown", false);
        _animator.SetBool("WalkLeft", false);
        _animator.SetBool("WalkRight", false);

        // Trigger the universal idle animation
        _animator.SetBool("Idle", true);
    }

    private void SetWalkingState(Vector3 movementDelta)
    {
        _currentState = EnemyState.Walking;

        if (!_enemyWalkSource.isPlaying)
        {
            _enemyWalkSource.clip = _walkClip;
            _enemyWalkSource.loop = true;
            _enemyWalkSource.Play();
        }

        _animator.SetBool("Idle", false);

        // Determine direction based on movement
        if (Mathf.Abs(movementDelta.y) > Mathf.Abs(movementDelta.x))
        {
            _currentDirection = (movementDelta.y > 0) ? EnemyDirection.Up : EnemyDirection.Down;
        }
        else
        {
            _currentDirection = (movementDelta.x > 0) ? EnemyDirection.Right : EnemyDirection.Left;
        }

        // Set the appropriate walking animation
        _animator.SetBool("WalkUp", _currentDirection == EnemyDirection.Up);
        _animator.SetBool("WalkDown", _currentDirection == EnemyDirection.Down);
        _animator.SetBool("WalkLeft", _currentDirection == EnemyDirection.Left);
        _animator.SetBool("WalkRight", _currentDirection == EnemyDirection.Right);
    }

    private bool IsPlayerOnSameNavMesh()
    {
        if (_target == null) return false;

        NavMeshPath path = new NavMeshPath();
        bool hasPath = _agent.CalculatePath(_target.position, path);
        return hasPath && path.status == NavMeshPathStatus.PathComplete;
    }

    public void TakeDamage(float _damage)
    {
        _health -= _damage;

        if (_health <= 0 && _currentState != EnemyState.Dead)
        {
            _currentState = EnemyState.Dead;
            _animator.SetBool("Hurt", false);
            _animator.SetBool("Dead", true);
            PlayDeathSound();

            // Stop all movement animations
            _animator.SetBool("WalkUp", false);
            _animator.SetBool("WalkDown", false);
            _animator.SetBool("WalkLeft", false);
            _animator.SetBool("WalkRight", false);
            _animator.SetBool("Idle", false);

            // Play the appropriate death animation based on the last direction
            switch (_currentDirection)
            {
                case EnemyDirection.Up:
                    _animator.SetTrigger("DeathUp");
                    break;
                case EnemyDirection.Down:
                    _animator.SetTrigger("DeathDown");
                    break;
                case EnemyDirection.Left:
                    _animator.SetTrigger("DeathLeft");
                    break;
                case EnemyDirection.Right:
                    _animator.SetTrigger("DeathRight");
                    break;
            }

            StartCoroutine(Die());
        }
    }

    public bool IsDead()
    {
        return _animator.GetBool("Dead");
    }

    private IEnumerator Die()
    {
        // Allow knockback to finish before disabling the NavMeshAgent
        yield return new WaitForSeconds(0.5f);
        _agent.enabled = false;

        // Wait for the remainder of the death animation
        yield return new WaitForSeconds(0.35f);
        Destroy(gameObject);
    }

    public void Knockback(Transform t)
    {
        if (!_agent.enabled) return;

        Vector3 _direction = (_center.position - t.position).normalized;

        _knocked = true;
        _agent.isStopped = true;
        _agent.velocity = _direction * _knockbackVel;

        // Enable Hurt state
        _animator.SetBool("Hurt", true);
        PlayHurtSound();

        switch (_currentDirection)
        {
            case EnemyDirection.Up:
                _animator.SetTrigger("HurtUp");
                break;
            case EnemyDirection.Down:
                _animator.SetTrigger("HurtDown");
                break;
            case EnemyDirection.Left:
                _animator.SetTrigger("HurtLeft");
                break;
            case EnemyDirection.Right:
                _animator.SetTrigger("HurtRight");
                break;
        }

        StartCoroutine(UnKnocked());
    }

    private IEnumerator UnKnocked()
    {
        yield return new WaitForSeconds(_knockTime);

        if (_currentState == EnemyState.Dead || !_agent.enabled)
        {
            yield break; // Exit the coroutine early if the enemy is dead or the NavMeshAgent is disabled
        }

        _knocked = false;
        _agent.isStopped = false;

        // Reset Hurt animation
        _animator.SetBool("Hurt", false);
    }

    private void PlayHurtSound()
    {
        if (_enemyDamageSource != null && _hurtClip != null)
        {
            _enemyDamageSource.PlayOneShot(_hurtClip);
        }
    }

    private void PlayDeathSound()
    {
        if (_enemyDamageSource != null && _deathClip != null)
        {
            _enemyDamageSource.PlayOneShot(_deathClip);
        }
    }
}