using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ButtonColourChanger : MonoBehaviour
{
    [Header("Button Colors")]
    public Color normalColor = Color.white;
    public Color highlightedColor = Color.green;
    public Color pressedColor = Color.blue;
    public Color disabledColor = Color.gray;

    private Button button;
    private TextMeshProUGUI tmpText;

    void Awake()
    {
        button = GetComponent<Button>();
        tmpText = GetComponentInChildren<TextMeshProUGUI>();  // Get TMP text component
    }

    void OnEnable()
    {
        button.onClick.AddListener(OnClick);
        button.onClick.AddListener(UpdateButtonColor);
    }

    void OnDisable()
    {
        button.onClick.RemoveListener(OnClick);
        button.onClick.RemoveListener(UpdateButtonColor);
    }

    void UpdateButtonColor()
    {
        // Change the color of the button text (TMP)
        if (tmpText != null)
        {
            tmpText.color = normalColor; // Default color
        }
    }

    void OnClick()
    {
        if (tmpText != null)
        {
            tmpText.color = pressedColor;
        }
    }
}
