using UnityEngine;

public class W3Projectile : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 1;
    private Rigidbody2D rb;
    private Animator anim;
    private bool hasExploded = false;
    private bool isInitialized = false;

    // The Wizard will call this immediately after spawning
    public void Setup(float direction)
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Set velocity based on direction passed from Wizard
        rb.linearVelocity = new Vector2(direction * speed, 0);

        // Flip the sprite
        transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        isInitialized = true;
        Destroy(gameObject, 4f); // lifetime
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded || !isInitialized) return;

        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(damage);
            ExplodeProjectile();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            ExplodeProjectile();
        }
    }

    void ExplodeProjectile()
    {
        hasExploded = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        if (anim != null) anim.SetTrigger("Explode");
        Destroy(gameObject, 0.5f);
    }
}