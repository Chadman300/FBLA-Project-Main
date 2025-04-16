using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using System.Collections;

public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private GameObject robotModel;
    [SerializeField] private int numberOfRobotModels;
    [SerializeField] private float modelForwardOffset = 4.5f;
    [SerializeField] private float conveyorSpeed = 10f;
    [SerializeField] private BoxCollider frontConveyorCollider;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private LayerMask robotLayer;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float modelForwardMove = 4.5f;
    [SerializeField] private float timeBetweenMoves = 0.2f;

    private List<GameObject> robots = new List<GameObject>();

    private void Start()
    {
        Vector3 currentPos = spawnPoint.transform.position;
        StartCoroutine(MoveRobots());

        for (int i = 0; i < numberOfRobotModels; i++)
        {
            currentPos.z += modelForwardOffset;
            var curModel = Instantiate(robotModel);
            curModel.transform.position = currentPos; 
            curModel.transform.SetParent(transform); 
            robots.Add(curModel);
        }
    }

    private void Update()
    {
        RobotCol(frontConveyorCollider.transform.position, frontConveyorCollider);
    }

    private void FixedUpdate()
    {
    }

    private IEnumerator MoveRobots()
    {  
        foreach (var robot in robots)
        {
            var smoothPos = Vector3.Lerp(robot.transform.position,
                new Vector3(robot.transform.position.x, robot.transform.position.y, robot.transform.position.z + (modelForwardMove)),
                smoothSpeed * Time.deltaTime);

            robot.transform.position = smoothPos;
        }
        Debug.Log("test:");

        yield return new WaitForSeconds(timeBetweenMoves);

        StartCoroutine(MoveRobots());
    }

    private void RobotCol(Vector3 pos, BoxCollider prefabCollider)
    {
        // Get the bounds of the prefab
        Vector3 halfExtents = prefabCollider.size / 2;

        // Perform an OverlapBox check
        Collider[] collisions = Physics.OverlapBox(pos + prefabCollider.center, halfExtents, Quaternion.identity, robotLayer);
        foreach (var col in collisions)
        {
            col.transform.position = spawnPoint.transform.position + new Vector3(0,0, modelForwardOffset);
            Debug.Log(col);
        }
    }
}
