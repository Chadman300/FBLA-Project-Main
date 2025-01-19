using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private AdvancedRagdollSettings settings;
    public static bool isPaused = false;

    void Start()
    {
        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        //pause
        if(Input.GetKeyDown(settings.pauseKey))
        {
            if(!isPaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void QuitGame()
    {
        Application.Quit();
        ResumeGame();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        ResumeGame();
    }
}
