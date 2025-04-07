using Unity.Loading;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string playSceneName = "MainMenu";

    public void Play()
    {
        SceneManager.LoadScene(playSceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
