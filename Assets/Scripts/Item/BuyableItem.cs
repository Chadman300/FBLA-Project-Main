using UnityEngine;

public class BuyableItem : MonoBehaviour
{
    [Header("Values")]
    public int price = 100;

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
            //buy
            playerController.ragdollValues.money -= price;
            Destroy(container);

            //feedbacks
            playerController.buyFeedback?.PlayFeedbacks();
            
            //item
            if(gameObject.TryGetComponent<ItemController>(out var itemController))
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
        else
        {
            Debug.Log("Not Enough Money");
        }
    }
}
