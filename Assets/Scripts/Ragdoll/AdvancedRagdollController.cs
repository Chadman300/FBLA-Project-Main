using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Controls;
using System;
using UnityEngine.UI;
using UnityEditor.Build;
using MoreMountains.Feedbacks;
using DG.Tweening;
using UnityEditor.ShaderGraph;
using Unity.Cinemachine;

public class AdvancedRagdollController : MonoBehaviour
{
    [Header("References")]
    public AdvancedRagdollSettings settings;
    public RagdollValuesController ragdollValues;
    public UIManager uiManager;

    [Header("Movement")]
    public bool canAirstrafe = true;
    public float speed = 250; //forward back speed
    [SerializeField] private float rotateTourqe = 15; //left right speed
    [SerializeField] private bool mouseLook = true;
    public float jumpForce = 3000;
    [Range(1, 20)] public float lungeForce = 2;
    public bool isGrounded = false;

    private float rotationY = 0;

    [Header("Physics Parameters")]
    public LayerMask whatIsGround;
    [SerializeField] private int limbCollisionLayer = 6;
    [SerializeField] private Rigidbody[] rigidbodies;
    [SerializeField] private ConfigurableJoint[] joints;
    [Tooltip("IMPORTANT: joints and animTrans must both have each respective joint in the same order and hips at the end of joints")]
    [SerializeField] private int solverIterations = 12;
    [Tooltip("Higher # the more accurate physics interactions are")]
    [SerializeField] private int velSolverIterations = 12;
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

    [Space(15)]

    [Header("Health Parameters")]
    public float maxHealth = 100f;
    [SerializeField] private float timeBeforeRegenStarts = 3f;
    [SerializeField] private float healthValueIncrement = 3;
    [Tooltip("Increments currentHealth by healthValueIncrement every time of this value")]
    [SerializeField] private float healthTimeIncrement = 0.1f;
    public float currentHealth;
    private Coroutine regeneratingHealth;
    public bool canRegenerate = false;
    public static Action<float> OnTakeDamage;
    public static Action<float> OnDamage;
    public static Action<float> OnHeal;

    [Header("Stun Parameters")]
    [SerializeField] private bool canGetStunned = true;
    [Range(0, 2)][SerializeField] private float stunTimeMultiplyer;
    [Tooltip("Multiplies stun time when damaged (stunTimeMultiplyer * damage = stunTime)")]

    [Header("Attack Paramenters")]
    public LayerMask enemyLayer;
    public bool canLimbAttack = true;
    public float limbAttackDamage = 10f;
    public float limbDamageThreshold = 5f;
    [Tooltip("Minimum attack damage that will hurt enemy")]
    public float limbDamageAttackDelay = 0.1f;
    [Tooltip("Duration after limb attack were you cannot deal limb damage")]
    [Range(0, 10)] public float limbVelocityDividend = 1f;
    [Range(0, 10)] public float handControl = 1.5f;
    [Tooltip("Hand Stiffness aka masss scale")]

    [Space(15)]

    [Header("Grabbing")]
    [SerializeField] private bool canGrab = true;
    public bool canRaiseHand = true;
    [SerializeField] private LayerMask grabbableObjects;
    [SerializeField] private Rigidbody rightHandRb;
    [SerializeField] private Rigidbody leftHandRb;
    [SerializeField] private float grabBreakForce = 1000f;
    [Tooltip("How much force is required to break off connected body of joint")]
    [SerializeField] private bool rightHandUp = false;
    [SerializeField] private bool leftHandUp = false;

    private GameObject grabbedObjRight;
    private GameObject grabbedObjLeft;

    [Header("Picking Up")]
    public bool canPickUpWeapons = true;
    public bool canPickUpItems = true;
    [SerializeField] private Transform rightHandTransform;
    [SerializeField] private Transform leftHandTransform;
    [SerializeField] private string pickUpTag;
    [SerializeField] private float pickRadius = 3f;

    public bool leftHandHasItem = false;
    public bool rightHandHasItem = false;
    public bool leftHandHasGun = false;
    public bool rightHandHasGun = false;
    public GameObject leftHandItemObj = null;
    public GameObject rightHandItemObj = null;

    [Space(15)]

    [Header("Mouse Look")]
    [SerializeField] private float lookTourqe = 200; //left right speed
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask lookLayer;
    [SerializeField] private bool lockYAxis = true;

    [Header("Ads")]
    [SerializeField] private CameraFollow camFollow;
    [SerializeField] private Transform mouseFollow;
    [SerializeField] private float zoomTime = 0.3f;
    private float defaultOrthoSize;
    private Coroutine zoomRoutine;
    private Transform defaultCamFollowTarget;

    [Header("Animation")]
    [SerializeField] private Animator anim;
    [SerializeField] private Transform[] animTransforms;

    [Header("Feedbakcs")]
    [SerializeField] private MMF_Player damageFeedback;
    [SerializeField] private MMF_Player healFeedback;
    [SerializeField] private MMF_Player jumpFeedback;
    [SerializeField] private MMF_Player adsFeedback;
    [SerializeField] private MMF_Player teleCommunicatorFeedback;
    public MMF_Player teleportFeedback;

    [Space(15)]

    //input
    private Vector2 currentInput;
    private Vector2 currentInputRaw;

    private float jumpInput;
    private float jumpInputRaw;

    private Vector2 mouseP;

    private bool isStunned = false;

    private void OnEnable()
    {
        OnTakeDamage += ApplyDamage;
        currentHealth = maxHealth;
    }

    private void OnDisable()
    {
        OnTakeDamage -= ApplyDamage;
    }

    void Start()
    {
        //camera
        //playerCamera = GetComponent<Camera>();
        defaultOrthoSize = camFollow.camera.Lens.OrthographicSize;
        defaultCamFollowTarget = camFollow.target;

        //settings
        if(!TryGetComponent<AdvancedRagdollSettings>(out settings))
        {
            Debug.LogError($"{this} is missing AdvancedRagdollSettings reference");
        }

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
        //dont update if paused
        if (UIManager.isPaused)
            return;


        HandleMovementInput();
        HandleLook();
        HandleAnimation();

        //grapping
        if (canGrab)
            TryGrab();

        //pickup (weapons, items)
        TryPickUp();

        //set animimated rots to joints to animate
        for (int i = 0; i < joints.Length; i++)
        {
            ConfigurableJointExtensions.SetTargetRotationLocal(joints[i], animTransforms[i].localRotation, jointsInitialStartRot[i]);
        }
    }

    private void FixedUpdate()
    {
        if(mouseLook && isGrounded)
            RayRotate();
        
        Balance();

        ApplyFinalMovements();
    }

    private void HandleLook()
    {
        mouseP.x += Input.GetAxis("Mouse X") * lookTourqe;
        mouseP.y -= Input.GetAxis("Mouse Y") * lookTourqe;
    }

    private void ApplyFinalMovements()
    {
        if (isGrounded || canAirstrafe)
        {
            //Strafe
            var moveVel = hipsRb.transform.forward * currentInput.x * Time.deltaTime;
            var sideMoveVel = Vector3.zero;

            if(mouseLook)
            {
                sideMoveVel = hipsRb.transform.right * currentInput.y * Time.deltaTime;
            }

            //when add lunge force if jumping and moving forward
            if (jumpInputRaw > 0 && currentInputRaw.x > 0 && isGrounded)
                moveVel *= lungeForce;

            hipsRb.linearVelocity = new Vector3(moveVel.x + sideMoveVel.x, hipsRb.linearVelocity.y, moveVel.z + sideMoveVel.z);

            //rotation
            if(!mouseLook)
            {
                rotationY -= (currentInput.y * rotateTourqe) * Time.deltaTime;
                //hipJoint.targetRotation = Quaternion.Euler(0, rotationY, 0); //keys
            }

            //Jumping
            if(isGrounded)
                hipsRb.AddForce(hipsRb.transform.up * jumpInput * Time.deltaTime, ForceMode.Impulse);

            //When grounded stiffen ragdoll joins
            if (!isStunned && isGrounded)
                RagDoll(false);
        }

        if(!isGrounded)
        {
            //When not grounded ragdoll
            //RagDoll(true);
        }

        //set isgrounded to false when jumping
        if (jumpInputRaw > 0)
        {
            //when jumping and has telecommunicator play feedback and play jump feedback
            if(isGrounded == true)
            {
                if(ragdollValues.hasTelecommunicator) teleCommunicatorFeedback?.PlayFeedbacks();
                jumpFeedback?.PlayFeedbacks();
            }
                

                
            isGrounded = false;
        }
            
    }

    private void HandleAnimation()
    {
        //run
        if (currentInputRaw.x > 0 && isGrounded) //forwards
        {
            anim.SetBool("isRun", true);
            anim.SetBool("isRunBackwards", false);
        }
        else if (currentInputRaw.x < 0 && isGrounded) //backward
        {
            anim.SetBool("isRunBackwards", true);
            anim.SetBool("isRun", false);
        }
        else
        {
            anim.SetBool("isRun", false);
            anim.SetBool("isRunBackwards", false);
        }

        //strafe
        if (currentInputRaw.y > 0 && isGrounded) //right
        {
            anim.SetBool("isRight", true);
            anim.SetBool("isLeft", false);
        }
        else if (currentInputRaw.y < 0 && isGrounded) //left
        {
            anim.SetBool("isLeft", true);
            anim.SetBool("isRight", false);
        }
        else
        {
            anim.SetBool("isLeft", false);
            anim.SetBool("isRight", false);
        }

        //jump
        if(!isGrounded)
        {
            anim.SetBool("isJump", true);
        }
        else
        {
            anim.SetBool("isJump", false);
        }

        //right arm
        if(rightHandUp)
        {
            //anim.SetTrigger("swing");
            if(rightHandHasGun)
            {
                anim.SetBool("isRightAim", true);
                anim.SetBool("isRightHandUp", false);
            }
            else
                anim.SetBool("isRightHandUp", true);
        }
        else
        {      
            anim.SetBool("isRightHandUp", false);
            anim.SetBool("isRightAim", false);
        }

        //left arm
        if (leftHandUp) 
        {
            if (leftHandHasGun)
            {
                anim.SetBool("isLeftAim", true);
                anim.SetBool("isLeftHandUp", false);
            }
            else
                anim.SetBool("isLeftHandUp", true);
        }
        else
        {
            anim.SetBool("isLeftHandUp", false);
            anim.SetBool("isLeftAim", false);
        }
    }

    private void HandleMovementInput()
    {
        //DEBUG : dellete latyer
        if(Input.GetKeyDown(KeyCode.F))
        {
            if(rightHandItemObj)
                Drop(true, rightHandTransform, rightHandRb);
            if(leftHandItemObj)
                Drop(false, leftHandTransform, leftHandRb);
        }

        //debug
        if (Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(RagdollStun(1));
        }

        //grabbing
        if (canRaiseHand)
        {
            if (Input.GetKey(settings.raiseRightHandKey) || (rightHandHasItem && rightHandHasGun))  //right arm
            { rightHandUp = true; }
            else
            { rightHandUp = false; }

            if (Input.GetKey(settings.raiseLeftHandKey) || (leftHandHasItem && leftHandHasGun))  //right arm
            { leftHandUp = true; }
            else
            { leftHandUp = false; }
        }
        else
        {
            rightHandUp = false;
            leftHandUp = false;
        }

        //move
        float horizonalMultiplyer = mouseLook ? speed / 2 : rotateTourqe;
        currentInput = new Vector2(speed * Input.GetAxis("Vertical"), horizonalMultiplyer * Input.GetAxis("Horizontal"));
        currentInputRaw = new Vector2(Input.GetAxisRaw("Vertical"), Input.GetAxisRaw("Horizontal"));

        if (isGrounded)
        {
            jumpInput = jumpForce * Input.GetAxis("Jump");
            jumpInputRaw = Input.GetAxis("Jump");
        }
    }

    private void TryPickUp()
    {
        //items
        if (canPickUpItems && (rightHandUp || leftHandUp))
        {
            EquipItems();
        }

        //weapons
        if (canPickUpWeapons)
        {
            //right hand
            if (rightHandUp && !rightHandHasItem)
            {
                Equip(true, rightHandTransform, rightHandRb);
            }

            //left hand
            if (leftHandUp && !leftHandHasItem)
            {
                Equip(false, leftHandTransform, leftHandRb);
            }
        }
    }

    private void EquipItems()
    {
        GameObject currentObject;
        ItemController currentItemController;

        Collider[] colliders = Physics.OverlapSphere(hipsRb.transform.position, pickRadius);
        foreach (Collider collider in colliders)
        {
            currentObject = collider.gameObject;
            if (currentObject.CompareTag(pickUpTag) && currentObject.TryGetComponent<ItemController>(out currentItemController))
            {
                ragdollValues.AddItem(currentItemController);
                currentItemController.grabFeedback?.PlayFeedbacks();
                uiManager.AddToQueue(currentItemController.item);
            }
        }
    }

    private void Equip(bool isRightHand, Transform handTransform, Rigidbody rb)
    {
        Collider[] colliders = null;
        MeeleWeapon meeleScript;
        GunController gunScript;
        GameObject currentObject;

        colliders = Physics.OverlapSphere(handTransform.position, pickRadius);

        Vector3 currentRot = Vector3.zero;
        Vector3 currentPos = Vector3.zero;

        foreach (Collider collider in colliders)
        {
            if (isRightHand)
            {
                rightHandItemObj = collider.gameObject;
                currentObject = rightHandItemObj;
            }
            else
            { 
                leftHandItemObj = collider.gameObject;
                currentObject = leftHandItemObj;
            }

            if (currentObject.CompareTag(pickUpTag) && !collider.TryGetComponent<ItemController>(out var currentItemController))
            {
                meeleScript = currentObject.GetComponent<MeeleWeapon>();
                gunScript = currentObject.GetComponent<GunController>();

                //gun 
                if (gunScript != null)
                {
                    //make sure weapons not already equipt
                    if (gunScript.isEquipt)
                        return;

                    currentRot = gunScript.pickRotOffset;
                    currentPos = gunScript.pickPosOffset;

                    gunScript.isEquipt = true;
                    gunScript.playerRb = hipsRb;
                    gunScript.playerController = this;
                    gunScript.OnItemsChange(ragdollValues.items);

                    //set has gun in hand
                    if (isRightHand)
                    {
                        rightHandHasGun = true;
                        gunScript.isRightHand = true;
                    }
                    else
                    {
                        leftHandHasGun = true;
                        gunScript.isRightHand = false;
                    }
                        
                }
                //meele weapon
                else if (meeleScript != null)
                {
                    //make sure weapons not already equipt
                    if (meeleScript.isEquipt)
                        return;

                    currentRot = meeleScript.pickRotOffset;
                    currentPos = meeleScript.pickPosOffset;

                    meeleScript.enabled = true;
                    meeleScript.isEquipt = true;
                    meeleScript.canAttack = true;

                    //reset has gun in hand
                    if (isRightHand)
                        rightHandHasGun = false;
                    else
                        leftHandHasGun = false;
                }

                //set that the current hand has an item
                if (isRightHand)
                    rightHandHasItem = true;
                else
                    leftHandHasItem = true;

                //enable rb if disabled
                currentObject.GetComponent<Rigidbody>().isKinematic = false;

                //if has bob script remove
                if (currentObject.TryGetComponent<BobAndRotate>(out var currentBobRotate))
                    currentBobRotate.enabled = false;

                //setpos and rot
                currentObject.transform.parent = rb.gameObject.transform;
                currentObject.transform.localPosition = currentPos;
                currentObject.transform.localRotation = Quaternion.Euler(currentRot);

                currentObject.layer = limbCollisionLayer;

                //pick item
                FixedJoint joint;
                if (!TryGetComponent<FixedJoint>(out joint))
                    joint = currentObject.AddComponent<FixedJoint>();

                joint.connectedBody = rb;
                joint.connectedMassScale = 0.5f;
                joint.massScale = handControl;

                return;
            }
        }
    }

    private void Drop(bool isRightHand, Transform handTransform, Rigidbody rb)
    {
        var currentObject = isRightHand ? rightHandItemObj : leftHandItemObj;
        var currentJoint = currentObject.GetComponent<FixedJoint>();

        var meeleScript = currentObject.GetComponent<MeeleWeapon>();
        var gunScript = currentObject.GetComponent<GunController>();

        //gun 
        if (gunScript != null)
        {
            gunScript.isEquipt = false;
            gunScript.DisableLaser();
        }
        //meele weapon
        else if (meeleScript != null)
        {
            meeleScript.enabled = false;
            meeleScript.isEquipt = false;
            meeleScript.canAttack = false;
        }

        currentObject.layer = 0;
        currentObject.transform.parent = null;

        currentJoint.connectedBody = null;
        Destroy(currentJoint);

        if (isRightHand)
        {
            rightHandHasItem = false;
            rightHandHasGun = false;
            rightHandItemObj = null;
        }
        else
        {
            leftHandHasGun = false;
            leftHandHasItem = false;
            leftHandItemObj = null;
        }
    }

    private void TryGrab()
    {
        FixedJoint fixedJointR = null;
        FixedJoint fixedJointL = null;
        RaycastHit hit;
        float sphereSize = 10;

        //right hand
        if (rightHandUp)
        {
            //get obj
            Physics.SphereCast(rightHandTransform.transform.position, sphereSize, rightHandRb.transform.forward, out hit, grabbableObjects);
            if(hit.collider != null)
                grabbedObjRight = hit.collider.gameObject;

            //set joints and stuff
            if (grabbedObjRight != null)
            {
                Debug.Log("Grabbed");
                fixedJointR = grabbedObjRight.AddComponent<FixedJoint>();
                if (fixedJointR.connectedBody == null)
                {
                    fixedJointR.connectedBody = rightHandRb;
                    fixedJointR.breakForce = grabBreakForce;
                }
            }
        }
        else
        {
            if (fixedJointR != null)
                Destroy(grabbedObjRight.GetComponent<FixedJoint>());

            grabbedObjRight = null;
        }

        //left hand
        if (leftHandUp)
        {
            //get obj
            Physics.SphereCast(leftHandTransform.transform.position, sphereSize, leftHandRb.transform.forward, out hit, grabbableObjects);
            if (hit.collider != null)
                grabbedObjLeft = hit.collider.gameObject;

            //set joints and stuff
            if (grabbedObjLeft != null)
            {
                Debug.Log("Grabbed");
                fixedJointL = grabbedObjLeft.AddComponent<FixedJoint>();
                if (fixedJointL.connectedBody == null)
                {
                    fixedJointL.connectedBody = leftHandRb;
                    fixedJointL.breakForce = grabBreakForce;
                }
            }     
        }
        else
        {
            if (fixedJointL != null)
                Destroy(grabbedObjLeft.GetComponent<FixedJoint>());

            grabbedObjLeft = null;
        }
    }

    public void HandleAds(bool isAds, float adsFov)
    {
        //ToggleZoom
        if (isAds)
        {
            //make camera follow mouse when adsed
            camFollow.target = mouseFollow;

            //lock camFollowY so it dosent infinatly go down whilst following
            camFollow.lockY = true;

            //play ads feedback
            adsFeedback?.PlayFeedbacks();

            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }

            zoomRoutine = StartCoroutine(ToggleZoom(true, adsFov, defaultOrthoSize, zoomTime, zoomRoutine));
        }
        else
        {
            //make camera follow player again when not adsed
            camFollow.target = defaultCamFollowTarget;
            camFollow.lockY = false;
            adsFeedback?.ResumeFeedbacks();

            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }

            zoomRoutine = StartCoroutine(ToggleZoom(false, adsFov, defaultOrthoSize, zoomTime, zoomRoutine));
        }
    }

    private IEnumerator ToggleZoom(bool isEnter, float _TargetOrthographicSize, float _defaultOrthographicSize, float _TimeToZoom, Coroutine routine)
    {
        float targetFov = isEnter ? _TargetOrthographicSize : _defaultOrthographicSize;
        float startFov = camFollow.camera.Lens.OrthographicSize;
        float timeElapsed = 0;

        while (timeElapsed < _TimeToZoom)
        {
            camFollow.camera.Lens.OrthographicSize = Mathf.Lerp(startFov, targetFov, timeElapsed / _TimeToZoom);
            timeElapsed += Time.deltaTime;
            Debug.Log($"{camFollow.camera.Lens.OrthographicSize}, {targetFov}");
            yield return null;
        }

        //pausefeedbacks
        //if(isEnter)
            //adsFeedback?.PauseFeedbacks();

        camFollow.camera.Lens.OrthographicSize = targetFov;

        routine = null;
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

    private void RayRotate()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, lookLayer))
        {
            Vector3 targetPoint = hit.point;
            Vector3 direction = targetPoint - transform.position;

            mouseFollow.position = targetPoint;

            if (lockYAxis)
            {
                direction.y = 0f;
            }

            Quaternion targetRotaion = Quaternion.LookRotation(direction);
            targetRotaion.w = -targetRotaion.w;

            //hipJoint.targetRotation = targetRotaion;
            hipJoint.targetRotation = Quaternion.Slerp(hipJoint.targetRotation, targetRotaion, lookTourqe * Time.deltaTime);

            //Debug.DrawLine(transform.position, hit.point, Color.green);
        }
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

    public IEnumerator LimbDelay()
    {
        canLimbAttack = false;
        yield return new WaitForSeconds(limbDamageAttackDelay);
        canLimbAttack = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(rightHandTransform.position, pickRadius);
        Gizmos.DrawWireSphere(leftHandTransform.position, pickRadius);
    }

    public void ApplyDamage(float damage)
    {
        currentHealth -= damage;
        OnDamage?.Invoke(currentHealth);

        //effects
        damageFeedback?.PlayFeedbacks();

        if (currentHealth <= 0)
            KillPlayer();
        else if (regeneratingHealth != null)
            StopCoroutine(regeneratingHealth);

        //stunPlayer
        if(canGetStunned)
            StartCoroutine(RagdollStun(damage * stunTimeMultiplyer));

        //start Regen
        if (canRegenerate)
            regeneratingHealth = StartCoroutine(RegenerateHealth());
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

    public void ApplyHealth(float health)
    {
        if (currentHealth < maxHealth)
        {
            currentHealth += health; 
        }

        OnHeal?.Invoke(currentHealth);

        //effects
        healFeedback?.PlayFeedbacks();
        if(canRegenerate)
            regeneratingHealth = StartCoroutine(RegenerateHealth());
    }

    public void KillPlayer()
    {
        currentHealth = 0;

        if (regeneratingHealth != null)
            StopCoroutine(regeneratingHealth);

        StartCoroutine(RagdollStun(100));

        //effects
        //deathFeedBack?.PlayFeedbacks();
        Debug.Log("dead", gameObject);
    }

    public IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(timeBeforeRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(healthTimeIncrement);

        while (currentHealth < maxHealth)
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
}
