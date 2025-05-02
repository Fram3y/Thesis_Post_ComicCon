using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")] 
    [SerializeField] AudioSource _musicSource;
    [SerializeField] AudioSource _sfxSource;

    [Header("Audio Clip")] 
    public AudioClip _background;
    public AudioClip _menuBackground;

    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "RoomGeneration")
        {
            _musicSource.clip = _background;
        }
        else if (sceneName == "MainMenu")
        {
            _musicSource.clip = _menuBackground;
        }

        if (_musicSource.clip != null) 
        {
            _musicSource.loop = true;
            _musicSource.Play();
        }
    }

    public void PlayLoopingSFX(AudioClip clip)
    {
        if (_sfxSource.clip != clip)
        {
            _sfxSource.clip = clip;
            _sfxSource.loop = true;
            _sfxSource.Play();
        }
    }

    public void StopLoopingSFX()
    {
        if (_sfxSource.loop)
        {
            _sfxSource.Stop();
            _sfxSource.clip = null;
            _sfxSource.loop = false;
        }
    }
}