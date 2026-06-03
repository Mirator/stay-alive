using UnityEngine;
using UnityEngine.UI;

public sealed class UIController : MonoBehaviour
{
    [SerializeField] private Text stoneText;
    [SerializeField] private Text glowCrystalText;
    [SerializeField] private Text rootFiberText;
    [SerializeField] private Text craftedText;
    [SerializeField] private Text vitalsText;
    [SerializeField] private Text dayText;
    [SerializeField] private Text objectiveText;
    [SerializeField] private Text messageText;
    [SerializeField] private Text promptText;
    [SerializeField] private GameObject deathPanel;

    private ResourceInventory inventory;
    private CraftedInventory craftedInventory;
    private PlayerVitals vitals;
    private DayNightCycle dayNightCycle;
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

        if (vitals != null)
        {
            vitals.Changed -= RefreshVitals;
        }

        if (dayNightCycle != null)
        {
            dayNightCycle.Changed -= RefreshDay;
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

    public void BindVitals(PlayerVitals newVitals)
    {
        if (vitals != null)
        {
            vitals.Changed -= RefreshVitals;
        }

        vitals = newVitals;

        if (vitals != null)
        {
            vitals.Changed += RefreshVitals;
        }

        RefreshVitals();
    }

    public void BindDayNightCycle(DayNightCycle cycle)
    {
        if (dayNightCycle != null)
        {
            dayNightCycle.Changed -= RefreshDay;
        }

        dayNightCycle = cycle;

        if (dayNightCycle != null)
        {
            dayNightCycle.Changed += RefreshDay;
        }

        RefreshDay();
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

    public void ShowDeath(bool visible)
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(visible);
        }
    }

    private void RefreshInventory()
    {
        int stone = inventory != null ? inventory.Stone : 0;
        int wood = inventory != null ? inventory.Wood : 0;
        int food = inventory != null ? inventory.Food : 0;
        int herbs = inventory != null ? inventory.Herbs : 0;

        if (stoneText != null)
        {
            stoneText.text = "Stone: " + stone;
        }

        if (glowCrystalText != null)
        {
            glowCrystalText.text = "Wood: " + wood;
        }

        if (rootFiberText != null)
        {
            rootFiberText.text = "Food: " + food + " | Herbs: " + herbs;
        }
    }

    private void RefreshCraftedInventory()
    {
        int spears = craftedInventory != null ? craftedInventory.StoneSpears : 0;
        int bandages = craftedInventory != null ? craftedInventory.Bandages : 0;
        int campfires = craftedInventory != null ? craftedInventory.Campfires : 0;
        int bedrolls = craftedInventory != null ? craftedInventory.Bedrolls : 0;

        if (craftedText != null)
        {
            craftedText.text = "Spear: " + spears + " | Bandage: " + bandages + " | Campfire: " + campfires + " | Bedroll: " + bedrolls;
        }
    }

    private void RefreshVitals()
    {
        if (vitalsText != null)
        {
            int health = vitals != null ? Mathf.RoundToInt(vitals.Health) : 0;
            int hunger = vitals != null ? Mathf.RoundToInt(vitals.Hunger) : 0;
            vitalsText.text = "Health: " + health + " | Hunger: " + hunger;
        }
    }

    private void RefreshDay()
    {
        if (dayText != null)
        {
            int day = dayNightCycle != null ? dayNightCycle.CurrentDay : 1;
            string phase = dayNightCycle != null ? dayNightCycle.CurrentPhase.ToString() : DayPhase.Day.ToString();
            dayText.text = "Day " + day + " - " + phase;
        }
    }
}
