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

public static class StayAliveWildernessSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string ArtRoot = "Assets/Art/Generated";
    private const string SpriteRoot = "Assets/Art/Generated/Sprites";
    private const int SpritePpu = 64;

    private static Material spriteLitMaterial;

    [MenuItem("Stay Alive/Build Wilderness Scene")]
    public static void BuildWilderness()
    {
        Build();
    }

    public static void BuildWildernessFromCommandLine()
    {
        Build();
    }

    private static void Build()
    {
        EnsureFolders();
        Dictionary<string, Sprite> sprites = GenerateSprites();
        CreateScene(sprites);
        AssetDatabase.SaveAssets();
        Debug.Log("Stay Alive wilderness scene generated.");
    }

    private static void EnsureFolders()
    {
        Directory.CreateDirectory(ArtRoot);
        Directory.CreateDirectory(SpriteRoot);
        Directory.CreateDirectory("Assets/Scenes");
    }

    private static Dictionary<string, Sprite> GenerateSprites()
    {
        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        sprites["grass"] = SaveSprite("wilderness_grass_tile", CreateGrassTile(), SpritePpu, false);
        sprites["soil"] = SaveSprite("wilderness_soil_tile", CreateSoilTile(), SpritePpu, false);
        sprites["water"] = SaveSprite("wilderness_water_tile", CreateWaterTile(), SpritePpu, false);
        sprites["tree"] = SaveSprite("wilderness_tree", CreateTree(), SpritePpu, true);
        sprites["log"] = SaveSprite("wilderness_log_node", CreateLogNode(), SpritePpu, true);
        sprites["stone"] = SaveSprite("wilderness_stone_node", CreateStoneNode(), SpritePpu, true);
        sprites["food"] = SaveSprite("wilderness_food_bush", CreateFoodBush(), SpritePpu, true);
        sprites["herb"] = SaveSprite("wilderness_herb_plant", CreateHerbPlant(), SpritePpu, true);
        sprites["floor"] = SaveSprite("wilderness_build_floor", CreateBuildFloor(), SpritePpu, true);
        sprites["wall"] = SaveSprite("wilderness_build_wall", CreateBuildWall(), SpritePpu, true);
        sprites["door"] = SaveSprite("wilderness_build_door", CreateBuildDoor(), SpritePpu, true);
        sprites["campfire"] = SaveSprite("wilderness_campfire", CreateCampfire(), SpritePpu, true);
        sprites["bedroll"] = SaveSprite("wilderness_bedroll", CreateBedroll(), SpritePpu, true);
        sprites["storage"] = SaveSprite("wilderness_storage_box", CreateStorageBox(), SpritePpu, true);
        sprites["workbench"] = SaveSprite("wilderness_workbench", CreateWorkbench(), SpritePpu, true);
        sprites["wolf"] = SaveSprite("wilderness_wolf", CreateWildAnimal(new Color(0.23f, 0.25f, 0.24f, 1f), new Color(0.85f, 0.36f, 0.12f, 1f)), SpritePpu, true);
        sprites["boar"] = SaveSprite("wilderness_boar", CreateWildAnimal(new Color(0.38f, 0.24f, 0.16f, 1f), new Color(0.95f, 0.8f, 0.55f, 1f)), SpritePpu, true);

        sprites["playerIdleDown"] = SaveSprite("wilderness_player_idle_down", CreatePlayerSprite(Facing.Down, 0, false), SpritePpu, true);
        sprites["playerIdleUp"] = SaveSprite("wilderness_player_idle_up", CreatePlayerSprite(Facing.Up, 0, false), SpritePpu, true);
        sprites["playerIdleSide"] = SaveSprite("wilderness_player_idle_side", CreatePlayerSprite(Facing.Side, 0, false), SpritePpu, true);
        sprites["playerWalkDownA"] = SaveSprite("wilderness_player_walk_down_a", CreatePlayerSprite(Facing.Down, 1, false), SpritePpu, true);
        sprites["playerWalkDownB"] = SaveSprite("wilderness_player_walk_down_b", CreatePlayerSprite(Facing.Down, 2, false), SpritePpu, true);
        sprites["playerWalkUpA"] = SaveSprite("wilderness_player_walk_up_a", CreatePlayerSprite(Facing.Up, 1, false), SpritePpu, true);
        sprites["playerWalkUpB"] = SaveSprite("wilderness_player_walk_up_b", CreatePlayerSprite(Facing.Up, 2, false), SpritePpu, true);
        sprites["playerWalkSideA"] = SaveSprite("wilderness_player_walk_side_a", CreatePlayerSprite(Facing.Side, 1, false), SpritePpu, true);
        sprites["playerWalkSideB"] = SaveSprite("wilderness_player_walk_side_b", CreatePlayerSprite(Facing.Side, 2, false), SpritePpu, true);
        sprites["playerMineDown"] = SaveSprite("wilderness_player_attack_down", CreatePlayerSprite(Facing.Down, 0, true), SpritePpu, true);
        sprites["playerMineUp"] = SaveSprite("wilderness_player_attack_up", CreatePlayerSprite(Facing.Up, 0, true), SpritePpu, true);
        sprites["playerMineSide"] = SaveSprite("wilderness_player_attack_side", CreatePlayerSprite(Facing.Side, 0, true), SpritePpu, true);
        return sprites;
    }

    private static void CreateScene(Dictionary<string, Sprite> sprites)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject root = new GameObject("Stay Alive Wilderness");

        GameObject environment = new GameObject("Handcrafted Wilderness Map");
        environment.transform.SetParent(root.transform);
        CreateOutdoorMap(environment.transform, sprites);
        CreateLandmark("Spawn Clearing", new Vector3(0f, 0f, 0f), environment.transform);
        CreateLandmark("Forest Resource Area", new Vector3(-9f, 4f, 0f), environment.transform);
        CreateLandmark("Stone Outcrop", new Vector3(8f, 4f, 0f), environment.transform);
        CreateLandmark("Food And Herb Area", new Vector3(-8f, -7f, 0f), environment.transform);
        CreateLandmark("Water Boundary", new Vector3(0f, 13f, 0f), environment.transform);
        CreateBuildableArea(environment.transform);
        CreateLandmark("Wolf Danger Zone", new Vector3(11f, 7f, 0f), environment.transform);
        CreateLandmark("Boar Danger Zone", new Vector3(12f, -7f, 0f), environment.transform);

        GameObject props = new GameObject("Wilderness Content");
        props.transform.SetParent(root.transform);
        CreateResourceNodes(props.transform, sprites);

        DayNightCycle cycle = CreateDayNight(root.transform);
        GameObject player = CreatePlayer(root.transform, sprites);
        GridBuildingSystem building = player.GetComponent<GridBuildingSystem>();
        GameObject placedParent = new GameObject("Placed Buildings");
        placedParent.transform.SetParent(root.transform);
        building.SetPlacedParent(placedParent.transform);

        CreateStartingSafeObjects(props.transform, sprites);
        CreateWildlife(props.transform, sprites, player.transform, cycle);

        SurvivalGameController game = CreateGameController(root.transform, player.GetComponent<PlayerVitals>(), cycle);
        SurvivalSaveSystem saves = CreateSaveSystem(root.transform, player, cycle, building);
        ObjectiveGuide guide = CreateObjectiveGuide(root.transform, player, cycle, building, saves);
        MainMenuController menu = CreateMainMenu(root.transform, saves);
        CreateCamera(player.transform);
        CreateUi(player, cycle, guide, menu);
        CreateEventSystem();

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
    }

    private static void CreateOutdoorMap(Transform parent, Dictionary<string, Sprite> sprites)
    {
        System.Random random = new System.Random(8811);
        for (int y = -12; y <= 12; y++)
        {
            for (int x = -18; x <= 18; x++)
            {
                bool isBuildArea = x >= -5 && x <= 7 && y >= -8 && y <= -2;
                Sprite sprite = isBuildArea || random.NextDouble() < 0.13 ? sprites["soil"] : sprites["grass"];
                GameObject tile = CreateSpriteObject("Outdoor Ground " + x + "," + y, sprite, new Vector3(x, y, 0f), -100, parent);
                tile.transform.rotation = Quaternion.Euler(0f, 0f, random.Next(0, 4) * 90f);
            }
        }

        CreateBoundary("Water Boundary North", new Vector3(0f, 13f, 0f), new Vector2(39f, 1f), parent, sprites["water"]);
        CreateBoundary("Water Boundary South", new Vector3(0f, -13f, 0f), new Vector2(39f, 1f), parent, sprites["water"]);
        CreateBoundary("Water Boundary East", new Vector3(19f, 0f, 0f), new Vector2(1f, 27f), parent, sprites["water"]);
        CreateBoundary("Water Boundary West", new Vector3(-19f, 0f, 0f), new Vector2(1f, 27f), parent, sprites["water"]);
    }

    private static void CreateBoundary(string name, Vector3 position, Vector2 size, Transform parent, Sprite sprite)
    {
        GameObject boundary = CreateSpriteObject(name, sprite, position, -90, parent);
        boundary.transform.localScale = new Vector3(size.x, size.y, 1f);
        BoxCollider2D collider = boundary.AddComponent<BoxCollider2D>();
        collider.size = Vector2.one;
    }

    private static void CreateLandmark(string name, Vector3 position, Transform parent)
    {
        GameObject landmark = new GameObject(name);
        landmark.transform.position = position;
        landmark.transform.SetParent(parent);
    }

    private static void CreateBuildableArea(Transform parent)
    {
        GameObject area = new GameObject("Open Building Area");
        area.transform.position = new Vector3(1f, -5f, 0f);
        area.transform.SetParent(parent);
        BoxCollider2D collider = area.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(14f, 8f);
        collider.isTrigger = true;
    }

    private static void CreateResourceNodes(Transform parent, Dictionary<string, Sprite> sprites)
    {
        CreateTreeLine(parent, sprites);
        CreateGatherable("Wood Log A", sprites["log"], new Vector3(2.2f, 0.4f, 0f), parent, "wood-a", "Fallen Log", ResourceType.Wood, 3);
        CreateGatherable("Wood Log B", sprites["log"], new Vector3(-8f, 4f, 0f), parent, "wood-b", "Fallen Log", ResourceType.Wood, 4);
        CreateGatherable("Wood Log C", sprites["log"], new Vector3(-11f, 5.5f, 0f), parent, "wood-c", "Fallen Log", ResourceType.Wood, 4);

        CreateGatherable("Stone Node A", sprites["stone"], new Vector3(7f, 4.2f, 0f), parent, "stone-a", "Stone Outcrop", ResourceType.Stone, 2);
        CreateGatherable("Stone Node B", sprites["stone"], new Vector3(9.5f, 3.3f, 0f), parent, "stone-b", "Stone Outcrop", ResourceType.Stone, 2);
        CreateGatherable("Stone Node C", sprites["stone"], new Vector3(10.6f, 5.2f, 0f), parent, "stone-c", "Stone Outcrop", ResourceType.Stone, 3);

        CreateGatherable("Berry Bush A", sprites["food"], new Vector3(-7.2f, -6.4f, 0f), parent, "food-a", "Berry Bush", ResourceType.Food, 2);
        CreateGatherable("Berry Bush B", sprites["food"], new Vector3(-9.7f, -7.1f, 0f), parent, "food-b", "Berry Bush", ResourceType.Food, 2);
        CreateGatherable("Herb Plant A", sprites["herb"], new Vector3(-6.2f, -8.4f, 0f), parent, "herb-a", "Herb Plant", ResourceType.Herbs, 2);
        CreateGatherable("Herb Plant B", sprites["herb"], new Vector3(-10.9f, -8.1f, 0f), parent, "herb-b", "Herb Plant", ResourceType.Herbs, 2);
    }

    private static void CreateTreeLine(Transform parent, Dictionary<string, Sprite> sprites)
    {
        Vector3[] positions =
        {
            new Vector3(-12f, 2.8f, 0f),
            new Vector3(-10.5f, 6.8f, 0f),
            new Vector3(-7.5f, 6.1f, 0f),
            new Vector3(-4.8f, 8.3f, 0f),
            new Vector3(4.5f, 8.6f, 0f),
            new Vector3(13.5f, 2.8f, 0f),
            new Vector3(14.2f, -3.8f, 0f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject tree = CreateSpriteObject("Tree " + i, sprites["tree"], positions[i], SortFromY(positions[i].y, 20), parent);
            tree.transform.localScale = Vector3.one * 1.25f;
            CircleCollider2D collider = tree.AddComponent<CircleCollider2D>();
            collider.radius = 0.45f;
        }
    }

    private static GatherableResource CreateGatherable(string name, Sprite sprite, Vector3 position, Transform parent, string id, string displayName, ResourceType type, int amount)
    {
        GameObject node = CreateSpriteObject(name, sprite, position, SortFromY(position.y, 15), parent);
        CircleCollider2D collider = node.AddComponent<CircleCollider2D>();
        collider.radius = 0.55f;
        collider.isTrigger = true;
        GatherableResource gatherable = node.AddComponent<GatherableResource>();
        gatherable.Configure(id, displayName, type, amount);
        return gatherable;
    }

    private static DayNightCycle CreateDayNight(Transform parent)
    {
        GameObject cycleObject = new GameObject("Day Night Cycle");
        cycleObject.transform.SetParent(parent);
        DayNightCycle cycle = cycleObject.AddComponent<DayNightCycle>();
        cycle.Configure(300f);
        return cycle;
    }

    private static GameObject CreatePlayer(Transform parent, Dictionary<string, Sprite> sprites)
    {
        GameObject player = CreateSpriteObject("Player Survivor", sprites["playerIdleDown"], Vector3.zero, 120, parent);
        player.tag = "Player";

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = player.AddComponent<CircleCollider2D>();
        collider.radius = 0.34f;
        collider.offset = new Vector2(0f, -0.12f);

        ResourceInventory resources = player.AddComponent<ResourceInventory>();

        CraftedInventory crafted = player.AddComponent<CraftedInventory>();
        player.AddComponent<PlayerController2D>();
        player.AddComponent<PlayerVitals>();
        player.AddComponent<MeleeWeapon>();
        GridBuildingSystem building = player.AddComponent<GridBuildingSystem>();
        building.Configure(new Rect(-6f, -9f, 15f, 10f), sprites["floor"], sprites["wall"], sprites["door"], sprites["campfire"], sprites["bedroll"], sprites["storage"], sprites["workbench"]);

        PlayerSpriteAnimator animator = player.AddComponent<PlayerSpriteAnimator>();
        AssignPlayerSprites(animator, sprites);

        PlayerInteraction interaction = player.AddComponent<PlayerInteraction>();
        EditorUtility.SetDirty(interaction);
        AddPointLight(player.transform, "Survivor Lantern", new Color(1f, 0.62f, 0.25f), 1.15f, 3.2f, 0.5f);
        _ = crafted;
        _ = resources;
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

    private static void CreateStartingSafeObjects(Transform parent, Dictionary<string, Sprite> sprites)
    {
        GameObject campfire = CreateSpriteObject("Starting Campfire", sprites["campfire"], new Vector3(-1.2f, -1.35f, 0f), 50, parent);
        CircleCollider2D campfireTrigger = campfire.AddComponent<CircleCollider2D>();
        campfireTrigger.radius = 0.6f;
        campfireTrigger.isTrigger = true;
        SaveStation station = campfire.AddComponent<SaveStation>();
        station.Configure(SaveStationType.Campfire);
        AddPointLight(campfire.transform, "Campfire Light", new Color(1f, 0.42f, 0.16f), 1.9f, 3.8f, 0.6f);

        GameObject workbench = CreateSpriteObject("Old Wilderness Workbench", sprites["workbench"], new Vector3(1.4f, -1.35f, 0f), 50, parent);
        CircleCollider2D workbenchTrigger = workbench.AddComponent<CircleCollider2D>();
        workbenchTrigger.radius = 0.65f;
        workbenchTrigger.isTrigger = true;
        workbench.AddComponent<CraftingStation>();
    }

    private static void CreateWildlife(Transform parent, Dictionary<string, Sprite> sprites, Transform player, DayNightCycle cycle)
    {
        CreateAnimal("Wolf Alpha", "wolf-alpha", sprites["wolf"], new Vector3(11f, 7f, 0f), WildAnimalType.Wolf, parent, player, cycle);
        CreateAnimal("Boar Brute", "boar-brute", sprites["boar"], new Vector3(12f, -7f, 0f), WildAnimalType.Boar, parent, player, cycle);
    }

    private static WildAnimalEnemy CreateAnimal(string name, string id, Sprite sprite, Vector3 position, WildAnimalType type, Transform parent, Transform player, DayNightCycle cycle)
    {
        GameObject animalObject = CreateSpriteObject(name, sprite, position, SortFromY(position.y, 35), parent);
        animalObject.transform.localScale = Vector3.one * 0.95f;
        Rigidbody2D body = animalObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.bodyType = RigidbodyType2D.Kinematic;
        CircleCollider2D collider = animalObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.45f;
        collider.isTrigger = true;
        WildAnimalEnemy animal = animalObject.AddComponent<WildAnimalEnemy>();
        animal.Configure(id, type, player, cycle, position);
        return animal;
    }

    private static SurvivalGameController CreateGameController(Transform parent, PlayerVitals vitals, DayNightCycle cycle)
    {
        GameObject gameObject = new GameObject("Wilderness Survival Controller");
        gameObject.transform.SetParent(parent);
        SurvivalGameController controller = gameObject.AddComponent<SurvivalGameController>();
        controller.Configure(vitals, cycle);
        return controller;
    }

    private static SurvivalSaveSystem CreateSaveSystem(Transform parent, GameObject player, DayNightCycle cycle, GridBuildingSystem building)
    {
        GameObject saveObject = new GameObject("Survival Save System");
        saveObject.transform.SetParent(parent);
        SurvivalSaveSystem saves = saveObject.AddComponent<SurvivalSaveSystem>();
        saves.Configure(player.transform, player.GetComponent<ResourceInventory>(), player.GetComponent<CraftedInventory>(), player.GetComponent<PlayerVitals>(), cycle, building);
        return saves;
    }

    private static MainMenuController CreateMainMenu(Transform parent, SurvivalSaveSystem saves)
    {
        GameObject menu = new GameObject("Main Menu Controller");
        menu.transform.SetParent(parent);
        MainMenuController controller = menu.AddComponent<MainMenuController>();
        controller.Configure(saves);
        return controller;
    }

    private static ObjectiveGuide CreateObjectiveGuide(Transform parent, GameObject player, DayNightCycle cycle, GridBuildingSystem building, SurvivalSaveSystem saves)
    {
        GameObject guideObject = new GameObject("First Session Objective Guide");
        guideObject.transform.SetParent(parent);
        ObjectiveGuide guide = guideObject.AddComponent<ObjectiveGuide>();
        guide.Configure(player.GetComponent<ResourceInventory>(), player.GetComponent<CraftedInventory>(), building, cycle, saves);
        return guide;
    }

    private static void CreateCamera(Transform player)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 7.2f;
        camera.backgroundColor = new Color(0.08f, 0.13f, 0.12f);
        cameraObject.transform.position = player.position + new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<UniversalAdditionalCameraData>();
        SmoothCameraFollow follow = cameraObject.AddComponent<SmoothCameraFollow>();
        follow.SetTarget(player);
    }

    private static void CreateUi(GameObject player, DayNightCycle cycle, ObjectiveGuide guide, MainMenuController menu)
    {
        GameObject canvasObject = new GameObject("Stay Alive UI");
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

        Text stone = CreateUiText("Stone Counter", canvasObject.transform, font, "Stone: 0", new Vector2(18f, -18f), new Vector2(260f, 24f), 18, TextAnchor.UpperLeft, new Color(0.9f, 0.86f, 0.75f));
        Text wood = CreateUiText("Wood Counter", canvasObject.transform, font, "Wood: 0", new Vector2(18f, -42f), new Vector2(260f, 24f), 18, TextAnchor.UpperLeft, new Color(0.8f, 0.95f, 0.68f));
        Text foodHerbs = CreateUiText("Food Herbs Counter", canvasObject.transform, font, "Food: 0 | Herbs: 0", new Vector2(18f, -66f), new Vector2(360f, 24f), 18, TextAnchor.UpperLeft, new Color(0.95f, 0.82f, 0.65f));
        Text crafted = CreateUiText("Crafted Counter", canvasObject.transform, font, "Spear: 0 | Bandage: 0 | Campfire: 0 | Bedroll: 0", new Vector2(18f, -90f), new Vector2(620f, 24f), 17, TextAnchor.UpperLeft, new Color(1f, 0.76f, 0.45f));
        Image healthFill = CreateBar("Health Bar", canvasObject.transform, new Vector2(18f, -126f), new Vector2(210f, 16f), new Color(0.2f, 0.05f, 0.05f, 0.82f), new Color(0.9f, 0.18f, 0.16f, 0.95f));
        Image hungerFill = CreateBar("Hunger Bar", canvasObject.transform, new Vector2(18f, -148f), new Vector2(210f, 16f), new Color(0.12f, 0.09f, 0.03f, 0.82f), new Color(0.95f, 0.68f, 0.2f, 0.95f));
        Text vitals = CreateUiText("Vitals", canvasObject.transform, font, "Health: 100 | Hunger: 100", new Vector2(238f, -123f), new Vector2(510f, 42f), 18, TextAnchor.UpperLeft, new Color(1f, 0.84f, 0.72f));
        Text day = CreateUiText("Day Phase", canvasObject.transform, font, "Day 1 - Day", new Vector2(0f, -18f), new Vector2(420f, 30f), 21, TextAnchor.UpperCenter, new Color(1f, 0.94f, 0.68f));
        ConfigureTopCenter(day.rectTransform);
        Text objective = CreateUiText("Objective", canvasObject.transform, font, "Gather Wood and Stone for your first tool.", new Vector2(0f, -50f), new Vector2(820f, 34f), 20, TextAnchor.UpperCenter, new Color(0.9f, 1f, 0.78f));
        ConfigureTopCenter(objective.rectTransform);
        Text recipe = CreateUiText("Recipe UX", canvasObject.transform, font, string.Empty, new Vector2(18f, -180f), new Vector2(760f, 28f), 17, TextAnchor.UpperLeft, new Color(0.8f, 0.95f, 1f));
        Text build = CreateUiText("Build UX", canvasObject.transform, font, string.Empty, new Vector2(18f, -208f), new Vector2(860f, 44f), 17, TextAnchor.UpperLeft, new Color(0.72f, 0.95f, 0.82f));
        Text save = CreateUiText("Save Slot UX", canvasObject.transform, font, string.Empty, new Vector2(18f, -254f), new Vector2(760f, 28f), 17, TextAnchor.UpperLeft, new Color(0.84f, 0.88f, 1f));
        Text message = CreateUiText("Message", canvasObject.transform, font, string.Empty, new Vector2(0f, 82f), new Vector2(860f, 42f), 23, TextAnchor.MiddleCenter, new Color(1f, 0.95f, 0.82f));
        ConfigureBottomCenter(message.rectTransform);
        Text prompt = CreateUiText("Interaction Prompt", canvasObject.transform, font, string.Empty, new Vector2(0f, 36f), new Vector2(700f, 34f), 21, TextAnchor.MiddleCenter, new Color(0.75f, 0.95f, 1f));
        ConfigureBottomCenter(prompt.rectTransform);
        GameObject deathPanel = CreateDeathPanel(canvasObject.transform, font, menu);
        GameObject pauseHelpPanel = CreatePauseHelpPanel(canvasObject.transform, font, menu);
        GameObject mainMenuPanel = CreateMainMenuPanel(
            canvasObject.transform,
            font,
            menu,
            out GameObject titleMenuPanel,
            out GameObject slotSelectionPanel,
            out Text slotSelectionTitle,
            out Button continueButton,
            out Button[] newButtons,
            out Text[] slotTexts,
            out Button[] loadButtons);
        GameObject overwritePanel = CreateOverwritePanel(canvasObject.transform, font, menu, out Text overwriteText);

        UIController ui = canvasObject.AddComponent<UIController>();
        SerializedObject serialized = new SerializedObject(ui);
        serialized.FindProperty("stoneText").objectReferenceValue = stone;
        serialized.FindProperty("glowCrystalText").objectReferenceValue = wood;
        serialized.FindProperty("rootFiberText").objectReferenceValue = foodHerbs;
        serialized.FindProperty("craftedText").objectReferenceValue = crafted;
        serialized.FindProperty("vitalsText").objectReferenceValue = vitals;
        serialized.FindProperty("dayText").objectReferenceValue = day;
        serialized.FindProperty("objectiveText").objectReferenceValue = objective;
        serialized.FindProperty("messageText").objectReferenceValue = message;
        serialized.FindProperty("promptText").objectReferenceValue = prompt;
        serialized.FindProperty("recipeText").objectReferenceValue = recipe;
        serialized.FindProperty("buildText").objectReferenceValue = build;
        serialized.FindProperty("saveSlotText").objectReferenceValue = save;
        serialized.FindProperty("pauseHelpPanel").objectReferenceValue = pauseHelpPanel;
        serialized.FindProperty("healthFill").objectReferenceValue = healthFill;
        serialized.FindProperty("hungerFill").objectReferenceValue = hungerFill;
        serialized.FindProperty("deathPanel").objectReferenceValue = deathPanel;
        serialized.ApplyModifiedPropertiesWithoutUndo();

        ui.BindInventory(player.GetComponent<ResourceInventory>());
        ui.BindCraftedInventory(player.GetComponent<CraftedInventory>());
        ui.BindVitals(player.GetComponent<PlayerVitals>());
        ui.BindDayNightCycle(cycle);
        ui.BindObjectiveGuide(guide);
        ui.ShowDeath(false);
        if (menu != null)
        {
            menu.ConfigureUi(
                mainMenuPanel,
                titleMenuPanel,
                slotSelectionPanel,
                pauseHelpPanel,
                overwritePanel,
                deathPanel,
                slotSelectionTitle,
                continueButton,
                newButtons,
                slotTexts,
                loadButtons,
                overwriteText);
        }
    }

    private static Image CreateBar(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color background, Color fill)
    {
        GameObject backgroundObject = new GameObject(name + " Background");
        backgroundObject.transform.SetParent(parent, false);
        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = background;
        RectTransform backgroundRect = backgroundImage.rectTransform;
        backgroundRect.anchorMin = new Vector2(0f, 1f);
        backgroundRect.anchorMax = new Vector2(0f, 1f);
        backgroundRect.pivot = new Vector2(0f, 1f);
        backgroundRect.anchoredPosition = anchoredPosition;
        backgroundRect.sizeDelta = size;

        GameObject fillObject = new GameObject(name + " Fill");
        fillObject.transform.SetParent(backgroundObject.transform, false);
        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = fill;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = 0;
        fillImage.fillAmount = 1f;
        RectTransform fillRect = fillImage.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        return fillImage;
    }

    private static GameObject CreateMainMenuPanel(
        Transform parent,
        Font font,
        MainMenuController menu,
        out GameObject titleMenuPanel,
        out GameObject slotSelectionPanel,
        out Text slotSelectionTitle,
        out Button continueButton,
        out Button[] newButtons,
        out Text[] slotTexts,
        out Button[] loadButtons)
    {
        newButtons = new Button[SurvivalSaveSystem.SlotCount];
        slotTexts = new Text[SurvivalSaveSystem.SlotCount];
        loadButtons = new Button[SurvivalSaveSystem.SlotCount];

        GameObject panel = new GameObject("Game Main Menu Panel");
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.025f, 0.05f, 0.045f, 0.94f);
        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(760f, 520f);

        titleMenuPanel = new GameObject("Title Menu Panel");
        titleMenuPanel.transform.SetParent(panel.transform, false);
        RectTransform titlePanelRect = titleMenuPanel.AddComponent<RectTransform>();
        titlePanelRect.anchorMin = Vector2.zero;
        titlePanelRect.anchorMax = Vector2.one;
        titlePanelRect.offsetMin = Vector2.zero;
        titlePanelRect.offsetMax = Vector2.zero;

        Text title = CreateUiText("Game Menu Title", titleMenuPanel.transform, font, "Stay Alive", new Vector2(0f, -60f), new Vector2(680f, 58f), 38, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.72f));
        title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        title.rectTransform.pivot = new Vector2(0.5f, 1f);

        Text subtitle = CreateUiText("Game Menu Subtitle", titleMenuPanel.transform, font, "Wilderness sandbox survival", new Vector2(0f, -116f), new Vector2(680f, 30f), 21, TextAnchor.MiddleCenter, new Color(0.78f, 0.95f, 0.86f));
        subtitle.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        subtitle.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        subtitle.rectTransform.pivot = new Vector2(0.5f, 1f);

        continueButton = CreateMenuButton("Continue Button", titleMenuPanel.transform, font, "Continue", new Vector2(280f, -190f), new Vector2(200f, 46f), menu, GameMenuButtonAction.ContinueGame, 1);
        CreateMenuButton("New Game Menu Button", titleMenuPanel.transform, font, "New Game", new Vector2(280f, -252f), new Vector2(200f, 46f), menu, GameMenuButtonAction.OpenNewGameSlots, 1);
        CreateMenuButton("Load Game Menu Button", titleMenuPanel.transform, font, "Load Game", new Vector2(280f, -314f), new Vector2(200f, 46f), menu, GameMenuButtonAction.OpenLoadGameSlots, 1);

        Text hint = CreateUiText("Game Menu Hint", titleMenuPanel.transform, font, "Save manually at a Campfire or Bedroll.", new Vector2(0f, -402f), new Vector2(680f, 30f), 18, TextAnchor.MiddleCenter, new Color(0.92f, 0.84f, 0.68f));
        hint.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        hint.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        hint.rectTransform.pivot = new Vector2(0.5f, 1f);

        slotSelectionPanel = new GameObject("Save Slot Selection Panel");
        slotSelectionPanel.transform.SetParent(panel.transform, false);
        RectTransform slotPanelRect = slotSelectionPanel.AddComponent<RectTransform>();
        slotPanelRect.anchorMin = Vector2.zero;
        slotPanelRect.anchorMax = Vector2.one;
        slotPanelRect.offsetMin = Vector2.zero;
        slotPanelRect.offsetMax = Vector2.zero;

        slotSelectionTitle = CreateUiText("Save Slot Selection Title", slotSelectionPanel.transform, font, "Choose Slot", new Vector2(0f, -34f), new Vector2(680f, 42f), 28, TextAnchor.MiddleCenter, new Color(1f, 0.94f, 0.72f));
        slotSelectionTitle.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        slotSelectionTitle.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        slotSelectionTitle.rectTransform.pivot = new Vector2(0.5f, 1f);

        for (int i = 0; i < SurvivalSaveSystem.SlotCount; i++)
        {
            int slot = i + 1;
            float y = -104f - i * 104f;
            GameObject row = new GameObject("Save Slot " + slot + " Row");
            row.transform.SetParent(slotSelectionPanel.transform, false);
            Image rowImage = row.AddComponent<Image>();
            rowImage.color = new Color(0.08f, 0.13f, 0.11f, 0.88f);
            RectTransform rowRect = rowImage.rectTransform;
            rowRect.anchorMin = new Vector2(0.5f, 1f);
            rowRect.anchorMax = new Vector2(0.5f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0f, y);
            rowRect.sizeDelta = new Vector2(660f, 78f);

            Text summary = CreateUiText("Save Slot " + slot + " Summary", row.transform, font, "Slot " + slot + ": Empty", new Vector2(18f, -14f), new Vector2(390f, 50f), 18, TextAnchor.UpperLeft, Color.white);
            slotTexts[i] = summary;

            newButtons[i] = CreateMenuButton("Save Slot " + slot + " New Button", row.transform, font, "New", new Vector2(538f, -18f), new Vector2(82f, 38f), menu, GameMenuButtonAction.NewGame, slot);
            loadButtons[i] = CreateMenuButton("Save Slot " + slot + " Load Button", row.transform, font, "Load", new Vector2(538f, -18f), new Vector2(82f, 38f), menu, GameMenuButtonAction.LoadGame, slot);
        }

        CreateMenuButton("Save Slot Back Button", slotSelectionPanel.transform, font, "Back", new Vector2(50f, -444f), new Vector2(116f, 40f), menu, GameMenuButtonAction.BackToTitleMenu, 1);

        panel.SetActive(true);
        titleMenuPanel.SetActive(true);
        slotSelectionPanel.SetActive(false);
        return panel;
    }

    private static GameObject CreateOverwritePanel(Transform parent, Font font, MainMenuController menu, out Text overwriteText)
    {
        GameObject panel = new GameObject("Overwrite Confirmation Panel");
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.16f, 0.08f, 0.04f, 0.96f);
        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(420f, 180f);

        overwriteText = CreateUiText("Overwrite Confirmation Text", panel.transform, font, "Overwrite Slot?", new Vector2(0f, -24f), new Vector2(360f, 44f), 22, TextAnchor.MiddleCenter, Color.white);
        overwriteText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        overwriteText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        overwriteText.rectTransform.pivot = new Vector2(0.5f, 1f);

        Text body = CreateUiText("Overwrite Confirmation Body", panel.transform, font, "Starting a new game here deletes that slot.", new Vector2(0f, -72f), new Vector2(360f, 34f), 17, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.72f));
        body.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        body.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        body.rectTransform.pivot = new Vector2(0.5f, 1f);

        CreateMenuButton("Confirm Overwrite Button", panel.transform, font, "Overwrite", new Vector2(84f, -124f), new Vector2(116f, 38f), menu, GameMenuButtonAction.ConfirmOverwrite, 1);
        CreateMenuButton("Cancel Overwrite Button", panel.transform, font, "Cancel", new Vector2(220f, -124f), new Vector2(116f, 38f), menu, GameMenuButtonAction.CancelOverwrite, 1);
        panel.SetActive(false);
        return panel;
    }

    private static GameObject CreatePauseHelpPanel(Transform parent, Font font, MainMenuController menu)
    {
        GameObject panel = new GameObject("Pause Help Panel");
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.03f, 0.06f, 0.05f, 0.88f);
        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(560f, 330f);

        string help = "Paused\nWASD move | Mouse aim | Left Click attack/use tool\nE gather/interact/craft/save | F eat food | H use bandage\n1-7 choose build piece | Right Click place | R rotate door | X cancel\nGather, craft, build, save, eat, heal, and avoid or fight wildlife.";
        Text label = CreateUiText("Pause Help Text", panel.transform, font, help, new Vector2(0f, 30f), new Vector2(500f, 220f), 20, TextAnchor.MiddleCenter, Color.white);
        label.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        label.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        label.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        CreateMenuButton("Resume Button", panel.transform, font, "Resume", new Vector2(145f, -262f), new Vector2(120f, 42f), menu, GameMenuButtonAction.Resume, 1);
        CreateMenuButton("Return To Main Menu Button", panel.transform, font, "Main Menu", new Vector2(295f, -262f), new Vector2(150f, 42f), menu, GameMenuButtonAction.ReturnToMainMenu, 1);
        panel.SetActive(false);
        return panel;
    }

    private static Button CreateMenuButton(string name, Transform parent, Font font, string label, Vector2 anchoredPosition, Vector2 size, MainMenuController menu, GameMenuButtonAction action, int slot)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.32f, 0.28f, 0.95f);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = CreateUiText(name + " Text", buttonObject.transform, font, label, Vector2.zero, size, 17, TextAnchor.MiddleCenter, Color.white);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;

        GameMenuButton adapter = buttonObject.AddComponent<GameMenuButton>();
        adapter.Configure(menu, action, slot);
        return button;
    }

    private static GameObject CreateDeathPanel(Transform parent, Font font, MainMenuController menu)
    {
        GameObject panel = new GameObject("Death Panel");
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.08f, 0.05f, 0.04f, 0.82f);
        RectTransform rect = image.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(460f, 220f);
        Text label = CreateUiText("Death Text", panel.transform, font, "You died", new Vector2(0f, -28f), new Vector2(390f, 42f), 28, TextAnchor.MiddleCenter, Color.white);
        label.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        label.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        label.rectTransform.pivot = new Vector2(0.5f, 1f);

        Text body = CreateUiText("Death Recovery Text", panel.transform, font, "Load the current slot or return to the title menu.", new Vector2(0f, -78f), new Vector2(390f, 44f), 18, TextAnchor.MiddleCenter, new Color(1f, 0.84f, 0.72f));
        body.rectTransform.anchorMin = new Vector2(0.5f, 1f);
        body.rectTransform.anchorMax = new Vector2(0.5f, 1f);
        body.rectTransform.pivot = new Vector2(0.5f, 1f);

        CreateMenuButton("Death Load Current Slot Button", panel.transform, font, "Load Slot", new Vector2(84f, -150f), new Vector2(128f, 42f), menu, GameMenuButtonAction.LoadActiveSlot, 1);
        CreateMenuButton("Death Return To Main Menu Button", panel.transform, font, "Main Menu", new Vector2(236f, -150f), new Vector2(140f, 42f), menu, GameMenuButtonAction.ReturnToMainMenu, 1);
        panel.SetActive(false);
        return panel;
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

    private static Texture2D CreateGrassTile()
    {
        Texture2D texture = NewTexture(64, 64, new Color(0.22f, 0.36f, 0.22f, 1f));
        System.Random random = new System.Random(21);
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float n = (float)random.NextDouble();
                texture.SetPixel(x, y, Color.Lerp(new Color(0.18f, 0.31f, 0.18f, 1f), new Color(0.34f, 0.48f, 0.25f, 1f), n * 0.55f));
            }
        }

        DrawLine(texture, 4, 48, 17, 53, new Color(0.45f, 0.64f, 0.32f, 0.35f), 1);
        DrawLine(texture, 41, 13, 57, 18, new Color(0.12f, 0.24f, 0.12f, 0.28f), 1);
        return texture;
    }

    private static Texture2D CreateSoilTile()
    {
        Texture2D texture = NewTexture(64, 64, new Color(0.36f, 0.25f, 0.15f, 1f));
        FillEllipse(texture, 21, 43, 18, 7, new Color(0.45f, 0.33f, 0.18f, 0.55f));
        FillEllipse(texture, 48, 20, 13, 8, new Color(0.22f, 0.15f, 0.1f, 0.35f));
        DrawLine(texture, 8, 14, 36, 21, new Color(0.16f, 0.1f, 0.07f, 0.35f), 1);
        return texture;
    }

    private static Texture2D CreateWaterTile()
    {
        Texture2D texture = NewTexture(64, 64, new Color(0.08f, 0.22f, 0.33f, 1f));
        DrawLine(texture, 0, 20, 64, 12, new Color(0.2f, 0.55f, 0.7f, 0.45f), 2);
        DrawLine(texture, 0, 43, 64, 36, new Color(0.35f, 0.75f, 0.86f, 0.35f), 2);
        return texture;
    }

    private static Texture2D CreateTree()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 10, 20, 5, new Color(0f, 0f, 0f, 0.22f));
        FillRect(texture, 28, 8, 36, 33, new Color(0.34f, 0.2f, 0.1f, 1f));
        FillEllipse(texture, 28, 39, 22, 18, new Color(0.08f, 0.29f, 0.12f, 1f));
        FillEllipse(texture, 41, 35, 18, 16, new Color(0.12f, 0.39f, 0.16f, 1f));
        FillEllipse(texture, 23, 30, 15, 14, new Color(0.15f, 0.45f, 0.18f, 1f));
        return texture;
    }

    private static Texture2D CreateLogNode()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 13, 24, 6, new Color(0f, 0f, 0f, 0.25f));
        DrawLine(texture, 12, 26, 51, 37, new Color(0.5f, 0.29f, 0.12f, 1f), 9);
        DrawLine(texture, 16, 29, 46, 37, new Color(0.78f, 0.52f, 0.24f, 0.45f), 2);
        FillEllipse(texture, 52, 38, 6, 7, new Color(0.75f, 0.48f, 0.22f, 1f));
        return texture;
    }

    private static Texture2D CreateStoneNode()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 12, 22, 5, new Color(0f, 0f, 0f, 0.22f));
        FillEllipse(texture, 28, 30, 21, 16, new Color(0.38f, 0.38f, 0.35f, 1f));
        FillEllipse(texture, 43, 28, 14, 13, new Color(0.25f, 0.25f, 0.25f, 1f));
        DrawLine(texture, 17, 34, 38, 39, new Color(0.64f, 0.61f, 0.54f, 0.55f), 2);
        return texture;
    }

    private static Texture2D CreateFoodBush()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 12, 20, 5, new Color(0f, 0f, 0f, 0.2f));
        FillEllipse(texture, 31, 28, 22, 15, new Color(0.12f, 0.39f, 0.13f, 1f));
        FillEllipse(texture, 20, 32, 12, 10, new Color(0.16f, 0.5f, 0.18f, 1f));
        FillEllipse(texture, 39, 31, 13, 11, new Color(0.1f, 0.31f, 0.12f, 1f));
        FillEllipse(texture, 26, 35, 3, 3, new Color(0.95f, 0.18f, 0.22f, 1f));
        FillEllipse(texture, 38, 26, 3, 3, new Color(0.95f, 0.18f, 0.22f, 1f));
        return texture;
    }

    private static Texture2D CreateHerbPlant()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 10, 14, 4, new Color(0f, 0f, 0f, 0.18f));
        DrawLine(texture, 32, 12, 32, 43, new Color(0.18f, 0.45f, 0.16f, 1f), 3);
        FillEllipse(texture, 24, 28, 10, 5, new Color(0.35f, 0.7f, 0.31f, 1f));
        FillEllipse(texture, 41, 34, 10, 5, new Color(0.29f, 0.64f, 0.26f, 1f));
        FillEllipse(texture, 32, 47, 5, 7, new Color(0.74f, 0.53f, 0.95f, 1f));
        return texture;
    }

    private static Texture2D CreateBuildFloor()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillRect(texture, 8, 16, 56, 48, new Color(0.45f, 0.29f, 0.14f, 1f));
        DrawLine(texture, 8, 25, 56, 24, new Color(0.7f, 0.48f, 0.25f, 0.45f), 1);
        DrawLine(texture, 8, 38, 56, 37, new Color(0.2f, 0.12f, 0.06f, 0.45f), 1);
        return texture;
    }

    private static Texture2D CreateBuildWall()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillRect(texture, 8, 24, 56, 42, new Color(0.35f, 0.2f, 0.1f, 1f));
        FillRect(texture, 8, 29, 56, 34, new Color(0.58f, 0.36f, 0.17f, 1f));
        DrawLine(texture, 13, 25, 52, 41, new Color(0.78f, 0.55f, 0.28f, 0.35f), 1);
        return texture;
    }

    private static Texture2D CreateBuildDoor()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillRect(texture, 21, 12, 43, 54, new Color(0.43f, 0.24f, 0.1f, 1f));
        FillRect(texture, 24, 16, 40, 51, new Color(0.55f, 0.34f, 0.16f, 1f));
        FillEllipse(texture, 38, 34, 3, 3, new Color(0.9f, 0.67f, 0.25f, 1f));
        return texture;
    }

    private static Texture2D CreateCampfire()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 12, 22, 5, new Color(0f, 0f, 0f, 0.25f));
        DrawLine(texture, 18, 18, 45, 30, new Color(0.36f, 0.19f, 0.08f, 1f), 5);
        DrawLine(texture, 45, 18, 18, 30, new Color(0.43f, 0.22f, 0.09f, 1f), 5);
        FillEllipse(texture, 31, 34, 14, 18, new Color(1f, 0.36f, 0.08f, 0.96f));
        FillEllipse(texture, 34, 38, 9, 14, new Color(1f, 0.84f, 0.18f, 0.94f));
        return texture;
    }

    private static Texture2D CreateBedroll()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 11, 21, 4, new Color(0f, 0f, 0f, 0.2f));
        FillRect(texture, 15, 17, 50, 42, new Color(0.2f, 0.38f, 0.42f, 1f));
        FillRect(texture, 15, 35, 50, 45, new Color(0.13f, 0.25f, 0.3f, 1f));
        FillRect(texture, 18, 20, 47, 24, new Color(0.58f, 0.78f, 0.8f, 0.5f));
        return texture;
    }

    private static Texture2D CreateStorageBox()
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 12, 22, 5, new Color(0f, 0f, 0f, 0.22f));
        FillRect(texture, 14, 20, 50, 42, new Color(0.34f, 0.18f, 0.08f, 1f));
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
        return texture;
    }

    private static Texture2D CreateWildAnimal(Color bodyColor, Color eyeColor)
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        FillEllipse(texture, 32, 12, 20, 5, new Color(0f, 0f, 0f, 0.25f));
        FillEllipse(texture, 34, 31, 19, 12, bodyColor);
        FillEllipse(texture, 19, 35, 10, 9, bodyColor * 0.9f);
        FillEllipse(texture, 18, 38, 2, 2, eyeColor);
        DrawLine(texture, 47, 33, 60, 41, bodyColor * 0.8f, 4);
        DrawLine(texture, 25, 20, 20, 10, bodyColor * 0.8f, 3);
        DrawLine(texture, 42, 20, 39, 10, bodyColor * 0.8f, 3);
        return texture;
    }

    private static Texture2D CreatePlayerSprite(Facing facing, int walkFrame, bool attacking)
    {
        Texture2D texture = NewTexture(64, 64, Color.clear);
        int bob = walkFrame == 1 ? 1 : walkFrame == 2 ? -1 : 0;
        FillEllipse(texture, 32, 10, 14, 5, new Color(0f, 0f, 0f, 0.25f));
        FillEllipse(texture, 32, 27 + bob, 11, 14, new Color(0.18f, 0.34f, 0.28f, 1f));
        FillRect(texture, 25, 18 + bob, 39, 31 + bob, new Color(0.16f, 0.22f, 0.18f, 1f));
        FillEllipse(texture, 32, 43 + bob, 12, 11, new Color(0.78f, 0.58f, 0.38f, 1f));
        FillEllipse(texture, 32, 46 + bob, 13, 7, new Color(0.32f, 0.22f, 0.14f, 1f));
        DrawLine(texture, 28, 18 + bob, 25, 9, new Color(0.1f, 0.12f, 0.15f, 1f), 4);
        DrawLine(texture, 36, 18 + bob, 39, 9, new Color(0.1f, 0.12f, 0.15f, 1f), 4);
        if (attacking)
        {
            DrawLine(texture, 21, 34, 52, 52, new Color(0.55f, 0.34f, 0.13f, 1f), 3);
            DrawLine(texture, 50, 53, 59, 49, new Color(0.74f, 0.74f, 0.66f, 1f), 3);
        }
        else
        {
            DrawLine(texture, 22, 30 + bob, 17, 22 - bob, new Color(0.78f, 0.58f, 0.38f, 1f), 4);
            DrawLine(texture, 42, 30 + bob, 47, 22 + bob, new Color(0.78f, 0.58f, 0.38f, 1f), 4);
        }

        if (facing == Facing.Side)
        {
            texture = FlipTextureHorizontally(texture);
        }

        return texture;
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

    private static Texture2D FlipTextureHorizontally(Texture2D source)
    {
        Texture2D flipped = NewTexture(source.width, source.height, Color.clear);
        for (int y = 0; y < source.height; y++)
        {
            for (int x = 0; x < source.width; x++)
            {
                flipped.SetPixel(source.width - 1 - x, y, source.GetPixel(x, y));
            }
        }

        UnityEngine.Object.DestroyImmediate(source);
        return flipped;
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
