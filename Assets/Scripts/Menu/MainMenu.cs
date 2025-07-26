using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioSource musicSource;      // Müzik çalacak kaynak
    public AudioClip menuMusic;          // Menü müziği

    private void Start()
    {
        StartCoroutine(PlayMusicWithDelay(0.5f)); // 1 saniye sonra başlat
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
        Debug.Log("⚙️ Ayarlar açıldı.");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
