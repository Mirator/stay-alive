using System;
using UnityEngine;

public sealed class ResourceInventory : MonoBehaviour
{
    [SerializeField] private int dirt;
    [SerializeField] private int stone;
    [SerializeField] private int glowCrystal;
    [SerializeField] private int rootFiber;
    [SerializeField] private int wood;
    [SerializeField] private int food;
    [SerializeField] private int herbs;

    public event Action Changed;

    public int Dirt => dirt;
    public int Stone => stone;
    public int GlowCrystal => glowCrystal;
    public int RootFiber => rootFiber;
    public int Wood => wood;
    public int Food => food;
    public int Herbs => herbs;

    public int Get(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Dirt:
                return dirt;
            case ResourceType.Stone:
                return stone;
            case ResourceType.GlowCrystal:
                return glowCrystal;
            case ResourceType.RootFiber:
                return rootFiber;
            case ResourceType.Wood:
                return wood;
            case ResourceType.Food:
                return food;
            case ResourceType.Herbs:
                return herbs;
            default:
                return 0;
        }
    }

    public void Add(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SetSilently(type, Get(type) + amount);
        Changed?.Invoke();
    }

    public bool Has(ResourceType type, int amount)
    {
        return amount <= 0 || Get(type) >= amount;
    }

    public bool Spend(ResourceType type, int amount)
    {
        if (!Has(type, amount))
        {
            return false;
        }

        SetSilently(type, Get(type) - amount);
        Changed?.Invoke();
        return true;
    }

    public void Set(ResourceType type, int amount)
    {
        SetSilently(type, amount);
        Changed?.Invoke();
    }

    public void ClearAll()
    {
        dirt = 0;
        stone = 0;
        glowCrystal = 0;
        rootFiber = 0;
        wood = 0;
        food = 0;
        herbs = 0;
        Changed?.Invoke();
    }

    private void SetSilently(ResourceType type, int value)
    {
        switch (type)
        {
            case ResourceType.Dirt:
                dirt = Mathf.Max(0, value);
                break;
            case ResourceType.Stone:
                stone = Mathf.Max(0, value);
                break;
            case ResourceType.GlowCrystal:
                glowCrystal = Mathf.Max(0, value);
                break;
            case ResourceType.RootFiber:
                rootFiber = Mathf.Max(0, value);
                break;
            case ResourceType.Wood:
                wood = Mathf.Max(0, value);
                break;
            case ResourceType.Food:
                food = Mathf.Max(0, value);
                break;
            case ResourceType.Herbs:
                herbs = Mathf.Max(0, value);
                break;
        }
    }
}
