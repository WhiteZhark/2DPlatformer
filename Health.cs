using UnityEngine;
using System.Collections;

public class Health : IBehaviour
{
    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public int RegenFactor { get; private set; }

    public Health(int maxHealth = 1, int currentHealth = 1)
    {
        MaxHealth = maxHealth;
        CurrentHealth = currentHealth > MaxHealth ? MaxHealth : currentHealth;
    }

    public void ModifyCurrentHealth(int changeInHealth)
    {
        CurrentHealth = (changeInHealth + CurrentHealth) > MaxHealth? MaxHealth : changeInHealth + CurrentHealth;
    }

    public void ModifyMaxHealth(int changeInHealth)
    {
        MaxHealth += changeInHealth;
    }

    public void ModifyRegenFactor(int changeInRegen)
    {
        RegenFactor += changeInRegen;
    }

    public void SetCurrentHealth(int currentHealth)
    {
        CurrentHealth = currentHealth > MaxHealth? MaxHealth : currentHealth;
    }

    public void SetMaxHealth(int maxHealth)
    {
        MaxHealth = maxHealth;
    }

    public void SetRegenFactor(int regenFactor)
    {
        RegenFactor = regenFactor;
    }

    public bool IsAlive()
    {
        return CurrentHealth > 0 ? true : false;
    }

    public void OnUpdate()
    {
        if (IsAlive())
        {
            CurrentHealth = Mathf.Clamp(0, RegenFactor + CurrentHealth, MaxHealth);
        }
    }
}
