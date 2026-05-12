using UnityEngine;

public class MinionDamage : MonoBehaviour
{
    public int damage = 1;
    private Animator parentAnim;

    void Start()
    {
        // Get the Animator from the parent (The Minion)
        parentAnim = GetComponentInParent<Animator>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // --- THE FIX ---
        // We check the Animator. If the "Attack" trigger isn't active 
        // or if we aren't in the Attack state, don't do damage.
        // This prevents "touch damage" while walking.
        if (parentAnim != null)
        {
            // Only deal damage if the Animator is currently playing the Attack state
            // Change "S_attack" to the exact name of your attack state in the Animator
            if (!parentAnim.GetCurrentAnimatorStateInfo(0).IsName("S_attack"))
            {
                return;
            }
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        PlayerKnockback playerKnockback = other.GetComponent<PlayerKnockback>();

        if (playerHealth != null)
        {
            bool didTakeDamage = playerHealth.TakeDamage(damage);

            if (didTakeDamage && playerKnockback != null)
            {
                playerKnockback.Knockback(transform.position);
            }
        }
    }
}