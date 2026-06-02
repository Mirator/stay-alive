using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum CaveCrawlerState
{
    Idle,
    Chasing,
    Retreating
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public sealed class CaveCrawlerEnemy : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float detectRadius = 4.5f;
    [SerializeField] private float moveSpeed = 1.15f;
    [SerializeField] private float retreatSpeed = 1.45f;
    [SerializeField] private float lightFearRadius = 2.45f;
    [SerializeField] private float minimumLightIntensity = 0.8f;
    [SerializeField] private float contactKnockbackForce = 3.6f;

    private Rigidbody2D body;
    private Vector2 homePosition;

    public CaveCrawlerState CurrentState { get; private set; }
    public Transform Target => target;
    public Vector2 HomePosition => homePosition;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.bodyType = RigidbodyType2D.Kinematic;

        Collider2D collider = GetComponent<Collider2D>();
        collider.isTrigger = true;

        if (homePosition == Vector2.zero)
        {
            homePosition = transform.position;
        }
    }

    private void FixedUpdate()
    {
        EnsureBody();

        EnemyIntent intent = EvaluateIntent();
        CurrentState = intent.State;

        Vector2 velocity = Vector2.zero;
        if (intent.State == CaveCrawlerState.Chasing && target != null)
        {
            velocity = ((Vector2)target.position - body.position).normalized * moveSpeed;
        }
        else if (intent.State == CaveCrawlerState.Retreating)
        {
            Vector2 away = (body.position - intent.LightPosition).normalized;
            if (away.sqrMagnitude < 0.001f)
            {
                away = (body.position - homePosition).normalized;
            }

            if (away.sqrMagnitude < 0.001f)
            {
                away = Vector2.down;
            }

            velocity = away * retreatSpeed;
        }
        else if (Vector2.Distance(body.position, homePosition) > 0.35f)
        {
            velocity = (homePosition - body.position).normalized * (moveSpeed * 0.45f);
        }

        body.MovePosition(body.position + velocity * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryApplyContact(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryApplyContact(other);
    }

    public void Configure(Transform newTarget, Vector2 newHomePosition)
    {
        target = newTarget;
        homePosition = newHomePosition;
    }

    public CaveCrawlerState EvaluateState()
    {
        return EvaluateIntent().State;
    }

    public bool ApplyContactConsequence(PlayerKnockback player)
    {
        if (player == null)
        {
            return false;
        }

        return player.ApplyKnockback(transform.position, contactKnockbackForce);
    }

    private void TryApplyContact(Collider2D other)
    {
        PlayerKnockback player = other != null ? other.GetComponentInParent<PlayerKnockback>() : null;
        ApplyContactConsequence(player);
    }

    private EnemyIntent EvaluateIntent()
    {
        if (TryFindNearestStrongWarmLight(out Vector2 lightPosition))
        {
            return new EnemyIntent(CaveCrawlerState.Retreating, lightPosition);
        }

        if (target != null && Vector2.Distance(transform.position, target.position) <= detectRadius)
        {
            return new EnemyIntent(CaveCrawlerState.Chasing, Vector2.zero);
        }

        return new EnemyIntent(CaveCrawlerState.Idle, Vector2.zero);
    }

    private bool TryFindNearestStrongWarmLight(out Vector2 lightPosition)
    {
        Light2D[] lights = FindObjectsByType<Light2D>(FindObjectsSortMode.None);
        float bestDistance = float.MaxValue;
        lightPosition = Vector2.zero;

        for (int i = 0; i < lights.Length; i++)
        {
            Light2D light = lights[i];
            if (light == null || !light.isActiveAndEnabled || light.lightType != Light2D.LightType.Point)
            {
                continue;
            }

            if (light.intensity < minimumLightIntensity || !IsWarm(light.color))
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, light.transform.position);
            float effectiveRadius = Mathf.Min(lightFearRadius, light.pointLightOuterRadius);
            if (distance <= effectiveRadius && distance < bestDistance)
            {
                bestDistance = distance;
                lightPosition = light.transform.position;
            }
        }

        return bestDistance < float.MaxValue;
    }

    private static bool IsWarm(Color color)
    {
        return color.r >= color.b + 0.15f && color.r >= color.g * 0.9f;
    }

    private void EnsureBody()
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }
    }

    private readonly struct EnemyIntent
    {
        public EnemyIntent(CaveCrawlerState state, Vector2 lightPosition)
        {
            State = state;
            LightPosition = lightPosition;
        }

        public CaveCrawlerState State { get; }
        public Vector2 LightPosition { get; }
    }
}
