using MoreMountains.Feedbacks;
using UnityEngine;

public class GoldCoin : MonoBehaviour
{
    public int rarity;
    [SerializeField] private int moneyGained;

    [SerializeField] private RagdollValuesController values;
    [SerializeField] private MMF_Player pickUpFeedBack;

    private void Awake()
    {
        values = FindAnyObjectByType<RagdollValuesController>();
    }


    private void OnTriggerEnter(Collider other)
    {
        values.money += moneyGained;
        pickUpFeedBack?.PlayFeedbacks();
        Destroy(this.gameObject);
    }
}
