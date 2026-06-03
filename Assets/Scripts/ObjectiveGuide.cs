using System;
using System.Linq;
using UnityEngine;

public enum ObjectiveGuideStep
{
    GatherWoodStone,
    CraftStoneSpear,
    GatherFoodHerbs,
    CraftBandage,
    CraftPlaceCampfire,
    SaveAtCampfire,
    BuildMinimalShelter,
    SurviveFirstNight,
    Complete
}

public sealed class ObjectiveGuide : MonoBehaviour
{
    [SerializeField] private ResourceInventory resources;
    [SerializeField] private CraftedInventory crafted;
    [SerializeField] private GridBuildingSystem buildingSystem;
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private SurvivalSaveSystem saveSystem;
    [SerializeField] private Transform optionalTargetMarker;

    private bool savedThisSession;
    private string currentText;

    public event Action Changed;

    public ObjectiveGuideStep CurrentStep { get; private set; } = ObjectiveGuideStep.GatherWoodStone;
    public bool IsComplete => CurrentStep == ObjectiveGuideStep.Complete;
    public string CurrentText => currentText;
    public Transform OptionalTargetMarker => optionalTargetMarker;

    private void Awake()
    {
        Subscribe();
        Evaluate();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Update()
    {
        Evaluate();
    }

    public void Configure(ResourceInventory resourceInventory, CraftedInventory craftedInventory, GridBuildingSystem gridBuilding, DayNightCycle cycle, SurvivalSaveSystem saves)
    {
        Unsubscribe();
        resources = resourceInventory;
        crafted = craftedInventory;
        buildingSystem = gridBuilding;
        dayNightCycle = cycle;
        saveSystem = saves;
        Subscribe();
        Evaluate();
    }

    public void SetOptionalTargetMarker(Transform marker)
    {
        optionalTargetMarker = marker;
        Changed?.Invoke();
    }

    public void NotifySaved()
    {
        savedThisSession = true;
        Evaluate();
    }

    public void ResetProgress()
    {
        CurrentStep = ObjectiveGuideStep.GatherWoodStone;
        savedThisSession = false;
        Evaluate();
    }

    public void Evaluate()
    {
        ObjectiveGuideStep previousStep = CurrentStep;
        string previousText = currentText;

        bool advanced;
        do
        {
            advanced = TryAdvance();
        }
        while (advanced);

        currentText = TextForStep(CurrentStep);
        UIController.Instance?.SetObjective(currentText);

        if (CurrentStep != previousStep || currentText != previousText)
        {
            Changed?.Invoke();
        }
    }

    private void Subscribe()
    {
        if (resources != null)
        {
            resources.Changed += Evaluate;
        }

        if (crafted != null)
        {
            crafted.Changed += Evaluate;
        }

        if (dayNightCycle != null)
        {
            dayNightCycle.Changed += Evaluate;
        }

        if (saveSystem != null)
        {
            saveSystem.Saved += OnSaved;
        }
    }

    private void Unsubscribe()
    {
        if (resources != null)
        {
            resources.Changed -= Evaluate;
        }

        if (crafted != null)
        {
            crafted.Changed -= Evaluate;
        }

        if (dayNightCycle != null)
        {
            dayNightCycle.Changed -= Evaluate;
        }

        if (saveSystem != null)
        {
            saveSystem.Saved -= OnSaved;
        }
    }

    private void OnSaved(int slot)
    {
        NotifySaved();
    }

    private bool TryAdvance()
    {
        switch (CurrentStep)
        {
            case ObjectiveGuideStep.GatherWoodStone:
                return AdvanceIf(resources != null && resources.Wood >= 2 && resources.Stone >= 1, ObjectiveGuideStep.CraftStoneSpear);
            case ObjectiveGuideStep.CraftStoneSpear:
                return AdvanceIf(crafted != null && crafted.StoneSpears >= 1, ObjectiveGuideStep.GatherFoodHerbs);
            case ObjectiveGuideStep.GatherFoodHerbs:
                return AdvanceIf(resources != null && resources.Food >= 1 && resources.Herbs >= 2, ObjectiveGuideStep.CraftBandage);
            case ObjectiveGuideStep.CraftBandage:
                return AdvanceIf(crafted != null && crafted.Bandages >= 1, ObjectiveGuideStep.CraftPlaceCampfire);
            case ObjectiveGuideStep.CraftPlaceCampfire:
                return AdvanceIf(HasPlaced(BuildableType.Campfire), ObjectiveGuideStep.SaveAtCampfire);
            case ObjectiveGuideStep.SaveAtCampfire:
                return AdvanceIf(savedThisSession, ObjectiveGuideStep.BuildMinimalShelter);
            case ObjectiveGuideStep.BuildMinimalShelter:
                return AdvanceIf(HasMinimalShelter(), ObjectiveGuideStep.SurviveFirstNight);
            case ObjectiveGuideStep.SurviveFirstNight:
                return AdvanceIf(dayNightCycle != null && dayNightCycle.CurrentDay >= 2, ObjectiveGuideStep.Complete);
            default:
                return false;
        }
    }

    private bool AdvanceIf(bool condition, ObjectiveGuideStep next)
    {
        if (!condition)
        {
            return false;
        }

        CurrentStep = next;
        return true;
    }

    private bool HasPlaced(BuildableType type)
    {
        return buildingSystem != null && buildingSystem.PlacedBuildables.Any(b => b != null && b.BuildableType == type);
    }

    private bool HasMinimalShelter()
    {
        if (buildingSystem == null)
        {
            return false;
        }

        int floors = 0;
        int walls = 0;
        int doors = 0;
        foreach (BuildableObject buildable in buildingSystem.PlacedBuildables)
        {
            if (buildable == null)
            {
                continue;
            }

            if (buildable.BuildableType == BuildableType.Floor)
            {
                floors++;
            }
            else if (buildable.BuildableType == BuildableType.Wall)
            {
                walls++;
            }
            else if (buildable.BuildableType == BuildableType.Door)
            {
                doors++;
            }
        }

        return floors >= 1 && walls >= 2 && doors >= 1;
    }

    private static string TextForStep(ObjectiveGuideStep step)
    {
        switch (step)
        {
            case ObjectiveGuideStep.GatherWoodStone:
                return "Gather Wood and Stone for your first tool.";
            case ObjectiveGuideStep.CraftStoneSpear:
                return "Craft a Stone Spear at the Workbench.";
            case ObjectiveGuideStep.GatherFoodHerbs:
                return "Gather Food and Herbs before nightfall.";
            case ObjectiveGuideStep.CraftBandage:
                return "Craft a Bandage at the Workbench.";
            case ObjectiveGuideStep.CraftPlaceCampfire:
                return "Craft and place a Campfire in the build area.";
            case ObjectiveGuideStep.SaveAtCampfire:
                return "Save at a Campfire or Bedroll.";
            case ObjectiveGuideStep.BuildMinimalShelter:
                return "Build a small shelter: floor, two walls, and a door.";
            case ObjectiveGuideStep.SurviveFirstNight:
                return "Survive the first night. Avoid or fight wildlife.";
            default:
                return "Keep surviving: gather, craft, build, save, and prepare for night.";
        }
    }
}
