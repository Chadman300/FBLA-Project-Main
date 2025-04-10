using UnityEngine;

public class EnemyRagdollLimbCollision : MonoBehaviour
{
    [Header("Utlility Parameters")]
    public EnemyRagdollController controller;
    [SerializeField] private bool canControllGrounded = false;

    [Header("Attack Parameters")]
    [SerializeField] private bool canAttack = true;

    private void OnCollisionEnter(Collision collision)
    {
        //make sure its ground layer
        if (canControllGrounded && CheckInLayerMask(collision.gameObject, controller.whatIsGround))
        {
            controller.isGrounded = true;
        }

        //get dmg
        var damage = controller.limbAttackDamage * (controller.hipsRb.linearVelocity.magnitude / controller.limbVelocityDividend);

        //allow for punching
        if (canAttack && controller.canLimbAttack && damage >= controller.limbDamageThreshold)
        {
            AdvancedLimbCollision playerLimb;
            if (collision.gameObject.TryGetComponent<AdvancedLimbCollision>(out playerLimb))
            {
                StartCoroutine(controller.LimbDelay());
                playerLimb.controller.ApplyDamage(damage);
                Debug.Log(damage);
            }
        }
    }

    private bool CheckInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) != 0;
    }
}
