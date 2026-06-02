using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class RewardRoomGoal : MonoBehaviour
{
    [SerializeField] private GameLoopController gameLoop;

    public GameLoopController GameLoop => gameLoop;

    private void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    public void Configure(GameLoopController controller)
    {
        gameLoop = controller;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerInteraction>() == null)
        {
            return;
        }

        gameLoop?.CompleteRun();
    }
}
