using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class InteractionPromptData
{
    public string inputLabel;
    public string actionLabel;
    public string targetName;
    public bool isEnabled = true;
    public string disabledReason;

    public string DisplayText
    {
        get
        {
            string text = string.IsNullOrEmpty(inputLabel) ? actionLabel : inputLabel + " " + actionLabel;
            if (!string.IsNullOrEmpty(targetName))
            {
                text += " " + targetName;
            }

            if (!isEnabled && !string.IsNullOrEmpty(disabledReason))
            {
                text += " - " + disabledReason;
            }

            return text.Trim();
        }
    }

    public static InteractionPromptData Create(string input, string action, string target, bool enabled = true, string reason = "")
    {
        return new InteractionPromptData
        {
            inputLabel = input,
            actionLabel = action,
            targetName = target,
            isEnabled = enabled,
            disabledReason = reason
        };
    }

    public static InteractionPromptData FromLegacyPrompt(string prompt)
    {
        return new InteractionPromptData
        {
            inputLabel = string.Empty,
            actionLabel = prompt,
            targetName = string.Empty,
            isEnabled = true,
            disabledReason = string.Empty
        };
    }
}

public interface IInteractionPromptProvider
{
    InteractionPromptData GetPromptData(PlayerInteraction player);
}

[Serializable]
public sealed class ResourceCostViewData
{
    public string resourceType;
    public string label;
    public int required;
    public int owned;
    public bool isMissing;

    public string DisplayText => label + " " + owned + "/" + required;
}

[Serializable]
public sealed class RecipeViewData
{
    public string recipeId;
    public string resultLabel;
    public int resultCount;
    public bool canCraft;
    public readonly List<ResourceCostViewData> costs = new List<ResourceCostViewData>();
    public readonly List<string> missingResources = new List<string>();

    public string CostSummary
    {
        get
        {
            if (costs.Count == 0)
            {
                return "No cost";
            }

            string[] parts = new string[costs.Count];
            for (int i = 0; i < costs.Count; i++)
            {
                parts[i] = costs[i].DisplayText;
            }

            return string.Join(" + ", parts);
        }
    }

    public string MissingSummary => missingResources.Count == 0 ? string.Empty : string.Join(", ", missingResources);

    public string DisplayText
    {
        get
        {
            string state = canCraft ? "Ready" : "Missing " + MissingSummary;
            return "Craft " + resultLabel + " x" + resultCount + " | " + CostSummary + " | " + state;
        }
    }
}

[Serializable]
public sealed class BuildPreviewState
{
    public BuildableType selectedBuildable;
    public string selectedLabel;
    public Vector2 snappedPosition;
    public bool isValid;
    public string invalidReason;
    public string costText;
    public bool rotated;

    public string DisplayText
    {
        get
        {
            string placement = isValid ? "Valid" : "Invalid: " + invalidReason;
            string rotation = rotated ? " | Rotated" : string.Empty;
            return selectedLabel + " | Cost: " + costText + " | " + placement + rotation + " | Right Click place, R rotate door, X cancel";
        }
    }
}

[Serializable]
public sealed class SaveSlotViewData
{
    public int slotNumber;
    public bool isOccupied;
    public int day;
    public string phaseOrTime;
    public string timestamp;
    public string playerSummary;
    public string displayName;

    public string DisplayText
    {
        get
        {
            if (!isOccupied)
            {
                return "Slot " + slotNumber + ": Empty";
            }

            return "Slot " + slotNumber + ": " + displayName + " | Day " + day + " " + phaseOrTime + " | " + playerSummary;
        }
    }
}
