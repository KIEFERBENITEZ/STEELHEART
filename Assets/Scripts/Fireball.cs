using UnityEngine;

public class Fireball : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 12f;
    public int damage = 2;
    public float lifetime = 3f;
    public float knockbackForce = 5f;

    [Header("Effects")]
    public GameObject explosionEffect;

    private float moveDirection = 1f; // 1 = Right, -1 = Left

    void Start()
    {
        // Ensure no gravity interference
        if (GetComponent<Rigidbody2D>()) GetComponent<Rigidbody2D>().gravityScale = 0;
        Destroy(gameObject, lifetime);
    }

    // This is called by the Player script immediately after Instantiate
    public void SetDirection(float dir)
    {
        moveDirection = dir;

        // Flip the fireball sprite art to face the travel direction
        transform.localScale = new Vector3(dir * Mathf.Abs(transform.localScale.x),
                                           transform.localScale.y,
                                           transform.localScale.z);
    }

    void Update()
    {
        // Move the fireball in the direction assigned
        transform.position += new Vector3(moveDirection * speed * Time.deltaTime, 0, 0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();

        if (enemy != null)
        {
            // Knockback matches the flight direction
            Vector2 knockback = new Vector2(moveDirection * knockbackForce, 0);
            enemy.TakeDamage(damage, knockback);
            Explode();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}