using MoreMountains.Feedbacks;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public Item item;
    public MMF_Player grabFeedback;

    public GameObject currentModel;

    public void OnPickup()
    {
        if(item.physicalModel != null && currentModel == null)
        {
            //instantiate & set parrent
            currentModel = Instantiate(item.physicalModel);

            var selectedParent = GameObject.Find(item.modelParentName);

            //make sure parrents not null
            if (selectedParent == null)
            {
                Debug.LogError($"Could not find parent named: {item.modelParentName} !");
                return;
            }

            Debug.Log(selectedParent);
            currentModel.transform.SetParent(selectedParent.transform);

            //set transfrom
            currentModel.transform.localPosition = item.physicalModelPos;
            currentModel.transform.localRotation = Quaternion.Euler(item.physicalModelRot);
            currentModel.transform.localScale = item.physicalModelScale;
        }
    }
}
