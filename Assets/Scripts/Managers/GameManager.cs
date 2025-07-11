using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject gameOverPanel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void GameOver()
    {
        Debug.Log("🏚️ Oyun Bitti!");
        Time.timeScale = 0f; // Oyunu durdur
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // Menü sahnenin ismi bu olmalı
    }
}
