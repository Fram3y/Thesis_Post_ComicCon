using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    public GameObject _meleeTop;
    public GameObject _meleeBottom;
    public GameObject _meleeLeft;
    public GameObject _meleeRight;

    private InterfaceManager _interfaceManager;
    private Animator _animator;
    
    [SerializeField] private InputAction _attackAction;
    public float _attackDuration = 0.5f;
    public float _attackCooldown = 0.5f;
    public float _cooldownTimer = 0f;

    public bool _attackTriggered = false;

    private PlayerMovement _playerMovement;
    
    [Header("Player Audio")]
    [SerializeField] private AudioSource _playerAttackSource;
    [SerializeField] private AudioClip _attackClip;
    
    void Awake()
    {
        _meleeTop.SetActive(false);
        _meleeBottom.SetActive(false);
        _meleeLeft.SetActive(false);
        _meleeRight.SetActive(false);

        _interfaceManager = FindObjectOfType<InterfaceManager>();
        _animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();
    }

    void OnEnable()
    {
        _attackAction.Enable();
    }

    void OnDisable()
    {
        _attackAction.Disable();
    }
    
    void Update()
    {
        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
        }

        if ((_attackAction.triggered || Input.GetKeyDown(KeyCode.Mouse0)) && _cooldownTimer <= 0f && _interfaceManager._isPaused == false)
        {
            Attack();
            PlayAttackSound();
        }
    }

    void Attack()
    {
        _attackTriggered = true;
        _cooldownTimer = _attackCooldown;
        _animator.SetBool("Attacking", true);
        Vector2 aimDirection = _playerMovement.HandleAim();

        DeactivateAllMeleeObjects();

        if (aimDirection == Vector2.up)  // Aim is upwards
        {
            _animator.SetTrigger("AttackUp");
            _meleeTop.SetActive(true);
        }
        else if (aimDirection == Vector2.down)  // Aim is downwards
        {
            _animator.SetTrigger("AttackDown");
            _meleeBottom.SetActive(true);
        }
        else if (aimDirection == Vector2.left)  // Aim is left
        {
            _animator.SetTrigger("AttackLeft");
            _meleeLeft.SetActive(true);
        }
        else if (aimDirection == Vector2.right)  // Aim is right
        {
            _animator.SetTrigger("AttackRight");
            _meleeRight.SetActive(true);
        }
        
        StartCoroutine(EndAttack());
    }

    private IEnumerator EndAttack()
    {
        yield return new WaitForSeconds(_attackDuration);
        _animator.SetBool("Attacking", false);
        _attackTriggered = false;
        DeactivateAllMeleeObjects();

        _playerMovement._isAttacking = false;
    }

    private void DeactivateAllMeleeObjects()
    {
        _meleeTop.SetActive(false);
        _meleeBottom.SetActive(false);
        _meleeRight.SetActive(false);
        _meleeLeft.SetActive(false);
    }

    private void PlayAttackSound()
    {
        if (_playerAttackSource != null && _attackClip != null)
        {
            _playerAttackSource.PlayOneShot(_attackClip); // Play the attack sound
        }
    }
}