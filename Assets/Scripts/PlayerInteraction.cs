using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ResourceInventory))]
[RequireComponent(typeof(CraftedInventory))]
public sealed class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float miningRange = 1.9f;
    [SerializeField] private float interactRange = 1.35f;
    [SerializeField] private GameObject torchPrefab;

    private PlayerController2D controller;
    private PlayerSpriteAnimator spriteAnimator;
    private Camera mainCamera;
    private bool paused;

    public ResourceInventory Inventory { get; private set; }
    public CraftedInventory CraftedInventory { get; private set; }

    private void Awake()
    {
        EnsureCachedComponents();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        UIController.Instance?.BindInventory(Inventory);
        UIController.Instance?.BindCraftedInventory(CraftedInventory);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        if (paused)
        {
            return;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryMine();
        }

        if (keyboard.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }

        if (keyboard.qKey.wasPressedThisFrame)
        {
            TryPlaceTorch();
        }

        UpdateInteractionPrompt();
    }

    public void AssignTorchPrefab(GameObject prefab)
    {
        torchPrefab = prefab;
    }

    private void TryMine()
    {
        Vector2 aimDirection = GetAimDirection();
        spriteAnimator?.PlayMine(aimDirection);

        MineableResource target = FindMineableTarget();
        if (target == null)
        {
            UIController.Instance?.ShowMessage("Nothing mineable in reach.", 1.3f);
            return;
        }

        target.Hit(Inventory, transform.position);
    }

    private MineableResource FindMineableTarget()
    {
        Vector2 playerPosition = transform.position;
        Vector2 mouseWorld = GetMouseWorld();
        Vector2 aimDirection = GetAimDirection();
        Collider2D[] hits = Physics2D.OverlapCircleAll(playerPosition, miningRange);

        MineableResource best = null;
        float bestScore = float.MaxValue;
        for (int i = 0; i < hits.Length; i++)
        {
            MineableResource mineable = hits[i].GetComponentInParent<MineableResource>();
            if (mineable == null || mineable.IsBroken)
            {
                continue;
            }

            float playerDistance = Vector2.Distance(playerPosition, mineable.transform.position);
            Vector2 toTarget = ((Vector2)mineable.transform.position - playerPosition).normalized;
            float aimScore = aimDirection.sqrMagnitude > 0.001f ? 1f - Mathf.Clamp01(Vector2.Dot(aimDirection, toTarget)) : 0.5f;
            float mouseDistance = Vector2.Distance(mouseWorld, mineable.transform.position);
            float cursorScore = Mathf.Min(mouseDistance, 1.8f);
            float score = playerDistance + aimScore * 0.8f + cursorScore * 0.35f;
            if (score < bestScore)
            {
                bestScore = score;
                best = mineable;
            }
        }

        return best;
    }

    private void TryInteract()
    {
        IInteractable interactable = FindNearestInteractable();
        if (interactable == null)
        {
            UIController.Instance?.ShowMessage("Nothing to interact with nearby.", 1.5f);
            return;
        }

        interactable.Interact(this);
    }

    private IInteractable FindNearestInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);
        IInteractable best = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            MonoBehaviour[] behaviours = hits[i].GetComponentsInParent<MonoBehaviour>();
            for (int j = 0; j < behaviours.Length; j++)
            {
                IInteractable interactable = behaviours[j] as IInteractable;
                if (interactable == null || ReferenceEquals(behaviours[j].gameObject, gameObject))
                {
                    continue;
                }

                float distance = Vector2.Distance(transform.position, behaviours[j].transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = interactable;
                }
            }
        }

        return best;
    }

    public bool TryPlaceTorch()
    {
        EnsureCachedComponents();

        if (torchPrefab == null)
        {
            UIController.Instance?.ShowMessage("No torch prefab assigned.", 1.5f);
            return false;
        }

        if (CraftedInventory == null || !CraftedInventory.Spend(CraftedItemType.Torch, 1))
        {
            UIController.Instance?.ShowMessage("Craft a Torch at the Workbench first.", 1.8f);
            return false;
        }

        Vector2 direction = controller != null ? controller.LastMoveDirection : Vector2.down;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector2.down;
        }

        Vector3 position = transform.position + (Vector3)(direction.normalized * 0.7f);
        GameObject torch = Instantiate(torchPrefab, position, Quaternion.identity);
        torch.name = "Placed Torch";
        UIController.Instance?.ShowMessage("Torch placed.", 1.2f);
        return true;
    }

    private void UpdateInteractionPrompt()
    {
        IInteractable interactable = FindNearestInteractable();
        if (interactable != null)
        {
            UIController.Instance?.SetPrompt(interactable.Prompt, true);
            return;
        }

        UIController.Instance?.SetPrompt(string.Empty, false);
    }

    private void TogglePause()
    {
        paused = !paused;
        Time.timeScale = paused ? 0f : 1f;
        UIController.Instance?.SetPrompt(string.Empty, false);
        UIController.Instance?.ShowMessage(paused ? "Paused" : "Back to the cave.", 1.2f);
    }

    private Vector2 GetAimDirection()
    {
        Vector2 direction = GetMouseWorld() - (Vector2)transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            return direction.normalized;
        }

        return controller != null ? controller.LastMoveDirection : Vector2.down;
    }

    private void EnsureCachedComponents()
    {
        if (Inventory == null)
        {
            Inventory = GetComponent<ResourceInventory>();
        }

        if (CraftedInventory == null)
        {
            CraftedInventory = GetComponent<CraftedInventory>();
        }

        if (controller == null)
        {
            controller = GetComponent<PlayerController2D>();
        }

        if (spriteAnimator == null)
        {
            spriteAnimator = GetComponent<PlayerSpriteAnimator>();
        }
    }

    private Vector2 GetMouseWorld()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null || Mouse.current == null)
        {
            return transform.position;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 world = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));
        return world;
    }
}
