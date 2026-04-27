using UnityEngine;

public class Hadouken : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifetime = 5f;

    void Start()
    {
        // Cleanup after 5 seconds
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move in the direction the fireball is facing
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we hit the player
        if (collision.CompareTag("Player"))
        {
            // Get the PlayerHealth component from the object we hit
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // We use the version of TakeDamage that takes (Damage, Position)
                // transform.position tells the player where the fireball hit from
                playerHealth.TakeDamage(damage, transform.position);
            }

            // Destroy the fireball on impact
            Destroy(gameObject);
        }

        // Destroy if it hits a wall/ground
        if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}