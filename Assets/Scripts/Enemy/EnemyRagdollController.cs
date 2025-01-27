using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Controls;
using System;
using UnityEngine.UI;
using MoreMountains.Feedbacks;
using DG.Tweening;
using UnityEngine.AI;
using Unity.Cinemachine;

public enum AIStates
{
    Patrolling,
    Chasing,
    Attacking
}

public class EnemyRagdollController : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private Transform player;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce = 3000;
    [SerializeField] private float lungeForce = 3000;
    [SerializeField] private float lungeStunTime = 0.35f;
    public bool isGrounded = false;

    [Header("AI Parameters")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private AdvancedRagdollController playerController;
    public AIStates currentState;
    [SerializeField] private float walkPointRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float chaseRange, lungeRange, attackRange;
    [SerializeField] private int lungeChance;
    [Tooltip("1 in Lunge Chance")]

    private Vector3 walkPoint;
    private bool walkPointSet;
    private bool attackAvailable = true, isGoingToLunge;
    private bool playerInSightRange, playerInLungeRange, playerInAttackRange;
    private bool getNewLungeRandom = true;

    [Header("Physics Parameters")]
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;
    [SerializeField] private int limbCollisionLayer = 10;
    [SerializeField] private Rigidbody[] rigidbodies;
    [SerializeField] private ConfigurableJoint[] joints;
    [Tooltip("IMPORTANT: joints and animTrans must both have each respective joint in the same order and hips at the end of joints")]
    [SerializeField] private int solverIterations = 6;
    [Tooltip("Higher # the more accurate physics interactions are")]
    [SerializeField] private int velSolverIterations = 1;
    [Tooltip("Higher # the more accurate physics interactions are")]
    [SerializeField] private int maxAngularVelocity = 20;
    [Tooltip("Generally dipicts how fast your player can move")]
    private Quaternion[] jointsInitialStartRot;

    [Header("Balance")]
    public Rigidbody hipsRb;
    public ConfigurableJoint hipJoint;
    [SerializeField] private float uprightTorque = 10000;
    [Tooltip("Defines how much torque percent is applied given the inclination angle percent [0, 1]")]
    [SerializeField] private AnimationCurve uprightTorqueFunction;
    [SerializeField] private float rotationTorque = 500;
    public Vector3 TargetDirection { get; set; }

    [Header("Ragdoll")]
    [SerializeField] private ConfigurableJoint[] legJoints;
    [SerializeField] private float driveStiffness = 90;
    [SerializeField] private float driveStiffnessLegs = 180;
    [Range(1, 15)][SerializeField] private float stiffnessDividend = 4f;
    [Range(1, 15)][SerializeField] private float massDividend = 4f;

    [Header("Animation")]
    [SerializeField] private bool canAnimate = true;
    [SerializeField] private Animator anim;
    [SerializeField] private Transform[] animTransforms;

    [Space(20)]

    [Header("Attack Parameters")]
    public bool canLimbAttack = true;
    public float limbAttackDamage = 10f;
    public float limbDamageThreshold = 5f;
    [Tooltip("Minimum attack damage that will hurt enemy")]
    public float limbDamageAttackDelay = 0.1f;
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

    private bool isStunned = false;

    private void OnEnable()
    {
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

    private void Start()
    {
        //set physics params
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.solverIterations = solverIterations;
            rb.solverVelocityIterations = velSolverIterations;
            rb.maxAngularVelocity = maxAngularVelocity;
        }

        //save start rots
        jointsInitialStartRot = new Quaternion[joints.Length];
        for (int i = 0; i < joints.Length; i++)
        {
            jointsInitialStartRot[i] = joints[i].transform.localRotation;

        }
    }

    private void Update()
    {
        if (isDead || PauseMenu.isPaused)
            return;

        if(canAnimate)
            HandleAnimation();

        GetAIStates();
        HandleAIStates();

        //set animimated rots to joints to animate
        if(canAnimate)
        {
            for (int i = 0; i < joints.Length; i++)
            {
                ConfigurableJointExtensions.SetTargetRotationLocal(joints[i], animTransforms[i].localRotation, jointsInitialStartRot[i]);
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDead || PauseMenu.isPaused)
            return;

        if (!isStunned)
            Balance();
    }

    private void HandleAnimation()
    {
        //Patrolling
        if(currentState == AIStates.Patrolling)
        {
            anim.SetBool("Patrolling", true);
        }
        else
        {
            anim.SetBool("Patrolling", false);
        }

        //Chasing
        if (currentState == AIStates.Chasing)
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

    private void LookAtPoint(Transform point)
    {
        var oldRot = transform.rotation;
        transform.LookAt(point);
        transform.rotation = new Quaternion (oldRot.x, transform.rotation.y, oldRot.z, transform.rotation.w);
    }

    private void Lunge()
    {
        //ragdoll and disable
        agent.enabled = false;
        StartCoroutine(RagdollStun(lungeStunTime));

        //Jumping
        hipsRb.linearVelocity += hipsRb.transform.up * jumpForce * Time.deltaTime;
        //hipsRb.AddForce(hipsRb.transform.up * jumpForce * Time.deltaTime, ForceMode.Impulse);

        //Lunging
        hipsRb.linearVelocity += hipsRb.transform.forward * lungeForce * Time.deltaTime;
        //hipsRb.AddForce(hipsRb.transform.forward * lungeForce * Time.deltaTime, ForceMode.Impulse);
    }

    private void RagDoll(bool ragdoll)
    {
        ConfigurableJoint joint; JointDrive yzDrive; JointDrive xDrive;

        if (ragdoll)
        {
            //ragdoll by destiffining joints
            for (int i = 0; i < joints.Length; i++)
            {
                joint = joints[i];

                yzDrive = joint.angularYZDrive;
                xDrive = joint.angularXDrive;

                yzDrive.positionSpring = driveStiffness / stiffnessDividend;
                xDrive.positionSpring = driveStiffness / stiffnessDividend;

                joint.angularYZDrive = yzDrive;
                joint.angularXDrive = xDrive;

                joint.massScale = 1.6f / massDividend;
            }

            //disable anim
            anim.enabled = false;
        }
        else
        {
            //dont ragdoll
            for (int i = 0; i < joints.Length; i++)
            {
                joint = joints[i];

                yzDrive = joint.angularYZDrive;
                xDrive = joint.angularXDrive;

                yzDrive.positionSpring = driveStiffness;
                xDrive.positionSpring = driveStiffness;

                joint.angularYZDrive = yzDrive;
                joint.angularXDrive = xDrive;

                joint.massScale = 1.6f;
            }

            //legs
            for (int i = 0; i < legJoints.Length; i++)
            {
                yzDrive = legJoints[i].angularYZDrive;
                xDrive = legJoints[i].angularXDrive;

                yzDrive.positionSpring = driveStiffnessLegs;
                xDrive.positionSpring = driveStiffnessLegs;

                legJoints[i].angularYZDrive = yzDrive;
                legJoints[i].angularXDrive = xDrive;
            }

            //renable anim
            anim.enabled = true;
        }
    }

    private IEnumerator RagdollStun(float stunTime)
    {
        //make player ragdoll untill stun times over
        isStunned = true;
        RagDoll(true);

        yield return new WaitForSeconds(stunTime);

        isStunned = false;
        RagDoll(false);
    }

    private void HandleAIStates()
    {
        //Lunging
        if(playerInLungeRange)
        {
            if(isGoingToLunge && isGrounded)
            {
                Debug.Log("Lunged!");

                Lunge();

                isGoingToLunge = false;
            }

            getNewLungeRandom = true;
        }
        else if(getNewLungeRandom)
        {
            getNewLungeRandom = false;
            var rand = UnityEngine.Random.Range(0, lungeChance);
            if(rand == lungeChance - 1)
            {
                isGoingToLunge = true;
            }
        }

        //ragdoll while in air and dont let agent move it
        if(!isGrounded || isStunned)
        {
            //RagDoll(true);
            //agent.SetDestination(transform.position);
            agent.enabled = false;
            canAnimate = false;
            return;
        }
        else if (!isStunned)
        {
            RagDoll(false);
            agent.enabled = true;
            canAnimate = true;
        }

        //Patrolling
        if(currentState == AIStates.Patrolling)
        {
            //get point
            if (!walkPointSet) SearchWalkPoint();

            //make agent go to point
            if (walkPointSet)
                agent.SetDestination(walkPoint);

            //make agent look at point
            //LookAtPoint(walkPoint);

            //check if we have arrived
            Vector3 distanceToWalkPoint = transform.position - walkPoint;

            if (distanceToWalkPoint.magnitude < 1f)
                walkPointSet = false;
        }
        
        //Chasing
        else if(currentState == AIStates.Chasing)
        {
            //make agent go to player
            agent.SetDestination(playerController.transform.position);
            LookAtPoint(playerController.transform);
        }

        //Attacking
        else if(currentState == AIStates.Attacking)
        {
            //make it stop moving
            //agent.SetDestination(transform.position);

            LookAtPoint(playerController.transform);

            //attack
            if(attackAvailable)
            {
                StartCoroutine(ResetAttack(attackCooldown));
            }
        }
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

    private void Balance()
    {
        //balance using upright tourqe
        var balancePercent = Vector3.Angle(hipsRb.transform.up, Vector3.up) / 180;
        balancePercent = uprightTorqueFunction.Evaluate(balancePercent);
        var rot = Quaternion.FromToRotation(hipsRb.transform.up, Vector3.up).normalized;
        //rot = new Quaternion(rot.x + targetLookRotation.x, rot.y + targetLookRotation.y, rot.z + targetLookRotation.z, rot.w + targetLookRotation.w);

        hipsRb.AddTorque(new Vector3(rot.x, rot.y, rot.z) * uprightTorque * balancePercent);

        var directionAnglePercent = Vector3.SignedAngle(hipsRb.transform.forward,
                            TargetDirection, Vector3.up) / 180;
        hipsRb.AddRelativeTorque(0, directionAnglePercent * rotationTorque, 0);
    }

    public IEnumerator LimbDelay()
    {
        canLimbAttack = false;
        yield return new WaitForSeconds(limbDamageAttackDelay);
        canLimbAttack = true;
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
        massDividend = 200;
        stiffnessDividend = 200;
        RagdollStun(1000);
        agent.enabled = false;
        anim.enabled = false;
        canAnimate = false;

        //effects
        deathFeedbacks?.PlayFeedbacks();
        Debug.Log("dead", gameObject);
        isDead = true;
    }
}
