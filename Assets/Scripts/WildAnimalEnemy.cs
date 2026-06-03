using UnityEngine;

public enum WildAnimalType
{
    Wolf,
    Boar
}

public enum WildAnimalState
{
    Idle,
    Chasing,
    Charging,
    Attacking,
    Returning,
    Defeated
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public sealed class WildAnimalEnemy : MonoBehaviour
{
    [SerializeField] private string animalId;
    [SerializeField] private WildAnimalType animalType = WildAnimalType.Wolf;
    [SerializeField] private Transform target;
    [SerializeField] private DayNightCycle dayNightCycle;
    [SerializeField] private float maxHealth = 50f;
    [SerializeField] private float health = 50f;
    [SerializeField] private float detectRadius = 4f;
    [SerializeField] private float nightDetectBonus = 2f;
    [SerializeField] private float attackRange = 0.75f;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float attackDamage = 16f;
    [SerializeField] private float attackCooldown = 1.2f;

    private Rigidbody2D body;
    private Vector2 homePosition;
    private float nextAttackTime;

    public string AnimalId => string.IsNullOrEmpty(animalId) ? name : animalId;
    public WildAnimalType AnimalType => animalType;
    public WildAnimalState CurrentState { get; private set; }
    public bool IsDefeated => CurrentState == WildAnimalState.Defeated;
    public float Health => health;
    public float EffectiveDetectRadius => animalType == WildAnimalType.Wolf && dayNightCycle != null && dayNightCycle.IsNight
        ? detectRadius + nightDetectBonus
        : detectRadius;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.bodyType = RigidbodyType2D.Kinematic;
        homePosition = transform.position;
        health = health <= 0f ? maxHealth : Mathf.Min(health, maxHealth);
    }

    private void FixedUpdate()
    {
        if (IsDefeated)
        {
            return;
        }

        CurrentState = EvaluateState();
        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        Vector2 velocity = Vector2.zero;
        if ((CurrentState == WildAnimalState.Chasing || CurrentState == WildAnimalState.Charging) && target != null)
        {
            float speed = CurrentState == WildAnimalState.Charging ? moveSpeed * 1.45f : moveSpeed;
            velocity = ((Vector2)target.position - body.position).normalized * speed;
        }
        else if (CurrentState == WildAnimalState.Returning)
        {
            velocity = (homePosition - body.position).normalized * (moveSpeed * 0.65f);
        }

        body.MovePosition(body.position + velocity * Time.fixedDeltaTime);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerVitals vitals = other.GetComponentInParent<PlayerVitals>();
        TryAttack(vitals);
    }

    public void Configure(string id, WildAnimalType type, Transform newTarget, DayNightCycle cycle, Vector2 home)
    {
        animalId = id;
        animalType = type;
        target = newTarget;
        dayNightCycle = cycle;
        homePosition = home;
        CurrentState = WildAnimalState.Idle;
    }

    public WildAnimalState EvaluateState()
    {
        if (IsDefeated || health <= 0f)
        {
            return WildAnimalState.Defeated;
        }

        if (target == null)
        {
            return Vector2.Distance(transform.position, homePosition) > 0.35f ? WildAnimalState.Returning : WildAnimalState.Idle;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        if (distanceToPlayer <= attackRange)
        {
            return WildAnimalState.Attacking;
        }

        if (distanceToPlayer <= EffectiveDetectRadius)
        {
            return animalType == WildAnimalType.Boar ? WildAnimalState.Charging : WildAnimalState.Chasing;
        }

        return Vector2.Distance(transform.position, homePosition) > 0.35f ? WildAnimalState.Returning : WildAnimalState.Idle;
    }

    public bool TryAttack(PlayerVitals vitals)
    {
        if (vitals == null || vitals.IsDead || IsDefeated || Time.time < nextAttackTime)
        {
            return false;
        }

        float distance = Vector2.Distance(transform.position, vitals.transform.position);
        if (distance > attackRange + 0.15f)
        {
            return false;
        }

        vitals.ApplyDamage(attackDamage);
        nextAttackTime = Time.time + attackCooldown;
        CurrentState = WildAnimalState.Attacking;
        return true;
    }

    public void ReceiveDamage(float amount)
    {
        if (IsDefeated)
        {
            return;
        }

        health = Mathf.Max(0f, health - Mathf.Max(0f, amount));
        if (health <= 0f)
        {
            SetDefeated(true);
        }
    }

    public void SetDefeated(bool defeated)
    {
        CurrentState = defeated ? WildAnimalState.Defeated : WildAnimalState.Idle;
        if (defeated)
        {
            health = 0f;
        }
        else if (health <= 0f)
        {
            health = maxHealth;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = !defeated;
        }

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = defeated ? new Color(0.35f, 0.35f, 0.35f, 0.6f) : Color.white;
        }
    }
}
