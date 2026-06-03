using UnityEngine;

public sealed class MeleeWeapon : MonoBehaviour
{
    [SerializeField] private float bareHandDamage = 5f;
    [SerializeField] private float spearDamage = 25f;
    [SerializeField] private float bareHandRange = 0.75f;
    [SerializeField] private float spearRange = 1.35f;

    private CraftedInventory inventory;
    private PlayerController2D controller;

    public bool HasStoneSpear => Inventory != null && Inventory.Has(CraftedItemType.StoneSpear, 1);
    public float CurrentDamage => HasStoneSpear ? spearDamage : bareHandDamage;
    public float CurrentRange => HasStoneSpear ? spearRange : bareHandRange;

    private CraftedInventory Inventory
    {
        get
        {
            if (inventory == null)
            {
                inventory = GetComponent<CraftedInventory>();
            }

            return inventory;
        }
    }

    private void Awake()
    {
        inventory = GetComponent<CraftedInventory>();
        controller = GetComponent<PlayerController2D>();
    }

    public bool TryAttack(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = controller != null ? controller.LastMoveDirection : Vector2.down;
        }

        Vector2 center = (Vector2)transform.position + direction.normalized * CurrentRange;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, CurrentRange * 0.7f);
        for (int i = 0; i < hits.Length; i++)
        {
            WildAnimalEnemy animal = hits[i].GetComponentInParent<WildAnimalEnemy>();
            if (animal != null && TryDamage(animal))
            {
                UIController.Instance?.ShowMessage("Hit " + animal.AnimalType + ".", 1f);
                return true;
            }
        }

        UIController.Instance?.ShowMessage("Swing.", 0.7f);
        return false;
    }

    public bool TryDamage(WildAnimalEnemy animal)
    {
        if (animal == null || animal.IsDefeated)
        {
            return false;
        }

        animal.ReceiveDamage(CurrentDamage);
        return true;
    }
}
