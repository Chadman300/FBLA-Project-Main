using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class RagdollValuesController : MonoBehaviour
{
    [Header("items")]
    public List<ItemController> items = new List<ItemController>();

    [Header("Refs")]
    [SerializeField] private AdvancedRagdollController playerController;

    [Header("Currency Values")]
    public int money = 0;
    public int score = 0;

    [Header("Multiplyer Values")]
    public float luck = 1f;
    public float moveSpeed = 1f;
    public float jumpForce = 1f;
    public float lungeForce = 1f;
    public float regenSpeed = 1f;
    public float maxHealth = 1f;

    //start values
    private float pStartMoveSpeed;
    private float pStartJumpForce;
    private float pStartLungeForce;
    private float pStartRegenSpeed;
    private float pStartMaxHealth;

    private void Start()
    {
        //set start player vals
        pStartMoveSpeed = playerController.speed;
        pStartJumpForce = playerController.jumpForce;
        pStartLungeForce = playerController.lungeForce;
        pStartMaxHealth = playerController.maxHealth;

        //update current item values
        foreach (ItemController item in items)
        {
            AddOrRemoveValues(item);
        }
    }

    private void Update()
    {
        playerController.speed      = pStartMoveSpeed  * moveSpeed;
        playerController.jumpForce  = pStartJumpForce  * jumpForce;
        playerController.lungeForce = pStartLungeForce * lungeForce;
        playerController.maxHealth  = pStartMaxHealth  * maxHealth;
    }

    public void AddItem(ItemController item)
    {
        OnItemsChange();

        //make sure item isnt already there
        for (int i = 0;  i < items.Count; i++)
        {
            if (item.name == items[i].name)
            {
                //sell item
                money += item.item.sellPrice;
                return;
            }
        }

        item.OnPickup();
        AddOrRemoveValues(item);
        items.Add(item);
    }

    public void AddOrRemoveValues(ItemController item, bool isAdd = true)
    {
        int addSubMul;
        if (isAdd)
            addSubMul = 1;
        else
            addSubMul = -1;

        luck += item.item.luck * addSubMul;
        moveSpeed += item.item.moveSpeed * addSubMul;
        jumpForce += item.item.jumpForce * addSubMul ;
        lungeForce += item.item.lungeForce * addSubMul;
        regenSpeed += item.item.regenSpeed * addSubMul;
        maxHealth += item.item.maxHealth * addSubMul;
    }

    public void RemoveItem(ItemController item)
    {
        OnItemsChange();

        for (int i = 0; i < items.Count; i++)
        {
            if (item.name == items[i].name)
            {
                items.Remove(items[i]);

                AddOrRemoveValues(items[i], false);
                Destroy(item.currentModel);

                return;
            }      
        }
    }

    private void OnItemsChange()
    {
        //tell player gun to do OnItemsChange
        if(playerController.leftHandHasGun && playerController.leftHandItemObj != null)
        {
            playerController.leftHandItemObj.GetComponent<GunController>().OnItemsChange(items);
        }

        if (playerController.rightHandHasGun && playerController.rightHandItemObj != null)
        {
            playerController.rightHandItemObj.GetComponent<GunController>().OnItemsChange(items);
        }
    }
}
