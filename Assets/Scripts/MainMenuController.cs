using UnityEngine;
using UnityEngine.UI;

public enum GameMenuSlotSelectionMode
{
    None,
    NewGame,
    LoadGame
}

public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private SurvivalSaveSystem saveSystem;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject titleMenuPanel;
    [SerializeField] private GameObject slotSelectionPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject overwritePanel;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Text slotSelectionTitle;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button[] newSlotButtons = new Button[SurvivalSaveSystem.SlotCount];
    [SerializeField] private Text[] slotSummaryTexts = new Text[SurvivalSaveSystem.SlotCount];
    [SerializeField] private Button[] loadSlotButtons = new Button[SurvivalSaveSystem.SlotCount];
    [SerializeField] private Text overwriteText;

    private int pendingOverwriteSlot;
    private GameMenuSlotSelectionMode slotSelectionMode;

    public static MainMenuController Instance { get; private set; }

    public int SlotCount => SurvivalSaveSystem.SlotCount;
    public bool MainMenuVisible => mainMenuPanel != null && mainMenuPanel.activeSelf;
    public bool TitleMenuVisible => MainMenuVisible && titleMenuPanel != null && titleMenuPanel.activeSelf;
    public bool SlotSelectionVisible => MainMenuVisible && slotSelectionPanel != null && slotSelectionPanel.activeSelf;
    public bool PauseMenuVisible => pauseMenuPanel != null && pauseMenuPanel.activeSelf;
    public bool OverwriteConfirmationVisible => overwritePanel != null && overwritePanel.activeSelf;
    public bool DeathMenuVisible => deathPanel != null && deathPanel.activeSelf;
    public int PendingOverwriteSlot => pendingOverwriteSlot;
    public GameMenuSlotSelectionMode SlotSelectionMode => slotSelectionMode;

    private void Awake()
    {
        Instance = this;
        if (Application.isPlaying)
        {
            RefreshSlotViews();
            ShowMainMenu();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Configure(SurvivalSaveSystem system)
    {
        saveSystem = system;
    }

    public void ConfigureUi(
        GameObject mainPanel,
        GameObject titlePanel,
        GameObject slotPanel,
        GameObject pausePanel,
        GameObject overwriteConfirmPanel,
        GameObject deathMenuPanel,
        Text slotTitle,
        Button continueMenuButton,
        Button[] newButtons,
        Text[] slotTexts,
        Button[] loadButtons,
        Text overwriteLabel)
    {
        mainMenuPanel = mainPanel;
        titleMenuPanel = titlePanel;
        slotSelectionPanel = slotPanel;
        pauseMenuPanel = pausePanel;
        overwritePanel = overwriteConfirmPanel;
        deathPanel = deathMenuPanel;
        slotSelectionTitle = slotTitle;
        continueButton = continueMenuButton;
        newSlotButtons = newButtons;
        slotSummaryTexts = slotTexts;
        loadSlotButtons = loadButtons;
        overwriteText = overwriteLabel;
        RefreshSlotViews();

        if (!Application.isPlaying)
        {
            SetPanel(mainMenuPanel, true);
            SetPanel(titleMenuPanel, true);
            SetPanel(slotSelectionPanel, false);
            SetPanel(pauseMenuPanel, false);
            SetPanel(overwritePanel, false);
            SetPanel(deathPanel, false);
        }
    }

    public bool ContinueGame()
    {
        int slot = FindContinueSlot();
        if (slot < 1)
        {
            UIController.Instance?.ShowMessage("No save to continue.", 1.5f);
            RefreshSlotViews();
            return false;
        }

        bool loaded = LoadSlot(slot);
        if (loaded)
        {
            UIController.Instance?.ShowMessage("Continued slot " + slot + ".", 1.5f);
        }

        return loaded;
    }

    public void OpenNewGameSlots()
    {
        ShowSlotSelection(GameMenuSlotSelectionMode.NewGame);
        UIController.Instance?.ShowMessage("Choose a slot for New Game.", 1.2f);
    }

    public void OpenLoadGameSlots()
    {
        ShowSlotSelection(GameMenuSlotSelectionMode.LoadGame);
        UIController.Instance?.ShowMessage("Choose a slot to Load.", 1.2f);
    }

    public void BackToTitleMenu()
    {
        slotSelectionMode = GameMenuSlotSelectionMode.None;
        SetPanel(mainMenuPanel, true);
        SetPanel(titleMenuPanel, true);
        SetPanel(slotSelectionPanel, false);
        SetPanel(overwritePanel, false);
        pendingOverwriteSlot = 0;
        Time.timeScale = 0f;
        RefreshSlotViews();
        UIController.Instance?.ShowMessage("Main menu.", 1f);
    }

    public bool StartNewGame(int slot)
    {
        if (saveSystem == null || !saveSystem.StartNewGame(slot))
        {
            UIController.Instance?.ShowMessage("Could not start slot " + slot + ".", 1.6f);
            return false;
        }

        pendingOverwriteSlot = 0;
        HideMenus();
        RefreshSlotViews();
        UIController.Instance?.ShowMessage("Started new game in slot " + slot + ".", 1.8f);
        return true;
    }

    public bool RequestNewGame(int slot)
    {
        if (saveSystem == null || slot < 1 || slot > SurvivalSaveSystem.SlotCount)
        {
            return false;
        }

        if (saveSystem.HasSlot(slot))
        {
            pendingOverwriteSlot = slot;
            ShowOverwriteConfirmation(slot);
            return false;
        }

        return StartNewGame(slot);
    }

    public bool ConfirmOverwrite()
    {
        if (pendingOverwriteSlot < 1)
        {
            return false;
        }

        return StartNewGame(pendingOverwriteSlot);
    }

    public void CancelOverwrite()
    {
        pendingOverwriteSlot = 0;
        if (overwritePanel != null)
        {
            overwritePanel.SetActive(false);
        }

        UIController.Instance?.ShowMessage("Overwrite canceled.", 1f);
    }

    public bool LoadActiveSlot()
    {
        if (saveSystem == null)
        {
            return false;
        }

        return LoadSlot(saveSystem.ActiveSlot);
    }

    public bool LoadSlot(int slot)
    {
        if (saveSystem == null || !saveSystem.LoadSlot(slot))
        {
            UIController.Instance?.ShowMessage("No save in slot " + slot + ".", 1.5f);
            RefreshSlotViews();
            return false;
        }

        HideMenus();
        RefreshSlotViews();
        UIController.Instance?.ShowMessage("Loaded slot " + slot + ".", 1.5f);
        return true;
    }

    public SaveSlotViewData GetSlotViewData(int slot)
    {
        if (saveSystem == null)
        {
            return new SaveSlotViewData { slotNumber = slot };
        }

        return saveSystem.GetSlotViewData(slot);
    }

    public void ShowMainMenu()
    {
        slotSelectionMode = GameMenuSlotSelectionMode.None;
        RefreshSlotViews();
        SetPanel(mainMenuPanel, true);
        SetPanel(titleMenuPanel, true);
        SetPanel(slotSelectionPanel, false);
        SetPanel(pauseMenuPanel, false);
        SetPanel(overwritePanel, false);
        SetPanel(deathPanel, false);
        Time.timeScale = 0f;
        UIController.Instance?.ShowDeath(false);
        UIController.Instance?.ShowPauseHelp(false);
    }

    public void TogglePauseMenu()
    {
        if (DeathMenuVisible)
        {
            return;
        }

        if (OverwriteConfirmationVisible)
        {
            CancelOverwrite();
            return;
        }

        if (MainMenuVisible)
        {
            return;
        }

        if (PauseMenuVisible)
        {
            ResumeGame();
            return;
        }

        ShowPauseMenu();
    }

    public void ShowPauseMenu()
    {
        RefreshSlotViews();
        SetPanel(mainMenuPanel, false);
        SetPanel(pauseMenuPanel, true);
        SetPanel(overwritePanel, false);
        SetPanel(deathPanel, false);
        Time.timeScale = 0f;
        UIController.Instance?.ShowDeath(false);
        UIController.Instance?.ShowPauseHelp(true);
        UIController.Instance?.ShowMessage("Paused", 1.2f);
    }

    public void ShowDeathMenu()
    {
        RefreshSlotViews();
        SetPanel(mainMenuPanel, false);
        SetPanel(pauseMenuPanel, false);
        SetPanel(overwritePanel, false);
        SetPanel(deathPanel, true);
        Time.timeScale = 0f;
        UIController.Instance?.ShowPauseHelp(false);
        UIController.Instance?.ShowDeath(true);
    }

    public void ResumeGame()
    {
        HideMenus();
        UIController.Instance?.ShowMessage("Back to the wild.", 1.2f);
    }

    public void ReturnToMainMenu()
    {
        ShowMainMenu();
    }

    public void RefreshSlotViews()
    {
        for (int i = 0; i < SurvivalSaveSystem.SlotCount; i++)
        {
            int slot = i + 1;
            SaveSlotViewData view = GetSlotViewData(slot);
            if (slotSummaryTexts != null && i < slotSummaryTexts.Length && slotSummaryTexts[i] != null)
            {
                slotSummaryTexts[i].text = view.DisplayText;
            }

            if (newSlotButtons != null && i < newSlotButtons.Length && newSlotButtons[i] != null)
            {
                newSlotButtons[i].gameObject.SetActive(slotSelectionMode == GameMenuSlotSelectionMode.NewGame);
                newSlotButtons[i].interactable = slotSelectionMode == GameMenuSlotSelectionMode.NewGame;
            }

            if (loadSlotButtons != null && i < loadSlotButtons.Length && loadSlotButtons[i] != null)
            {
                loadSlotButtons[i].gameObject.SetActive(slotSelectionMode == GameMenuSlotSelectionMode.LoadGame);
                loadSlotButtons[i].interactable = slotSelectionMode == GameMenuSlotSelectionMode.LoadGame && view.isOccupied;
            }
        }

        if (continueButton != null)
        {
            continueButton.interactable = FindContinueSlot() > 0;
        }

        if (slotSelectionTitle != null)
        {
            slotSelectionTitle.text = slotSelectionMode == GameMenuSlotSelectionMode.NewGame
                ? "New Game: Choose Slot"
                : slotSelectionMode == GameMenuSlotSelectionMode.LoadGame
                    ? "Load Game: Choose Slot"
                    : string.Empty;
        }
    }

    private void HideMenus()
    {
        SetPanel(mainMenuPanel, false);
        SetPanel(titleMenuPanel, false);
        SetPanel(slotSelectionPanel, false);
        SetPanel(pauseMenuPanel, false);
        SetPanel(overwritePanel, false);
        SetPanel(deathPanel, false);
        slotSelectionMode = GameMenuSlotSelectionMode.None;
        UIController.Instance?.ShowPauseHelp(false);
        UIController.Instance?.ShowDeath(false);
        Time.timeScale = 1f;
    }

    private void ShowSlotSelection(GameMenuSlotSelectionMode mode)
    {
        slotSelectionMode = mode;
        SetPanel(mainMenuPanel, true);
        SetPanel(titleMenuPanel, false);
        SetPanel(slotSelectionPanel, true);
        SetPanel(pauseMenuPanel, false);
        SetPanel(overwritePanel, false);
        SetPanel(deathPanel, false);
        Time.timeScale = 0f;
        UIController.Instance?.ShowDeath(false);
        UIController.Instance?.ShowPauseHelp(false);
        RefreshSlotViews();
    }

    private void ShowOverwriteConfirmation(int slot)
    {
        if (overwriteText != null)
        {
            overwriteText.text = "Overwrite Slot " + slot + "?";
        }

        SetPanel(pauseMenuPanel, false);
        SetPanel(deathPanel, false);
        SetPanel(overwritePanel, true);
        Time.timeScale = 0f;
        UIController.Instance?.ShowPauseHelp(false);
        UIController.Instance?.ShowDeath(false);
    }

    private int FindContinueSlot()
    {
        if (saveSystem == null)
        {
            return 0;
        }

        int fallbackSlot = 0;
        int bestSlot = 0;
        string bestTimestamp = string.Empty;
        for (int slot = 1; slot <= SurvivalSaveSystem.SlotCount; slot++)
        {
            SaveSlotData data = saveSystem.ReadSlot(slot);
            if (data == null)
            {
                continue;
            }

            if (fallbackSlot == 0)
            {
                fallbackSlot = slot;
            }

            string timestamp = data.savedAtUtc ?? string.Empty;
            if (!string.IsNullOrEmpty(timestamp) && (bestSlot == 0 || string.CompareOrdinal(timestamp, bestTimestamp) > 0))
            {
                bestSlot = slot;
                bestTimestamp = timestamp;
            }
        }

        return bestSlot > 0 ? bestSlot : fallbackSlot;
    }

    private static void SetPanel(GameObject panel, bool visible)
    {
        if (panel != null)
        {
            panel.SetActive(visible);
        }
    }
}
