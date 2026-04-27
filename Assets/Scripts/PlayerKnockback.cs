using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    public float knockbackForce = 8f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Knockback(Vector2 enemyPosition)
    {
        Vector2 direction = (transform.position - (Vector3)enemyPosition).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
    }
}