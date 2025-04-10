using MoreMountains.Feedbacks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class FlyingEnemy : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private Transform player;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float lungeForce = 3000;
    [SerializeField] private float lungeStunTime = 0.35f;

    [Header("Physics Parameters")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;
    [SerializeField] private Rigidbody rb;

    [Header("AI Parameters")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private AdvancedRagdollController playerController;
    public AIStates currentState;
    [SerializeField] private float walkPointRange;
    [SerializeField] private float chaseRange, lungeRange, attackRange;
    [SerializeField] private int lungeChance;

    private Vector3 walkPoint;
    private bool walkPointSet;
    private bool attackAvailable, isGoingToLunge = true;
    private bool playerInSightRange, playerInLungeRange, playerInAttackRange;
    private bool getNewLungeRandom = true;

    [Header("Attack Parameters")]
    public bool canAttack = true;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attckSpinSpeed = 360;
    public float limbAttackDamage = 10f;
    public float limbDamageThreshold = 5f;
    [Tooltip("Minimum attack damage that will hurt enemy")]
    public float damageAttackDelay = 0.1f;
    [Tooltip("Duration after limb attack were you cannot deal limb damage")]
    [Range(0, 10)] public float limbVelocityDividend = 1f;

    [Header("Health Parameters")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float timeBeforeRegenStarts = 3f;
    [SerializeField] private float healthValueIncrement = 3;
    [Tooltip("Increments currentHealth by healthValueIncrement every time of this value")]
    [SerializeField] private float healthTimeIncrement = 0.1f;
    [SerializeField] private Slider HealthSlider;
    public float currentHealth;
    private Coroutine regeneratingHealth;
    public static Action<float> OnTakeDamage;
    public static Action<float> OnDamage;
    public static Action<float> OnHeal;
    public bool isDead = false;

    [Header("Stun Parameters")]
    [SerializeField] private bool canGetStunned = true;
    [Range(0, 2)][SerializeField] private float stunTimeMultiplyer;
    [Tooltip("Multiplies stun time when damaged (stunTimeMultiplyer * damage = stunTime)")]

    [Header("Feedbacks Parameters")]
    public MMF_Player damageFeedbacks;
    public MMF_Player deathFeedbacks;

    [Header("Animation")]
    [SerializeField] private bool canAnimate = true;
    [SerializeField] private Animator anim;

    private bool isStunned = false;

    private void OnEnable()
    {
        attackAvailable = true;
        anim.applyRootMotion = false;

        OnTakeDamage += ApplyDamage;
        if (HealthSlider != null)
        {
            HealthSlider.maxValue = maxHealth;
            HealthSlider.value = currentHealth;
        }
    }

    private void OnDisable()
    {
        OnTakeDamage -= ApplyDamage;
    }

    private void Awake()
    {
        currentHealth = maxHealth;
        ApplyDamage(0); // to reset health bar

        playerController = GameObject.FindAnyObjectByType<AdvancedRagdollController>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
    }

    private void Update()
    {
        if (isDead || UIManager.isPaused)
            return;


        if(canAnimate)
          HandleAnimation();

        GetAIStates();
        HandleAIStates();
    }
    private void FixedUpdate()
    {
        if (isDead || UIManager.isPaused)
            return;
    }

    private void GetAIStates()
    {
        //check in attackrange
        playerInSightRange = Physics.CheckSphere(transform.position, chaseRange, whatIsPlayer);
        playerInLungeRange = Physics.CheckSphere(transform.position, lungeRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) currentState = AIStates.Patrolling;
        if (playerInSightRange && !playerInAttackRange) currentState = AIStates.Chasing;
        if (playerInSightRange && playerInAttackRange) currentState = AIStates.Attacking;
    }

    private void HandleAnimation()
    {
        //Patrolling
        if (currentState == AIStates.Patrolling)
        {
            anim.SetBool("Patrolling", true);
        }
        else
        {
            anim.SetBool("Patrolling", false);
        }

        //Chasing
        if (currentState == AIStates.Chasing && !isStunned)
        {
            anim.SetBool("Chasing", true);
        }
        else
        {
            anim.SetBool("Chasing", false);
        }

        //Attacking
        if (currentState == AIStates.Attacking)
        {
            anim.SetBool("Attacking", true);
        }
        else
        {
            anim.SetBool("Attacking", false);
        }
    }

    private void LookAtPoint(Transform point)
    {
        var oldRot = transform.rotation;
        //transform.LookAt(point);
        rb.MoveRotation(Quaternion.LookRotation(point.position - transform.position));
        transform.rotation = new Quaternion(oldRot.x, transform.rotation.y, oldRot.z, transform.rotation.w);
    }

    private void Lunge()
    {
        rb.isKinematic = false;

        //ragdoll and disable
        agent.enabled = false;
        StartCoroutine(Stun(lungeStunTime));

        //Lunging
        rb.linearVelocity += rb.transform.forward * lungeForce * Time.deltaTime;
        //hipsRb.AddForce(hipsRb.transform.forward * lungeForce * Time.deltaTime, ForceMode.Impulse);
    }

    private void HandleAIStates()
    {
        //Lunging
        if (playerInLungeRange)
        {
            if (isGoingToLunge)
            {
                Debug.Log("Lunged!");

                Lunge();

                isGoingToLunge = false;
            }

            getNewLungeRandom = true;
        }
        else if (getNewLungeRandom)
        {
            getNewLungeRandom = false;
            var rand = UnityEngine.Random.Range(0, lungeChance);
            if (rand == lungeChance - 1)
            {
                isGoingToLunge = true;
            }
        }

        //ragdoll while in air and dont let agent move it
        if (isStunned)
        {
            //RagDoll(true);
            //agent.SetDestination(transform.position);
            rb.isKinematic = false;
            agent.enabled = false;
            canAnimate = false;
            return;
        }
        else
        {
            agent.enabled = true;
            canAnimate = true;
        }

        //Patrolling
        if (currentState == AIStates.Patrolling)
        {

            //get point
            if (!walkPointSet) SearchWalkPoint();

            //make agent go to point
            if (walkPointSet)
                agent.SetDestination(walkPoint);

            //make agent look at point
            transform.LookAt(walkPoint);

            //check if we have arrived
            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            if (distanceToWalkPoint.magnitude < 1f)
                walkPointSet = false;
        }

        //Chasing
        else if (currentState == AIStates.Chasing)
        {

            //make agent go to player
            agent.SetDestination(playerController.transform.position);
            LookAtPoint(playerController.transform);
        }

        //Attacking
        else if (currentState == AIStates.Attacking)
        {

            //make it stop moving
            //agent.SetDestination(transform.position);

            //LookAtPoint(playerController.transform);

            //attack
            if (attackAvailable)
            {
                Quaternion deltaRotation = Quaternion.Euler(0, attckSpinSpeed * Time.fixedDeltaTime, 0);
                rb.MoveRotation(rb.rotation * deltaRotation);

                StartCoroutine(ResetAttack(attackCooldown));
            }
        }
    }

    private IEnumerator Stun(float stunTime)
    {
        //make player ragdoll untill stun times over
        isStunned = true;
        rb.isKinematic = false;
        rb.useGravity = true;

        yield return new WaitForSeconds(stunTime);

        isStunned = false;
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private IEnumerator ResetAttack(float resetTime)
    {
        attackAvailable = false;
        yield return new WaitForSeconds(resetTime);
        attackAvailable = true;
    }

    private void SearchWalkPoint()
    {
        //calc random point in range
        float randZ = UnityEngine.Random.Range(-walkPointRange, walkPointRange);
        float randX = UnityEngine.Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randX, transform.position.y, transform.position.z + randZ);

        //make sure theres actually ground below this point
        if (Physics.Raycast(walkPoint + new Vector3(0, 0f, 0), -transform.up, 3f, whatIsGround))
            walkPointSet = true;
    }

    public void ApplyDamage(float damage)
    {
        currentHealth -= damage;
        OnDamage?.Invoke(currentHealth);

        //effects
        damageFeedbacks?.PlayFeedbacks(damageFeedbacks.transform.position, Mathf.RoundToInt(damage));


        if (currentHealth <= 0)
            KillEnemy();
        else if (regeneratingHealth != null)
            StopCoroutine(regeneratingHealth);

        regeneratingHealth = StartCoroutine(RegenerateHealth());
    }

    public IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(timeBeforeRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(healthTimeIncrement);

        while (currentHealth < maxHealth && !isDead)
        {
            currentHealth += healthValueIncrement;

            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            OnHeal?.Invoke(currentHealth);

            //effects
            //regenerateHealthFeedBack?.PlayFeedbacks();

            yield return timeToWait;
        }

        regeneratingHealth = null;
    }
    private void KillEnemy()
    {
        currentHealth = 0;

        if (regeneratingHealth != null)
            StopCoroutine(regeneratingHealth);

        //Stun
        isDead = true;
        agent.enabled = false;
        anim.enabled = false;
        canAnimate = false;
        rb.isKinematic = false;
        rb.useGravity = true;

        //effects
        deathFeedbacks?.PlayFeedbacks();
        Debug.Log("dead", gameObject);
        isDead = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //get dmg
        var damage = limbAttackDamage * (rb.linearVelocity.magnitude / limbVelocityDividend);

        if(attackAvailable && currentState == AIStates.Attacking)
        {
            damage += limbAttackDamage / 5;
        }

        //allow for punching
        if (canAttack && damage >= limbDamageThreshold)
        {
            AdvancedLimbCollision playerLimb;
            if (collision.gameObject.TryGetComponent<AdvancedLimbCollision>(out playerLimb))
            {
                StartCoroutine(AttackDelay());
                playerLimb.controller.ApplyDamage(damage);
                Debug.Log(damage);
            }
        }
    }

    public IEnumerator AttackDelay()
    {
        canAttack = false;
        yield return new WaitForSeconds(damageAttackDelay);
        canAttack = true;
    }
}
