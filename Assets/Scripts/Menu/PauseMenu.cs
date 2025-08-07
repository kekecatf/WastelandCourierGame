using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;

    private bool isPaused = false;
    public GameObject settingsPanel;
    private PlayerInput playerInput;
    public static bool IsPaused { get; private set; }
    private bool isInSettings = false;




    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
    }


    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isInSettings)
            {
                CloseSettings();
                return;
            }

            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }


    public void PauseGame()
    {
        isPaused = true;
        IsPaused = true; // <-- static kontrol
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        IsPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }


    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenSettings()
{
    pausePanel.SetActive(false);
    settingsPanel.SetActive(true);
    isInSettings = true;
    Debug.Log("⚙️ Ayarlar paneli açıldı.");
}
    public void CloseSettings()
{
    settingsPanel.SetActive(false);
    pausePanel.SetActive(true);
    isInSettings = false;
    Debug.Log("⬅️ Ayarlardan çıkıldı, Pause menüsüne dönüldü.");
}

}
