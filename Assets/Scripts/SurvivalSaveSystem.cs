using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class SurvivalSaveSystem : MonoBehaviour
{
    public const int SlotCount = 3;

    [SerializeField] private Transform player;
    [SerializeField] private ResourceInventory resources;
    [SerializeField] private CraftedInventory crafted;
    [SerializeField] private PlayerVitals vitals;
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private GridBuildingSystem buildingSystem;

    private string storageRoot;
    private int activeSlot = 1;
    private Vector3 startingPlayerPosition;
    private bool hasStartingPlayerPosition;

    public static SurvivalSaveSystem Instance { get; private set; }
    public int ActiveSlot => activeSlot;
    public event Action<int> Saved;
    public event Action<int> Loaded;

    private void Awake()
    {
        Instance = this;
        EnsureStorageRoot();
        CaptureStartingPlayerPosition();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Configure(Transform playerTransform, ResourceInventory resourceInventory, CraftedInventory craftedInventory, PlayerVitals playerVitals, DayNightCycle cycle, GridBuildingSystem gridBuilding)
    {
        player = playerTransform;
        resources = resourceInventory;
        crafted = craftedInventory;
        vitals = playerVitals;
        dayNightCycle = cycle;
        buildingSystem = gridBuilding;
        CaptureStartingPlayerPosition();
    }

    public void SetStorageRootForTests(string root)
    {
        storageRoot = root;
    }

    public bool SaveSlot(int slot)
    {
        if (!IsValidSlot(slot) || player == null || vitals == null || vitals.IsDead)
        {
            return false;
        }

        activeSlot = slot;
        EnsureStorageRoot();
        Directory.CreateDirectory(storageRoot);
        SaveSlotData data = Capture(slot);
        File.WriteAllText(SlotPath(slot), JsonUtility.ToJson(data, true));
        Saved?.Invoke(slot);
        UIController.Instance?.ShowSaveSlotView(GetSlotViewData(slot));
        return true;
    }

    public bool SaveActiveSlot()
    {
        return SaveSlot(activeSlot);
    }

    public bool LoadSlot(int slot)
    {
        if (!IsValidSlot(slot) || !File.Exists(SlotPath(slot)))
        {
            return false;
        }

        activeSlot = slot;
        SaveSlotData data = JsonUtility.FromJson<SaveSlotData>(File.ReadAllText(SlotPath(slot)));
        Restore(data);
        Loaded?.Invoke(slot);
        UIController.Instance?.ShowSaveSlotView(GetSlotViewData(slot));
        return true;
    }

    public bool HasSlot(int slot)
    {
        return IsValidSlot(slot) && File.Exists(SlotPath(slot));
    }

    public SaveSlotData ReadSlot(int slot)
    {
        if (!HasSlot(slot))
        {
            return null;
        }

        return JsonUtility.FromJson<SaveSlotData>(File.ReadAllText(SlotPath(slot)));
    }

    public SaveSlotViewData GetSlotViewData(int slot)
    {
        SaveSlotViewData view = new SaveSlotViewData
        {
            slotNumber = slot,
            isOccupied = false,
            displayName = string.Empty,
            phaseOrTime = string.Empty,
            timestamp = string.Empty,
            playerSummary = string.Empty
        };

        SaveSlotData data = ReadSlot(slot);
        if (data == null)
        {
            return view;
        }

        view.isOccupied = true;
        view.displayName = string.IsNullOrEmpty(data.displayName) ? "Wilderness Save" : data.displayName;
        view.day = data.day;
        view.phaseOrTime = DayNightCycle.PhaseFromNormalizedTime(data.normalizedTime).ToString();
        view.timestamp = data.savedAtUtc;
        view.playerSummary = "Health " + Mathf.RoundToInt(data.health) + " | Hunger " + Mathf.RoundToInt(data.hunger);
        return view;
    }

    public bool StartNewGame(int slot)
    {
        if (!IsValidSlot(slot))
        {
            return false;
        }

        activeSlot = slot;
        EnsureStorageRoot();
        Directory.CreateDirectory(storageRoot);
        string path = SlotPath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        ResetWorldState();
        UIController.Instance?.ShowSaveSlotView(GetSlotViewData(slot));
        return true;
    }

    private SaveSlotData Capture(int slot)
    {
        SaveSlotData data = new SaveSlotData
        {
            slotNumber = slot,
            displayName = "Wilderness Day " + (dayNightCycle != null ? dayNightCycle.CurrentDay : 1),
            savedAtUtc = DateTime.UtcNow.ToString("O"),
            playerX = player.position.x,
            playerY = player.position.y,
            health = vitals != null ? vitals.Health : 100f,
            hunger = vitals != null ? vitals.Hunger : 100f,
            day = dayNightCycle != null ? dayNightCycle.CurrentDay : 1,
            normalizedTime = dayNightCycle != null ? dayNightCycle.NormalizedTime : 0f,
            resources = CaptureResources(),
            crafted = CaptureCrafted(),
            placedBuildings = buildingSystem != null ? buildingSystem.CapturePlacedBuildings() : new List<SavePlacedBuildingData>(),
            harvestedNodeIds = CaptureHarvestedNodes(),
            defeatedEnemyIds = CaptureDefeatedEnemies()
        };
        return data;
    }

    private void Restore(SaveSlotData data)
    {
        if (data == null)
        {
            return;
        }

        if (player != null)
        {
            player.position = new Vector3(data.playerX, data.playerY, player.position.z);
        }

        if (vitals != null)
        {
            vitals.SetState(data.health, data.hunger);
        }

        if (dayNightCycle != null)
        {
            dayNightCycle.SetState(data.day, data.normalizedTime);
        }

        RestoreResources(data.resources);
        RestoreCrafted(data.crafted);
        if (buildingSystem != null)
        {
            buildingSystem.RestorePlacedBuildings(data.placedBuildings);
        }

        RestoreHarvestedNodes(data.harvestedNodeIds);
        RestoreDefeatedEnemies(data.defeatedEnemyIds);
    }

    private List<SaveResourceAmountData> CaptureResources()
    {
        List<SaveResourceAmountData> entries = new List<SaveResourceAmountData>();
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            entries.Add(new SaveResourceAmountData { type = type.ToString(), amount = resources != null ? resources.Get(type) : 0 });
        }

        return entries;
    }

    private List<SaveCraftedAmountData> CaptureCrafted()
    {
        List<SaveCraftedAmountData> entries = new List<SaveCraftedAmountData>();
        foreach (CraftedItemType type in Enum.GetValues(typeof(CraftedItemType)))
        {
            entries.Add(new SaveCraftedAmountData { type = type.ToString(), amount = crafted != null ? crafted.Get(type) : 0 });
        }

        return entries;
    }

    private List<string> CaptureHarvestedNodes()
    {
        List<string> ids = new List<string>();
        GatherableResource[] gatherables = FindObjectsByType<GatherableResource>(FindObjectsSortMode.None);
        for (int i = 0; i < gatherables.Length; i++)
        {
            if (gatherables[i].IsHarvested)
            {
                ids.Add(gatherables[i].GatherableId);
            }
        }

        return ids;
    }

    private List<string> CaptureDefeatedEnemies()
    {
        List<string> ids = new List<string>();
        WildAnimalEnemy[] animals = FindObjectsByType<WildAnimalEnemy>(FindObjectsSortMode.None);
        for (int i = 0; i < animals.Length; i++)
        {
            if (animals[i].IsDefeated)
            {
                ids.Add(animals[i].AnimalId);
            }
        }

        return ids;
    }

    private void RestoreResources(List<SaveResourceAmountData> entries)
    {
        if (resources == null)
        {
            return;
        }

        resources.ClearAll();
        if (entries == null)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (Enum.TryParse(entries[i].type, out ResourceType type))
            {
                resources.Set(type, entries[i].amount);
            }
        }
    }

    private void RestoreCrafted(List<SaveCraftedAmountData> entries)
    {
        if (crafted == null)
        {
            return;
        }

        crafted.ClearAll();
        if (entries == null)
        {
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            if (Enum.TryParse(entries[i].type, out CraftedItemType type))
            {
                crafted.Set(type, entries[i].amount);
            }
        }
    }

    private static void RestoreHarvestedNodes(List<string> ids)
    {
        HashSet<string> harvested = ids != null ? new HashSet<string>(ids) : new HashSet<string>();
        GatherableResource[] gatherables = FindObjectsByType<GatherableResource>(FindObjectsSortMode.None);
        for (int i = 0; i < gatherables.Length; i++)
        {
            gatherables[i].SetHarvested(harvested.Contains(gatherables[i].GatherableId));
        }
    }

    private static void RestoreDefeatedEnemies(List<string> ids)
    {
        HashSet<string> defeated = ids != null ? new HashSet<string>(ids) : new HashSet<string>();
        WildAnimalEnemy[] animals = FindObjectsByType<WildAnimalEnemy>(FindObjectsSortMode.None);
        for (int i = 0; i < animals.Length; i++)
        {
            animals[i].SetDefeated(defeated.Contains(animals[i].AnimalId));
        }
    }

    private string SlotPath(int slot)
    {
        EnsureStorageRoot();
        return Path.Combine(storageRoot, "slot-" + slot + ".json");
    }

    private void CaptureStartingPlayerPosition()
    {
        if (!hasStartingPlayerPosition && player != null)
        {
            startingPlayerPosition = player.position;
            hasStartingPlayerPosition = true;
        }
    }

    private void ResetWorldState()
    {
        CaptureStartingPlayerPosition();
        if (player != null && hasStartingPlayerPosition)
        {
            player.position = startingPlayerPosition;
        }

        if (vitals != null)
        {
            vitals.SetState(vitals.MaxHealth, vitals.MaxHunger);
        }

        if (dayNightCycle != null)
        {
            dayNightCycle.SetState(1, 0f);
        }

        resources?.ClearAll();
        crafted?.ClearAll();
        buildingSystem?.ClearPlacedBuildings();
        RestoreHarvestedNodes(new List<string>());
        ResetWildlife();
        ResetObjectiveGuides();
        UIController.Instance?.ShowDeath(false);
    }

    private static void ResetWildlife()
    {
        WildAnimalEnemy[] animals = FindObjectsByType<WildAnimalEnemy>(FindObjectsSortMode.None);
        for (int i = 0; i < animals.Length; i++)
        {
            animals[i].ResetAnimal();
        }
    }

    private static void ResetObjectiveGuides()
    {
        ObjectiveGuide[] guides = FindObjectsByType<ObjectiveGuide>(FindObjectsSortMode.None);
        for (int i = 0; i < guides.Length; i++)
        {
            guides[i].ResetProgress();
        }
    }

    private void EnsureStorageRoot()
    {
        if (string.IsNullOrEmpty(storageRoot))
        {
            storageRoot = Path.Combine(Application.persistentDataPath, "StayAliveSaves");
        }
    }

    private static bool IsValidSlot(int slot)
    {
        return slot >= 1 && slot <= SlotCount;
    }
}

[Serializable]
public sealed class SaveSlotData
{
    public int slotNumber;
    public string displayName;
    public string savedAtUtc;
    public float playerX;
    public float playerY;
    public float health;
    public float hunger;
    public int day;
    public float normalizedTime;
    public List<SaveResourceAmountData> resources = new List<SaveResourceAmountData>();
    public List<SaveCraftedAmountData> crafted = new List<SaveCraftedAmountData>();
    public List<SavePlacedBuildingData> placedBuildings = new List<SavePlacedBuildingData>();
    public List<string> harvestedNodeIds = new List<string>();
    public List<string> defeatedEnemyIds = new List<string>();
}

[Serializable]
public sealed class SaveResourceAmountData
{
    public string type;
    public int amount;
}

[Serializable]
public sealed class SaveCraftedAmountData
{
    public string type;
    public int amount;
}

[Serializable]
public sealed class SavePlacedBuildingData
{
    public string id;
    public string type;
    public float x;
    public float y;
    public bool rotated;
}
