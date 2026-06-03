using UnityEngine;

public sealed class SurvivalGameController : MonoBehaviour
{
    [SerializeField] private PlayerVitals playerVitals;
    [SerializeField] private DayNightCycle dayNightCycle;

    public bool HasWinningCondition => false;
    public PlayerVitals PlayerVitals => playerVitals;
    public DayNightCycle DayNightCycle => dayNightCycle;

    private void Start()
    {
        UIController.Instance?.SetObjective("Survive in the wild. Gather, craft, build shelter, fight or avoid animals, and save at camp.");
        UIController.Instance?.ShowMessage("Outdoor sandbox MVP: survive the first 3 days, then keep going.", 5f);
    }

    public void Configure(PlayerVitals vitals, DayNightCycle cycle)
    {
        playerVitals = vitals;
        dayNightCycle = cycle;
    }
}
