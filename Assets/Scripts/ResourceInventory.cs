using System;
using UnityEngine;

public sealed class ResourceInventory : MonoBehaviour
{
    [SerializeField] private int dirt;
    [SerializeField] private int stone;
    [SerializeField] private int glowCrystal;
    [SerializeField] private int rootFiber;

    public event Action Changed;

    public int Dirt => dirt;
    public int Stone => stone;
    public int GlowCrystal => glowCrystal;
    public int RootFiber => rootFiber;

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
        }
    }
}
