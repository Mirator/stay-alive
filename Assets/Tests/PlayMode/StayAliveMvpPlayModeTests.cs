using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class StayAliveMvpPlayModeTests
{
    [UnitySetUp]
    public IEnumerator LoadMvpScene()
    {
        SceneManager.LoadScene("SampleScene");
        yield return null;
        yield return null;
    }

    [UnityTest]
    public IEnumerator SurvivalVitalsAndDayNightAdvanceInPlayMode()
    {
        PlayerVitals vitals = Object.FindFirstObjectByType<PlayerVitals>();
        DayNightCycle cycle = Object.FindFirstObjectByType<DayNightCycle>();
        ResourceInventory resources = Object.FindFirstObjectByType<ResourceInventory>();
        Assert.IsNotNull(vitals);
        Assert.IsNotNull(cycle);
        Assert.IsNotNull(resources);

        vitals.SetState(100f, 100f);
        vitals.AdvanceSurvivalTime(120f);
        Assert.Less(vitals.Hunger, 100f);
        float hungerBeforeFood = vitals.Hunger;
        resources.Add(ResourceType.Food, 1);
        Assert.IsTrue(vitals.EatFood(resources));
        Assert.Greater(vitals.Hunger, hungerBeforeFood);

        cycle.Configure(300f);
        cycle.SetState(1, 0f);
        cycle.AdvanceTime(610f);
        Assert.AreEqual(3, cycle.CurrentDay);
        cycle.AdvanceTime(900f);
        Assert.Greater(cycle.CurrentDay, 3);

        yield return null;
    }

    [UnityTest]
    public IEnumerator WildlifeCombatAndHealingRunInPlayMode()
    {
        GameObject player = GameObject.Find("Player Survivor");
        PlayerVitals vitals = player.GetComponent<PlayerVitals>();
        CraftedInventory crafted = player.GetComponent<CraftedInventory>();
        MeleeWeapon weapon = player.GetComponent<MeleeWeapon>();
        WildAnimalEnemy wolf = Object.FindObjectsByType<WildAnimalEnemy>(FindObjectsSortMode.None)
            .First(a => a.AnimalType == WildAnimalType.Wolf);

        player.transform.position = wolf.transform.position + Vector3.left * 0.55f;
        vitals.SetState(100f, 100f);
        yield return null;

        Assert.IsTrue(wolf.TryAttack(vitals));
        Assert.Less(vitals.Health, 100f);

        crafted.Add(CraftedItemType.StoneSpear, 1);
        Assert.IsTrue(weapon.HasStoneSpear);
        Assert.IsTrue(weapon.TryDamage(wolf));

        float damagedHealth = vitals.Health;
        crafted.Add(CraftedItemType.Bandage, 1);
        Assert.IsTrue(vitals.UseBandage(crafted));
        Assert.Greater(vitals.Health, damagedHealth);
    }

    [UnityTest]
    public IEnumerator GridBuildingConsumesOnlyOnSuccessfulPlayModePlacement()
    {
        GameObject player = GameObject.Find("Player Survivor");
        ResourceInventory resources = player.GetComponent<ResourceInventory>();
        CraftedInventory crafted = player.GetComponent<CraftedInventory>();
        GridBuildingSystem building = player.GetComponent<GridBuildingSystem>();

        resources.ClearAll();
        crafted.ClearAll();
        resources.Add(ResourceType.Wood, 10);
        resources.Add(ResourceType.Stone, 3);
        crafted.Add(CraftedItemType.Campfire, 1);

        Assert.IsTrue(building.TryPlace(BuildableType.Floor, new Vector2(3f, -5f), false, out GameObject floor));
        Assert.IsNotNull(floor.GetComponent<BuildableObject>());
        Assert.AreEqual(9, resources.Wood);

        Assert.IsTrue(building.TryPlace(BuildableType.Door, new Vector2(4f, -5f), true, out GameObject door));
        Assert.IsTrue(door.GetComponent<BuildableObject>().Rotated);
        Assert.AreEqual(7, resources.Wood);
        Assert.AreEqual(2, resources.Stone);

        GameObject blocker = new GameObject("PlayMode Build Blocker");
        blocker.transform.position = new Vector2(5f, -5f);
        blocker.AddComponent<BoxCollider2D>();
        Physics2D.SyncTransforms();

        int woodBeforeInvalid = resources.Wood;
        Assert.IsFalse(building.TryPlace(BuildableType.Wall, new Vector2(5f, -5f), false, out _));
        Assert.IsFalse(building.TryPlace(BuildableType.Wall, new Vector2(100f, 100f), false, out _));
        Assert.AreEqual(woodBeforeInvalid, resources.Wood);

        Assert.IsTrue(building.TryPlace(BuildableType.Campfire, new Vector2(6f, -5f), false, out GameObject campfire));
        Assert.IsNotNull(campfire.GetComponent<SaveStation>());
        Assert.AreEqual(0, crafted.Campfires);

        Object.Destroy(blocker);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SaveLoadRoundTripsSlotStateInPlayMode()
    {
        string storageRoot = Path.Combine(Application.temporaryCachePath, "StayAliveMvpPlayModeSaves");
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
        Assert.IsTrue(menu.StartNewGame(3));

        player.transform.position = new Vector3(2.5f, -4.5f, 0f);
        resources.ClearAll();
        resources.Set(ResourceType.Wood, 9);
        resources.Set(ResourceType.Stone, 4);
        resources.Set(ResourceType.Food, 1);
        resources.Set(ResourceType.Herbs, 2);
        crafted.ClearAll();
        crafted.Set(CraftedItemType.Campfire, 1);
        vitals.SetState(70f, 40f);
        cycle.SetState(2, 0.86f);
        Assert.IsTrue(building.TryPlace(BuildableType.Campfire, new Vector2(1f, -4f), false, out _));

        GatherableResource node = Object.FindObjectsByType<GatherableResource>(FindObjectsSortMode.None)
            .First(g => g.ResourceType == ResourceType.Herbs);
        int expectedHerbs = resources.Herbs + node.Amount;
        Assert.IsTrue(node.Harvest(resources));

        WildAnimalEnemy animal = Object.FindFirstObjectByType<WildAnimalEnemy>();
        animal.ReceiveDamage(1000f);

        Assert.IsTrue(saves.SaveSlot(3));

        player.transform.position = Vector3.zero;
        resources.ClearAll();
        crafted.ClearAll();
        vitals.SetState(5f, 5f);
        cycle.SetState(1, 0f);
        building.ClearPlacedBuildings();
        node.SetHarvested(false);
        animal.SetDefeated(false);
        yield return null;

        Assert.IsTrue(menu.LoadSlot(3));
        Assert.AreEqual(new Vector3(2.5f, -4.5f, 0f), player.transform.position);
        Assert.AreEqual(70f, vitals.Health);
        Assert.AreEqual(40f, vitals.Hunger);
        Assert.AreEqual(2, cycle.CurrentDay);
        Assert.AreEqual(DayPhase.Night, cycle.CurrentPhase);
        Assert.AreEqual(9, resources.Wood);
        Assert.AreEqual(4, resources.Stone);
        Assert.AreEqual(1, resources.Food);
        Assert.AreEqual(expectedHerbs, resources.Herbs);
        Assert.AreEqual(1, building.PlacedBuildables.Count);
        Assert.IsTrue(node.IsHarvested);
        Assert.IsTrue(animal.IsDefeated);
    }
}
