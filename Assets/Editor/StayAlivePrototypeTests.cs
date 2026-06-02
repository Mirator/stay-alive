using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public sealed class StayAlivePrototypeTests
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    [SetUp]
    public void OpenPrototypeScene()
    {
        EditorSceneManager.OpenScene(ScenePath);
    }

    [Test]
    public void GeneratedAssetsExist()
    {
        Assert.IsTrue(File.Exists("Assets/Art/Generated/stay_alive_concept.png"));
        Assert.IsTrue(File.Exists("Assets/Art/Generated/Sprites/player_idle_down.png"));
        Assert.IsTrue(File.Exists("Assets/Art/Generated/Sprites/mineable_crystal.png"));
        Assert.IsTrue(File.Exists("Assets/Art/Generated/Sprites/cave_crawler.png"));
        Assert.IsTrue(File.Exists("Assets/Prefabs/Torch.prefab"));
    }

    [Test]
    public void SceneContainsPlayableVerticalSlice()
    {
        GameObject player = GameObject.Find("Player Explorer");
        Assert.IsNotNull(player);
        Assert.IsNotNull(player.GetComponent<Rigidbody2D>());
        Assert.IsNotNull(player.GetComponent<Collider2D>());
        Assert.IsNotNull(player.GetComponent<PlayerController2D>());
        Assert.IsNotNull(player.GetComponent<PlayerInteraction>());
        Assert.IsNotNull(player.GetComponent<ResourceInventory>());
        Assert.IsNotNull(player.GetComponent<CraftedInventory>());
        Assert.IsNotNull(player.GetComponent<PlayerKnockback>());
        Assert.IsNotNull(UnityEngine.Object.FindFirstObjectByType<GameLoopController>());
        Assert.IsNotNull(UnityEngine.Object.FindFirstObjectByType<RewardRoomGoal>());

        Camera camera = Camera.main;
        Assert.IsNotNull(camera);
        SmoothCameraFollow follow = camera.GetComponent<SmoothCameraFollow>();
        Assert.IsNotNull(follow);
        Assert.AreEqual(player.transform, follow.Target);

        GameObject cave = GameObject.Find("Handcrafted Cave Route");
        Assert.IsNotNull(cave);
        Assert.Greater(cave.transform.childCount, 120);
    }

    [Test]
    public void SceneHasMiningCrystalsDoorUiAndLights()
    {
        MineableResource[] mineables = UnityEngine.Object.FindObjectsByType<MineableResource>(FindObjectsSortMode.None);
        Assert.GreaterOrEqual(mineables.Count(m => m.ResourceType == ResourceType.Stone), 2);
        Assert.GreaterOrEqual(mineables.Count(m => m.ResourceType == ResourceType.GlowCrystal && m.HitsToBreak == 3), 3);
        Assert.GreaterOrEqual(mineables.Count(m => m.ResourceType == ResourceType.RootFiber), 1);

        AncientDoor door = UnityEngine.Object.FindFirstObjectByType<AncientDoor>();
        Assert.IsNotNull(door);
        Assert.AreEqual(ResourceType.GlowCrystal, door.RequiredResource);
        Assert.AreEqual(3, door.RequiredAmount);

        Assert.IsNotNull(UnityEngine.Object.FindFirstObjectByType<UIController>());
        Assert.GreaterOrEqual(UnityEngine.Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None).Length, 7);
    }

    [Test]
    public void SceneContainsCraftingAndEnemySystems()
    {
        GameObject workbench = GameObject.Find("Workbench");
        Assert.IsNotNull(workbench);
        Assert.IsNotNull(workbench.GetComponent<CraftingStation>());
        Assert.IsNotNull(workbench.GetComponent<Collider2D>());

        CaveCrawlerEnemy crawler = UnityEngine.Object.FindFirstObjectByType<CaveCrawlerEnemy>();
        Assert.IsNotNull(crawler);
        Assert.AreEqual("Cave Crawler", crawler.name);
        Assert.Greater(Vector2.Distance(crawler.transform.position, new Vector2(-15f, 0.2f)), 10f);

        Collider2D enemyCollider = crawler.GetComponent<Collider2D>();
        Assert.IsNotNull(enemyCollider);
        Assert.IsTrue(enemyCollider.isTrigger, "The enemy must not physically block the critical path.");
        Assert.Greater(crawler.transform.position.x, 4f, "The first enemy should appear after the crystal chamber begins.");
    }

    [Test]
    public void CoreLoopProgressesThroughMiningCrystalsDoorAndReward()
    {
        GameLoopController loop = UnityEngine.Object.FindFirstObjectByType<GameLoopController>();
        Assert.IsNotNull(loop);

        ResourceInventory inventory = GameObject.Find("Player Explorer").GetComponent<ResourceInventory>();
        AncientDoor door = UnityEngine.Object.FindFirstObjectByType<AncientDoor>();
        MineableResource blocker = GameObject.Find("Weak Stone Gate").GetComponent<MineableResource>();

        loop.BeginLoop();
        Assert.AreEqual(CoreLoopStage.MineBlockedTunnel, loop.CurrentStage);

        blocker.gameObject.SetActive(false);
        loop.EvaluateProgress();
        Assert.AreEqual(CoreLoopStage.CollectGlowCrystals, loop.CurrentStage);

        inventory.Add(ResourceType.GlowCrystal, 3);
        loop.EvaluateProgress();
        Assert.AreEqual(CoreLoopStage.OpenAncientDoor, loop.CurrentStage);

        door.Open();
        loop.EvaluateProgress();
        Assert.AreEqual(CoreLoopStage.ReachRewardRoom, loop.CurrentStage);

        loop.CompleteRun();
        Assert.AreEqual(CoreLoopStage.Complete, loop.CurrentStage);
        Assert.IsTrue(loop.IsComplete);
    }

    [Test]
    public void MineableAddsResourceAndBreaks()
    {
        GameObject inventoryObject = new GameObject("Inventory");
        ResourceInventory inventory = inventoryObject.AddComponent<ResourceInventory>();

        GameObject rock = new GameObject("Rock Under Test");
        rock.AddComponent<SpriteRenderer>();
        rock.AddComponent<BoxCollider2D>();
        MineableResource mineable = rock.AddComponent<MineableResource>();
        mineable.Configure("Stone Rock", 2, ResourceType.Stone, 1);

        mineable.Hit(inventory, Vector2.zero);
        Assert.AreEqual(0, inventory.Stone);
        Assert.IsFalse(mineable.IsBroken);

        mineable.Hit(inventory, Vector2.zero);
        Assert.AreEqual(1, inventory.Stone);
        Assert.IsTrue(mineable.IsBroken);
        Assert.IsFalse(rock.activeSelf);

        UnityEngine.Object.DestroyImmediate(rock);
        UnityEngine.Object.DestroyImmediate(inventoryObject);
    }

    [Test]
    public void AncientDoorOpensOnlyWithEnoughGlowCrystals()
    {
        GameObject inventoryObject = new GameObject("Inventory");
        ResourceInventory inventory = inventoryObject.AddComponent<ResourceInventory>();

        GameObject doorObject = new GameObject("Door Under Test");
        doorObject.AddComponent<SpriteRenderer>();
        BoxCollider2D collider = doorObject.AddComponent<BoxCollider2D>();
        AncientDoor door = doorObject.AddComponent<AncientDoor>();
        door.Configure(ResourceType.GlowCrystal, 3, null);

        Assert.IsFalse(door.TryOpen(inventory));
        Assert.IsFalse(door.IsOpen);
        Assert.IsTrue(collider.enabled);

        inventory.Add(ResourceType.GlowCrystal, 3);

        Assert.IsTrue(door.TryOpen(inventory));
        Assert.IsTrue(door.IsOpen);
        Assert.IsFalse(collider.enabled);

        UnityEngine.Object.DestroyImmediate(doorObject);
        UnityEngine.Object.DestroyImmediate(inventoryObject);
    }

    [Test]
    public void CraftingStationCraftsRecipesAndConsumesCostsOnce()
    {
        GameObject player = new GameObject("Crafting Test Player");
        ResourceInventory resources = player.AddComponent<ResourceInventory>();
        CraftedInventory crafted = player.AddComponent<CraftedInventory>();
        PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();

        GameObject stationObject = new GameObject("Workbench Under Test");
        CircleCollider2D collider = stationObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        CraftingStation station = stationObject.AddComponent<CraftingStation>();

        station.SelectRecipe(CraftedItemType.Torch);
        station.Interact(interaction);
        Assert.AreEqual(0, resources.RootFiber);
        Assert.AreEqual(0, crafted.Torches);

        resources.Add(ResourceType.RootFiber, 1);
        station.SelectRecipe(CraftedItemType.Torch);
        station.Interact(interaction);
        Assert.AreEqual(0, resources.RootFiber);
        Assert.AreEqual(1, crafted.Torches);

        resources.Add(ResourceType.Stone, 2);
        station.SelectRecipe(CraftedItemType.StoneMarker);
        station.Interact(interaction);
        Assert.AreEqual(0, resources.Stone);
        Assert.AreEqual(1, crafted.StoneMarkers);

        resources.Add(ResourceType.GlowCrystal, 1);
        resources.Add(ResourceType.Stone, 1);
        station.SelectRecipe(CraftedItemType.CrystalKeyShard);
        station.Interact(interaction);
        Assert.AreEqual(0, resources.GlowCrystal);
        Assert.AreEqual(0, resources.Stone);
        Assert.AreEqual(1, crafted.CrystalKeyShards);

        UnityEngine.Object.DestroyImmediate(stationObject);
        UnityEngine.Object.DestroyImmediate(player);
    }

    [Test]
    public void TorchPlacementRequiresCraftedTorchAndConsumesOne()
    {
        GameObject player = new GameObject("Torch Test Player");
        player.transform.position = Vector3.zero;
        player.AddComponent<ResourceInventory>();
        CraftedInventory crafted = player.AddComponent<CraftedInventory>();
        PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();

        GameObject torchPrefab = new GameObject("Torch Prefab Under Test");
        torchPrefab.AddComponent<SpriteRenderer>();
        Light2D light = torchPrefab.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.color = new Color(1f, 0.55f, 0.2f);
        light.intensity = 1.2f;
        light.pointLightOuterRadius = 2.5f;
        interaction.AssignTorchPrefab(torchPrefab);

        Assert.IsFalse(interaction.TryPlaceTorch());
        Assert.AreEqual(0, crafted.Torches);
        Assert.IsNull(GameObject.Find("Placed Torch"));

        crafted.Add(CraftedItemType.Torch, 1);

        Assert.IsTrue(interaction.TryPlaceTorch());
        Assert.AreEqual(0, crafted.Torches);

        GameObject placedTorch = GameObject.Find("Placed Torch");
        Assert.IsNotNull(placedTorch);
        Assert.IsNotNull(placedTorch.GetComponent<Light2D>());

        UnityEngine.Object.DestroyImmediate(placedTorch);
        UnityEngine.Object.DestroyImmediate(torchPrefab);
        UnityEngine.Object.DestroyImmediate(player);
    }

    [Test]
    public void CaveCrawlerChasesPlayerButRetreatsFromWarmLight()
    {
        GameObject player = new GameObject("Crawler Target Under Test");
        player.transform.position = new Vector3(51f, 50f, 0f);

        GameObject enemyObject = new GameObject("Crawler State Under Test");
        enemyObject.transform.position = new Vector3(50f, 50f, 0f);
        enemyObject.AddComponent<Rigidbody2D>();
        enemyObject.AddComponent<CircleCollider2D>();
        CaveCrawlerEnemy crawler = enemyObject.AddComponent<CaveCrawlerEnemy>();
        crawler.Configure(player.transform, enemyObject.transform.position);

        Assert.AreEqual(CaveCrawlerState.Chasing, crawler.EvaluateState());

        GameObject warmLightObject = new GameObject("Warm Light Under Test");
        warmLightObject.transform.position = enemyObject.transform.position;
        Light2D warmLight = warmLightObject.AddComponent<Light2D>();
        warmLight.lightType = Light2D.LightType.Point;
        warmLight.color = new Color(1f, 0.48f, 0.16f);
        warmLight.intensity = 1.2f;
        warmLight.pointLightOuterRadius = 3f;

        Assert.AreEqual(CaveCrawlerState.Retreating, crawler.EvaluateState());

        UnityEngine.Object.DestroyImmediate(warmLightObject);
        UnityEngine.Object.DestroyImmediate(enemyObject);
        UnityEngine.Object.DestroyImmediate(player);
    }

    [Test]
    public void EnemyContactAppliesKnockbackConsequence()
    {
        GameObject player = new GameObject("Knockback Test Player");
        player.transform.position = Vector3.zero;
        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        PlayerKnockback knockback = player.AddComponent<PlayerKnockback>();

        GameObject enemyObject = new GameObject("Contact Crawler Under Test");
        enemyObject.transform.position = Vector3.left;
        enemyObject.AddComponent<Rigidbody2D>();
        enemyObject.AddComponent<CircleCollider2D>();
        CaveCrawlerEnemy crawler = enemyObject.AddComponent<CaveCrawlerEnemy>();

        Assert.IsTrue(crawler.ApplyContactConsequence(knockback));
        Assert.AreEqual(1, knockback.KnockbackCount);

        UnityEngine.Object.DestroyImmediate(enemyObject);
        UnityEngine.Object.DestroyImmediate(player);
    }

    [Test]
    public void ClearedCriticalPathIsWalkableFromSpawnToRewardRoom()
    {
        foreach (MineableResource mineable in UnityEngine.Object.FindObjectsByType<MineableResource>(FindObjectsSortMode.None))
        {
            mineable.gameObject.SetActive(false);
        }

        AncientDoor door = UnityEngine.Object.FindFirstObjectByType<AncientDoor>();
        Assert.IsNotNull(door);
        door.Open();
        Physics2D.SyncTransforms();

        Vector2[] route =
        {
            new Vector2(-15f, 0.2f),
            new Vector2(-10.5f, 0f),
            new Vector2(-6.2f, 0f),
            new Vector2(-2f, 0f),
            new Vector2(2.5f, 0f),
            new Vector2(6.8f, 0f),
            new Vector2(10.2f, 0f),
            new Vector2(13.5f, 0f),
            new Vector2(14f, -4.3f)
        };

        StringBuilder blocked = new StringBuilder();
        const float playerRadius = 0.34f;
        for (int i = 0; i < route.Length - 1; i++)
        {
            Vector2 start = route[i];
            Vector2 end = route[i + 1];
            float distance = Vector2.Distance(start, end);
            int steps = Mathf.CeilToInt(distance / 0.25f);

            for (int step = 0; step <= steps; step++)
            {
                Vector2 sample = Vector2.Lerp(start, end, step / (float)steps);
                Collider2D[] overlaps = Physics2D.OverlapCircleAll(sample, playerRadius);
                foreach (Collider2D overlap in overlaps)
                {
                    if (!IsBlockingPath(overlap))
                    {
                        continue;
                    }

                    blocked.AppendLine(overlap.name + " at " + sample);
                }
            }
        }

        Assert.IsEmpty(blocked.ToString());
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

        if (overlap.GetComponentInParent<MineableResource>() != null)
        {
            return false;
        }

        return true;
    }
}
