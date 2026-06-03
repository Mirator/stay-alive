using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class StayAlivePrototypeTests
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    [SetUp]
    public void OpenPrototypeScene()
    {
        EditorSceneManager.OpenScene(ScenePath);
    }

    [Test]
    public void MvpGeneratedAssetsExist()
    {
        Assert.IsTrue(File.Exists("Assets/Art/Generated/Sprites/mvp_grass_tile.png"));
        Assert.IsTrue(File.Exists("Assets/Art/Generated/Sprites/mvp_soil_tile.png"));
        Assert.IsTrue(File.Exists("Assets/Art/Generated/Sprites/mvp_tree.png"));
        Assert.IsTrue(File.Exists("Assets/Art/Generated/Sprites/mvp_build_wall.png"));
        Assert.IsTrue(File.Exists("Assets/Art/Generated/Sprites/mvp_campfire.png"));
        Assert.IsTrue(File.Exists("Assets/Art/Generated/Sprites/mvp_bedroll.png"));
        Assert.IsTrue(File.Exists("Assets/Art/Generated/Sprites/mvp_wolf.png"));
        Assert.IsTrue(File.Exists("Assets/Art/Generated/Sprites/mvp_boar.png"));
    }

    [Test]
    public void SceneContainsOutdoorSurvivalMvp()
    {
        GameObject player = GameObject.Find("Player Survivor");
        Assert.IsNotNull(player);
        Assert.IsNotNull(player.GetComponent<Rigidbody2D>());
        Assert.IsNotNull(player.GetComponent<Collider2D>());
        Assert.IsNotNull(player.GetComponent<PlayerController2D>());
        Assert.IsNotNull(player.GetComponent<PlayerInteraction>());
        Assert.IsNotNull(player.GetComponent<ResourceInventory>());
        Assert.IsNotNull(player.GetComponent<CraftedInventory>());
        Assert.IsNotNull(player.GetComponent<PlayerVitals>());
        Assert.IsNotNull(player.GetComponent<MeleeWeapon>());
        Assert.IsNotNull(player.GetComponent<GridBuildingSystem>());

        SurvivalGameController game = Object.FindFirstObjectByType<SurvivalGameController>();
        Assert.IsNotNull(game);
        Assert.IsFalse(game.HasWinningCondition);
        Assert.IsNotNull(Object.FindFirstObjectByType<DayNightCycle>());
        Assert.IsNotNull(Object.FindFirstObjectByType<SurvivalSaveSystem>());
        Assert.IsNotNull(Object.FindFirstObjectByType<MainMenuController>());
        Assert.IsNotNull(Object.FindFirstObjectByType<UIController>());

        Camera camera = Camera.main;
        Assert.IsNotNull(camera);
        SmoothCameraFollow follow = camera.GetComponent<SmoothCameraFollow>();
        Assert.IsNotNull(follow);
        Assert.AreEqual(player.transform, follow.Target);

        Assert.IsNotNull(GameObject.Find("Handcrafted Wilderness Map"));
        Assert.IsNull(GameObject.Find("Handcrafted Cave Route"));
        Assert.IsNull(Object.FindFirstObjectByType<AncientDoor>());
        Assert.IsNull(Object.FindFirstObjectByType<CaveCrawlerEnemy>());
    }

    [Test]
    public void SceneHasRequiredLandmarksResourcesAndWildlife()
    {
        string[] landmarks =
        {
            "Spawn Clearing",
            "Forest Resource Area",
            "Stone Outcrop",
            "Food And Herb Area",
            "Water Boundary",
            "Open Building Area",
            "Wolf Danger Zone",
            "Boar Danger Zone"
        };

        for (int i = 0; i < landmarks.Length; i++)
        {
            Assert.IsNotNull(GameObject.Find(landmarks[i]), "Missing landmark " + landmarks[i]);
        }

        GatherableResource[] gatherables = Object.FindObjectsByType<GatherableResource>(FindObjectsSortMode.None);
        Assert.GreaterOrEqual(gatherables.Count(g => g.ResourceType == ResourceType.Wood), 3);
        Assert.GreaterOrEqual(gatherables.Count(g => g.ResourceType == ResourceType.Stone), 3);
        Assert.GreaterOrEqual(gatherables.Count(g => g.ResourceType == ResourceType.Food), 2);
        Assert.GreaterOrEqual(gatherables.Count(g => g.ResourceType == ResourceType.Herbs), 2);

        WildAnimalEnemy[] animals = Object.FindObjectsByType<WildAnimalEnemy>(FindObjectsSortMode.None);
        Assert.AreEqual(1, animals.Count(a => a.AnimalType == WildAnimalType.Wolf));
        Assert.AreEqual(1, animals.Count(a => a.AnimalType == WildAnimalType.Boar));
        Assert.Greater(Vector2.Distance(GameObject.Find("Player Survivor").transform.position, animals[0].transform.position), 6f);

        Assert.IsNotNull(GameObject.Find("Starting Campfire").GetComponent<SaveStation>());
        Assert.IsNotNull(GameObject.Find("Old Wilderness Workbench").GetComponent<CraftingStation>());
        Assert.AreEqual(3, Object.FindFirstObjectByType<MainMenuController>().SlotCount);
    }

    [Test]
    public void OutdoorRoutesFromSpawnAreWalkable()
    {
        Vector2 spawn = Vector2.zero;
        Vector2[] destinations =
        {
            new Vector2(-8f, 4f),
            new Vector2(8f, 4f),
            new Vector2(-8f, -7f),
            new Vector2(1f, -5f)
        };

        StringBuilder blocked = new StringBuilder();
        for (int i = 0; i < destinations.Length; i++)
        {
            CollectBlockingSamples(spawn, destinations[i], blocked);
        }

        Assert.IsEmpty(blocked.ToString());
    }

    [Test]
    public void GatherableResourceHarvestsMvpResourcesOnce()
    {
        GameObject inventoryObject = new GameObject("Gather Inventory");
        ResourceInventory inventory = inventoryObject.AddComponent<ResourceInventory>();

        ResourceType[] types = { ResourceType.Wood, ResourceType.Stone, ResourceType.Food, ResourceType.Herbs };
        for (int i = 0; i < types.Length; i++)
        {
            GameObject nodeObject = new GameObject("Gather Node " + types[i]);
            nodeObject.AddComponent<CircleCollider2D>();
            GatherableResource node = nodeObject.AddComponent<GatherableResource>();
            node.Configure("node-" + types[i], types[i].ToString(), types[i], i + 1);

            Assert.IsTrue(node.Harvest(inventory));
            Assert.AreEqual(i + 1, inventory.Get(types[i]));
            Assert.IsTrue(node.IsHarvested);
            Assert.IsFalse(node.Harvest(inventory));
            Assert.AreEqual(i + 1, inventory.Get(types[i]));

            Object.DestroyImmediate(nodeObject);
        }

        Object.DestroyImmediate(inventoryObject);
    }

    [Test]
    public void MvpCraftingRecipesUseExactCosts()
    {
        GameObject player = new GameObject("Crafting Test Player");
        ResourceInventory resources = player.AddComponent<ResourceInventory>();
        CraftedInventory crafted = player.AddComponent<CraftedInventory>();
        PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();

        GameObject stationObject = new GameObject("Crafting Station Under Test");
        CircleCollider2D collider = stationObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        CraftingStation station = stationObject.AddComponent<CraftingStation>();

        station.SelectRecipe(CraftedItemType.StoneSpear);
        station.Interact(interaction);
        Assert.AreEqual(0, crafted.StoneSpears);

        CraftAndAssert(station, interaction, resources, crafted, CraftedItemType.StoneSpear, 2, 1, 0, 0);
        CraftAndAssert(station, interaction, resources, crafted, CraftedItemType.Bandage, 0, 0, 0, 2);
        CraftAndAssert(station, interaction, resources, crafted, CraftedItemType.Campfire, 3, 1, 0, 0);
        CraftAndAssert(station, interaction, resources, crafted, CraftedItemType.Bedroll, 4, 0, 0, 2);
        CraftAndAssert(station, interaction, resources, crafted, CraftedItemType.StorageBox, 6, 0, 0, 0);
        CraftAndAssert(station, interaction, resources, crafted, CraftedItemType.Workbench, 5, 2, 0, 0);

        Object.DestroyImmediate(stationObject);
        Object.DestroyImmediate(player);
    }

    [Test]
    public void PlayerVitalsSurvivalFoodBandageAndDeath()
    {
        GameObject player = new GameObject("Vitals Test Player");
        ResourceInventory resources = player.AddComponent<ResourceInventory>();
        CraftedInventory crafted = player.AddComponent<CraftedInventory>();
        PlayerVitals vitals = player.AddComponent<PlayerVitals>();
        vitals.Configure(100f, 100f);

        vitals.AdvanceSurvivalTime(100f);
        Assert.Less(vitals.Hunger, 100f);
        float hungerBeforeFood = vitals.Hunger;
        resources.Add(ResourceType.Food, 1);
        Assert.IsTrue(vitals.EatFood(resources));
        Assert.Greater(vitals.Hunger, hungerBeforeFood);
        Assert.AreEqual(0, resources.Food);

        vitals.ApplyDamage(40f);
        Assert.AreEqual(60f, vitals.Health);
        crafted.Add(CraftedItemType.Bandage, 1);
        Assert.IsTrue(vitals.UseBandage(crafted));
        Assert.Greater(vitals.Health, 60f);
        Assert.AreEqual(0, crafted.Bandages);

        vitals.SetState(5f, 0f);
        vitals.AdvanceSurvivalTime(10f);
        Assert.IsTrue(vitals.IsDead);
        Assert.AreEqual(0f, vitals.Health);

        Object.DestroyImmediate(player);
    }

    [Test]
    public void DayNightCycleReachesDayThreeAndContinues()
    {
        GameObject cycleObject = new GameObject("Day Night Cycle Test");
        DayNightCycle cycle = cycleObject.AddComponent<DayNightCycle>();
        cycle.Configure(300f);
        cycle.SetState(1, 0f);

        cycle.AdvanceTime(610f);
        Assert.AreEqual(3, cycle.CurrentDay);
        Assert.AreEqual(DayPhase.Day, cycle.CurrentPhase);

        cycle.AdvanceTime(900f);
        Assert.Greater(cycle.CurrentDay, 3);

        GameObject gameObject = new GameObject("Survival Game Test");
        SurvivalGameController game = gameObject.AddComponent<SurvivalGameController>();
        Assert.IsFalse(game.HasWinningCondition);

        Object.DestroyImmediate(gameObject);
        Object.DestroyImmediate(cycleObject);
    }

    [Test]
    public void WildlifeStatesNightAggressionAndSpearDamage()
    {
        GameObject player = new GameObject("Wildlife Target");
        player.transform.position = Vector2.zero;

        GameObject cycleObject = new GameObject("Wildlife Cycle");
        DayNightCycle cycle = cycleObject.AddComponent<DayNightCycle>();
        cycle.Configure(300f);

        WildAnimalEnemy wolf = CreateAnimalUnderTest("Wolf Under Test", WildAnimalType.Wolf, new Vector2(5.5f, 0f), player.transform, cycle);
        cycle.SetState(1, 0.2f);
        Assert.AreEqual(WildAnimalState.Idle, wolf.EvaluateState());
        cycle.SetState(1, 0.86f);
        Assert.AreEqual(WildAnimalState.Chasing, wolf.EvaluateState());

        WildAnimalEnemy boar = CreateAnimalUnderTest("Boar Under Test", WildAnimalType.Boar, new Vector2(3f, 0f), player.transform, cycle);
        Assert.AreEqual(WildAnimalState.Charging, boar.EvaluateState());

        CraftedInventory crafted = player.AddComponent<CraftedInventory>();
        crafted.Add(CraftedItemType.StoneSpear, 1);
        MeleeWeapon weapon = player.AddComponent<MeleeWeapon>();
        Assert.IsTrue(weapon.HasStoneSpear);
        Assert.IsTrue(weapon.TryDamage(boar));
        Assert.Less(boar.Health, 50f);
        Assert.IsTrue(weapon.TryDamage(boar));
        Assert.IsTrue(boar.IsDefeated);

        Object.DestroyImmediate(boar.gameObject);
        Object.DestroyImmediate(wolf.gameObject);
        Object.DestroyImmediate(cycleObject);
        Object.DestroyImmediate(player);
    }

    [Test]
    public void AnimalAttackDamagesPlayer()
    {
        GameObject player = new GameObject("Animal Damage Target");
        player.transform.position = Vector2.zero;
        PlayerVitals vitals = player.AddComponent<PlayerVitals>();
        vitals.Configure(100f, 100f);

        WildAnimalEnemy wolf = CreateAnimalUnderTest("Attacking Wolf", WildAnimalType.Wolf, new Vector2(0.55f, 0f), player.transform, null);
        Assert.IsTrue(wolf.TryAttack(vitals));
        Assert.Less(vitals.Health, 100f);

        Object.DestroyImmediate(wolf.gameObject);
        Object.DestroyImmediate(player);
    }

    [Test]
    public void GridBuildingPlacementRulesAndCosts()
    {
        GameObject player = new GameObject("Grid Builder");
        ResourceInventory resources = player.AddComponent<ResourceInventory>();
        CraftedInventory crafted = player.AddComponent<CraftedInventory>();
        GridBuildingSystem building = player.AddComponent<GridBuildingSystem>();
        building.Configure(new Rect(40f, 40f, 20f, 20f), null, null, null, null, null, null, null);

        GameObject placedParent = new GameObject("Placed Parent");
        building.SetPlacedParent(placedParent.transform);
        resources.Add(ResourceType.Wood, 10);
        resources.Add(ResourceType.Stone, 3);
        crafted.Add(CraftedItemType.Campfire, 1);
        crafted.Add(CraftedItemType.Bedroll, 1);
        crafted.Add(CraftedItemType.StorageBox, 1);
        crafted.Add(CraftedItemType.Workbench, 1);

        Assert.IsTrue(building.TryPlace(BuildableType.Floor, new Vector2(41f, 41f), false, out GameObject floor));
        Assert.IsNotNull(floor.GetComponent<BuildableObject>());
        Assert.AreEqual(9, resources.Wood);

        Assert.IsTrue(building.TryPlace(BuildableType.Wall, new Vector2(42f, 41f), false, out GameObject wall));
        Assert.IsNotNull(wall.GetComponent<BoxCollider2D>());
        Assert.AreEqual(7, resources.Wood);

        Assert.IsTrue(building.TryPlace(BuildableType.Door, new Vector2(43f, 41f), true, out GameObject door));
        Assert.IsTrue(door.GetComponent<BuildableObject>().Rotated);
        Assert.AreEqual(5, resources.Wood);
        Assert.AreEqual(2, resources.Stone);

        Assert.IsTrue(building.TryPlace(BuildableType.Campfire, new Vector2(44f, 41f), false, out GameObject campfire));
        Assert.IsNotNull(campfire.GetComponent<SaveStation>());
        Assert.AreEqual(0, crafted.Campfires);

        GameObject blocker = new GameObject("Blocking Rock");
        blocker.transform.position = new Vector2(45f, 41f);
        blocker.AddComponent<BoxCollider2D>();
        int woodBeforeInvalid = resources.Wood;
        Assert.IsFalse(building.TryPlace(BuildableType.Wall, new Vector2(45f, 41f), false, out _));
        Assert.IsFalse(building.TryPlace(BuildableType.Wall, new Vector2(100f, 100f), false, out _));
        Assert.AreEqual(woodBeforeInvalid, resources.Wood);

        Assert.IsTrue(building.TryPlace(BuildableType.Bedroll, new Vector2(46f, 41f), false, out GameObject bedroll));
        Assert.IsNotNull(bedroll.GetComponent<SaveStation>());
        Assert.IsTrue(building.TryPlace(BuildableType.StorageBox, new Vector2(47f, 41f), false, out GameObject storage));
        Assert.IsNotNull(storage.GetComponent<BasicInteractable>());
        Assert.IsTrue(building.TryPlace(BuildableType.Workbench, new Vector2(48f, 41f), false, out GameObject workbench));
        Assert.IsNotNull(workbench.GetComponent<CraftingStation>());

        Object.DestroyImmediate(blocker);
        Object.DestroyImmediate(placedParent);
        Object.DestroyImmediate(player);
    }

    [Test]
    public void SaveSlotsRoundTripWorldState()
    {
        string storageRoot = Path.Combine("Temp", "StayAliveMvpTestSaves");
        if (Directory.Exists(storageRoot))
        {
            Directory.Delete(storageRoot, true);
        }

        GameObject player = GameObject.Find("Player Survivor");
        ResourceInventory resources = player.GetComponent<ResourceInventory>();
        CraftedInventory crafted = player.GetComponent<CraftedInventory>();
        PlayerVitals vitals = player.GetComponent<PlayerVitals>();
        DayNightCycle cycle = Object.FindFirstObjectByType<DayNightCycle>();
        GridBuildingSystem building = player.GetComponent<GridBuildingSystem>();
        SurvivalSaveSystem saves = Object.FindFirstObjectByType<SurvivalSaveSystem>();
        MainMenuController menu = Object.FindFirstObjectByType<MainMenuController>();

        saves.SetStorageRootForTests(storageRoot);
        Assert.AreEqual(3, menu.SlotCount);
        Assert.IsTrue(menu.StartNewGame(2));
        Assert.IsFalse(saves.HasSlot(2));

        player.transform.position = new Vector3(3.5f, -4.5f, 0f);
        resources.ClearAll();
        resources.Set(ResourceType.Wood, 8);
        resources.Set(ResourceType.Stone, 3);
        resources.Set(ResourceType.Food, 1);
        resources.Set(ResourceType.Herbs, 2);
        crafted.ClearAll();
        crafted.Set(CraftedItemType.Campfire, 1);
        vitals.SetState(64f, 35f);
        cycle.SetState(2, 0.84f);
        Assert.IsTrue(building.TryPlace(BuildableType.Campfire, new Vector2(2f, -4f), false, out _));

        GatherableResource node = Object.FindObjectsByType<GatherableResource>(FindObjectsSortMode.None).First(g => g.ResourceType == ResourceType.Wood);
        int expectedWood = resources.Wood + node.Amount;
        Assert.IsTrue(node.Harvest(resources));

        WildAnimalEnemy enemy = Object.FindFirstObjectByType<WildAnimalEnemy>();
        enemy.ReceiveDamage(1000f);
        Assert.IsTrue(enemy.IsDefeated);

        Assert.IsTrue(saves.SaveSlot(2));
        SaveSlotData saved = saves.ReadSlot(2);
        Assert.IsNotNull(saved);
        Assert.AreEqual(2, saved.slotNumber);
        Assert.AreEqual(1, saved.placedBuildings.Count);
        CollectionAssert.Contains(saved.harvestedNodeIds, node.GatherableId);
        CollectionAssert.Contains(saved.defeatedEnemyIds, enemy.AnimalId);

        player.transform.position = Vector3.zero;
        resources.ClearAll();
        crafted.ClearAll();
        vitals.SetState(10f, 10f);
        cycle.SetState(1, 0f);
        building.ClearPlacedBuildings();
        node.SetHarvested(false);
        enemy.SetDefeated(false);

        Assert.IsTrue(menu.LoadSlot(2));
        Assert.AreEqual(new Vector3(3.5f, -4.5f, 0f), player.transform.position);
        Assert.AreEqual(64f, vitals.Health);
        Assert.AreEqual(35f, vitals.Hunger);
        Assert.AreEqual(2, cycle.CurrentDay);
        Assert.AreEqual(DayPhase.Night, cycle.CurrentPhase);
        Assert.AreEqual(expectedWood, resources.Wood);
        Assert.AreEqual(3, resources.Stone);
        Assert.AreEqual(1, resources.Food);
        Assert.AreEqual(2, resources.Herbs);
        Assert.AreEqual(1, building.PlacedBuildables.Count);
        Assert.IsTrue(node.IsHarvested);
        Assert.IsTrue(enemy.IsDefeated);
    }

    [Test]
    public void SpecsIndexMarksMvpImplemented()
    {
        string readme = File.ReadAllText(Path.Combine("docs", "README.md"));
        for (int spec = 1; spec <= 15; spec++)
        {
            Assert.IsTrue(readme.Contains("SPEC-" + spec.ToString("000")));
        }

        Assert.IsTrue(readme.Contains("PoC Baseline Specs"));
        Assert.IsTrue(readme.Contains("MVP Implemented Specs"));
        Assert.IsTrue(readme.Contains("SPEC-008 | Wilderness Survival Loop | Implemented"));
        Assert.IsTrue(readme.Contains("SPEC-015 | MVP Testing And Acceptance | Implemented"));

        string[] mvpDocs =
        {
            "wilderness-survival-loop.md",
            "wilderness-world-and-content.md",
            "vitals-day-night-death.md",
            "melee-combat-and-wildlife.md",
            "gathering-crafting-inventory.md",
            "grid-building.md",
            "save-slots-and-menus.md",
            "mvp-testing-and-acceptance.md"
        };

        for (int i = 0; i < mvpDocs.Length; i++)
        {
            string content = File.ReadAllText(Path.Combine("docs", mvpDocs[i]));
            Assert.IsTrue(content.Contains("Status: Implemented"), mvpDocs[i] + " is not marked implemented.");
        }
    }

    private static void CraftAndAssert(
        CraftingStation station,
        PlayerInteraction interaction,
        ResourceInventory resources,
        CraftedInventory crafted,
        CraftedItemType output,
        int wood,
        int stone,
        int food,
        int herbs)
    {
        resources.ClearAll();
        crafted.ClearAll();
        resources.Set(ResourceType.Wood, wood);
        resources.Set(ResourceType.Stone, stone);
        resources.Set(ResourceType.Food, food);
        resources.Set(ResourceType.Herbs, herbs);

        station.SelectRecipe(output);
        station.Interact(interaction);

        Assert.AreEqual(0, resources.Wood, output + " should consume exact Wood cost.");
        Assert.AreEqual(0, resources.Stone, output + " should consume exact Stone cost.");
        Assert.AreEqual(0, resources.Food, output + " should consume exact Food cost.");
        Assert.AreEqual(0, resources.Herbs, output + " should consume exact Herbs cost.");
        Assert.AreEqual(1, crafted.Get(output), output + " should be crafted once.");
    }

    private static WildAnimalEnemy CreateAnimalUnderTest(string name, WildAnimalType type, Vector2 position, Transform target, DayNightCycle cycle)
    {
        GameObject animalObject = new GameObject(name);
        animalObject.transform.position = position;
        animalObject.AddComponent<Rigidbody2D>();
        CircleCollider2D collider = animalObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        WildAnimalEnemy animal = animalObject.AddComponent<WildAnimalEnemy>();
        animal.Configure(name.ToLowerInvariant(), type, target, cycle, position);
        return animal;
    }

    private static void CollectBlockingSamples(Vector2 start, Vector2 end, StringBuilder blocked)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance / 0.25f);
        const float playerRadius = 0.34f;

        for (int step = 0; step <= steps; step++)
        {
            Vector2 sample = Vector2.Lerp(start, end, step / (float)steps);
            Collider2D[] overlaps = Physics2D.OverlapCircleAll(sample, playerRadius);
            for (int i = 0; i < overlaps.Length; i++)
            {
                Collider2D overlap = overlaps[i];
                if (IsBlockingPath(overlap))
                {
                    blocked.AppendLine(overlap.name + " at " + sample + " on route to " + end);
                }
            }
        }
    }

    private static bool IsBlockingPath(Collider2D overlap)
    {
        if (overlap == null || !overlap.enabled || overlap.isTrigger)
        {
            return false;
        }

        if (overlap.GetComponentInParent<PlayerController2D>() != null)
        {
            return false;
        }

        if (overlap.GetComponentInParent<GatherableResource>() != null)
        {
            return false;
        }

        if (overlap.GetComponentInParent<WildAnimalEnemy>() != null)
        {
            return false;
        }

        return true;
    }
}
