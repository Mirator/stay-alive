using UnityEngine;
using UnityEngine.UI;

public enum GameMenuButtonAction
{
    ContinueGame,
    OpenNewGameSlots,
    OpenLoadGameSlots,
    BackToTitleMenu,
    NewGame,
    LoadGame,
    LoadActiveSlot,
    ConfirmOverwrite,
    CancelOverwrite,
    Resume,
    ReturnToMainMenu
}

[RequireComponent(typeof(Button))]
public sealed class GameMenuButton : MonoBehaviour
{
    [SerializeField] private MainMenuController menuController;
    [SerializeField] private GameMenuButtonAction action;
    [SerializeField] private int slot = 1;

    private void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(InvokeAction);
    }

    public void Configure(MainMenuController controller, GameMenuButtonAction newAction, int newSlot)
    {
        menuController = controller;
        action = newAction;
        slot = Mathf.Clamp(newSlot, 1, SurvivalSaveSystem.SlotCount);
    }

    private void InvokeAction()
    {
        MainMenuController controller = menuController != null ? menuController : MainMenuController.Instance;
        if (controller == null)
        {
            return;
        }

        switch (action)
        {
            case GameMenuButtonAction.ContinueGame:
                controller.ContinueGame();
                break;
            case GameMenuButtonAction.OpenNewGameSlots:
                controller.OpenNewGameSlots();
                break;
            case GameMenuButtonAction.OpenLoadGameSlots:
                controller.OpenLoadGameSlots();
                break;
            case GameMenuButtonAction.BackToTitleMenu:
                controller.BackToTitleMenu();
                break;
            case GameMenuButtonAction.NewGame:
                controller.RequestNewGame(slot);
                break;
            case GameMenuButtonAction.LoadGame:
                controller.LoadSlot(slot);
                break;
            case GameMenuButtonAction.LoadActiveSlot:
                controller.LoadActiveSlot();
                break;
            case GameMenuButtonAction.ConfirmOverwrite:
                controller.ConfirmOverwrite();
                break;
            case GameMenuButtonAction.CancelOverwrite:
                controller.CancelOverwrite();
                break;
            case GameMenuButtonAction.Resume:
                controller.ResumeGame();
                break;
            case GameMenuButtonAction.ReturnToMainMenu:
                controller.ReturnToMainMenu();
                break;
        }
    }
}
