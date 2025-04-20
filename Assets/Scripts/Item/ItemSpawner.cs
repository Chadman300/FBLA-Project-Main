using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Items")]
    public GameObject[] itemsPrefabPool;

    [Header("Refs")]
    [SerializeField] private RagdollValuesController playerValues;

    private void Awake()
    {
        playerValues = FindAnyObjectByType<RagdollValuesController>();

        var newItem = Instantiate(GetRandomItem());
        newItem.transform.parent = transform;
        newItem.transform.position = transform.position;
    }

    public GameObject GetRandomItem()
    {
        List<float> adjustedWeights = new List<float>();
        float totalWeight = 0f;

        foreach (var item in itemsPrefabPool)
        {
            if (item.TryGetComponent<MeeleWeapon>(out var meeleController))
            {
                totalWeight += CalcWeight(meeleController.itemRarity, adjustedWeights);
            }
            else if (item.TryGetComponent<GunController>(out var gunController))
            {
                totalWeight += CalcWeight(gunController.itemRarity, adjustedWeights);
            }
            else 
            {
                var itemController = item.GetComponent<ItemController>();
                totalWeight += CalcWeight(itemController.item.itemRarity, adjustedWeights);
            }
        }

        float randomValue = Random.Range(0f, totalWeight);
        float runningTotal = 0f;

        for (int i = 0; i < itemsPrefabPool.Length; i++)
        {
            runningTotal += adjustedWeights[i];
            if (randomValue <= runningTotal)
            {
                return itemsPrefabPool[i];
            }
        }

        return itemsPrefabPool[0]; // fallback
    }

    private float CalcWeight(float rarity, List<float> adjustedWeights)
    {
        float adjustedWeight = rarity * (1f + playerValues.luck);
        adjustedWeights.Add(adjustedWeight);
        return adjustedWeight;
    }
}
