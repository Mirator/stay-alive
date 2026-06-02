using System;
using UnityEngine;

public enum CraftedItemType
{
    Torch,
    StoneMarker,
    CrystalKeyShard
}

public sealed class CraftedInventory : MonoBehaviour
{
    [SerializeField] private int torches;
    [SerializeField] private int stoneMarkers;
    [SerializeField] private int crystalKeyShards;

    public event Action Changed;

    public int Torches => torches;
    public int StoneMarkers => stoneMarkers;
    public int CrystalKeyShards => crystalKeyShards;

    public int Get(CraftedItemType type)
    {
        switch (type)
        {
            case CraftedItemType.Torch:
                return torches;
            case CraftedItemType.StoneMarker:
                return stoneMarkers;
            case CraftedItemType.CrystalKeyShard:
                return crystalKeyShards;
            default:
                return 0;
        }
    }

    public void Add(CraftedItemType type, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SetSilently(type, Get(type) + amount);
        Changed?.Invoke();
    }

    public bool Has(CraftedItemType type, int amount)
    {
        return amount <= 0 || Get(type) >= amount;
    }

    public bool Spend(CraftedItemType type, int amount)
    {
        if (!Has(type, amount))
        {
            return false;
        }

        SetSilently(type, Get(type) - amount);
        Changed?.Invoke();
        return true;
    }

    public static string Label(CraftedItemType type)
    {
        switch (type)
        {
            case CraftedItemType.Torch:
                return "Torch";
            case CraftedItemType.StoneMarker:
                return "Stone Marker";
            case CraftedItemType.CrystalKeyShard:
                return "Crystal Key Shard";
            default:
                return "Crafted Item";
        }
    }

    private void SetSilently(CraftedItemType type, int value)
    {
        switch (type)
        {
            case CraftedItemType.Torch:
                torches = Mathf.Max(0, value);
                break;
            case CraftedItemType.StoneMarker:
                stoneMarkers = Mathf.Max(0, value);
                break;
            case CraftedItemType.CrystalKeyShard:
                crystalKeyShards = Mathf.Max(0, value);
                break;
        }
    }
}
