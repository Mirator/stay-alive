using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class MineableResource : MonoBehaviour
{
    [SerializeField] private string displayName = "Mineable";
    [SerializeField] private int hitsToBreak = 2;
    [SerializeField] private ResourceType resourceType = ResourceType.Stone;
    [SerializeField] private int resourceAmount = 1;
    [SerializeField] private Color flashColor = Color.white;

    private int remainingHits;
    private SpriteRenderer spriteRenderer;
    private Color baseColor;
    private Vector3 baseLocalPosition;

    public string DisplayName => displayName;
    public int RemainingHits => remainingHits;
    public int HitsToBreak => hitsToBreak;
    public ResourceType ResourceType => resourceType;
    public int ResourceAmount => resourceAmount;
    public bool IsBroken { get; private set; }

    private void Awake()
    {
        EnsureInitialized();
    }

    public void Configure(string newDisplayName, int newHitsToBreak, ResourceType newResourceType, int newResourceAmount)
    {
        displayName = newDisplayName;
        hitsToBreak = Mathf.Max(1, newHitsToBreak);
        resourceType = newResourceType;
        resourceAmount = Mathf.Max(1, newResourceAmount);
        remainingHits = hitsToBreak;
        IsBroken = false;
        EnsureInitialized();
    }

    public void Hit(ResourceInventory inventory, Vector2 sourcePosition)
    {
        EnsureInitialized();

        if (IsBroken)
        {
            return;
        }

        remainingHits--;

        if (remainingHits <= 0)
        {
            IsBroken = true;
            if (inventory != null)
            {
                inventory.Add(resourceType, resourceAmount);
            }

            UIController.Instance?.ShowMessage("Collected " + ResourceLabel(resourceType) + ".", 1.7f);
            gameObject.SetActive(false);
            return;
        }

        UIController.Instance?.ShowMessage(displayName + " hit (" + remainingHits + " left).", 1.2f);

        if (Application.isPlaying)
        {
            StopAllCoroutines();
            StartCoroutine(HitFeedback(sourcePosition));
        }
    }

    private void EnsureInitialized()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseColor = spriteRenderer.color;
            baseLocalPosition = transform.localPosition;
        }

        if (remainingHits <= 0)
        {
            remainingHits = Mathf.Max(1, hitsToBreak);
        }
    }

    private IEnumerator HitFeedback(Vector2 sourcePosition)
    {
        Vector3 start = baseLocalPosition;
        Vector2 away = ((Vector2)transform.position - sourcePosition).normalized;
        if (away.sqrMagnitude < 0.001f)
        {
            away = Vector2.up;
        }

        float duration = 0.14f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float pulse = Mathf.Sin(elapsed * 80f) * 0.035f;
            transform.localPosition = start + (Vector3)(away * pulse);
            spriteRenderer.color = Color.Lerp(flashColor, baseColor, elapsed / duration);
            yield return null;
        }

        transform.localPosition = start;
        spriteRenderer.color = baseColor;
    }

    private static string ResourceLabel(ResourceType type)
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
            default:
                return "Resource";
        }
    }
}
