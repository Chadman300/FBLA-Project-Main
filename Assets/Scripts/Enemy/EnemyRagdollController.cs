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
using UnityEngine.InputSystem.Processors;

public class EnemyRagdollController : MonoBehaviour
{
    [Header("Movement Parameters")]
    [SerializeField] private Transform player;
    [SerializeField] private float moveSpeed;

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

    [Header("Feedbacks Parameters")]
    public MMF_Player damageFeedbacks;

    public Rigidbody rb;

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
        rb = GetComponent<Rigidbody>();

        currentHealth = maxHealth;
        ApplyDamage(0); // to reset health bar
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

        //unfreeze rb
        Rigidbody rb;
        if (TryGetComponent<Rigidbody>(out rb))
            rb.constraints &= ~RigidbodyConstraints.FreezeRotation;

        //effects
        //deathFeedBack?.PlayFeedbacks();
        Debug.Log("dead", gameObject);
        isDead = true;
    }
}
