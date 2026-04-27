using UnityEngine;

public class EarthSpike : MonoBehaviour
{
    public int damage = 1;
    private BoxCollider2D col;

    void Start()
    {
        col = GetComponent<BoxCollider2D>();

        // Start with collider OFF so the player can see the 
        // "warning" frames without taking damage yet.
        if (col != null) col.enabled = false;

        // Adjust this time to match the total length of your animation
        Destroy(gameObject, 1.5f);
    }

    // CALLED BY ANIMATION EVENT
    public void EnableDamage()
    {
        if (col != null) col.enabled = true;
    }

    // CALLED BY ANIMATION EVENT
    public void DisableDamage()
    {
        if (col != null) col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Apply upward knockback to make the spike feel "powerful"
            collision.GetComponent<PlayerHealth>()?.TakeDamage(damage, Vector2.up * 5f);
        }
    }
}