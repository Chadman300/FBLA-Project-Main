using UnityEngine;

public class BuyableItem : MonoBehaviour
{
    [Header("Values")]
    public int price = 100;
    public bool isItem = false;
    public bool hasBeenBought = false;

    [Header("Refs")]
    [SerializeField] private GameObject container;
    [SerializeField] private AdvancedRagdollController playerController;

    private void Awake()
    {
        playerController.ragdollValues = FindAnyObjectByType<RagdollValuesController>();
    }

    public void TryBuyItem()
    {
        if(playerController.ragdollValues.money >= price)
        {
            playerController.ragdollValues.money -= price;
            ForceBuyItem();

            //feedbacks
            playerController.buyFeedback?.PlayFeedbacks();
        }
        else
        {
            Debug.Log("Not Enough Money");
        }
    }

    public void ForceBuyItem()
    {
        hasBeenBought = true;

        //buy
        Destroy(container);

        //item
        if (gameObject.TryGetComponent<ItemController>(out var itemController))
        {
            itemController.canBePickedup = true;
        }
        //meele weapon
        else if (gameObject.TryGetComponent<MeeleWeapon>(out var meeleWeapon))
        {
            meeleWeapon.canBePickedup = true;
        }
        //gun
        else if (gameObject.TryGetComponent<GunController>(out var gunController))
        {
            gunController.canBePickedup = true;
        }
        else
        {
            Debug.Log("No Item Found");
        }

        Destroy(this);
    }
}
