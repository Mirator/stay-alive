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
        Assert.GreaterOrEqual(UnityEngine.Object.FindObjectsByType<Light2D>(FindObjectsSortMode.None).Length, 6);
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
