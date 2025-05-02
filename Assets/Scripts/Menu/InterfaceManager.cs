using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class InterfaceManager : MonoBehaviour
{
    [Header("Canvas Panels")]
    [SerializeField] private CinemachineCamera _cineCam;
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _optionsMenu;
    [SerializeField] private GameObject _loadingScreen;
    [SerializeField] private GameObject _userInterface;

    private GameObject _player;
    public bool _isPaused = false;
    private RoomManager _roomManager;
    [SerializeField] private InputAction _pauseButton;
    [SerializeField] private GameObject _musicSlider;
    [SerializeField] private GameObject _resumeButton;
    
    [Header("Interface Audio")]
    [SerializeField] private AudioSource _interfaceAudioSource;
    [SerializeField] private AudioClip _openMenuClip;
    [SerializeField] private AudioClip _closeMenuClip;
    
    void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        _pauseMenu.SetActive(_isPaused);
        _optionsMenu.SetActive(false);

        _roomManager = FindObjectOfType<RoomManager>();
        if (_cineCam == null)
        {
            _cineCam = GameObject.FindObjectOfType<CinemachineCamera>();    
        }   

        _player = GameObject.FindWithTag("Player");

        if (_roomManager == null)
        {
            Debug.LogError("RoomManager not found in the scene!");
        }

        if (_player != null && _cineCam != null)
        {
            _cineCam.Follow = _player.transform;
            _cineCam.LookAt = _player.transform;
        }
        else
        {
            Debug.LogWarning("Player or Cinemachine Camera not found!");
        }
    }

    void OnEnable()
    {
        _pauseButton.Enable();
    }

    void OnDisable()
    {
        _pauseButton.Disable();
    }

    void Update()
    {
        if (_roomManager != null)
        {
            _loadingScreen.SetActive(!_roomManager.generationComplete);
        }

        _userInterface.SetActive(!_isPaused && !_loadingScreen.activeSelf);

        if (Input.GetKeyDown(KeyCode.Escape) || _pauseButton.triggered)
        {
            TogglePauseMenu();
        }
    }

    private void TogglePauseMenu()
    {
        _isPaused = !_isPaused;
        _pauseMenu.SetActive(_isPaused);
        _userInterface.SetActive(false);

        PlayInterfaceSound(_isPaused);

        _userInterface.SetActive(!_isPaused && !_loadingScreen.activeSelf);
        
        // Pause the game when the menu is open
        Time.timeScale = _isPaused ? 0f : 1f;
    }

    public void ToggleOptionsMenu(bool open)
    {
        _pauseMenu.SetActive(!open);
        _optionsMenu.SetActive(open);

        if (open)
        {
            EventSystem.current.SetSelectedGameObject(_musicSlider);
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(_resumeButton);
        }
    }

    private void PlayInterfaceSound(bool isPaused)
    {
        if (_interfaceAudioSource != null)
        {
            AudioClip clipToPlay = isPaused ? _openMenuClip : _closeMenuClip;
            _interfaceAudioSource.PlayOneShot(clipToPlay);
        }
    }

    public void ResumeGame()
    {
        _isPaused = false;
        _pauseMenu.SetActive(false);

        PlayInterfaceSound(_isPaused);
        _userInterface.SetActive(!_loadingScreen.activeSelf);
        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}