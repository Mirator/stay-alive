using System;
using UnityEngine;

public sealed class PlayerVitals : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float health = 100f;
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float hunger = 100f;
    [SerializeField] private float hungerDrainPerSecond = 0.08f;
    [SerializeField] private float starvationDamagePerSecond = 1.5f;
    [SerializeField] private float foodRestore = 30f;
    [SerializeField] private float bandageHeal = 35f;

    public event Action Changed;

    public float MaxHealth => maxHealth;
    public float Health => health;
    public float MaxHunger => maxHunger;
    public float Hunger => hunger;
    public bool IsDead { get; private set; }

    private void Awake()
    {
        health = Mathf.Clamp(health <= 0f ? maxHealth : health, 0f, maxHealth);
        hunger = Mathf.Clamp(hunger <= 0f ? maxHunger : hunger, 0f, maxHunger);
    }

    private void Update()
    {
        AdvanceSurvivalTime(Time.deltaTime);
    }

    public void Configure(float newMaxHealth, float newMaxHunger)
    {
        maxHealth = Mathf.Max(1f, newMaxHealth);
        maxHunger = Mathf.Max(1f, newMaxHunger);
        health = maxHealth;
        hunger = maxHunger;
        IsDead = false;
        Changed?.Invoke();
    }

    public void SetState(float newHealth, float newHunger)
    {
        health = Mathf.Clamp(newHealth, 0f, maxHealth);
        hunger = Mathf.Clamp(newHunger, 0f, maxHunger);
        IsDead = health <= 0f;
        Changed?.Invoke();
    }

    public void AdvanceSurvivalTime(float seconds)
    {
        if (seconds <= 0f || IsDead)
        {
            return;
        }

        hunger = Mathf.Max(0f, hunger - hungerDrainPerSecond * seconds);
        if (hunger <= 0f)
        {
            ApplyDamage(starvationDamagePerSecond * seconds);
            return;
        }

        Changed?.Invoke();
    }

    public bool EatFood(ResourceInventory inventory)
    {
        if (inventory == null || IsDead || !inventory.Spend(ResourceType.Food, 1))
        {
            return false;
        }

        RestoreHunger(foodRestore);
        UIController.Instance?.ShowMessage("Ate food.", 1.2f);
        return true;
    }

    public bool UseBandage(CraftedInventory inventory)
    {
        if (inventory == null || IsDead || !inventory.Spend(CraftedItemType.Bandage, 1))
        {
            return false;
        }

        Heal(bandageHeal);
        UIController.Instance?.ShowMessage("Used bandage.", 1.2f);
        return true;
    }

    public void RestoreHunger(float amount)
    {
        if (IsDead)
        {
            return;
        }

        hunger = Mathf.Min(maxHunger, hunger + Mathf.Max(0f, amount));
        Changed?.Invoke();
    }

    public void Heal(float amount)
    {
        if (IsDead)
        {
            return;
        }

        health = Mathf.Min(maxHealth, health + Mathf.Max(0f, amount));
        Changed?.Invoke();
    }

    public void ApplyDamage(float amount)
    {
        if (IsDead)
        {
            return;
        }

        health = Mathf.Max(0f, health - Mathf.Max(0f, amount));
        if (health <= 0f)
        {
            IsDead = true;
            UIController.Instance?.ShowDeath(true);
            UIController.Instance?.ShowMessage("You died. Load a save or return to menu.", 5f);
        }

        Changed?.Invoke();
    }
}
