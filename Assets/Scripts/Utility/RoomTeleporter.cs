using UnityEngine;

public class RoomTeleporter : MonoBehaviour
{
    [Header("Teleporter Parameters")]
    [SerializeField] private Transform teleportPoint;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent<AdvancedRagdollController>(out var playerController))
        {
            //Change Position
            playerController.hipsRb.transform.position = teleportPoint.position;

            //Feedback
            playerController.teleportFeedback?.PlayFeedbacks();

            Debug.Log("Teleported");
        }
    }
}
