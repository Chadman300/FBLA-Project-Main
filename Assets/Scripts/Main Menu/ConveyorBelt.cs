using System.Collections.Generic;
using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private GameObject robotModel;
    [SerializeField] private int numberOfRobotModels;
    [SerializeField] private float modelForwardOffset = 4.5f;
    [SerializeField] private float conveyorSpeed = 10f;
    [SerializeField] private BoxCollider backConveyorCollider;
    [SerializeField] private BoxCollider frontConveyorCollider;

    private List<GameObject> robots = new List<GameObject>();

    private void Start()
    {
        Vector3 currentPos = backConveyorCollider.transform.position;

        for(int i = 0; i < numberOfRobotModels; i++)
        {
            currentPos.z += modelForwardOffset;
            var curModel = Instantiate(robotModel);
            robotModel.transform.position = currentPos; 
            robots.Add(curModel);
        }
    }
}
