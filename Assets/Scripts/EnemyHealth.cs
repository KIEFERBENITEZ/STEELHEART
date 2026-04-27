using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    public int currentHealth;

    [HideInInspector] public bool isDead = false;
    private Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(int damage, Vector2 knockback)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Apply Knockback
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(knockback, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Still alive: play Hurt flinch
            if (anim != null && HasParameter("Hurt", anim))
                anim.SetTrigger("Hurt");
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1. STOP MOVEMENT (Safe for everyone)
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // 2. DISABLE COLLISION (Safe for everyone)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 3. SMART ANIMATION (This is the "Safe" part)
        if (anim != null)
        {
            // We check which trigger exists so we don't cause errors
            if (HasParameter("Death", anim))
            {
                anim.SetTrigger("Death"); // Boss usually has "Death"
            }
            else if (HasParameter("Die", anim))
            {
                anim.SetTrigger("Die");   // Demonkin usually has "Die"
            }
            else if (HasParameter("Hurt", anim))
            {
                anim.SetTrigger("Hurt");  // Samurai fallback
            }
        }

        // 4. SMART DESTROY TIMER
        // If it's a boss (tagged Boss), give it more time. 
        // If it's a small enemy, destroy it faster.
        float destroyDelay = gameObject.CompareTag("Boss") ? 3.0f : 1.5f;
        Destroy(gameObject, destroyDelay);
    }

    // This helper checks if the "Hurt" or "Die" trigger actually exists 
    // in your Animator controller to prevent errors.
    private bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }
}