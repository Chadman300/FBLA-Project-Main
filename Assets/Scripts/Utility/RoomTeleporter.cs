using UnityEngine;

public class RoomTeleporter : MonoBehaviour
{
    [Header("Teleporter Parameters")]
    [SerializeField] private Transform teleportPoint;
    [SerializeField] private AdvancedRoomController nextRoom;
    [SerializeField] private AdvancedRoomController currentRoom;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<AdvancedRagdollController>(out var playerController) && currentRoom.roomComplete)
        {
            //Change Position
            playerController.hipsRb.transform.position = teleportPoint.position;
            playerController.curRoom = nextRoom;

            if ((playerController.rightHandHasGun || playerController.rightHandHasItem) && playerController.rightHandItemObj != null)
            {
                playerController.rightHandItemObj.transform.position = teleportPoint.position;
            }

            if ((playerController.leftHandHasGun || playerController.leftHandHasItem) && playerController.leftHandItemObj != null)
            {
                playerController.leftHandItemObj.transform.position = teleportPoint.position;
            }


            //Feedback
            playerController.teleportFeedback?.PlayFeedbacks();

            //do stuff for other room and cur room
            if (nextRoom != null)
            {
                nextRoom.OnRoomStart();
            }

            if (currentRoom != null)
            {
                currentRoom.OnRoomStop();
            }

            Debug.Log("Teleported");
        }
    }
}
