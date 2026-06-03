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
    [SerializeField] private Text recipeText;
    [SerializeField] private Text buildText;
    [SerializeField] private Text saveSlotText;
    [SerializeField] private GameObject pauseHelpPanel;
    [SerializeField] private Image healthFill;
    [SerializeField] private Image hungerFill;
    [SerializeField] private GameObject deathPanel;

    private ResourceInventory inventory;
    private CraftedInventory craftedInventory;
    private PlayerVitals vitals;
    private DayNightCycle dayNightCycle;
    private ObjectiveGuide objectiveGuide;
    private float messageUntil;

    public static UIController Instance { get; private set; }
    public InteractionPromptData CurrentPrompt { get; private set; }
    public RecipeViewData CurrentRecipeView { get; private set; }
    public BuildPreviewState CurrentBuildPreview { get; private set; }
    public SaveSlotViewData CurrentSaveSlotView { get; private set; }
    public string LastMessage { get; private set; }
    public bool PauseHelpVisible => pauseHelpPanel != null && pauseHelpPanel.activeSelf;
    public bool DeathVisible => deathPanel != null && deathPanel.activeSelf;

    private void Awake()
    {
        Instance = this;
        SetPrompt(string.Empty, false);
        ShowPauseHelp(false);
        ShowMessage("Gather resources, craft tools, build shelter, and survive the night.", 4f);
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

        if (objectiveGuide != null)
        {
            objectiveGuide.Changed -= RefreshObjective;
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

    public void BindObjectiveGuide(ObjectiveGuide guide)
    {
        if (objectiveGuide != null)
        {
            objectiveGuide.Changed -= RefreshObjective;
        }

        objectiveGuide = guide;

        if (objectiveGuide != null)
        {
            objectiveGuide.Changed += RefreshObjective;
        }

        RefreshObjective();
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
        SetPrompt(visible ? InteractionPromptData.FromLegacyPrompt(prompt) : null);
    }

    public void SetPrompt(InteractionPromptData prompt)
    {
        CurrentPrompt = prompt;
        if (promptText != null)
        {
            promptText.text = prompt != null ? prompt.DisplayText : string.Empty;
            promptText.enabled = prompt != null;
        }
    }

    public void ShowMessage(string message, float duration = 2.5f)
    {
        LastMessage = message;
        if (messageText == null)
        {
            return;
        }

        messageText.text = message;
        messageUntil = Time.unscaledTime + duration;
    }

    public void ShowRecipeView(RecipeViewData view)
    {
        CurrentRecipeView = view;
        if (recipeText != null)
        {
            recipeText.text = view != null ? view.DisplayText : string.Empty;
            recipeText.enabled = view != null;
        }
    }

    public void ShowBuildPreview(BuildPreviewState preview)
    {
        CurrentBuildPreview = preview;
        if (buildText != null)
        {
            buildText.text = preview != null ? preview.DisplayText : string.Empty;
            buildText.enabled = preview != null;
        }
    }

    public void ShowSaveSlotView(SaveSlotViewData view)
    {
        CurrentSaveSlotView = view;
        if (saveSlotText != null)
        {
            saveSlotText.text = view != null ? view.DisplayText : string.Empty;
            saveSlotText.enabled = view != null;
        }
    }

    public void ShowPauseHelp(bool visible)
    {
        if (pauseHelpPanel != null)
        {
            pauseHelpPanel.SetActive(visible);
        }
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
        float healthRatio = vitals != null && vitals.MaxHealth > 0f ? Mathf.Clamp01(vitals.Health / vitals.MaxHealth) : 0f;
        float hungerRatio = vitals != null && vitals.MaxHunger > 0f ? Mathf.Clamp01(vitals.Hunger / vitals.MaxHunger) : 0f;

        if (healthFill != null)
        {
            healthFill.fillAmount = healthRatio;
        }

        if (hungerFill != null)
        {
            hungerFill.fillAmount = hungerRatio;
        }

        if (vitalsText != null)
        {
            int health = vitals != null ? Mathf.RoundToInt(vitals.Health) : 0;
            int hunger = vitals != null ? Mathf.RoundToInt(vitals.Hunger) : 0;
            vitalsText.text = "Health: " + health + " | Hunger: " + hunger + " | " + BarText(healthRatio) + " " + BarText(hungerRatio);
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

    private void RefreshObjective()
    {
        if (objectiveGuide != null)
        {
            SetObjective(objectiveGuide.CurrentText);
        }
    }

    private static string BarText(float ratio)
    {
        int filled = Mathf.RoundToInt(Mathf.Clamp01(ratio) * 8f);
        return "[" + new string('#', filled) + new string('-', 8 - filled) + "]";
    }
}
