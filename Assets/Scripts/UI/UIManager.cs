using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Debug Menu UI")]
    [SerializeField] private GameObject debugMenu;

    [Header("Settings UI")]
    [SerializeField] private SettingsManager settingsManager;
    [SerializeField] private GameObject settingsMenu;

    [Header("PauseMenu UI")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private AdvancedRagdollSettings settings;
    public static bool isPaused = false;

    [Header("DeadMenu UI")]
    [SerializeField] private GameObject deadMenu;
    [SerializeField] private GameObject loadingMenu;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMP_Text finialScoreText;

    [Header("Game Complete UI")]
    [SerializeField] private GameObject gameCompleteMenu;
    [SerializeField] private TMP_Text finialScoreTextComplete;

    [Header("AlertSystem UI")]
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private TMP_Text popupSubText;
    [SerializeField] private GameObject popupWindow;
    [SerializeField] private Animator popupAnimator;
    [SerializeField] private string popupAnimationName = "AlertPopupAnim";
    [SerializeField] private int QueueMax = 4;
    private Queue<(string, string)> popupQueue; //make it different type for more detailed popups, you can add different types, titles, descriptions etc
    private Coroutine queueChecker;
    private WobblyText wobblyPopupText;

    [Header("Ammo UI")]
    [SerializeField] private AdvancedRagdollController playerController;
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Health UI")]
    [SerializeField] private MMProgressBar healthBar;
    [SerializeField] private Transform barTransform;
    [SerializeField] private float barSizeMultiplyer = 0.5f;
    private float defaultHealthBarSize;

    [SerializeField] private GameObject[] destroyOnLoads;

    [Header("Player Info UI")]
    [SerializeField] private RagdollValuesController ragdollValues;
    [SerializeField] private TMP_Text maxHealthText;
    [SerializeField] private TMP_Text luckText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private TMP_Text dmgText;
    [SerializeField] private TMP_Text defenseTextText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text currencyText;

    private SaveValues saveValues;

    private void OnEnable()
    {
        ragdollValues = FindAnyObjectByType<RagdollValuesController>();

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
        pauseMenu.SetActive(false);

        defaultHealthBarSize = barTransform.localScale.x;
        popupWindow.SetActive(false);
        popupQueue = new Queue<(string, string)>();
        pauseMenu.SetActive(false);
        healthBar.UpdateBar(playerController.maxHealth, 0f, playerController.maxHealth);

        wobblyPopupText = popupText.GetComponent<WobblyText>();

        //saved vals
        var newObj = Instantiate(new GameObject());
        DontDestroyOnLoad(newObj);

        var oldSaved = FindAnyObjectByType<SaveValues>();
        if(oldSaved != null)
        {
            playerController.ragdollValues.money = oldSaved.savedMoney;
            playerController.ragdollValues.savedPrisoner = oldSaved.savedPrisonerStatus;
            Destroy(oldSaved.gameObject);
        }

        saveValues = newObj.AddComponent<SaveValues>();
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

        UpdatePlayerStats();
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

        //update bar size
        Vector3 newScale = barTransform.localScale;
        newScale.x = defaultHealthBarSize * (((playerController.ragdollValues.maxHealth - 1) * barSizeMultiplyer) + 1);
        barTransform.localScale = newScale;
    }

    private void UpdateHealth(float currentHealth)
    {
        healthBar.UpdateBar(currentHealth, 0f, playerController.maxHealth);
    }

    public void AddToQueue(Item item)
    {
        Debug.Log("Added To Queue");

        //parameter the same type as queue
        wobblyPopupText.amplitude = item.waveTextAmplitude;
        wobblyPopupText.speed = item.waveTextSpeed;
        wobblyPopupText.waveLength = item.waveTextWaveLength;

        if (popupQueue.Count > 0 ? item.itemName != popupQueue.Last().Item1 : true && popupQueue.Count < QueueMax - 1)
        {
            popupQueue.Enqueue((item.itemName, item.itemDescription));
            if (queueChecker == null)
                queueChecker = StartCoroutine(CheckQueue(item.nameColor, item.descriptionColor));
        }
    }

    private void ShowPopup(string text, Color textColor, string subText, Color subTextColor)
    { //parameter the same type as queue
        popupWindow.SetActive(true);
        popupText.text = text;
        popupText.color = textColor;
        popupSubText.text = subText;
        popupSubText.color = subTextColor;
        popupAnimator.Play(popupAnimationName);
    }

    private IEnumerator CheckQueue(Color textColor, Color subTextColor)
    {
        do
        {
            var texts = popupQueue.Dequeue();
            ShowPopup(texts.Item1, textColor, texts.Item2, subTextColor);
            do
            {
                yield return null;
            } while (!popupAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Idle"));

        } while (popupQueue.Count > 0);
        popupWindow.SetActive(false);
        queueChecker = null;
    }

    private void UpdatePlayerStats()
    {
        maxHealthText.text = $"{ragdollValues.maxHealth * 100} HP";
        luckText.text = $"{ragdollValues.luck} X";
        speedText.text = $"{ragdollValues.moveSpeed} X";
        dmgText.text = $"{ragdollValues.damage} X";
        defenseTextText.text = $"{ragdollValues.defense} X";
        scoreText.text = $"{ragdollValues.score} Pts";
        currencyText.text = $"{ragdollValues.money} $";
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void OpenSettings()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void OpenDebug()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        debugMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        debugMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void PlayerDied()
    {
        ResumeGame();
        isPaused = true;
        Time.timeScale = 0f;
        deadMenu.SetActive(true);
        finialScoreText.text = ragdollValues.score.ToString();
    }

    public void PlayAgain()
    {
        deadMenu.SetActive(false);
        ResumeGame();

        saveValues.savedMoney = playerController.ragdollValues.money;
        saveValues.savedPrisonerStatus = playerController.ragdollValues.savedPrisoner;
        StartCoroutine(LoadSceneASync(SceneManager.GetActiveScene().name));
    }

    public void GameComplete()
    {
        ResumeGame();
        isPaused = true;
        Time.timeScale = 0f;
        gameCompleteMenu.SetActive(true);
        finialScoreTextComplete.text = ragdollValues.score.ToString();
    }

    public void QuitGame()
    {
        Application.Quit();
        ResumeGame();
    }

    IEnumerator LoadSceneASync(string levelToLoad)
    {
        isPaused = true;
        loadingMenu.SetActive(true);
        foreach (var item in destroyOnLoads)
        {
            Destroy(item.gameObject);
        }

        isPaused = false;
        Destroy(gameObject);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);

        while (!loadOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);
            loadingSlider.value = progressValue;
            yield return null;
        }
    }

    public void MainMenu()
    {
        StartCoroutine(LoadSceneASync("Main Menu"));
        deadMenu.SetActive(false);
        ResumeGame();
    }
}
