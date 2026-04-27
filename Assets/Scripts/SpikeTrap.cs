using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public int damageAmount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the thing hitting the spike is the Player
        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();

            if (health != null)
            {
                // We pass 'transform.position' so the player knows which way to fly back!
                health.TakeDamage(damageAmount, transform.position);
            }
        }
    }
}