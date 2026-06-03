using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class StayAliveCaveSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string ArtRoot = "Assets/Art/Generated";
    private const string SpriteRoot = "Assets/Art/Generated/Sprites";
    private const string PrefabRoot = "Assets/Prefabs";
    private const int SpritePpu = 64;

    private static Material spriteLitMaterial;

    [MenuItem("Stay Alive/Build Cave Baseline")]
    public static void BuildCaveBaseline()
    {
        BuildCaveScene();
    }

    public static void BuildCaveSceneFromCommandLine()
    {
        BuildCaveScene();
    }

    private static void BuildCaveScene()
    {
        EnsureFolders();
        Dictionary<string, Sprite> sprites = GenerateSprites();
        ConfigureConceptArt();
        GameObject torchPrefab = CreateTorchPrefab(sprites);
        CreateScene(sprites, torchPrefab);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Stay Alive cave baseline scene generated.");
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory(ArtRoot);
        Directory.CreateDirectory(SpriteRoot);
        Directory.CreateDirectory(PrefabRoot);
        Directory.CreateDirectory("Assets/Scenes");
    }

    private static Dictionary<string, Sprite> GenerateSprites()
    {
        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        sprites["floor"] = SaveSprite("floor_cave", CreateFloorTile(), SpritePpu, false);
        sprites["wall"] = SaveSprite("wall_rock", CreateWallRock(), SpritePpu, true);
        sprites["rock"] = SaveSprite("mineable_stone", CreateStoneRock(), SpritePpu, true);
        sprites["dirt"] = SaveSprite("mineable_dirt", CreateDirtMound(), SpritePpu, true);
        sprites["crystal"] = SaveSprite("mineable_crystal", CreateCrystalNode(), SpritePpu, true);
        sprites["root"] = SaveSprite("mineable_root", CreateRootBlockage(), SpritePpu, true);
        sprites["doorClosed"] = SaveSprite("ancient_door_closed", CreateAncientDoor(false), SpritePpu, true);
        sprites["doorOpen"] = SaveSprite("ancient_door_open", CreateAncientDoor(true), SpritePpu, true);
        sprites["torch"] = SaveSprite("torch", CreateTorch(), SpritePpu, true);
        sprites["campfire"] = SaveSprite("campfire", CreateCampfire(), SpritePpu, true);
        sprites["mushroom"] = SaveSprite("mushroom_cluster", CreateMushrooms(), SpritePpu, true);
        sprites["chest"] = SaveSprite("chest", CreateChest(), SpritePpu, true);
        sprites["workbench"] = SaveSprite("workbench", CreateWorkbench(), SpritePpu, true);
        sprites["planks"] = SaveSprite("wooden_planks", CreatePlanks(), SpritePpu, true);
        sprites["crawler"] = SaveSprite("cave_crawler", CreateCaveCrawler(), SpritePpu, true);

        sprites["playerIdleDown"] = SaveSprite("player_idle_down", CreatePlayerSprite(Facing.Down, 0, false), SpritePpu, true);
        sprites["playerIdleUp"] = SaveSprite("player_idle_up", CreatePlayerSprite(Facing.Up, 0, false), SpritePpu, true);
        sprites["playerIdleSide"] = SaveSprite("player_idle_side", CreatePlayerSprite(Facing.Side, 0, false), SpritePpu, true);
        sprites["playerWalkDownA"] = SaveSprite("player_walk_down_a", CreatePlayerSprite(Facing.Down, 1, false), SpritePpu, true);
        sprites["playerWalkDownB"] = SaveSprite("player_walk_down_b", CreatePlayerSprite(Facing.Down, 2, false), SpritePpu, true);
        sprites["playerWalkUpA"] = SaveSprite("player_walk_up_a", CreatePlayerSprite(Facing.Up, 1, false), SpritePpu, true);
        sprites["playerWalkUpB"] = SaveSprite("player_walk_up_b", CreatePlayerSprite(Facing.Up, 2, false), SpritePpu, true);
        sprites["playerWalkSideA"] = SaveSprite("player_walk_side_a", CreatePlayerSprite(Facing.Side, 1, false), SpritePpu, true);
        sprites["playerWalkSideB"] = SaveSprite("player_walk_side_b", CreatePlayerSprite(Facing.Side, 2, false), SpritePpu, true);
        sprites["playerMineDown"] = SaveSprite("player_mine_down", CreatePlayerSprite(Facing.Down, 0, true), SpritePpu, true);
        sprites["playerMineUp"] = SaveSprite("player_mine_up", CreatePlayerSprite(Facing.Up, 0, true), SpritePpu, true);
        sprites["playerMineSide"] = SaveSprite("player_mine_side", CreatePlayerSprite(Facing.Side, 0, true), SpritePpu, true);

        return sprites;
    }

    private static void ConfigureConceptArt()
    {
        string conceptPath = ArtRoot + "/stay_alive_concept.png";
        if (!File.Exists(conceptPath))
        {
            return;
        }

        AssetDatabase.ImportAsset(conceptPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(conceptPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 320f;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.SaveAndReimport();
    }

    private static GameObject CreateTorchPrefab(Dictionary<string, Sprite> sprites)
    {
        string prefabPath = PrefabRoot + "/Torch.prefab";
        GameObject torch = new GameObject("Torch");
        SpriteRenderer renderer = torch.AddComponent<SpriteRenderer>();
        renderer.sprite = sprites["torch"];
        renderer.sortingOrder = 80;
        renderer.sharedMaterial = GetSpriteLitMaterial();

        AddPointLight(torch.transform, "Torch Light", new Color(1f, 0.55f, 0.2f), 1.65f, 2.8f, 0.45f);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(torch, prefabPath);
        UnityEngine.Object.DestroyImmediate(torch);
        return prefab;
    }

    private static void CreateScene(Dictionary<string, Sprite> sprites, GameObject torchPrefab)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject("Stay Alive Cave Baseline");

        GameObject environment = new GameObject("Handcrafted Cave Route");
        environment.transform.SetParent(root.transform);
        HashSet<Vector2Int> floorCells = BuildFloorCells();
        CreateCaveTiles(floorCells, environment.transform, sprites);

        GameObject props = new GameObject("Props and Interactables");
        props.transform.SetParent(root.transform);
        CreateDecorations(props.transform, sprites);
        MineableResource firstBlocker = CreateMineables(props.transform, sprites);
        AncientDoor ancientDoor = CreateDoor(props.transform, sprites);
        RewardRoomGoal rewardGoal = CreateRewardGoal(props.transform);
        CreateConceptMural(props.transform);

        GameObject player = CreatePlayer(sprites, torchPrefab);
        player.transform.SetParent(root.transform);
        CreateEnemy(props.transform, sprites, player.transform);
        CreateGameLoop(root.transform, player.GetComponent<ResourceInventory>(), ancientDoor, firstBlocker, rewardGoal);

        CreateLights();
        CreateCamera(player.transform);
        CreateUi(player.GetComponent<ResourceInventory>(), player.GetComponent<CraftedInventory>());
        CreateEventSystem();

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    private static HashSet<Vector2Int> BuildFloorCells()
    {
        HashSet<Vector2Int> cells = new HashSet<Vector2Int>();
        AddEllipse(cells, new Vector2Int(-14, 0), 5, 4);
        AddEllipse(cells, new Vector2Int(-9, -4), 3, 2);
        AddHorizontalCorridor(cells, -11, -4, 0, 2);
        AddEllipse(cells, new Vector2Int(2, 0), 6, 4);
        AddHorizontalCorridor(cells, -5, 9, 0, 2);
        AddVerticalCorridor(cells, 4, -3, 3, 2);
        AddEllipse(cells, new Vector2Int(13, 0), 5, 3);
        AddHorizontalCorridor(cells, 8, 11, 0, 2);
        AddEllipse(cells, new Vector2Int(14, -5), 3, 2);
        AddVerticalCorridor(cells, 14, -5, -2, 1);
        return cells;
    }

    private static void CreateCaveTiles(HashSet<Vector2Int> floorCells, Transform parent, Dictionary<string, Sprite> sprites)
    {
        System.Random random = new System.Random(7331);
        for (int y = -12; y <= 12; y++)
        {
            for (int x = -20; x <= 20; x++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (floorCells.Contains(cell))
                {
                    GameObject floor = CreateSpriteObject("Floor " + x + "," + y, sprites["floor"], new Vector3(x, y, 0f), -100, parent);
                    floor.transform.rotation = Quaternion.Euler(0f, 0f, random.Next(0, 4) * 90f);
                    float tint = 0.9f + (float)random.NextDouble() * 0.18f;
                    floor.GetComponent<SpriteRenderer>().color = new Color(tint, tint, tint, 1f);
                }
                else if (HasFloorNeighbor(cell, floorCells))
                {
                    GameObject wall = CreateSpriteObject("Cave Wall " + x + "," + y, sprites["wall"], new Vector3(x, y, 0f), SortFromY(y, 10), parent);
                    wall.transform.rotation = Quaternion.Euler(0f, 0f, random.Next(0, 4) * 90f);
                    wall.AddComponent<BoxCollider2D>().size = new Vector2(0.95f, 0.95f);
                }
            }
        }
    }

    private static void CreateDecorations(Transform parent, Dictionary<string, Sprite> sprites)
    {
        CreateProp("Campfire", sprites["campfire"], new Vector3(-15f, -1.1f, 0f), parent, 0.9f, true, "Press E to warm up", "The campfire makes this corner feel almost safe.");
        AddPointLight(parent.Find("Campfire"), "Campfire Light", new Color(1f, 0.42f, 0.16f), 2.4f, 4.2f, 0.8f);

        CreateProp("Old Chest", sprites["chest"], new Vector3(-9f, -4.2f, 0f), parent, 0.95f, true, "Press E to inspect chest", "Storage not implemented, but the chest smells usefully dusty.");

        GameObject workbench = CreateProp("Workbench", sprites["workbench"], new Vector3(-11f, -4.4f, 0f), parent, 1.1f, false, string.Empty, string.Empty);
        CircleCollider2D workbenchCollider = workbench.AddComponent<CircleCollider2D>();
        workbenchCollider.radius = 0.55f;
        workbenchCollider.isTrigger = true;
        workbench.AddComponent<CraftingStation>();

        CreateProp("Mushrooms A", sprites["mushroom"], new Vector3(-17f, 2.2f, 0f), parent, 0.75f, false, string.Empty, string.Empty);
        CreateProp("Mushrooms B", sprites["mushroom"], new Vector3(5.8f, -3.2f, 0f), parent, 0.85f, false, string.Empty, string.Empty);
        CreateProp("Wooden Planks", sprites["planks"], new Vector3(-7.6f, 1.85f, 0f), parent, 1.05f, false, string.Empty, string.Empty);
        CreatePathTorch("Passage Guide Torch A", sprites["torch"], new Vector3(-8.3f, -1.95f, 0f), parent);
        CreatePathTorch("Passage Guide Torch B", sprites["torch"], new Vector3(-3.5f, 1.95f, 0f), parent);
        CreatePathTorch("Crystal Room Guide Torch", sprites["torch"], new Vector3(5.6f, 1.8f, 0f), parent);
        CreateProp("Reward Chest", sprites["chest"], new Vector3(14f, -4.9f, 0f), parent, 1.1f, true, "Press E to inspect reward", "You reached the reward room. Cave baseline complete.");
    }

    private static MineableResource CreateMineables(Transform parent, Dictionary<string, Sprite> sprites)
    {
        MineableResource firstBlocker = CreateMineable("Weak Stone Gate", sprites["rock"], new Vector3(-5.9f, 0f, 0f), parent, "Weak Stone", 1, ResourceType.Stone, 1, 0.95f).GetComponent<MineableResource>();
        CreateMineable("Side Stone A", sprites["rock"], new Vector3(-6.6f, 1.35f, 0f), parent, "Stone Rock", 2, ResourceType.Stone, 1, 0.95f);
        CreateMineable("Side Dirt Mound", sprites["dirt"], new Vector3(-4.7f, -1.35f, 0f), parent, "Dirt Mound", 1, ResourceType.Dirt, 1, 0.85f);
        CreateMineable("Root Blockage", sprites["root"], new Vector3(4.6f, -2.35f, 0f), parent, "Root Blockage", 2, ResourceType.RootFiber, 1, 0.95f);

        CreateCrystal("Crystal Node A", sprites["crystal"], new Vector3(1f, 1.7f, 0f), parent);
        CreateCrystal("Crystal Node B", sprites["crystal"], new Vector3(3.2f, 1.1f, 0f), parent);
        CreateCrystal("Crystal Node C", sprites["crystal"], new Vector3(2.2f, -1.9f, 0f), parent);
        return firstBlocker;
    }

    private static void CreatePathTorch(string name, Sprite sprite, Vector3 position, Transform parent)
    {
        GameObject torch = CreateProp(name, sprite, position, parent, 0.75f, false, string.Empty, string.Empty);
        AddPointLight(torch.transform, "Guide Light", new Color(1f, 0.5f, 0.18f), 1.1f, 2.4f, 0.35f);
    }

    private static void CreateCrystal(string name, Sprite sprite, Vector3 position, Transform parent)
    {
        GameObject crystal = CreateMineable(name, sprite, position, parent, "Crystal Node", 3, ResourceType.GlowCrystal, 1, 1.15f);
        AddPointLight(crystal.transform, "Crystal Glow", new Color(0.25f, 0.85f, 1f), 1.4f, 3.2f, 0.55f);
    }

    private static AncientDoor CreateDoor(Transform parent, Dictionary<string, Sprite> sprites)
    {
        GameObject door = CreateSpriteObject("Ancient Door", sprites["doorClosed"], new Vector3(8.25f, 0f, 0f), SortFromY(0f, 40), parent);
        door.transform.localScale = new Vector3(1.35f, 3.15f, 1f);
        BoxCollider2D collider = door.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.72f, 0.92f);
        AncientDoor ancientDoor = door.AddComponent<AncientDoor>();
        ancientDoor.Configure(ResourceType.GlowCrystal, 3, sprites["doorOpen"]);
        AddPointLight(door.transform, "Door Seal Glow", new Color(0.2f, 0.75f, 1f), 0.8f, 2.2f, 0.2f);
        return ancientDoor;
    }

    private static RewardRoomGoal CreateRewardGoal(Transform parent)
    {
        GameObject goal = new GameObject("Reward Room Goal");
        goal.transform.position = new Vector3(14f, -4.3f, 0f);
        goal.transform.SetParent(parent);
        CircleCollider2D collider = goal.AddComponent<CircleCollider2D>();
        collider.radius = 1.15f;
        collider.isTrigger = true;
        RewardRoomGoal rewardGoal = goal.AddComponent<RewardRoomGoal>();
        AddPointLight(goal.transform, "Reward Glow", new Color(0.95f, 0.72f, 0.25f), 1.15f, 2.3f, 0.45f);
        return rewardGoal;
    }

    private static void CreateGameLoop(Transform parent, ResourceInventory inventory, AncientDoor door, MineableResource firstBlocker, RewardRoomGoal rewardGoal)
    {
        GameObject loopObject = new GameObject("Core Game Loop");
        loopObject.transform.SetParent(parent);
        GameLoopController loop = loopObject.AddComponent<GameLoopController>();
        loop.Configure(inventory, door, firstBlocker, rewardGoal);
        EditorUtility.SetDirty(loop);
    }

    private static CaveCrawlerEnemy CreateEnemy(Transform parent, Dictionary<string, Sprite> sprites, Transform player)
    {
        GameObject enemy = CreateSpriteObject("Cave Crawler", sprites["crawler"], new Vector3(5.8f, -1.45f, 0f), SortFromY(-1.45f, 30), parent);
        enemy.transform.localScale = Vector3.one * 0.9f;

        Rigidbody2D body = enemy.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.bodyType = RigidbodyType2D.Kinematic;

        CircleCollider2D collider = enemy.AddComponent<CircleCollider2D>();
        collider.radius = 0.42f;
        collider.isTrigger = true;

        CaveCrawlerEnemy crawler = enemy.AddComponent<CaveCrawlerEnemy>();
        crawler.Configure(player, enemy.transform.position);
        EditorUtility.SetDirty(crawler);
        return crawler;
    }

    private static void CreateConceptMural(Transform parent)
    {
        Sprite concept = AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + "/stay_alive_concept.png");
        if (concept == null)
        {
            return;
        }

        GameObject mural = CreateSpriteObject("Ancient Cave Mural", concept, new Vector3(13.6f, 1.7f, 0f), SortFromY(1.7f, -2), parent);
        mural.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
        SpriteRenderer renderer = mural.GetComponent<SpriteRenderer>();
        renderer.color = new Color(0.72f, 0.82f, 0.95f, 0.82f);
    }

    private static GameObject CreatePlayer(Dictionary<string, Sprite> sprites, GameObject torchPrefab)
    {
        GameObject player = CreateSpriteObject("Player Explorer", sprites["playerIdleDown"], new Vector3(-15f, 0.2f, 0f), 120, null);
        player.tag = "Player";
        player.transform.localScale = Vector3.one;

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = player.AddComponent<CircleCollider2D>();
        collider.radius = 0.34f;
        collider.offset = new Vector2(0f, -0.12f);

        player.AddComponent<ResourceInventory>();
        player.AddComponent<CraftedInventory>();
        player.AddComponent<PlayerController2D>();
        player.AddComponent<PlayerKnockback>();
        PlayerSpriteAnimator animator = player.AddComponent<PlayerSpriteAnimator>();
        AssignPlayerSprites(animator, sprites);

        PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();
        interaction.AssignTorchPrefab(torchPrefab);
        EditorUtility.SetDirty(interaction);

        AddPointLight(player.transform, "Lantern Light", new Color(1f, 0.63f, 0.27f), 1.8f, 4.6f, 0.85f);
        return player;
    }

    private static void AssignPlayerSprites(PlayerSpriteAnimator animator, Dictionary<string, Sprite> sprites)
    {
        SerializedObject serialized = new SerializedObject(animator);
        serialized.FindProperty("idleDown").objectReferenceValue = sprites["playerIdleDown"];
        serialized.FindProperty("idleUp").objectReferenceValue = sprites["playerIdleUp"];
        serialized.FindProperty("idleSide").objectReferenceValue = sprites["playerIdleSide"];
        serialized.FindProperty("mineDown").objectReferenceValue = sprites["playerMineDown"];
        serialized.FindProperty("mineUp").objectReferenceValue = sprites["playerMineUp"];
        serialized.FindProperty("mineSide").objectReferenceValue = sprites["playerMineSide"];
        AssignSpriteArray(serialized.FindProperty("walkDown"), sprites["playerWalkDownA"], sprites["playerWalkDownB"]);
        AssignSpriteArray(serialized.FindProperty("walkUp"), sprites["playerWalkUpA"], sprites["playerWalkUpB"]);
        AssignSpriteArray(serialized.FindProperty("walkSide"), sprites["playerWalkSideA"], sprites["playerWalkSideB"]);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignSpriteArray(SerializedProperty property, Sprite first, Sprite second)
    {
        property.arraySize = 2;
        property.GetArrayElementAtIndex(0).objectReferenceValue = first;
        property.GetArrayElementAtIndex(1).objectReferenceValue = second;
    }

    private static void CreateLights()
    {
        GameObject globalLight = new GameObject("Dim Global Cave Light");
        Light2D light = globalLight.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.color = new Color(0.08f, 0.11f, 0.14f);
        light.intensity = 0.22f;
    }

    private static void CreateCamera(Transform player)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6.4f;
        camera.backgroundColor = new Color(0.02f, 0.035f, 0.045f);
        cameraObject.transform.position = player.position + new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<UniversalAdditionalCameraData>();

        SmoothCameraFollow follow = cameraObject.AddComponent<SmoothCameraFollow>();
        follow.SetTarget(player);
        EditorUtility.SetDirty(follow);
    }

    private static void CreateUi(ResourceInventory inventory, CraftedInventory craftedInventory)
    {
        GameObject canvasObject = new GameObject("Cave UI");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        canvasObject.AddComponent<GraphicRaycaster>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        Text stone = CreateUiText("Stone Counter", canvasObject.transform, font, "Stone: 0", new Vector2(18f, -18f), new Vector2(260f, 26f), 21, TextAnchor.UpperLeft, new Color(0.9f, 0.86f, 0.75f));
        Text crystal = CreateUiText("Crystal Counter", canvasObject.transform, font, "Glow Crystal: 0", new Vector2(18f, -46f), new Vector2(300f, 26f), 21, TextAnchor.UpperLeft, new Color(0.65f, 0.92f, 1f));
        Text root = CreateUiText("Root Counter", canvasObject.transform, font, "Root Fiber: 0", new Vector2(18f, -74f), new Vector2(300f, 26f), 21, TextAnchor.UpperLeft, new Color(0.74f, 0.93f, 0.66f));
        Text crafted = CreateUiText("Crafted Counter", canvasObject.transform, font, "Torches: 0 | Markers: 0 | Shards: 0", new Vector2(18f, -102f), new Vector2(420f, 26f), 20, TextAnchor.UpperLeft, new Color(1f, 0.72f, 0.42f));
        Text objective = CreateUiText("Objective", canvasObject.transform, font, "Find 3 Glow Crystals and open the sealed door.", new Vector2(0f, -18f), new Vector2(720f, 34f), 21, TextAnchor.UpperCenter, new Color(1f, 0.9f, 0.68f));
        ConfigureTopCenter(objective.rectTransform);

        Text message = CreateUiText("Message", canvasObject.transform, font, string.Empty, new Vector2(0f, 82f), new Vector2(800f, 42f), 23, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.82f));
        ConfigureBottomCenter(message.rectTransform);

        Text prompt = CreateUiText("Interaction Prompt", canvasObject.transform, font, string.Empty, new Vector2(0f, 34f), new Vector2(560f, 34f), 21, TextAnchor.MiddleCenter, new Color(0.75f, 0.95f, 1f));
        ConfigureBottomCenter(prompt.rectTransform);

        UIController controller = canvasObject.AddComponent<UIController>();
        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("stoneText").objectReferenceValue = stone;
        serialized.FindProperty("glowCrystalText").objectReferenceValue = crystal;
        serialized.FindProperty("rootFiberText").objectReferenceValue = root;
        serialized.FindProperty("craftedText").objectReferenceValue = crafted;
        serialized.FindProperty("objectiveText").objectReferenceValue = objective;
        serialized.FindProperty("messageText").objectReferenceValue = message;
        serialized.FindProperty("promptText").objectReferenceValue = prompt;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        controller.BindInventory(inventory);
        controller.BindCraftedInventory(craftedInventory);
        controller.SetObjective("Find 3 Glow Crystals and open the sealed door.");
    }

    private static Text CreateUiText(string name, Transform parent, Font font, string text, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        Text label = textObject.AddComponent<Text>();
        label.font = font;
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rect = label.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return label;
    }

    private static void ConfigureTopCenter(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
    }

    private static void ConfigureBottomCenter(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<InputSystemUIInputModule>();
    }

    private static GameObject CreateProp(string name, Sprite sprite, Vector3 position, Transform parent, float scale, bool interactable, string prompt, string message)
    {
        GameObject prop = CreateSpriteObject(name, sprite, position, SortFromY(position.y, 20), parent);
        prop.transform.localScale = Vector3.one * scale;

        if (interactable)
        {
            CircleCollider2D collider = prop.AddComponent<CircleCollider2D>();
            collider.radius = 0.48f;
            collider.isTrigger = true;
            BasicInteractable basic = prop.AddComponent<BasicInteractable>();
            basic.Configure(prompt, message);
        }

        return prop;
    }

    private static GameObject CreateMineable(string name, Sprite sprite, Vector3 position, Transform parent, string displayName, int hits, ResourceType resourceType, int amount, float scale)
    {
        GameObject mineable = CreateSpriteObject(name, sprite, position, SortFromY(position.y, 35), parent);
        mineable.transform.localScale = Vector3.one * scale;
        BoxCollider2D collider = mineable.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(0.55f, 0.55f);
        MineableResource resource = mineable.AddComponent<MineableResource>();
        resource.Configure(displayName, hits, resourceType, amount);
        return mineable;
    }

    private static GameObject CreateSpriteObject(string name, Sprite sprite, Vector3 position, int sortingOrder, Transform parent)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.position = position;
        if (parent != null)
        {
            gameObject.transform.SetParent(parent);
        }

        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        renderer.sharedMaterial = GetSpriteLitMaterial();
        return gameObject;
    }

    private static void AddPointLight(Transform parent, string name, Color color, float intensity, float outerRadius, float innerRadius)
    {
        if (parent == null)
        {
            return;
        }

        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent, false);
        Light2D light = lightObject.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.pointLightOuterRadius = outerRadius;
        light.pointLightInnerRadius = innerRadius;
    }

    private static Material GetSpriteLitMaterial()
    {
        if (spriteLitMaterial != null)
        {
            return spriteLitMaterial;
        }

        string materialPath = ArtRoot + "/SpriteLitGenerated.mat";
        spriteLitMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (spriteLitMaterial != null)
        {
            return spriteLitMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        spriteLitMaterial = new Material(shader);
        AssetDatabase.CreateAsset(spriteLitMaterial, materialPath);
        return spriteLitMaterial;
    }

    private static int SortFromY(float y, int offset)
    {
        return Mathf.RoundToInt(-y * 10f) + offset;
    }

    private static bool HasFloorNeighbor(Vector2Int cell, HashSet<Vector2Int> floorCells)
    {
        for (int y = -1; y <= 1; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                if (floorCells.Contains(new Vector2Int(cell.x + x, cell.y + y)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void AddEllipse(HashSet<Vector2Int> cells, Vector2Int center, int radiusX, int radiusY)
    {
        for (int y = center.y - radiusY; y <= center.y + radiusY; y++)
        {
            for (int x = center.x - radiusX; x <= center.x + radiusX; x++)
            {
                float nx = (x - center.x) / (float)radiusX;
                float ny = (y - center.y) / (float)radiusY;
                if (nx * nx + ny * ny <= 1.08f)
                {
                    cells.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    private static void AddHorizontalCorridor(HashSet<Vector2Int> cells, int xMin, int xMax, int yCenter, int halfHeight)
    {
        for (int x = xMin; x <= xMax; x++)
        {
            for (int y = yCenter - halfHeight; y <= yCenter + halfHeight; y++)
            {
                cells.Add(new Vector2Int(x, y));
            }
        }
    }

    private static void AddVerticalCorridor(HashSet<Vector2Int> cells, int xCenter, int yMin, int yMax, int halfWidth)
    {
        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xCenter - halfWidth; x <= xCenter + halfWidth; x++)
            {
                cells.Add(new Vector2Int(x, y));
            }
        }
    }

    private static Sprite SaveSprite(string name, Texture2D texture, float pixelsPerUnit, bool alpha)
    {
        string path = SpriteRoot + "/" + name + ".png";
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            throw new InvalidOperationException("Could not import generated sprite: " + path);
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.alphaIsTransparency = alpha;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Texture2D CreateFloorTile()
    {
        Texture2D texture = NewTexture(64, 64, new Color(0.17f, 0.12f, 0.1f, 1f));
        System.Random random = new System.Random(11);

        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float noise = (float)random.NextDouble();
                Color baseColor = new Color(0.16f, 0.115f, 0.095f, 1f);
                Color tint = Color.Lerp(baseColor, new Color(0.24f, 0.18f, 0.13f, 1f), noise * 0.28f);
                texture.SetPixel(x, y, tint);
            }
        }

        FillEllipse(texture, 18, 44, 18, 7, new Color(0.24f, 0.18f, 0.14f, 0.28f));
        FillEllipse(texture, 48, 17, 12, 8, new Color(0.09f, 0.16f, 0.15f, 0.18f));
        DrawLine(texture, 7, 13, 34, 20, new Color(0.08f, 0.07f, 0.06f, 0.28f), 1);
        DrawLine(texture, 38, 49, 59, 39, new Color(0.28f, 0.22f, 0.16f, 0.22f), 1);
        return texture;
    }

    private static Texture2D CreateWallRock()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 31, 27, 29, 23, new Color(0.08f, 0.075f, 0.08f, 1f));
        FillEllipse(texture, 25, 34, 24, 22, new Color(0.2f, 0.17f, 0.15f, 1f));
        FillEllipse(texture, 44, 26, 17, 18, new Color(0.13f, 0.12f, 0.13f, 1f));
        FillEllipse(texture, 18, 22, 16, 14, new Color(0.11f, 0.1f, 0.1f, 1f));
        DrawLine(texture, 13, 38, 31, 48, new Color(0.31f, 0.25f, 0.2f, 0.45f), 2);
        DrawLine(texture, 31, 12, 51, 22, new Color(0.05f, 0.04f, 0.04f, 0.5f), 2);
        return texture;
    }

    private static Texture2D CreateStoneRock()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 12, 21, 6, new Color(0f, 0f, 0f, 0.28f));
        FillEllipse(texture, 26, 30, 20, 17, new Color(0.32f, 0.31f, 0.3f, 1f));
        FillEllipse(texture, 40, 28, 15, 14, new Color(0.22f, 0.22f, 0.23f, 1f));
        FillEllipse(texture, 22, 24, 13, 11, new Color(0.2f, 0.19f, 0.19f, 1f));
        DrawLine(texture, 18, 34, 35, 39, new Color(0.58f, 0.54f, 0.48f, 0.6f), 2);
        DrawLine(texture, 31, 18, 48, 28, new Color(0.08f, 0.08f, 0.08f, 0.5f), 2);
        return texture;
    }

    private static Texture2D CreateDirtMound()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 14, 24, 7, new Color(0f, 0f, 0f, 0.22f));
        FillEllipse(texture, 32, 27, 25, 16, new Color(0.35f, 0.21f, 0.11f, 1f));
        FillEllipse(texture, 21, 31, 12, 8, new Color(0.48f, 0.29f, 0.13f, 0.9f));
        FillEllipse(texture, 45, 25, 11, 8, new Color(0.22f, 0.13f, 0.08f, 0.75f));
        DrawLine(texture, 15, 30, 43, 36, new Color(0.62f, 0.39f, 0.18f, 0.45f), 1);
        return texture;
    }

    private static Texture2D CreateCrystalNode()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 11, 22, 6, new Color(0f, 0.25f, 0.32f, 0.3f));
        FillEllipse(texture, 32, 20, 21, 10, new Color(0.16f, 0.15f, 0.16f, 1f));
        FillDiamond(texture, 29, 35, 9, 23, new Color(0.2f, 0.95f, 1f, 1f));
        FillDiamond(texture, 42, 31, 7, 18, new Color(0.12f, 0.62f, 0.95f, 1f));
        FillDiamond(texture, 20, 29, 6, 15, new Color(0.52f, 1f, 0.95f, 1f));
        DrawLine(texture, 29, 17, 29, 53, new Color(0.86f, 1f, 1f, 0.45f), 1);
        DrawLine(texture, 42, 17, 42, 45, new Color(0.82f, 1f, 1f, 0.35f), 1);
        return texture;
    }

    private static Texture2D CreateRootBlockage()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 12, 23, 6, new Color(0f, 0f, 0f, 0.25f));
        DrawLine(texture, 10, 23, 51, 35, new Color(0.32f, 0.2f, 0.1f, 1f), 6);
        DrawLine(texture, 13, 38, 48, 18, new Color(0.45f, 0.28f, 0.13f, 1f), 5);
        DrawLine(texture, 24, 48, 37, 12, new Color(0.24f, 0.15f, 0.08f, 1f), 5);
        DrawLine(texture, 16, 26, 27, 35, new Color(0.68f, 0.47f, 0.22f, 0.55f), 2);
        DrawLine(texture, 34, 22, 46, 29, new Color(0.68f, 0.47f, 0.22f, 0.45f), 2);
        return texture;
    }

    private static Texture2D CreateAncientDoor(bool open)
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 8, 25, 6, new Color(0f, 0f, 0f, 0.24f));
        FillRect(texture, 12, 12, 52, 53, new Color(0.17f, 0.16f, 0.18f, 1f));
        FillRect(texture, 17, 17, 47, 53, new Color(0.28f, 0.25f, 0.24f, 1f));
        FillEllipse(texture, 32, 52, 20, 17, new Color(0.28f, 0.25f, 0.24f, 1f));

        if (open)
        {
            FillRect(texture, 23, 15, 41, 52, new Color(0.02f, 0.06f, 0.08f, 0.82f));
            FillEllipse(texture, 32, 52, 9, 9, new Color(0.02f, 0.06f, 0.08f, 0.82f));
        }
        else
        {
            FillRect(texture, 22, 15, 42, 51, new Color(0.11f, 0.1f, 0.12f, 1f));
            DrawLine(texture, 24, 26, 40, 42, new Color(0.16f, 0.82f, 1f, 0.8f), 2);
            DrawLine(texture, 40, 26, 24, 42, new Color(0.16f, 0.82f, 1f, 0.8f), 2);
            FillEllipse(texture, 32, 34, 5, 5, new Color(0.6f, 1f, 1f, 1f));
        }

        DrawLine(texture, 14, 21, 50, 21, new Color(0.45f, 0.38f, 0.3f, 0.55f), 2);
        return texture;
    }

    private static Texture2D CreateTorch()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        DrawLine(texture, 31, 8, 35, 38, new Color(0.42f, 0.24f, 0.12f, 1f), 5);
        FillEllipse(texture, 34, 40, 13, 14, new Color(1f, 0.45f, 0.1f, 0.92f));
        FillEllipse(texture, 34, 43, 8, 11, new Color(1f, 0.86f, 0.25f, 0.95f));
        FillEllipse(texture, 34, 46, 4, 7, new Color(1f, 1f, 0.76f, 0.95f));
        return texture;
    }

    private static Texture2D CreateCampfire()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 13, 22, 7, new Color(0f, 0f, 0f, 0.25f));
        DrawLine(texture, 18, 17, 45, 29, new Color(0.36f, 0.19f, 0.08f, 1f), 5);
        DrawLine(texture, 45, 17, 18, 29, new Color(0.43f, 0.22f, 0.09f, 1f), 5);
        FillEllipse(texture, 31, 32, 14, 19, new Color(1f, 0.36f, 0.08f, 0.96f));
        FillEllipse(texture, 34, 36, 9, 15, new Color(1f, 0.84f, 0.18f, 0.94f));
        FillEllipse(texture, 32, 39, 5, 9, new Color(1f, 1f, 0.74f, 0.9f));
        return texture;
    }

    private static Texture2D CreateMushrooms()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 12, 19, 5, new Color(0f, 0f, 0f, 0.18f));
        DrawMushroom(texture, 24, 26, 12, new Color(0.76f, 0.22f, 0.32f, 1f));
        DrawMushroom(texture, 38, 23, 10, new Color(0.97f, 0.72f, 0.34f, 1f));
        DrawMushroom(texture, 32, 33, 14, new Color(0.45f, 0.26f, 0.8f, 1f));
        return texture;
    }

    private static void DrawMushroom(Texture2D texture, int cx, int cy, int size, Color capColor)
    {
        FillRect(texture, cx - 2, cy - size, cx + 2, cy, new Color(0.82f, 0.72f, 0.58f, 1f));
        FillEllipse(texture, cx, cy, size, size / 2f, capColor);
        FillEllipse(texture, cx - size / 3f, cy + 1, 2, 2, new Color(1f, 0.92f, 0.78f, 0.9f));
        FillEllipse(texture, cx + size / 4f, cy, 2, 2, new Color(1f, 0.92f, 0.78f, 0.85f));
    }

    private static Texture2D CreateChest()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 12, 22, 5, new Color(0f, 0f, 0f, 0.22f));
        FillRect(texture, 14, 20, 50, 42, new Color(0.34f, 0.18f, 0.08f, 1f));
        FillEllipse(texture, 32, 42, 18, 11, new Color(0.45f, 0.25f, 0.1f, 1f));
        FillRect(texture, 14, 31, 50, 36, new Color(0.72f, 0.48f, 0.18f, 1f));
        FillRect(texture, 29, 28, 35, 40, new Color(0.9f, 0.66f, 0.25f, 1f));
        return texture;
    }

    private static Texture2D CreateWorkbench()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 12, 24, 5, new Color(0f, 0f, 0f, 0.22f));
        FillRect(texture, 12, 29, 52, 39, new Color(0.42f, 0.24f, 0.11f, 1f));
        FillRect(texture, 16, 16, 21, 31, new Color(0.26f, 0.14f, 0.07f, 1f));
        FillRect(texture, 43, 16, 48, 31, new Color(0.26f, 0.14f, 0.07f, 1f));
        DrawLine(texture, 20, 42, 43, 49, new Color(0.68f, 0.48f, 0.28f, 1f), 3);
        FillEllipse(texture, 42, 47, 4, 4, new Color(0.48f, 0.48f, 0.46f, 1f));
        return texture;
    }

    private static Texture2D CreateCaveCrawler()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 11, 20, 5, new Color(0f, 0f, 0f, 0.28f));
        FillEllipse(texture, 32, 30, 18, 13, new Color(0.08f, 0.1f, 0.1f, 1f));
        FillEllipse(texture, 23, 33, 11, 9, new Color(0.13f, 0.16f, 0.15f, 1f));
        FillEllipse(texture, 42, 30, 13, 10, new Color(0.05f, 0.07f, 0.07f, 1f));
        FillEllipse(texture, 25, 37, 3, 3, new Color(0.95f, 0.36f, 0.12f, 1f));
        FillEllipse(texture, 36, 38, 3, 3, new Color(0.95f, 0.36f, 0.12f, 1f));
        DrawLine(texture, 17, 29, 4, 21, new Color(0.04f, 0.05f, 0.05f, 1f), 3);
        DrawLine(texture, 20, 22, 7, 16, new Color(0.04f, 0.05f, 0.05f, 1f), 3);
        DrawLine(texture, 46, 28, 60, 21, new Color(0.04f, 0.05f, 0.05f, 1f), 3);
        DrawLine(texture, 43, 21, 57, 15, new Color(0.04f, 0.05f, 0.05f, 1f), 3);
        DrawLine(texture, 22, 42, 10, 52, new Color(0.04f, 0.05f, 0.05f, 1f), 3);
        DrawLine(texture, 42, 41, 55, 50, new Color(0.04f, 0.05f, 0.05f, 1f), 3);
        DrawLine(texture, 20, 31, 41, 26, new Color(0.22f, 0.28f, 0.25f, 0.45f), 2);
        return texture;
    }

    private static Texture2D CreatePlanks()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        DrawLine(texture, 11, 20, 53, 34, new Color(0.42f, 0.24f, 0.12f, 1f), 7);
        DrawLine(texture, 16, 37, 49, 24, new Color(0.33f, 0.18f, 0.09f, 1f), 7);
        DrawLine(texture, 15, 20, 50, 32, new Color(0.72f, 0.48f, 0.24f, 0.38f), 1);
        return texture;
    }

    private static Texture2D CreatePlayerSprite(Facing facing, int walkFrame, bool mining)
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        int bob = walkFrame == 1 ? 1 : walkFrame == 2 ? -1 : 0;

        FillEllipse(texture, 32, 10, 14, 5, new Color(0f, 0f, 0f, 0.25f));
        FillEllipse(texture, 32, 26 + bob, 11, 14, new Color(0.2f, 0.31f, 0.25f, 1f));
        FillRect(texture, 25, 18 + bob, 39, 31 + bob, new Color(0.16f, 0.22f, 0.2f, 1f));

        if (facing == Facing.Up)
        {
            FillEllipse(texture, 32, 43 + bob, 12, 11, new Color(0.37f, 0.32f, 0.24f, 1f));
            FillRect(texture, 24, 32 + bob, 40, 42 + bob, new Color(0.12f, 0.09f, 0.08f, 1f));
            FillEllipse(texture, 32, 42 + bob, 10, 6, new Color(0.78f, 0.58f, 0.33f, 1f));
        }
        else if (facing == Facing.Side)
        {
            FillEllipse(texture, 32, 43 + bob, 12, 11, new Color(0.78f, 0.58f, 0.38f, 1f));
            FillEllipse(texture, 35, 43 + bob, 9, 9, new Color(0.37f, 0.32f, 0.24f, 1f));
            FillRect(texture, 34, 40 + bob, 44, 45 + bob, new Color(0.93f, 0.72f, 0.42f, 1f));
            FillEllipse(texture, 43, 43 + bob, 4, 4, new Color(1f, 0.78f, 0.24f, 1f));
        }
        else
        {
            FillEllipse(texture, 32, 43 + bob, 12, 11, new Color(0.78f, 0.58f, 0.38f, 1f));
            FillEllipse(texture, 32, 46 + bob, 13, 7, new Color(0.37f, 0.32f, 0.24f, 1f));
            FillRect(texture, 27, 40 + bob, 29, 42 + bob, new Color(0.08f, 0.06f, 0.04f, 1f));
            FillRect(texture, 35, 40 + bob, 37, 42 + bob, new Color(0.08f, 0.06f, 0.04f, 1f));
            FillEllipse(texture, 40, 26 + bob, 4, 4, new Color(1f, 0.76f, 0.2f, 1f));
        }

        DrawLegs(texture, bob);

        if (mining)
        {
            DrawPickaxe(texture, facing);
        }
        else
        {
            DrawLine(texture, 22, 29 + bob, 17, 21 - bob, new Color(0.78f, 0.58f, 0.38f, 1f), 4);
            DrawLine(texture, 42, 29 + bob, 47, 21 + bob, new Color(0.78f, 0.58f, 0.38f, 1f), 4);
        }

        return texture;
    }

    private static void DrawLegs(Texture2D texture, int bob)
    {
        DrawLine(texture, 28, 18 + bob, 25, 9, new Color(0.1f, 0.12f, 0.15f, 1f), 4);
        DrawLine(texture, 36, 18 + bob, 39, 9, new Color(0.1f, 0.12f, 0.15f, 1f), 4);
    }

    private static void DrawPickaxe(Texture2D texture, Facing facing)
    {
        if (facing == Facing.Up)
        {
            DrawLine(texture, 20, 50, 47, 32, new Color(0.48f, 0.27f, 0.12f, 1f), 3);
            DrawLine(texture, 17, 50, 27, 58, new Color(0.62f, 0.66f, 0.67f, 1f), 3);
        }
        else if (facing == Facing.Side)
        {
            DrawLine(texture, 40, 35, 56, 47, new Color(0.48f, 0.27f, 0.12f, 1f), 3);
            DrawLine(texture, 51, 50, 61, 42, new Color(0.62f, 0.66f, 0.67f, 1f), 3);
        }
        else
        {
            DrawLine(texture, 20, 33, 48, 50, new Color(0.48f, 0.27f, 0.12f, 1f), 3);
            DrawLine(texture, 43, 53, 56, 45, new Color(0.62f, 0.66f, 0.67f, 1f), 3);
        }
    }

    private enum Facing
    {
        Down,
        Up,
        Side
    }

    private static Texture2D NewTexture(int width, int height, Color fill)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, fill);
            }
        }

        return texture;
    }

    private static void FillRect(Texture2D texture, int xMin, int yMin, int xMax, int yMax, Color color)
    {
        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                BlendPixel(texture, x, y, color);
            }
        }
    }

    private static void FillEllipse(Texture2D texture, float cx, float cy, float rx, float ry, Color color)
    {
        int xMin = Mathf.FloorToInt(cx - rx);
        int xMax = Mathf.CeilToInt(cx + rx);
        int yMin = Mathf.FloorToInt(cy - ry);
        int yMax = Mathf.CeilToInt(cy + ry);

        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                float nx = (x - cx) / rx;
                float ny = (y - cy) / ry;
                if (nx * nx + ny * ny <= 1f)
                {
                    BlendPixel(texture, x, y, color);
                }
            }
        }
    }

    private static void FillDiamond(Texture2D texture, float cx, float cy, float rx, float ry, Color color)
    {
        int xMin = Mathf.FloorToInt(cx - rx);
        int xMax = Mathf.CeilToInt(cx + rx);
        int yMin = Mathf.FloorToInt(cy - ry);
        int yMax = Mathf.CeilToInt(cy + ry);

        for (int y = yMin; y <= yMax; y++)
        {
            for (int x = xMin; x <= xMax; x++)
            {
                float nx = Mathf.Abs((x - cx) / rx);
                float ny = Mathf.Abs((y - cy) / ry);
                if (nx + ny <= 1f)
                {
                    BlendPixel(texture, x, y, color);
                }
            }
        }
    }

    private static void DrawLine(Texture2D texture, int x0, int y0, int x1, int y1, Color color, int thickness)
    {
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            FillEllipse(texture, x0, y0, thickness, thickness, color);
            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    private static void BlendPixel(Texture2D texture, int x, int y, Color source)
    {
        if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
        {
            return;
        }

        Color destination = texture.GetPixel(x, y);
        float alpha = source.a + destination.a * (1f - source.a);
        if (alpha <= 0f)
        {
            texture.SetPixel(x, y, Color.clear);
            return;
        }

        Color blended = new Color(
            (source.r * source.a + destination.r * destination.a * (1f - source.a)) / alpha,
            (source.g * source.a + destination.g * destination.a * (1f - source.a)) / alpha,
            (source.b * source.a + destination.b * destination.a * (1f - source.a)) / alpha,
            alpha);
        texture.SetPixel(x, y, blended);
    }
}
