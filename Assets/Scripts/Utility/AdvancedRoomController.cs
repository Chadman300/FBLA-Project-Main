using UnityEngine;

public class AdvancedRoomController : MonoBehaviour
{
    public bool hasRoomStarted = false;

    public GameObject[] enimies;
    public GameObject[] items;

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
}
