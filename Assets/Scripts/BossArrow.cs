using UnityEngine;

public class BossArrow : MonoBehaviour
{
    public float speed = 15f;
    public float stunTime = 3f;
    private bool hasHit = false;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearVelocity = transform.right * speed;

        // If it misses the player, delete after 4 seconds
        Destroy(gameObject, 4f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Simplified: Only care about Player and Ground. Ignore everything else.
        if (hasHit || other.CompareTag("Enemy")) return;

        if (other.CompareTag("Player"))
        {
            hasHit = true;
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null) pc.ApplyStun(stunTime);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}