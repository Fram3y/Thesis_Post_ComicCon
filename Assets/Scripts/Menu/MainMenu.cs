using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Canvas Panels")] 
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _optionsPanel;

    [Header("Buttons")]
    [SerializeField] private GameObject _newGameButton;
    [SerializeField] private GameObject _musicSlider;

    [Header("Button Text")] 
    [SerializeField] public TMP_Text _newText;
    [SerializeField] public TMP_Text _optionText;
    [SerializeField] public TMP_Text _exitText;
    [SerializeField] public TMP_Text _saveText;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _backgroundMusic;

    private EventSystem _eventSystem;

    void Awake()
    {
        _eventSystem = EventSystem.current;
        _mainPanel.SetActive(true);
        _optionsPanel.SetActive(false);
        SetTextColors();
        PlayMenuMusic();
        SetSelectedButton(_newGameButton);
    }

    public void LoadRoomGeneration()
    {
        SceneManager.LoadScene("RoomGeneration");
    }

    public void OpenOptions()
    {
        _mainPanel.SetActive(false);
        _optionsPanel.SetActive(true);
        SetSelectedButton(_musicSlider);
    }

    public void CloseOptions()
    {
        _optionsPanel.SetActive(false);
        _mainPanel.SetActive(true);
        SetSelectedButton(_newGameButton);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void SetTextColors()
    {
        ChangeTextColor(_newText);
        ChangeTextColor(_optionText);
        ChangeTextColor(_exitText);
        ChangeTextColor(_saveText);
    }

    private void ChangeTextColor(TMP_Text text)
    {
        if (text != null)
        {
            text.enableVertexGradient = false;
            text.color = Color.white;

            Material newMaterial = new Material(text.fontMaterial);
            newMaterial.SetColor("_FaceColor", Color.white);
            text.fontMaterial = newMaterial;
        }
    }

    private void PlayMenuMusic()
    {
        if (_audioSource != null && _backgroundMusic != null)
        {
            _audioSource.clip = _backgroundMusic;
            _audioSource.loop = true;
            _audioSource.Play();
        }
    }

    private void SetSelectedButton(GameObject button)
    {
        if (_eventSystem != null && button != null)
        {
            _eventSystem.SetSelectedGameObject(button);
        }
    }
}
