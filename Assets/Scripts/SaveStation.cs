using UnityEngine;

public enum SaveStationType
{
    Campfire,
    Bedroll
}

[RequireComponent(typeof(Collider2D))]
public sealed class SaveStation : MonoBehaviour, IInteractable, IInteractionPromptProvider
{
    [SerializeField] private SaveStationType stationType = SaveStationType.Campfire;

    public SaveStationType StationType => stationType;
    public string Prompt => "Press E: save at " + stationType;

    public InteractionPromptData GetPromptData(PlayerInteraction player)
    {
        PlayerVitals vitals = player != null ? player.GetComponent<PlayerVitals>() : null;
        bool enabled = SurvivalSaveSystem.Instance != null && (vitals == null || !vitals.IsDead);
        return InteractionPromptData.Create("E", "Save at", stationType.ToString(), enabled, enabled ? string.Empty : "Unavailable");
    }

    public void Configure(SaveStationType type)
    {
        stationType = type;
    }

    public void Interact(PlayerInteraction player)
    {
        if (SurvivalSaveSystem.Instance != null && SurvivalSaveSystem.Instance.SaveActiveSlot())
        {
            UIController.Instance?.ShowMessage("Saved to slot " + SurvivalSaveSystem.Instance.ActiveSlot + ".", 1.6f);
        }
        else
        {
            UIController.Instance?.ShowMessage("Could not save here.", 1.6f);
        }
    }
}
