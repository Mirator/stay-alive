using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class CraftingStation : MonoBehaviour, IInteractable, IInteractionPromptProvider
{
    [SerializeField] private int selectedRecipeIndex;

    private static readonly CraftingRecipe[] Recipes =
    {
        new CraftingRecipe(
            CraftedItemType.StoneSpear,
            new ResourceCost(ResourceType.Wood, 2),
            new ResourceCost(ResourceType.Stone, 1)),
        new CraftingRecipe(
            CraftedItemType.Bandage,
            new ResourceCost(ResourceType.Herbs, 2)),
        new CraftingRecipe(
            CraftedItemType.Campfire,
            new ResourceCost(ResourceType.Wood, 3),
            new ResourceCost(ResourceType.Stone, 1)),
        new CraftingRecipe(
            CraftedItemType.Bedroll,
            new ResourceCost(ResourceType.Wood, 4),
            new ResourceCost(ResourceType.Herbs, 2)),
        new CraftingRecipe(
            CraftedItemType.StorageBox,
            new ResourceCost(ResourceType.Wood, 6)),
        new CraftingRecipe(
            CraftedItemType.Workbench,
            new ResourceCost(ResourceType.Wood, 5),
            new ResourceCost(ResourceType.Stone, 2)),
        new CraftingRecipe(
            CraftedItemType.Torch,
            new ResourceCost(ResourceType.RootFiber, 1)),
        new CraftingRecipe(
            CraftedItemType.StoneMarker,
            new ResourceCost(ResourceType.Stone, 2)),
        new CraftingRecipe(
            CraftedItemType.CrystalKeyShard,
            new ResourceCost(ResourceType.GlowCrystal, 1),
            new ResourceCost(ResourceType.Stone, 1))
    };

    public CraftedItemType CurrentOutput => CurrentRecipe.Output;
    public string Prompt => "Press E: craft " + CraftedInventory.Label(CurrentRecipe.Output) + " (" + CurrentRecipe.CostText + ")";

    public InteractionPromptData GetPromptData(PlayerInteraction player)
    {
        ResourceInventory inventory = player != null ? player.Inventory : null;
        RecipeViewData view = GetCurrentRecipeView(inventory, player != null ? player.CraftedInventory : null);
        string reason = view.canCraft ? string.Empty : "Need " + view.MissingSummary;
        return InteractionPromptData.Create("E", "Craft", CraftedInventory.Label(CurrentRecipe.Output), view.canCraft, reason);
    }

    public void Interact(PlayerInteraction player)
    {
        ResourceInventory resourceInventory = player != null ? player.Inventory : null;
        CraftedInventory craftedInventory = player != null ? player.CraftedInventory : null;

        if (player != null)
        {
            if (resourceInventory == null)
            {
                resourceInventory = player.GetComponent<ResourceInventory>();
            }

            if (craftedInventory == null)
            {
                craftedInventory = player.GetComponent<CraftedInventory>();
            }
        }

        if (resourceInventory == null || craftedInventory == null)
        {
            UIController.Instance?.ShowMessage("No inventory available for crafting.", 1.7f);
            AdvanceRecipe();
            UIController.Instance?.ShowRecipeView(GetCurrentRecipeView(resourceInventory, craftedInventory));
            return;
        }

        CraftingRecipe recipe = CurrentRecipe;
        if (!CanAfford(resourceInventory, recipe, out string missingText))
        {
            UIController.Instance?.ShowMessage("Need " + missingText + " for " + CraftedInventory.Label(recipe.Output) + ".", 2f);
            AdvanceRecipe();
            UIController.Instance?.ShowRecipeView(GetCurrentRecipeView(resourceInventory, craftedInventory));
            return;
        }

        for (int i = 0; i < recipe.Costs.Length; i++)
        {
            ResourceCost cost = recipe.Costs[i];
            resourceInventory.Spend(cost.Type, cost.Amount);
        }

        craftedInventory.Add(recipe.Output, 1);
        UIController.Instance?.ShowMessage("Crafted " + CraftedInventory.Label(recipe.Output) + ".", 1.7f);
        AdvanceRecipe();
        UIController.Instance?.ShowRecipeView(GetCurrentRecipeView(resourceInventory, craftedInventory));
    }

    public void SelectRecipe(CraftedItemType output)
    {
        for (int i = 0; i < Recipes.Length; i++)
        {
            if (Recipes[i].Output == output)
            {
                selectedRecipeIndex = i;
                return;
            }
        }
    }

    private CraftingRecipe CurrentRecipe
    {
        get
        {
            if (selectedRecipeIndex < 0 || selectedRecipeIndex >= Recipes.Length)
            {
                selectedRecipeIndex = 0;
            }

            return Recipes[selectedRecipeIndex];
        }
    }

    public RecipeViewData GetCurrentRecipeView(ResourceInventory resourceInventory, CraftedInventory craftedInventory)
    {
        CraftingRecipe recipe = CurrentRecipe;
        RecipeViewData view = new RecipeViewData
        {
            recipeId = recipe.Output.ToString(),
            resultLabel = CraftedInventory.Label(recipe.Output),
            resultCount = craftedInventory != null ? craftedInventory.Get(recipe.Output) : 0
        };

        for (int i = 0; i < recipe.Costs.Length; i++)
        {
            ResourceCost cost = recipe.Costs[i];
            int owned = resourceInventory != null ? resourceInventory.Get(cost.Type) : 0;
            bool missing = owned < cost.Amount;
            view.costs.Add(new ResourceCostViewData
            {
                resourceType = cost.Type.ToString(),
                label = ResourceLabel(cost.Type),
                required = cost.Amount,
                owned = owned,
                isMissing = missing
            });

            if (missing)
            {
                view.missingResources.Add((cost.Amount - owned) + " " + ResourceLabel(cost.Type));
            }
        }

        view.canCraft = view.missingResources.Count == 0;
        return view;
    }

    private void AdvanceRecipe()
    {
        selectedRecipeIndex = (selectedRecipeIndex + 1) % Recipes.Length;
    }

    private static bool CanAfford(ResourceInventory inventory, CraftingRecipe recipe, out string missingText)
    {
        List<string> missing = new List<string>();
        for (int i = 0; i < recipe.Costs.Length; i++)
        {
            ResourceCost cost = recipe.Costs[i];
            int have = inventory.Get(cost.Type);
            if (have < cost.Amount)
            {
                missing.Add((cost.Amount - have) + " " + ResourceLabel(cost.Type));
            }
        }

        missingText = string.Join(", ", missing);
        return missing.Count == 0;
    }

    public static string ResourceLabel(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Dirt:
                return "Dirt";
            case ResourceType.Stone:
                return "Stone";
            case ResourceType.GlowCrystal:
                return "Glow Crystal";
            case ResourceType.RootFiber:
                return "Root Fiber";
            case ResourceType.Wood:
                return "Wood";
            case ResourceType.Food:
                return "Food";
            case ResourceType.Herbs:
                return "Herbs";
            default:
                return "Resource";
        }
    }

    private readonly struct ResourceCost
    {
        public ResourceCost(ResourceType type, int amount)
        {
            Type = type;
            Amount = Mathf.Max(1, amount);
        }

        public ResourceType Type { get; }
        public int Amount { get; }
    }

    private sealed class CraftingRecipe
    {
        public CraftingRecipe(CraftedItemType output, params ResourceCost[] costs)
        {
            Output = output;
            Costs = costs;
            CostText = BuildCostText(costs);
        }

        public CraftedItemType Output { get; }
        public ResourceCost[] Costs { get; }
        public string CostText { get; }

        private static string BuildCostText(ResourceCost[] costs)
        {
            string[] parts = new string[costs.Length];
            for (int i = 0; i < costs.Length; i++)
            {
                parts[i] = costs[i].Amount + " " + ResourceLabel(costs[i].Type);
            }

            return string.Join(" + ", parts);
        }
    }
}
