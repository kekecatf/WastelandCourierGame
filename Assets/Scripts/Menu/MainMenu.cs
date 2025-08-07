using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip menuMusic;

    [Header("UI Panelleri")]
    public GameObject mainPanel;
    public GameObject settingsPanel;

    private bool isInSettings = false;

    private void Start()
    {
        StartCoroutine(PlayMusicWithDelay(0.5f));
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame && isInSettings)
        {
            CloseSettings(); // ESC ile ayarlardan geri dön
        }
    }

    private System.Collections.IEnumerator PlayMusicWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (musicSource != null && menuMusic != null)
        {
            musicSource.clip = menuMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Bolum1");
    }

    public void OpenSettings()
    {
        if (settingsPanel != null && mainPanel != null)
        {
            mainPanel.SetActive(false);
            settingsPanel.SetActive(true);
            isInSettings = true;
            Debug.Log("⚙️ Ayarlar paneli açıldı.");
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null && mainPanel != null)
        {
            settingsPanel.SetActive(false);
            mainPanel.SetActive(true);
            isInSettings = false;
            Debug.Log("⬅️ Ayarlardan ana menüye dönüldü.");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
