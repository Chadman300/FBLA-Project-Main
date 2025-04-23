using MoreMountains.Feedbacks;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public bool canBePickedup = true;
    public Item item;
    public MMF_Player grabFeedback;

    public GameObject[] currentModel;

    private void Start()
    {
        currentModel = new GameObject[item.physicalModel.Length];
    }

    public void OnPickup()
    {
        for(int i = 0; i < currentModel.Length; i++)
        {
            if (item.physicalModel[i] != null && currentModel[i] == null)
            {
                //instantiate & set parrent
                currentModel[i] = Instantiate(item.physicalModel[i]);

                var selectedParent = GameObject.Find(item.modelParentName[i]);

                //make sure parrents not null
                if (selectedParent == null)
                {
                    Debug.LogError($"Could not find parent named: {item.modelParentName} !");
                    return;
                }

                Debug.Log(selectedParent);
                currentModel[i].transform.SetParent(selectedParent.transform);

                //set transfrom
                currentModel[i].transform.localPosition = item.physicalModelPos[i];
                currentModel[i].transform.localRotation = Quaternion.Euler(item.physicalModelRot[i]);
                currentModel[i].transform.localScale = item.physicalModelScale[i];
            }
        }
    }
}
