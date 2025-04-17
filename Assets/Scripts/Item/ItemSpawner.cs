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
            var curConroller = item.GetComponent<ItemController>();

            float adjustedWeight = curConroller.item.itemRarity * (1f + playerValues.luck);
            adjustedWeights.Add(adjustedWeight);
            totalWeight += adjustedWeight;
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
}
