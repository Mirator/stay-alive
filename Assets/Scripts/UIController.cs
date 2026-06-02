using UnityEngine;
using UnityEngine.UI;

public sealed class UIController : MonoBehaviour
{
    [SerializeField] private Text stoneText;
    [SerializeField] private Text glowCrystalText;
    [SerializeField] private Text rootFiberText;
    [SerializeField] private Text craftedText;
    [SerializeField] private Text objectiveText;
    [SerializeField] private Text messageText;
    [SerializeField] private Text promptText;

    private ResourceInventory inventory;
    private CraftedInventory craftedInventory;
    private float messageUntil;

    public static UIController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        SetPrompt(string.Empty, false);
        ShowMessage("Mine rocks, gather 3 Glow Crystals, and open the sealed door.", 4f);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (inventory != null)
        {
            inventory.Changed -= RefreshInventory;
        }

        if (craftedInventory != null)
        {
            craftedInventory.Changed -= RefreshCraftedInventory;
        }
    }

    private void Update()
    {
        if (messageText != null && messageText.text.Length > 0 && Time.unscaledTime > messageUntil)
        {
            messageText.text = string.Empty;
        }
    }

    public void BindInventory(ResourceInventory newInventory)
    {
        if (inventory != null)
        {
            inventory.Changed -= RefreshInventory;
        }

        inventory = newInventory;

        if (inventory != null)
        {
            inventory.Changed += RefreshInventory;
        }

        RefreshInventory();
    }

    public void BindCraftedInventory(CraftedInventory newInventory)
    {
        if (craftedInventory != null)
        {
            craftedInventory.Changed -= RefreshCraftedInventory;
        }

        craftedInventory = newInventory;

        if (craftedInventory != null)
        {
            craftedInventory.Changed += RefreshCraftedInventory;
        }

        RefreshCraftedInventory();
    }

    public void SetObjective(string text)
    {
        if (objectiveText != null)
        {
            objectiveText.text = text;
        }
    }

    public void SetPrompt(string prompt, bool visible)
    {
        if (promptText != null)
        {
            promptText.text = visible ? prompt : string.Empty;
            promptText.enabled = visible;
        }
    }

    public void ShowMessage(string message, float duration = 2.5f)
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = message;
        messageUntil = Time.unscaledTime + duration;
    }

    private void RefreshInventory()
    {
        int stone = inventory != null ? inventory.Stone : 0;
        int glowCrystal = inventory != null ? inventory.GlowCrystal : 0;
        int rootFiber = inventory != null ? inventory.RootFiber : 0;

        if (stoneText != null)
        {
            stoneText.text = "Stone: " + stone;
        }

        if (glowCrystalText != null)
        {
            glowCrystalText.text = "Glow Crystal: " + glowCrystal;
        }

        if (rootFiberText != null)
        {
            rootFiberText.text = "Root Fiber: " + rootFiber;
        }
    }

    private void RefreshCraftedInventory()
    {
        int torches = craftedInventory != null ? craftedInventory.Torches : 0;
        int markers = craftedInventory != null ? craftedInventory.StoneMarkers : 0;
        int shards = craftedInventory != null ? craftedInventory.CrystalKeyShards : 0;

        if (craftedText != null)
        {
            craftedText.text = "Torches: " + torches + " | Markers: " + markers + " | Shards: " + shards;
        }
    }
}
