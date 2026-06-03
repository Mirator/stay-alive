using UnityEngine;

public enum SaveStationType
{
    Campfire,
    Bedroll
}

[RequireComponent(typeof(Collider2D))]
public sealed class SaveStation : MonoBehaviour, IInteractable
{
    [SerializeField] private SaveStationType stationType = SaveStationType.Campfire;

    public SaveStationType StationType => stationType;
    public string Prompt => "Press E: save at " + stationType;

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
