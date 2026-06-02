using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerKnockback : MonoBehaviour
{
    [SerializeField] private float cooldown = 0.75f;

    private Rigidbody2D body;
    private float nextAllowedTime = float.NegativeInfinity;

    public int KnockbackCount { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public bool ApplyKnockback(Vector2 sourcePosition, float force)
    {
        float now = Time.time;
        if (now < nextAllowedTime)
        {
            return false;
        }

        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        Vector2 away = ((Vector2)transform.position - sourcePosition).normalized;
        if (away.sqrMagnitude < 0.001f)
        {
            away = Vector2.up;
        }

        if (body != null)
        {
            body.AddForce(away * Mathf.Max(0f, force), ForceMode2D.Impulse);
        }

        nextAllowedTime = now + Mathf.Max(0f, cooldown);
        KnockbackCount++;
        UIController.Instance?.ShowMessage("The cave crawler knocks you back.", 1.4f);
        return true;
    }
}
