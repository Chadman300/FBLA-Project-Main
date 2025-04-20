using MoreMountains.Feedbacks;
using UnityEngine;

public class AdvancedRoomController : MonoBehaviour
{
    public bool hasRoomStarted = false;
    public int completePrice = 100;
    [Tooltip("money gained on complete")]

    public GameObject[] enimies;
    public GameObject[] items;

    [Space]
    public GameObject[] doorPins;
    public Material[] doorPinColors;
    [Tooltip("first is active second is inactive")]

    public bool roomComplete = false;

    [SerializeField] private MMF_Player roomCompleteFeedback;

    public void OnEnable()
    {
        //freeze enimies
        foreach (var enemy in enimies)
        {
            if (enemy == null)
                continue;

            //ragdol
            if (enemy.TryGetComponent<EnemyRagdollController>(out var enemyRagdollController))
            {
                enemyRagdollController.enabled = false;
            }

            //flying
            else if (enemy.TryGetComponent<FlyingEnemy>(out var enemyFlyingController))
            {
                enemyFlyingController.enabled = false;
            }
        }
    }

    public void OnRoomStart()
    {
        //unfreeze enemies
        foreach (var enemy in enimies)
        {
            if (enemy == null)
                continue;

            //ragdol
            if (enemy.TryGetComponent<EnemyRagdollController>(out var enemyRagdollController))
            {
                enemyRagdollController.enabled = true;
            }

            //flying
            else if (enemy.TryGetComponent<FlyingEnemy>(out var enemyFlyingController))
            {
                enemyFlyingController.enabled = true;
            }
        }

        hasRoomStarted = true;
    }

    public void OnRoomStop()
    {
        //kill enemys
        foreach (var enemy in enimies)
        {
            if(enemy == null) 
                continue;

            //ragdol
            if(enemy.TryGetComponent<EnemyRagdollController>(out var enemyRagdollController))
            {
                enemyRagdollController.KillEnemy();
            }

            //flying
            else if (enemy.TryGetComponent<FlyingEnemy>(out var enemyFlyingController))
            {
                enemyFlyingController.KillEnemy();
            }
        }

        //items
        /*
        foreach (var item in items)
        {
            //destroy
            if (item == null)
                continue;

            Destroy(item.gameObject);
        }
        */

        hasRoomStarted = false;
    }

    private void Update()
    {
        if(roomComplete == false)
        {
            foreach(var pin in doorPins)
            {
                pin.GetComponent<Renderer>().material = doorPinColors[1];
            }

            bool allDead = true;
            foreach (var enemy in enimies)
            {
                if (enemy != null)
                    allDead = false;
            }

            roomComplete = allDead;
            if(allDead == true && enimies.Length > 0)
            {
                roomCompleteFeedback?.PlayFeedbacks();

                //addMoney
                var playerValues = FindAnyObjectByType<RagdollValuesController>().money += completePrice; 
            }
        }
        else
        {
            foreach (var pin in doorPins)
            {
                pin.GetComponent<Renderer>().material = doorPinColors[0];
            }
        }
    }
}
