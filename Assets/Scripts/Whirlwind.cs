using UnityEngine;

public class Whirlwind : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 5f;
    public float stunDuration = 2.5f;
    public float lifetime = 4f;
    public int damage = 1;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 1. Find the player to decide which way to move
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float direction = (player.transform.position.x > transform.position.x) ? 1 : -1;

            // Set velocity - using linearVelocity for Unity 6 compatibility
            rb.linearVelocity = new Vector2(direction * speed, 0);

            // Flip the sprite based on direction
            if (direction < 0)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        // 2. Self-destruct after a few seconds
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 3. Check for Player
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            PlayerHealth ph = other.GetComponent<PlayerHealth>();

            // Apply effects
            if (pc != null) pc.ApplyStun(stunDuration);
            if (ph != null) ph.TakeDamage(damage, transform.position);

            // Destroy the whirlwind after hitting the player
            Destroy(gameObject);
        }
        // 4. Destroy if it hits a wall (make sure your walls are tagged "Ground")
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}