using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    public GameObject panel;
    public Slider musicSlider;
    public Slider sfxSlider;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;




    void Start()
    {
        // Ayarları yükle
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", QualitySettings.GetQualityLevel());
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        ApplySettings();

        // Dinleyiciler
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        qualityDropdown.onValueChanged.AddListener(SetQuality);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void OpenPanel() => panel.SetActive(true);
    public void ClosePanel() => panel.SetActive(false);

    void SetMusicVolume(float value)
    {
        AudioListener.volume = value; // örnek
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    void SetSFXVolume(float value)
    {
        // Eğer SFX için ayrı ses kaynağın varsa onu kontrol et
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("QualityLevel", index);
    }

    void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    void ApplySettings()
    {
        SetMusicVolume(musicSlider.value);
        SetSFXVolume(sfxSlider.value);
        SetQuality(qualityDropdown.value);
        SetFullscreen(fullscreenToggle.isOn);
    }
}
