using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class GunController : MonoBehaviour
{
    public bool isEquipt = false;

    [Header("Feedbacks Parameters")]
    [SerializeField] private MMF_Player shootFeedback;

    [Header("Player refs")]
    public Rigidbody playerRb;
    public AdvancedRagdollController playerController;

    [Header("Gun Parameters")]
    [SerializeField] private Rigidbody gunRb;
    [SerializeField] private Vector2 damage;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float fireRate = 0.25f;
    [SerializeField] private float maxRaycastDistance = float.MaxValue;
    [SerializeField] private float maxSpreadTime = 10f;
    [SerializeField] private Vector2 recoilForce = new Vector2(10f, 15f);
    [SerializeField] private Vector2 playerRecoilForce = new Vector2(10f, 15f);
    [SerializeField] private float hitForce = 10f;
    public bool isRightHand = true;
    [Space]
    public GunShootType currentShootType = GunShootType.FullAuto;
    public GunShootType[] availableShootTypes = { GunShootType.FullAuto, GunShootType.SemiAuto };

    [Header("Aim Assist Parameters")]
    [SerializeField] private bool aimAssist = true;
    [SerializeField] private float aimForce = 10f;    // The strength of the aim assist force
    [SerializeField] private float aimRadius = 5f;    // The radius within which aim assist applies
    [SerializeField] private float aimMaxSpeed = 20f; // Max speed for the object
    [SerializeField] private Vector3 aimAssistBoxSize = new Vector3(10, 4, 5); // L,W,H
    [SerializeField] private float aimBoxForwardOffset = 5f; // L,W,H

    [Header("Bullet Config")]
    [SerializeField] private bool isHitScan = true;
    [SerializeField] private Transform bulletSpawn;

    [Header("Ammo Config")]
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private int currentAmmo = 30;

    /*
    [Header("Non Hitscan Parameters")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletVelocity = 250f;
    */

    [Header("Trail Renderer Proppertys")]
    // Proppertys for trail renderer
    [SerializeField] private Material trailMaterial;
    [SerializeField] private AnimationCurve trailWidthCurve;
    [SerializeField] private float trailDuration = 0.5f;
    [SerializeField] private float trailMinVertexDistance = 0.1f;
    [SerializeField] private Gradient trailColor;
    [SerializeField] private bool trailEmmiting = true;
    [SerializeField] private bool trailShadowCasting = false;

    [Header("Trail Movement")]
    // Sim speed is how quickky the trail moves, miss dis is how far it gose after you miss
    [SerializeField] private float trailMissDistance = 100f;
    [SerializeField] private float trailSimulationSpeed = 100f;

    private ObjectPool<TrailRenderer> trailPool;
    private float lastShootTime;
    private float stopShootingTime;
    private float initialClickTime;

    private bool isShooting = false;

    void Awake()
    {
        trailPool = new ObjectPool<TrailRenderer>(CreateTrail);
        gunRb = GetComponent<Rigidbody>();
        lastShootTime = 0; // in editor this will not be propperly reset, in build it's fine
        //currentAmmo = maxAmmo;
    }

    private void Update()
    {
        if (UIManager.isPaused)
            return;

        if (currentShootType == GunShootType.SemiAuto)
        {
            if (Input.GetKeyDown(isRightHand ? KeyCode.Mouse1 : KeyCode.Mouse0))
            {
                TryToShoot();
                isShooting = currentAmmo > 0 ? true : false;
            }
            else
            {
                isShooting = false;
            }
                
        }
        else if (currentShootType == GunShootType.FullAuto)
        {
            if (Input.GetKey(isRightHand ? KeyCode.Mouse1 : KeyCode.Mouse0))
            {
                TryToShoot();
                isShooting = currentAmmo > 0 ? true : false;
            }
            else
            {
                isShooting = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            currentAmmo = maxAmmo;
        }
    }

    private void FixedUpdate()
    {
        if (!isShooting && aimAssist && playerController != null && playerController.isGrounded)
            AimAssist();
    }

    private void AimAssist()
    {
        //get enemys
        Collider[] collisions = 
            Physics.OverlapBox(
                bulletSpawn.position + (bulletSpawn.forward * aimBoxForwardOffset), 
                aimAssistBoxSize / 2,
                Quaternion.Euler(0, bulletSpawn.rotation.eulerAngles.y, 0), 
                playerController.enemyLayer
                );

        //find the closest enemy
        Transform target = null;
        float targetDistance = 0;
        for(int i = 0; i < collisions.Length; i++)
        {
            if(target == null)
            {
                target = collisions[0].transform;
                targetDistance = Vector3.Distance(transform.position, target.position);
            }
            else
            {
                Transform currentTransform = collisions[i].transform;
                float dis = Vector3.Distance(transform.position, currentTransform.position);
                if(dis < targetDistance)
                {
                    targetDistance = dis;
                    target = currentTransform;
                }
            }
        }

        if (target == null)
            return;

        //at forces to gun rb to create a aim assist
        // Calculate the direction to the target
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        // Check if the target is within the aim assist radius
        if (Vector3.Distance(transform.position, target.position) <= aimRadius)
        {
            // Apply force toward the target
            gunRb.AddForce(directionToTarget * aimForce, ForceMode.Acceleration);

            // Limit the Rigidbody's speed
            if (gunRb.linearVelocity.magnitude > aimMaxSpeed)
            {
                gunRb.linearVelocity = gunRb.linearVelocity.normalized * aimMaxSpeed;
            }
        }

        Debug.DrawLine(bulletSpawn.position, target.position, Color.green);
    }

    public void TryToShoot()
    {
        if (!isEquipt)
            return;

        if (Time.time > fireRate + lastShootTime)
        {
            if (currentAmmo == 0)
            {
                //AudioConfig.PlayOutOfAmmoClip(shootingAudioSource);
                return;
            }

            lastShootTime = Time.time;
            //shootSystem.Play();
            //AudioConfig.PlayShootingClip(shootingAudioSource, AmmoConfig.CurrentClipAmmo == 1);

            if (isHitScan)
            {
                DoHitScanShoot();
            }
            else
            {
                //DoProjectileShoot(shootDirection);
            }

            //recoil camera add forces
            gunRb.AddForce(-bulletSpawn.forward * Random.Range(recoilForce.x, recoilForce.y), ForceMode.Impulse);
            playerRb.AddForce(-playerRb.transform.forward * Random.Range(playerRecoilForce.x, playerRecoilForce.y), ForceMode.Impulse);

            currentAmmo--;

            // play shell eject
            shootFeedback?.PlayFeedbacks();

            /*
            if (TrailConfig.isShellEjecting && TrailConfig.ShellModel != null)
            {
                activeMonoBehavior.StartCoroutine(PlayShellEject());
            }
            */
        }
    }

    private void DoHitScanShoot()
    {
        //shootDirection.Normalize();
        if (Physics.Raycast(
            bulletSpawn.position,
            bulletSpawn.forward,
            out RaycastHit hit,
            maxRaycastDistance,
            hitMask
            ))
        {
            StartCoroutine(
                PlayTrail(
                    bulletSpawn.position,
                    hit.point,
                    hit
                    ));

            BulletCollision(hit);
        }
        else
        {
            StartCoroutine(
                PlayTrail(
                    bulletSpawn.position,
                    bulletSpawn.position + (bulletSpawn.forward * trailMissDistance),
                    new RaycastHit()
                    ));
        }
    }

    private void BulletCollision(RaycastHit hit)
    {
        //normal enemy
        if(hit.transform.gameObject.TryGetComponent<EnemyController>(out EnemyController enemyController))
        {
            enemyController.ApplyDamage(Random.Range(damage.x, damage.y));
            enemyController.rb.AddForce(hitForce * bulletSpawn.transform.forward, ForceMode.Impulse);
        }

        //ragdoll enemy
        if (hit.transform.gameObject.TryGetComponent<EnemyRagdollLimbCollision>(out EnemyRagdollLimbCollision enemyLimb))
        {
            enemyLimb.controller.ApplyDamage(Random.Range(damage.x, damage.y));
            enemyLimb.GetComponent<Rigidbody>().AddForce(hitForce * bulletSpawn.transform.forward, ForceMode.Impulse);
            enemyLimb.controller.hipsRb.AddForce(hitForce * bulletSpawn.transform.forward, ForceMode.Impulse);
        }
    }

    private IEnumerator DeleyedDisableTrail(TrailRenderer trail)
    {
        yield return new WaitForSeconds(trailDuration);
        yield return null;
        trail.emitting = false;
        trail.gameObject.SetActive(false);
        trailPool.Release(trail);
    }

    private IEnumerator PlayTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
    {
        TrailRenderer instance = trailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = startPoint;
        //instance.gameObject.layer = TrailConfig.weaponLayer;

        yield return null; // avoid position carry-over from last frame if resued

        instance.emitting = trailEmmiting;

        float distance = Vector3.Distance(startPoint, endPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(
                startPoint,
                endPoint,
                Mathf.Clamp01(1 - (remainingDistance / distance))
                );
            remainingDistance -= trailSimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = endPoint;

        // Surface Manager
        /*
        if (hit.collider != null)
        {
            HandleBulletImpact(distance, endPoint, hit.normal, hit.collider);
        }
        */

        yield return new WaitForSeconds(trailDuration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        trailPool.Release(instance);
    }

    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();

        // stuff from the TrailConfigScriptableObj
        trail.colorGradient = trailColor;
        trail.material = trailMaterial;
        trail.widthCurve = trailWidthCurve;
        trail.time = trailDuration;
        trail.minVertexDistance = trailMinVertexDistance;

        trail.emitting = false; // >may want to play with this later (add some smoke effects maybe?)<
        trail.shadowCastingMode = trailShadowCasting ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }

    private void OnDrawGizmos()
    {
        if (isEquipt)
        {
            // Save the current Gizmos matrix
            Matrix4x4 oldMatrix = Gizmos.matrix;

            // Extract the Z-axis rotation from bulletSpawn's rotation
            Quaternion axisRotation = Quaternion.Euler(0, bulletSpawn.rotation.eulerAngles.y, 0);

            // Create a new matrix for the Gizmo with the Z-axis rotation
            Gizmos.matrix = Matrix4x4.TRS(
                bulletSpawn.position + (bulletSpawn.forward * aimBoxForwardOffset), // Position
                axisRotation,                                                     // Rotation around Z-axis
                Vector3.one                                                        // Scale
            );

            // Draw the wire cube with the new matrix
            Gizmos.DrawWireCube(Vector3.zero, aimAssistBoxSize);

            // Restore the previous Gizmos matrix
            Gizmos.matrix = oldMatrix;
        }
    }
}
