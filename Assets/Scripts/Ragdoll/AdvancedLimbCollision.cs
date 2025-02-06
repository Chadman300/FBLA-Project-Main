using UnityEngine;

public class AdvancedLimbCollision : MonoBehaviour
{
    [Header("Utlility Parameters")]
    [SerializeField] private AdvancedRagdollController controller;
    [SerializeField] private bool canControllGrounded = false;

    [Header("Attack Parameters")]
    [SerializeField] private bool canAttack = true;
    [SerializeField] private bool isRightHand = false;
    [SerializeField] private bool isLeftHand = false;

    private void Start()
    {
        controller = GameObject.FindAnyObjectByType<AdvancedRagdollController>().GetComponent<AdvancedRagdollController>();
    }

    private void OnCollisionStay(Collision collision)
    {
        //make sure its ground layer
        if (canControllGrounded && CheckInLayerMask(collision.gameObject, controller.whatIsGround))
        {
            controller.isGrounded = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //get dmg
        var damage = controller.limbAttackDamage * (controller.hipsRb.linearVelocity.magnitude / controller.limbVelocityDividend);

        //punching
        if (isRightHand && controller.rightHandUp && controller.rightHandItemObj == null)
        {
            damage = controller.fistDamageMultiplyer * controller.limbAttackDamage;
            Debug.Log("PunchR");
        }

        if (isLeftHand && controller.leftHandUp && controller.leftHandItemObj == null)
        {
            damage = controller.fistDamageMultiplyer * controller.limbAttackDamage;
            Debug.Log("PunchL");
        }

        //allow for punching
        if (canAttack && controller.canLimbAttack && damage >= controller.limbDamageThreshold)
        {
            EnemyController enemyController;
            EnemyRagdollController enemyRagdollController;

            //normal
            if (collision.gameObject.TryGetComponent<EnemyController>(out enemyController))
            {
                StartCoroutine(controller.LimbDelay());
                enemyController.ApplyDamage(damage);
                //Debug.Log(damage);
            }
            //ragdoll
            else if (collision.gameObject.TryGetComponent<EnemyRagdollController>(out enemyRagdollController))
            {
                StartCoroutine(controller.LimbDelay());
                enemyRagdollController.ApplyDamage(damage);
                //Debug.Log(damage);
            }
        }
    }

    private bool CheckInLayerMask(GameObject obj, LayerMask layerMask)
    {
        return (layerMask.value & (1 << obj.layer)) != 0;
    }
}
