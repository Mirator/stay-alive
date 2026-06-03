using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public sealed class GridBuildingSystem : MonoBehaviour
{
    [SerializeField] private Rect buildBounds = new Rect(-8f, -7f, 16f, 12f);
    [SerializeField] private Sprite floorSprite;
    [SerializeField] private Sprite wallSprite;
    [SerializeField] private Sprite doorSprite;
    [SerializeField] private Sprite campfireSprite;
    [SerializeField] private Sprite bedrollSprite;
    [SerializeField] private Sprite storageBoxSprite;
    [SerializeField] private Sprite workbenchSprite;

    private readonly List<BuildableObject> placed = new List<BuildableObject>();
    private ResourceInventory resources;
    private CraftedInventory crafted;
    private Transform placedParent;
    private BuildableType selectedType = BuildableType.Floor;
    private bool hasSelection;
    private bool doorRotated;
    private int nextBuildId = 1;

    public Rect BuildBounds => buildBounds;
    public BuildableType SelectedType => selectedType;
    public bool HasSelection => hasSelection;
    public bool DoorRotated => doorRotated;

    private void Awake()
    {
        resources = GetComponent<ResourceInventory>();
        crafted = GetComponent<CraftedInventory>();
    }

    private void Update()
    {
        ReadBuildInput();
    }

    public void Configure(Rect newBuildBounds, Sprite floor, Sprite wall, Sprite door, Sprite campfire, Sprite bedroll, Sprite storage, Sprite workbench)
    {
        buildBounds = newBuildBounds;
        floorSprite = floor;
        wallSprite = wall;
        doorSprite = door;
        campfireSprite = campfire;
        bedrollSprite = bedroll;
        storageBoxSprite = storage;
        workbenchSprite = workbench;
    }

    public void SetPlacedParent(Transform parent)
    {
        placedParent = parent;
    }

    public IReadOnlyList<BuildableObject> PlacedBuildables => placed;

    public void Select(BuildableType type)
    {
        selectedType = type;
        hasSelection = true;
        UIController.Instance?.ShowMessage("Selected " + type + ".", 1f);
        UIController.Instance?.ShowBuildPreview(GetPreviewState(transform.position));
    }

    public void RotateSelection()
    {
        if (selectedType == BuildableType.Door)
        {
            doorRotated = !doorRotated;
            UIController.Instance?.ShowMessage("Door rotated.", 0.8f);
            UIController.Instance?.ShowBuildPreview(GetPreviewState(transform.position));
        }
    }

    public void CancelPlacement()
    {
        hasSelection = false;
        UIController.Instance?.ShowMessage("Build canceled.", 0.8f);
        UIController.Instance?.ShowBuildPreview(null);
    }

    public bool TryPlaceSelected(Vector2 worldPosition, out GameObject placedObject)
    {
        placedObject = null;
        return hasSelection && TryPlace(selectedType, worldPosition, selectedType == BuildableType.Door && doorRotated, out placedObject);
    }

    public bool TryPlace(BuildableType type, Vector2 worldPosition, bool rotated, out GameObject placedObject)
    {
        EnsureInventories();
        placedObject = null;
        Vector2 snapped = Snap(worldPosition);
        string invalidReason = GetInvalidPlacementReason(type, snapped);
        if (!string.IsNullOrEmpty(invalidReason))
        {
            UIController.Instance?.ShowMessage("Cannot build there: " + invalidReason + ".", 1.6f);
            UIController.Instance?.ShowBuildPreview(GetPreviewState(type, snapped, rotated));
            return false;
        }

        if (!SpendCost(type))
        {
            UIController.Instance?.ShowMessage("Not enough materials for " + type + ".", 1.3f);
            UIController.Instance?.ShowBuildPreview(GetPreviewState(type, snapped, rotated));
            return false;
        }

        placedObject = CreatePlacedObject(type, snapped, rotated, "build-" + nextBuildId++);
        UIController.Instance?.ShowMessage("Built " + type + ".", 1.2f);
        UIController.Instance?.ShowBuildPreview(GetPreviewState(type, snapped, rotated));
        return true;
    }

    public bool IsValidPlacement(BuildableType type, Vector2 worldPosition)
    {
        return string.IsNullOrEmpty(GetInvalidPlacementReason(type, worldPosition));
    }

    public BuildPreviewState GetPreviewState(Vector2 worldPosition)
    {
        return GetPreviewState(selectedType, worldPosition, selectedType == BuildableType.Door && doorRotated);
    }

    public BuildPreviewState GetPreviewState(BuildableType type, Vector2 worldPosition, bool rotated)
    {
        EnsureInventories();
        Vector2 snapped = Snap(worldPosition);
        string reason = GetInvalidPlacementReason(type, snapped);
        if (string.IsNullOrEmpty(reason) && !CanAffordCost(type))
        {
            reason = "Need " + CostText(type);
        }

        return new BuildPreviewState
        {
            selectedBuildable = type,
            selectedLabel = type.ToString(),
            snappedPosition = snapped,
            isValid = string.IsNullOrEmpty(reason),
            invalidReason = reason,
            costText = CostText(type),
            rotated = rotated
        };
    }

    public string GetInvalidPlacementReason(BuildableType type, Vector2 worldPosition)
    {
        Vector2 snapped = Snap(worldPosition);
        if (!buildBounds.Contains(snapped))
        {
            return "outside build area";
        }

        Collider2D[] overlaps = Physics2D.OverlapBoxAll(snapped, new Vector2(0.82f, 0.82f), 0f);
        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider2D overlap = overlaps[i];
            if (overlap == null || !overlap.enabled || overlap.isTrigger)
            {
                continue;
            }

            if (overlap.GetComponentInParent<PlayerController2D>() != null)
            {
                continue;
            }

            if (type == BuildableType.Floor && overlap.GetComponentInParent<BuildableObject>() != null)
            {
                continue;
            }

            return "blocked by " + DescribeOverlap(overlap);
        }

        return string.Empty;
    }

    public List<SavePlacedBuildingData> CapturePlacedBuildings()
    {
        List<SavePlacedBuildingData> data = new List<SavePlacedBuildingData>();
        for (int i = 0; i < placed.Count; i++)
        {
            BuildableObject buildable = placed[i];
            if (buildable == null)
            {
                continue;
            }

            data.Add(new SavePlacedBuildingData
            {
                id = buildable.BuildableId,
                type = buildable.BuildableType.ToString(),
                x = buildable.transform.position.x,
                y = buildable.transform.position.y,
                rotated = buildable.Rotated
            });
        }

        return data;
    }

    public void RestorePlacedBuildings(List<SavePlacedBuildingData> data)
    {
        ClearPlacedBuildings();
        if (data == null)
        {
            return;
        }

        for (int i = 0; i < data.Count; i++)
        {
            SavePlacedBuildingData entry = data[i];
            if (System.Enum.TryParse(entry.type, out BuildableType type))
            {
                CreatePlacedObject(type, new Vector2(entry.x, entry.y), entry.rotated, entry.id);
            }
        }
    }

    public void ClearPlacedBuildings()
    {
        for (int i = placed.Count - 1; i >= 0; i--)
        {
            if (placed[i] != null)
            {
                DestroyPlacedObject(placed[i].gameObject);
            }
        }

        placed.Clear();
    }

    private void ReadBuildInput()
    {
        UnityEngine.InputSystem.Keyboard keyboard = UnityEngine.InputSystem.Keyboard.current;
        UnityEngine.InputSystem.Mouse mouse = UnityEngine.InputSystem.Mouse.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame) Select(BuildableType.Floor);
        if (keyboard.digit2Key.wasPressedThisFrame) Select(BuildableType.Wall);
        if (keyboard.digit3Key.wasPressedThisFrame) Select(BuildableType.Door);
        if (keyboard.digit4Key.wasPressedThisFrame) Select(BuildableType.Campfire);
        if (keyboard.digit5Key.wasPressedThisFrame) Select(BuildableType.Bedroll);
        if (keyboard.digit6Key.wasPressedThisFrame) Select(BuildableType.StorageBox);
        if (keyboard.digit7Key.wasPressedThisFrame) Select(BuildableType.Workbench);
        if (keyboard.rKey.wasPressedThisFrame) RotateSelection();
        if (keyboard.xKey.wasPressedThisFrame) CancelPlacement();

        if (mouse != null && hasSelection)
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                Vector2 mousePosition = mouse.position.ReadValue();
                Vector3 world = camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -camera.transform.position.z));
                UIController.Instance?.ShowBuildPreview(GetPreviewState(world));
                if (mouse.rightButton.wasPressedThisFrame)
                {
                    TryPlaceSelected(world, out _);
                }
            }
        }
    }

    private GameObject CreatePlacedObject(BuildableType type, Vector2 position, bool rotated, string id)
    {
        GameObject gameObject = new GameObject("Placed " + type);
        gameObject.transform.position = position;
        if (placedParent != null)
        {
            gameObject.transform.SetParent(placedParent);
        }

        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = SpriteFor(type);
        renderer.sortingOrder = type == BuildableType.Floor ? -30 : Mathf.RoundToInt(-position.y * 10f) + 25;

        if (type == BuildableType.Wall || type == BuildableType.Door)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = type == BuildableType.Door && rotated ? new Vector2(0.35f, 0.9f) : new Vector2(0.9f, 0.35f);
        }

        if (type == BuildableType.Campfire)
        {
            CircleCollider2D trigger = gameObject.AddComponent<CircleCollider2D>();
            trigger.radius = 0.55f;
            trigger.isTrigger = true;
            SaveStation station = gameObject.AddComponent<SaveStation>();
            station.Configure(SaveStationType.Campfire);
            AddPointLight(gameObject.transform, "Campfire Light", new Color(1f, 0.42f, 0.16f), 1.8f, 3.4f, 0.55f);
        }
        else if (type == BuildableType.Bedroll)
        {
            CircleCollider2D trigger = gameObject.AddComponent<CircleCollider2D>();
            trigger.radius = 0.55f;
            trigger.isTrigger = true;
            SaveStation station = gameObject.AddComponent<SaveStation>();
            station.Configure(SaveStationType.Bedroll);
        }
        else if (type == BuildableType.StorageBox)
        {
            CircleCollider2D trigger = gameObject.AddComponent<CircleCollider2D>();
            trigger.radius = 0.55f;
            trigger.isTrigger = true;
            BasicInteractable storage = gameObject.AddComponent<BasicInteractable>();
            storage.Configure("Press E: inspect storage", "Storage is ready for supplies.");
        }
        else if (type == BuildableType.Workbench)
        {
            CircleCollider2D trigger = gameObject.AddComponent<CircleCollider2D>();
            trigger.radius = 0.65f;
            trigger.isTrigger = true;
            gameObject.AddComponent<CraftingStation>();
        }

        BuildableObject buildable = gameObject.AddComponent<BuildableObject>();
        buildable.Configure(id, type, rotated);
        placed.Add(buildable);
        return gameObject;
    }

    private bool SpendCost(BuildableType type)
    {
        EnsureInventories();
        if (resources == null || crafted == null)
        {
            return false;
        }

        switch (type)
        {
            case BuildableType.Floor:
                return resources.Spend(ResourceType.Wood, 1);
            case BuildableType.Wall:
                return resources.Spend(ResourceType.Wood, 2);
            case BuildableType.Door:
                if (!resources.Has(ResourceType.Wood, 2) || !resources.Has(ResourceType.Stone, 1))
                {
                    return false;
                }

                resources.Spend(ResourceType.Wood, 2);
                resources.Spend(ResourceType.Stone, 1);
                return true;
            case BuildableType.Campfire:
                return crafted.Spend(CraftedItemType.Campfire, 1);
            case BuildableType.Bedroll:
                return crafted.Spend(CraftedItemType.Bedroll, 1);
            case BuildableType.StorageBox:
                return crafted.Spend(CraftedItemType.StorageBox, 1);
            case BuildableType.Workbench:
                return crafted.Spend(CraftedItemType.Workbench, 1);
            default:
                return false;
        }
    }

    private bool CanAffordCost(BuildableType type)
    {
        EnsureInventories();
        if (resources == null || crafted == null)
        {
            return false;
        }

        switch (type)
        {
            case BuildableType.Floor:
                return resources.Has(ResourceType.Wood, 1);
            case BuildableType.Wall:
                return resources.Has(ResourceType.Wood, 2);
            case BuildableType.Door:
                return resources.Has(ResourceType.Wood, 2) && resources.Has(ResourceType.Stone, 1);
            case BuildableType.Campfire:
                return crafted.Has(CraftedItemType.Campfire, 1);
            case BuildableType.Bedroll:
                return crafted.Has(CraftedItemType.Bedroll, 1);
            case BuildableType.StorageBox:
                return crafted.Has(CraftedItemType.StorageBox, 1);
            case BuildableType.Workbench:
                return crafted.Has(CraftedItemType.Workbench, 1);
            default:
                return false;
        }
    }

    private static string CostText(BuildableType type)
    {
        switch (type)
        {
            case BuildableType.Floor:
                return "1 Wood";
            case BuildableType.Wall:
                return "2 Wood";
            case BuildableType.Door:
                return "2 Wood + 1 Stone";
            case BuildableType.Campfire:
                return "1 Campfire";
            case BuildableType.Bedroll:
                return "1 Bedroll";
            case BuildableType.StorageBox:
                return "1 Storage Box";
            case BuildableType.Workbench:
                return "1 Workbench";
            default:
                return "Unknown";
        }
    }

    private static string DescribeOverlap(Collider2D overlap)
    {
        if (overlap.GetComponentInParent<GatherableResource>() != null)
        {
            return "resource";
        }

        if (overlap.GetComponentInParent<WildAnimalEnemy>() != null)
        {
            return "wildlife";
        }

        if (overlap.GetComponentInParent<BuildableObject>() != null)
        {
            return "building";
        }

        return string.IsNullOrEmpty(overlap.name) ? "terrain" : overlap.name;
    }

    private void EnsureInventories()
    {
        if (resources == null)
        {
            resources = GetComponent<ResourceInventory>();
        }

        if (crafted == null)
        {
            crafted = GetComponent<CraftedInventory>();
        }
    }

    private Sprite SpriteFor(BuildableType type)
    {
        switch (type)
        {
            case BuildableType.Floor:
                return floorSprite;
            case BuildableType.Wall:
                return wallSprite;
            case BuildableType.Door:
                return doorSprite;
            case BuildableType.Campfire:
                return campfireSprite;
            case BuildableType.Bedroll:
                return bedrollSprite;
            case BuildableType.StorageBox:
                return storageBoxSprite;
            case BuildableType.Workbench:
                return workbenchSprite;
            default:
                return null;
        }
    }

    private static Vector2 Snap(Vector2 worldPosition)
    {
        return new Vector2(Mathf.Round(worldPosition.x), Mathf.Round(worldPosition.y));
    }

    private static void DestroyPlacedObject(GameObject gameObject)
    {
        if (Application.isPlaying)
        {
            Destroy(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    private static void AddPointLight(Transform parent, string name, Color color, float intensity, float outerRadius, float innerRadius)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent, false);
        Light2D light = lightObject.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.pointLightOuterRadius = outerRadius;
        light.pointLightInnerRadius = innerRadius;
    }
}
