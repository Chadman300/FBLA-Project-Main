using MoreMountains.Feedbacks;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    public bool isAvailable = true;
    [SerializeField] private Animator animator;
    [SerializeField] private RagdollValuesController values;
    [SerializeField] private UIManager UIManager;

    [SerializeField] private GameObject[] objectsToDestroy;
    [SerializeField] private GameObject[] objectsToEnable;

    [SerializeField] private bool releasesPrisoner = false;
    [SerializeField] private Animator prisonerAnim;
    [SerializeField] private ButtonController otherButton;

    [SerializeField] private MMF_Player saveFeedback;
    [SerializeField] private MMF_Player killFeedback;

    [SerializeField] private Item saveItem;
    [SerializeField] private Item killItem;
    private void Start()
    {
        values = FindAnyObjectByType<RagdollValuesController>();
        values = FindAnyObjectByType<RagdollValuesController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<AdvancedRagdollController>(out var controller))
        {
            Debug.Log("Down");
            ButtonDown();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<AdvancedRagdollController>(out var controller))
        {
            Debug.Log("Up");
            ButtonUp();
        }
    }

    private void ButtonDown()
    {
        animator.SetBool("ButtonDown", true);
        animator.speed = 1.0f;

        if (!isAvailable)
            return;

        isAvailable = false;
        otherButton.isAvailable = false;

        foreach(var obj in objectsToDestroy)
        {
            if(obj != null)
            {
                Destroy(obj);
            }
        }

        foreach (var obj in objectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        if (releasesPrisoner)
        {
            values.savedPrisoner = true;
            prisonerAnim.SetBool("Thriller", true);
            saveFeedback?.PlayFeedbacks();
            UIManager.AddToQueue(saveItem);
            values.luck += 0.5f;
            //UIManager.AddToQueue("⚠ Cannot Drop Whilst Reloading", )
        }
        else
        {
            UIManager.AddToQueue(killItem);
            killFeedback?.PlayFeedbacks();
            values.luck -= 0.5f;
        }
    }

    public void ButtonNuteral()
    {
        animator.speed = 0.0f;
        Debug.Log("Nuetral");
    }

    private void ButtonUp()
    {
        animator.SetBool("ButtonDown", false);
        animator.speed = 1.0f;
    }
}
