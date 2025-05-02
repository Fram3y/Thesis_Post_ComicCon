using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float _damage = 1;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy _enemy = collision.GetComponent<Enemy>();

        if(_enemy != null)
        {
            _enemy.TakeDamage(_damage);
            _enemy.Knockback(transform);
        }
    }
}
