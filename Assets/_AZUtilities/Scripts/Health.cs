using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;
    public float normalizedHealth => HelperUtilities.Remap(currentHealth, 0, maxHealth, 0, 1);
    public float timeOfLastDamage { get; private set; } = 0;
    public float timeSinceLastDamage => Time.time - timeOfLastDamage;

    public bool startWithMaxHealth = true;

    public UnityEvent OnDamageTaken;
    public UnityEvent OnHealthDepleted;

    private bool healthDepleted = false;

    void Awake()
    {
        if (startWithMaxHealth)
        {
            currentHealth = maxHealth;
        }
    }

    void Update()
    {
        if (!healthDepleted && currentHealth <= 0)
        {
            healthDepleted = true;
            OnHealthDepleted?.Invoke();
        }
    }

    public void UpdateHealth(float updateAmount)
    {
        currentHealth += updateAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void TakeDamage(float damage)
    {
        UpdateHealth(-damage);
        OnDamageTaken?.Invoke();
        timeOfLastDamage = Time.time;
    }
}