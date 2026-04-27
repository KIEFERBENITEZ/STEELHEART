using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

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