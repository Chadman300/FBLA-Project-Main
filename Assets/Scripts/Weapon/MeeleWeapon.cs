using System.Collections;
using UnityEngine;

public class MeeleWeapon : MonoBehaviour
{
    public bool canBePickedup = true;

    [Header("Values")]
    public int itemRarity = 1;
    public int sellPrice = 1;

    [Header("Attack Parameters")]
    public Vector3 pickRotOffset = Vector3.zero;
    public Vector3 pickPosOffset = Vector3.zero;
    [Space]
    public float attackDamage = 10f;
    public Vector2 damageThreshold = new Vector2(5f, 100f);
    [Tooltip("Minimum attack damage that will hurt enemy")]
    [Range(0, 10)] public float velocityDividend = 1f;
    public float attackDelay = 0.1f;
    [Tooltip("Duration after limb attack were you cannot deal limb damage")]
    public bool canAttack = false;
    public bool isEquipt = false;

    private Rigidbody rb;
    private AdvancedRagdollController advancedRagdollController;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        advancedRagdollController = FindAnyObjectByType<AdvancedRagdollController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!gameObject.active == true || rb == null)
            return;

        //get dmg
        var damage = attackDamage * (rb.linearVelocity.magnitude / velocityDividend);

        //clamp dmg
        if(damage > damageThreshold.y)
        {
            damage = damageThreshold.y;
        }

        damage *= advancedRagdollController.damageMultiplier;

        //allow for punching
        if (canAttack && damage >= damageThreshold.x)
        {
            //enemy
            if (collision.gameObject.TryGetComponent<EnemyController>(out EnemyController enemyController))
            {
                StartCoroutine(AttackDelay());
                enemyController.ApplyDamage(damage);
                Debug.Log(damage);
            }

            //enemy ragdoll
            if (collision.gameObject.TryGetComponent<EnemyRagdollLimbCollision>(out EnemyRagdollLimbCollision enemyRagdollController))
            {
                StartCoroutine(AttackDelay());

                //enable shop keeper
                if(enemyRagdollController.controller.isShopKeeper)
                {
                    enemyRagdollController.controller.isShopKeeper = false;
                }

                enemyRagdollController.controller.ApplyDamage(damage);
                Debug.Log(damage);
            }

            //enemy flying
            if (collision.gameObject.TryGetComponent<FlyingEnemy>(out FlyingEnemy flyingController))
            {
                StartCoroutine(AttackDelay());
                flyingController.ApplyDamage(damage);
                StartCoroutine(flyingController.Stun(damage / 10));
                Debug.Log(damage);
            }
        }
    }

    public IEnumerator AttackDelay()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackDelay);
        canAttack = true;
    }
}
