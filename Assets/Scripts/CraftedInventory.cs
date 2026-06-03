using System;
using UnityEngine;

public enum CraftedItemType
{
    Torch,
    StoneMarker,
    CrystalKeyShard,
    StoneSpear,
    Bandage,
    BuildFloor,
    BuildWall,
    BuildDoor,
    Campfire,
    Bedroll,
    StorageBox,
    Workbench
}

public sealed class CraftedInventory : MonoBehaviour
{
    [SerializeField] private int torches;
    [SerializeField] private int stoneMarkers;
    [SerializeField] private int crystalKeyShards;
    [SerializeField] private int stoneSpears;
    [SerializeField] private int bandages;
    [SerializeField] private int buildFloors;
    [SerializeField] private int buildWalls;
    [SerializeField] private int buildDoors;
    [SerializeField] private int campfires;
    [SerializeField] private int bedrolls;
    [SerializeField] private int storageBoxes;
    [SerializeField] private int workbenches;

    public event Action Changed;

    public int Torches => torches;
    public int StoneMarkers => stoneMarkers;
    public int CrystalKeyShards => crystalKeyShards;
    public int StoneSpears => stoneSpears;
    public int Bandages => bandages;
    public int BuildFloors => buildFloors;
    public int BuildWalls => buildWalls;
    public int BuildDoors => buildDoors;
    public int Campfires => campfires;
    public int Bedrolls => bedrolls;
    public int StorageBoxes => storageBoxes;
    public int Workbenches => workbenches;

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
            case CraftedItemType.StoneSpear:
                return stoneSpears;
            case CraftedItemType.Bandage:
                return bandages;
            case CraftedItemType.BuildFloor:
                return buildFloors;
            case CraftedItemType.BuildWall:
                return buildWalls;
            case CraftedItemType.BuildDoor:
                return buildDoors;
            case CraftedItemType.Campfire:
                return campfires;
            case CraftedItemType.Bedroll:
                return bedrolls;
            case CraftedItemType.StorageBox:
                return storageBoxes;
            case CraftedItemType.Workbench:
                return workbenches;
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

    public void Set(CraftedItemType type, int amount)
    {
        SetSilently(type, amount);
        Changed?.Invoke();
    }

    public void ClearAll()
    {
        torches = 0;
        stoneMarkers = 0;
        crystalKeyShards = 0;
        stoneSpears = 0;
        bandages = 0;
        buildFloors = 0;
        buildWalls = 0;
        buildDoors = 0;
        campfires = 0;
        bedrolls = 0;
        storageBoxes = 0;
        workbenches = 0;
        Changed?.Invoke();
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
            case CraftedItemType.StoneSpear:
                return "Stone Spear";
            case CraftedItemType.Bandage:
                return "Bandage";
            case CraftedItemType.BuildFloor:
                return "Floor";
            case CraftedItemType.BuildWall:
                return "Wall";
            case CraftedItemType.BuildDoor:
                return "Door";
            case CraftedItemType.Campfire:
                return "Campfire";
            case CraftedItemType.Bedroll:
                return "Bedroll";
            case CraftedItemType.StorageBox:
                return "Storage Box";
            case CraftedItemType.Workbench:
                return "Workbench";
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
            case CraftedItemType.StoneSpear:
                stoneSpears = Mathf.Max(0, value);
                break;
            case CraftedItemType.Bandage:
                bandages = Mathf.Max(0, value);
                break;
            case CraftedItemType.BuildFloor:
                buildFloors = Mathf.Max(0, value);
                break;
            case CraftedItemType.BuildWall:
                buildWalls = Mathf.Max(0, value);
                break;
            case CraftedItemType.BuildDoor:
                buildDoors = Mathf.Max(0, value);
                break;
            case CraftedItemType.Campfire:
                campfires = Mathf.Max(0, value);
                break;
            case CraftedItemType.Bedroll:
                bedrolls = Mathf.Max(0, value);
                break;
            case CraftedItemType.StorageBox:
                storageBoxes = Mathf.Max(0, value);
                break;
            case CraftedItemType.Workbench:
                workbenches = Mathf.Max(0, value);
                break;
        }
    }
}
