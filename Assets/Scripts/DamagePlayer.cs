using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    public int damageAmount = 1; // Increased to 10 so you can see the health bar move
    public float slowAmount = 0.5f;
    public float slowDuration = 2f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. DEAL DAMAGE
            // We find the PlayerHealth script on whatever we touched
            PlayerHealth health = other.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damageAmount);
                Debug.Log("Wizard dealt " + damageAmount + " damage to Player!");
            }
            else
            {
                Debug.LogError("Hit Player, but could not find a 'PlayerHealth' script on them!");
            }

            // 2. APPLY SLOW (Optional)
            other.SendMessage("ApplySlow", new float[] { slowAmount, slowDuration }, SendMessageOptions.DontRequireReceiver);
        }
    }
}