using UnityEngine;

public class KnockbackTrigger : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Enemy _enemy;
    private PlayerMovement _player;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _enemy = GetComponent<Enemy>();
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        _player = other.collider.GetComponent<PlayerMovement>();

        if (_player != null && !_enemy.IsDead())
        {
            // Temporarily set Rigidbody to kinematic
            _rb.isKinematic = true;

            // Apply knockback to player
            _player.Knockback(transform);

            // Revert Rigidbody to dynamic after a short delay
            Invoke(nameof(ResetRigidbody), 0.5f);
        }
    }

    private void ResetRigidbody()
    {
        _rb.isKinematic = false;
        _rb.linearVelocity = Vector2.zero; // Reset velocity to prevent unwanted movement
    }
}
