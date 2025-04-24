using Unity.Loading;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string playSceneName = "MainMenu";

    [SerializeField] private GameObject tutorialObj;
    [SerializeField] private GameObject tutorialPgOne;
    [SerializeField] private GameObject tutorialPgTwo;
    private int curPg = 0;

    public void Play()
    {
        SceneManager.LoadScene(playSceneName);
    }

    public void OpenTutorial()
    {
        tutorialObj.SetActive(true);
        curPg = 0;
        tutorialPgOne.SetActive(true);
    }

    public void NextPageTutorial()
    {
        if (curPg == 0)
            curPg++;
        else
            curPg = 0;
        
        if(curPg == 0)
        {
            tutorialPgOne.SetActive(true);
            tutorialPgTwo.SetActive(false);
        }
        else
        {
            tutorialPgTwo.SetActive(true);
            tutorialPgOne.SetActive(false);
        }
    }

    public void Resume()
    {
        tutorialObj.SetActive(false);
        tutorialPgOne.SetActive(false);
        tutorialPgTwo.SetActive(false);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
