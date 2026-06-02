using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class AncientDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private ResourceType requiredResource = ResourceType.GlowCrystal;
    [SerializeField] private int requiredAmount = 3;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private string lockedMessage = "The seal needs 3 Glow Crystals.";
    [SerializeField] private string openedMessage = "The ancient door grinds open.";

    private Collider2D blockingCollider;
    private SpriteRenderer spriteRenderer;

    public ResourceType RequiredResource => requiredResource;
    public int RequiredAmount => requiredAmount;
    public bool IsOpen { get; private set; }
    public string Prompt => IsOpen ? "Door is open" : "Press E to open sealed door";

    private void Awake()
    {
        blockingCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Configure(ResourceType resource, int amount, Sprite newOpenSprite)
    {
        requiredResource = resource;
        requiredAmount = Mathf.Max(0, amount);
        openSprite = newOpenSprite;
    }

    public void Interact(PlayerInteraction player)
    {
        ResourceInventory inventory = player != null ? player.Inventory : null;
        TryOpen(inventory);
    }

    public bool TryOpen(ResourceInventory inventory)
    {
        if (IsOpen)
        {
            UIController.Instance?.ShowMessage("The path is already open.", 1.5f);
            return true;
        }

        if (inventory != null && inventory.Has(requiredResource, requiredAmount))
        {
            Open();
            UIController.Instance?.ShowMessage(openedMessage, 3f);
            return true;
        }

        UIController.Instance?.ShowMessage(lockedMessage, 2.2f);
        return false;
    }

    public void Open()
    {
        IsOpen = true;

        if (blockingCollider == null)
        {
            blockingCollider = GetComponent<Collider2D>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (blockingCollider != null)
        {
            blockingCollider.enabled = false;
        }

        if (spriteRenderer != null)
        {
            if (openSprite != null)
            {
                spriteRenderer.sprite = openSprite;
            }

            spriteRenderer.color = new Color(0.55f, 0.8f, 0.9f, 0.72f);
        }
    }
}
