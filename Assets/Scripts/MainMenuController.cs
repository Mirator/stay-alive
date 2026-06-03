using UnityEngine;

public sealed class MainMenuController : MonoBehaviour
{
    [SerializeField] private SurvivalSaveSystem saveSystem;

    public int SlotCount => SurvivalSaveSystem.SlotCount;

    public void Configure(SurvivalSaveSystem system)
    {
        saveSystem = system;
    }

    public bool StartNewGame(int slot)
    {
        return saveSystem != null && saveSystem.StartNewGame(slot);
    }

    public bool LoadSlot(int slot)
    {
        return saveSystem != null && saveSystem.LoadSlot(slot);
    }
}
