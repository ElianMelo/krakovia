using UnityEngine;
using UnityEngine.UI;

public class MenuInterfaceController : MonoBehaviour
{
    public Button continueButton;
    public Button tutorialButton;
    public Slider audioSlider;
    public Slider sensibilitySlider;
    public Button closeButton;

    [Header("Tutorial")]
    public GameObject menuVisuals;
    public GameObject tutorialButtonObject;

    void Start()
    {
        AudioListener.volume = 0.5f;
        audioSlider.onValueChanged.AddListener(delegate { OnAudioSliderChanged(); });
        sensibilitySlider.onValueChanged.AddListener(delegate { OnSensibilitySliderChanged(); });
        continueButton.onClick.AddListener(OnContinueButtonClick);
        tutorialButton.onClick.AddListener(OnTutorialButtonClick);
        closeButton.onClick.AddListener(OnCloseButtonClick);
    }

    public void ShowMenuVisuals()
    {
        menuVisuals.SetActive(true);
    }

    public void OnContinueButtonClick()
    {
        menuVisuals.SetActive(false);
        SystemManager.Instance.ForceLockMouse();
    }

    public void OnTutorialButtonClick()
    {
        tutorialButtonObject.SetActive(true);
    }

    public void OnCloseButtonClick()
    {
        Application.Quit();
    }

    public void OnAudioSliderChanged()
    {
        AudioListener.volume = audioSlider.value;
    }

    public void OnSensibilitySliderChanged()
    {
        MouseRotator.Instance.SetupRotationPower(sensibilitySlider.value);
    }
}
