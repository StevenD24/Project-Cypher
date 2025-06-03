using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropItem
{
    public GameObject itemPrefab;
    public float dropChance = 1.0f; // 1.0 = 100% chance
    public int minQuantity = 1;
    public int maxQuantity = 1;
    public string itemName = "";
}

public class BossDropSystem : MonoBehaviour
{
    [Header("Drop Configuration")]
    public List<DropItem> guaranteedDrops = new List<DropItem>();
    public List<DropItem> randomDrops = new List<DropItem>();

    [Header("Drop Physics")]
    public float dropRadius = 2f; // How spread out the drops should be
    public float dropHeight = 2f; // Height to drop items from
    public LayerMask groundLayer = 1; // What counts as ground for landing

    [Header("Drop Timing")]
    public float dropDelay = 0.5f; // Delay before dropping items
    public float dropInterval = 0.1f; // Time between each item drop

    [Header("Ground Detection")]
    public float groundCheckDistance = 10f; // How far down to check for ground

    public void TriggerItemDrops()
    {
        StartCoroutine(DropItemsCoroutine());
    }

    System.Collections.IEnumerator DropItemsCoroutine()
    {
        // Wait for drop delay
        yield return new WaitForSeconds(dropDelay);

        List<GameObject> itemsToDrop = new List<GameObject>();

        // Add guaranteed drops
        foreach (DropItem drop in guaranteedDrops)
        {
            if (drop.itemPrefab != null)
            {
                int quantity = Random.Range(drop.minQuantity, drop.maxQuantity + 1);
                for (int i = 0; i < quantity; i++)
                {
                    itemsToDrop.Add(drop.itemPrefab);
                }
            }
        }

        // Add random drops based on chance
        foreach (DropItem drop in randomDrops)
        {
            if (drop.itemPrefab != null && Random.Range(0f, 1f) <= drop.dropChance)
            {
                int quantity = Random.Range(drop.minQuantity, drop.maxQuantity + 1);
                for (int i = 0; i < quantity; i++)
                {
                    itemsToDrop.Add(drop.itemPrefab);
                }
            }
        }

        // Drop each item
        for (int i = 0; i < itemsToDrop.Count; i++)
        {
            DropSingleItem(itemsToDrop[i], i, itemsToDrop.Count);
            yield return new WaitForSeconds(dropInterval);
        }
    }

    void DropSingleItem(GameObject itemPrefab, int index, int totalItems)
    {
        // Calculate random position around the boss
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(0.5f, dropRadius);

        Vector3 dropPosition =
            transform.position + new Vector3(randomDirection.x * randomDistance, dropHeight, 0);

        // Find ground position
        Vector3 targetPosition = FindGroundPosition(dropPosition);

        // Create the drop
        GameObject droppedItem = ItemDrop.CreateDrop(itemPrefab, dropPosition, targetPosition);

        // Add slight variation to landing time based on index
        if (droppedItem != null)
        {
            ItemDrop itemDrop = droppedItem.GetComponent<ItemDrop>();
            if (itemDrop != null)
            {
                // Slightly stagger the drop duration for visual variety
                itemDrop.dropDuration += Random.Range(-0.2f, 0.2f);
                itemDrop.dropDuration = Mathf.Max(0.5f, itemDrop.dropDuration); // Minimum duration
            }
        }

        Debug.Log($"Dropped item {index + 1}/{totalItems}: {itemPrefab.name} at {targetPosition}");
    }

    Vector3 FindGroundPosition(Vector3 startPosition)
    {
        // Raycast down to find ground
        RaycastHit2D hit = Physics2D.Raycast(
            startPosition,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        if (hit.collider != null)
        {
            // Found ground, position item well above it to prevent any clipping
            return new Vector3(startPosition.x, hit.point.y - 1.0f, startPosition.z);
        }
        else
        {
            // No ground found, use boss position as fallback
            Debug.LogWarning(
                $"No ground found below {startPosition}, using boss position as fallback"
            );
            return new Vector3(startPosition.x, transform.position.y + 0.5f, startPosition.z);
        }
    }

    // Method to add drops at runtime
    public void AddGuaranteedDrop(GameObject itemPrefab, int quantity = 1)
    {
        DropItem newDrop = new DropItem
        {
            itemPrefab = itemPrefab,
            dropChance = 1.0f,
            minQuantity = quantity,
            maxQuantity = quantity,
            itemName = itemPrefab.name,
        };
        guaranteedDrops.Add(newDrop);
    }

    public void AddRandomDrop(GameObject itemPrefab, float chance, int minQty = 1, int maxQty = 1)
    {
        DropItem newDrop = new DropItem
        {
            itemPrefab = itemPrefab,
            dropChance = chance,
            minQuantity = minQty,
            maxQuantity = maxQty,
            itemName = itemPrefab.name,
        };
        randomDrops.Add(newDrop);
    }

    // Visual debug in Scene view
    void OnDrawGizmosSelected()
    {
        // Draw drop radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dropRadius);

        // Draw drop height
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position + Vector3.up * dropHeight,
            new Vector3(dropRadius * 2, 0.1f, 0.1f)
        );

        // Draw ground check range
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.down * groundCheckDistance
        );
    }
}
