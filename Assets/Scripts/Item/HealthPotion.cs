using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [Header("Health Audio")]
    [SerializeField] private AudioSource _healthAudioSource;
    [SerializeField] private AudioClip _usePotionClip;

    void OnTriggerEnter2D(Collider2D other)
    {
        var _player = other.GetComponent<PlayerHealth>();

        if (_player != null && _player._health < 100)
        {
            _player._health = Mathf.Min(_player._health + 15, 100);
            PlayPotionSound();
            Destroy(gameObject);
        }
    }

    private void PlayPotionSound()
    {
        if (_healthAudioSource != null && _usePotionClip != null)
        {
            // Create a new empty GameObject to hold the audio
            GameObject audioHolder = new GameObject("PotionSound");
            AudioSource tempAudio = audioHolder.AddComponent<AudioSource>();

            // Copy the properties and play the sound
            tempAudio.clip = _usePotionClip;
            tempAudio.volume = _healthAudioSource.volume;
            tempAudio.pitch = _healthAudioSource.pitch;
            tempAudio.Play();

            // Destroy the audio holder after the clip finishes
            Destroy(audioHolder, _usePotionClip.length);
        }
    }
}