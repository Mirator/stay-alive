using UnityEngine;

public enum CoreLoopStage
{
    MineBlockedTunnel,
    CollectGlowCrystals,
    OpenAncientDoor,
    ReachRewardRoom,
    Complete
}

public sealed class GameLoopController : MonoBehaviour
{
    [SerializeField] private ResourceInventory playerInventory;
    [SerializeField] private AncientDoor ancientDoor;
    [SerializeField] private MineableResource firstBlocker;
    [SerializeField] private RewardRoomGoal rewardGoal;

    private int lastCrystalCount = -1;

    public static GameLoopController Instance { get; private set; }
    public CoreLoopStage CurrentStage { get; private set; } = CoreLoopStage.MineBlockedTunnel;
    public bool IsComplete => CurrentStage == CoreLoopStage.Complete;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BeginLoop();
    }

    private void Update()
    {
        EvaluateProgress();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (playerInventory != null)
        {
            playerInventory.Changed -= EvaluateProgress;
        }
    }

    public void Configure(ResourceInventory inventory, AncientDoor door, MineableResource blocker, RewardRoomGoal goal)
    {
        if (playerInventory != null)
        {
            playerInventory.Changed -= EvaluateProgress;
        }

        playerInventory = inventory;
        ancientDoor = door;
        firstBlocker = blocker;
        rewardGoal = goal;

        if (playerInventory != null)
        {
            playerInventory.Changed += EvaluateProgress;
        }

        if (rewardGoal != null)
        {
            rewardGoal.Configure(this);
        }
    }

    public void BeginLoop()
    {
        Configure(playerInventory, ancientDoor, firstBlocker, rewardGoal);
        CurrentStage = CoreLoopStage.MineBlockedTunnel;
        lastCrystalCount = -1;
        UIController.Instance?.SetObjective("Mine the weak stone and push into the crystal chamber.");
        UIController.Instance?.ShowMessage("Core loop: mine, collect 3 crystals, open the door, reach the reward room.", 5f);
        EvaluateProgress();
    }

    public void EvaluateProgress()
    {
        if (IsComplete)
        {
            return;
        }

        if (CurrentStage == CoreLoopStage.MineBlockedTunnel && IsBlockerCleared())
        {
            SetStage(CoreLoopStage.CollectGlowCrystals, "Tunnel cleared. Mine 3 Glow Crystals.");
        }

        if (CurrentStage == CoreLoopStage.CollectGlowCrystals)
        {
            UpdateCrystalObjective();

            if (HasRequiredCrystals())
            {
                SetStage(CoreLoopStage.OpenAncientDoor, "You have enough Glow Crystals. Return to the sealed door and press E.");
            }
        }

        if (CurrentStage == CoreLoopStage.OpenAncientDoor && ancientDoor != null && ancientDoor.IsOpen)
        {
            SetStage(CoreLoopStage.ReachRewardRoom, "Door open. Enter the reward room.");
        }
    }

    public void CompleteRun()
    {
        if (IsComplete)
        {
            return;
        }

        CurrentStage = CoreLoopStage.Complete;
        UIController.Instance?.SetObjective("Loop complete: reward room reached.");
        UIController.Instance?.ShowMessage("Loop complete. You mined, collected, opened the door, and reached the reward.", 8f);
    }

    private bool IsBlockerCleared()
    {
        return firstBlocker == null || firstBlocker.IsBroken || !firstBlocker.gameObject.activeInHierarchy;
    }

    private bool HasRequiredCrystals()
    {
        if (playerInventory == null || ancientDoor == null)
        {
            return false;
        }

        return playerInventory.Has(ancientDoor.RequiredResource, ancientDoor.RequiredAmount);
    }

    private void UpdateCrystalObjective()
    {
        if (playerInventory == null || ancientDoor == null)
        {
            return;
        }

        int crystals = playerInventory.Get(ancientDoor.RequiredResource);
        if (crystals == lastCrystalCount)
        {
            return;
        }

        lastCrystalCount = crystals;
        UIController.Instance?.SetObjective("Mine 3 Glow Crystals: " + Mathf.Min(crystals, ancientDoor.RequiredAmount) + "/" + ancientDoor.RequiredAmount + ".");
    }

    private void SetStage(CoreLoopStage stage, string objective)
    {
        CurrentStage = stage;
        UIController.Instance?.SetObjective(objective);
        UIController.Instance?.ShowMessage(objective, 3f);
    }
}
