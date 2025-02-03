using UnityEngine;

public class DebugLaunch : MonoBehaviour
{
    [SerializeField] private float launchForce = 10f;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.V))
        {
            var rb = GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * launchForce, ForceMode.Impulse);
        }
    }
}
