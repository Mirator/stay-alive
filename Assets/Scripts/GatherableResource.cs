using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class GatherableResource : MonoBehaviour, IInteractable
{
    [SerializeField] private string gatherableId;
    [SerializeField] private ResourceType resourceType = ResourceType.Wood;
    [SerializeField] private int amount = 1;
    [SerializeField] private string displayName = "Resource";

    public string GatherableId => string.IsNullOrEmpty(gatherableId) ? name : gatherableId;
    public ResourceType ResourceType => resourceType;
    public int Amount => amount;
    public bool IsHarvested { get; private set; }
    public string Prompt => IsHarvested ? displayName + " depleted" : "Press E: gather " + displayName;

    public void Configure(string id, string newDisplayName, ResourceType type, int newAmount)
    {
        gatherableId = id;
        displayName = newDisplayName;
        resourceType = type;
        amount = Mathf.Max(1, newAmount);
    }

    public void Interact(PlayerInteraction player)
    {
        ResourceInventory inventory = player != null ? player.Inventory : null;
        Harvest(inventory);
    }

    public bool Harvest(ResourceInventory inventory)
    {
        if (IsHarvested || inventory == null)
        {
            return false;
        }

        inventory.Add(resourceType, amount);
        SetHarvested(true);
        UIController.Instance?.ShowMessage("Gathered " + amount + " " + ResourceLabel(resourceType) + ".", 1.5f);
        return true;
    }

    public void SetHarvested(bool harvested)
    {
        IsHarvested = harvested;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = harvested ? new Color(0.45f, 0.45f, 0.45f, 0.45f) : Color.white;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = !harvested;
        }
    }

    private static string ResourceLabel(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Wood:
                return "Wood";
            case ResourceType.Stone:
                return "Stone";
            case ResourceType.Food:
                return "Food";
            case ResourceType.Herbs:
                return "Herbs";
            default:
                return "Resource";
        }
    }
}
