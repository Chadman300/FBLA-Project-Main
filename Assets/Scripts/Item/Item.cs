using UnityEngine;
using UnityEngine.UIElements;


[CreateAssetMenu(fileName = "Item", menuName = "ItemManager/Item")]
public class Item : ScriptableObject
{
    [Header("Text")]
    public string itemName;
    public string itemDescription;
    public Color nameColor;
    public Color descriptionColor;

    [Header("Appearance")]
    public GameObject physicalModel;
    public string modelParentName;
    public Vector3 physicalModelPos;
    public Vector3 physicalModelRot;
    public Vector3 physicalModelScale;
    [Space]
    public Image spriteModel;

    [Header("Values")]
    public int sellPrice = 1;

    public float luck = 0f;
    public float moveSpeed = 0f;
    public float jumpForce = 0f;
    public float lungeForce = 0f;
    public float regenSpeed = 0f;
}
