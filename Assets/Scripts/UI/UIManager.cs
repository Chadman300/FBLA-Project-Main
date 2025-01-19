using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("PauseMenu UI")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private AdvancedRagdollSettings settings;
    public static bool isPaused = false;

    [Header("AlertSystem UI")]
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private GameObject window;
    [SerializeField] private Animator popupAnimator;
    [SerializeField] private int QueueMax = 4;
    private Queue<string> popupQueue; //make it different type for more detailed popups, you can add different types, titles, descriptions etc
    private Coroutine queueChecker;

    [Header("Ammo UI")]
    [SerializeField] private AdvancedRagdollController playerController;
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Health UI")]
    [SerializeField] private TextMeshProUGUI healthText = default;
    [SerializeField] private Slider healthSlider = default;

    private void OnEnable()
    {
        AdvancedRagdollController.OnDamage += UpdateHealth;
        AdvancedRagdollController.OnHeal += UpdateHealth;
    }

    private void OnDisable()
    {
        AdvancedRagdollController.OnDamage -= UpdateHealth;
        AdvancedRagdollController.OnHeal -= UpdateHealth;
    }

    private void Start()
    {
        window.SetActive(false);
        popupQueue = new Queue<string>();
        pauseMenu.SetActive(false);
    }

    private void Update()
    {
        //pause
        if (Input.GetKeyDown(settings.pauseKey))
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    private void FixedUpdate()
    {
        /*
        if (gunSelector.ActiveGun != null && gunSelector.Gun != GunType.noGunFists)
        {
            ammoText.SetText($"{gunSelector.ActiveGun.AmmoConfig.CurrentClipAmmo} / {gunSelector.ActiveGun.AmmoConfig.CurrentAmmo}");
        }
        else
        {
            ammoText.SetText($"");
        }
        */
    }

    private void UpdateHealth(float currentHealth)
    {
        healthText.text = currentHealth.ToString("00");
        healthSlider.value = currentHealth;
    }

    public void AddToQueue(string text, Color textColor)
    {//parameter the same type as queue
        if (popupQueue.Count > 0 ? text != popupQueue.Last() : true && popupQueue.Count < QueueMax - 1)
        {
            popupQueue.Enqueue(text);
            if (queueChecker == null)
                queueChecker = StartCoroutine(CheckQueue(textColor));
        }
    }

    private void ShowPopup(string text, Color textColor)
    { //parameter the same type as queue
        window.SetActive(true);
        popupText.text = text;
        popupText.color = textColor;
        popupAnimator.Play("AlertPopupAnim");
    }

    private IEnumerator CheckQueue(Color textColor)
    {
        do
        {
            ShowPopup(popupQueue.Dequeue(), textColor);
            do
            {
                yield return null;
            } while (!popupAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"));

        } while (popupQueue.Count > 0);
        window.SetActive(false);
        queueChecker = null;
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
