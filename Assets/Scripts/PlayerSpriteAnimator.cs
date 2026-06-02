using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class PlayerSpriteAnimator : MonoBehaviour
{
    [Header("Idle")]
    [SerializeField] private Sprite idleDown;
    [SerializeField] private Sprite idleUp;
    [SerializeField] private Sprite idleSide;

    [Header("Walk")]
    [SerializeField] private Sprite[] walkDown;
    [SerializeField] private Sprite[] walkUp;
    [SerializeField] private Sprite[] walkSide;

    [Header("Tool")]
    [SerializeField] private Sprite mineDown;
    [SerializeField] private Sprite mineUp;
    [SerializeField] private Sprite mineSide;

    [SerializeField] private float frameDuration = 0.14f;
    [SerializeField] private float mineFrameDuration = 0.22f;

    private SpriteRenderer spriteRenderer;
    private PlayerController2D controller;
    private Vector2 facing = Vector2.down;
    private int frameIndex;
    private float nextFrameTime;
    private float mineUntil;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        controller = GetComponent<PlayerController2D>();

        if (idleDown != null)
        {
            spriteRenderer.sprite = idleDown;
        }
    }

    private void Update()
    {
        if (Time.time < mineUntil)
        {
            return;
        }

        Vector2 movement = controller != null ? controller.MoveInput : Vector2.zero;
        if (movement.sqrMagnitude > 0.001f)
        {
            facing = movement.normalized;
            AnimateWalk(facing);
            return;
        }

        ShowIdle(facing);
    }

    public void PlayMine(Vector2 direction)
    {
        if (direction.sqrMagnitude > 0.001f)
        {
            facing = direction.normalized;
        }

        Sprite mineSprite = ChooseDirectionalSprite(facing, mineDown, mineUp, mineSide);
        if (mineSprite != null)
        {
            spriteRenderer.sprite = mineSprite;
        }

        mineUntil = Time.time + mineFrameDuration;
    }

    private void AnimateWalk(Vector2 direction)
    {
        Sprite[] frames = ChooseWalkFrames(direction);
        if (frames == null || frames.Length == 0)
        {
            ShowIdle(direction);
            return;
        }

        if (Time.time >= nextFrameTime)
        {
            frameIndex = (frameIndex + 1) % frames.Length;
            nextFrameTime = Time.time + frameDuration;
        }

        Sprite frame = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
        if (frame != null)
        {
            spriteRenderer.sprite = frame;
        }
    }

    private void ShowIdle(Vector2 direction)
    {
        frameIndex = 0;
        Sprite idle = ChooseDirectionalSprite(direction, idleDown, idleUp, idleSide);
        if (idle != null)
        {
            spriteRenderer.sprite = idle;
        }
    }

    private Sprite[] ChooseWalkFrames(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            spriteRenderer.flipX = direction.x < 0f;
            return walkSide;
        }

        spriteRenderer.flipX = false;
        return direction.y > 0f ? walkUp : walkDown;
    }

    private Sprite ChooseDirectionalSprite(Vector2 direction, Sprite down, Sprite up, Sprite side)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            spriteRenderer.flipX = direction.x < 0f;
            return side != null ? side : down;
        }

        spriteRenderer.flipX = false;
        return direction.y > 0f ? (up != null ? up : down) : down;
    }
}
