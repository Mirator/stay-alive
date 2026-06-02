using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerController2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4.2f;

    private Rigidbody2D body;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LastMoveDirection { get; private set; } = Vector2.down;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        ReadMovement();
    }

    private void FixedUpdate()
    {
        Vector2 nextPosition = body.position + MoveInput * moveSpeed * Time.fixedDeltaTime;
        body.MovePosition(nextPosition);
    }

    private void ReadMovement()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            MoveInput = Vector2.zero;
            return;
        }

        Vector2 input = Vector2.zero;

        if (keyboard.aKey.isPressed)
        {
            input.x -= 1f;
        }

        if (keyboard.dKey.isPressed)
        {
            input.x += 1f;
        }

        if (keyboard.sKey.isPressed)
        {
            input.y -= 1f;
        }

        if (keyboard.wKey.isPressed)
        {
            input.y += 1f;
        }

        MoveInput = input.sqrMagnitude > 1f ? input.normalized : input;

        if (MoveInput.sqrMagnitude > 0.001f)
        {
            LastMoveDirection = MoveInput.normalized;
        }
    }
}
