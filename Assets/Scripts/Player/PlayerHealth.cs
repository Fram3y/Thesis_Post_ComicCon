using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] public float _health = 100f;
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private Image _healthBar;

    private const float MaxHealthBarWidth = 331.1807f;
    private const float HealthBarHeight = 38.539f;

    [Header("Player Audio")]
    [SerializeField] private AudioSource _playerDamageSource;
    [SerializeField] private AudioClip _deathClip;

    void Awake()
    {
        _healthBar = GameObject.Find("HealthBar").GetComponent<Image>();

        // Set initial fixed height for the health bar
        _healthBar.rectTransform.sizeDelta = new Vector2(MaxHealthBarWidth, HealthBarHeight);
    }

    void Update()
    {
        // Calculate health percentage for the health bar
        float healthPercent = Mathf.Clamp01(_health / _maxHealth);

        // Dynamically adjust the health bar's width based on health percentage
        _healthBar.rectTransform.sizeDelta = new Vector2(MaxHealthBarWidth * healthPercent, HealthBarHeight);

        // Player death logic
        if (_health <= 0)
        {
            PlayDeathSound();
            _health = Mathf.Max(_health, 0);
            gameObject.SetActive(false);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        var enemy = collision.collider.GetComponent<Enemy>();

        if (enemy != null && !enemy.IsDead())
        {
            _health -= 10;
        }
    }

    private void PlayDeathSound()
    {
        if (_playerDamageSource != null && _deathClip != null)
        {
            _playerDamageSource.PlayOneShot(_deathClip);
        }
    }
}