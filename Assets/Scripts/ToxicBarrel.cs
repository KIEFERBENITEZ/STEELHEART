using UnityEngine;

public class ToxicBarrel : MonoBehaviour
{
    private Animator anim;

    [Header("Damage Settings")]
    public int damageAmount = 1;
    public float damageInterval = 1.0f;
    private float nextDamageTime;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Turn on the bubbling
            anim.SetBool("IsNear", true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Go back to idle
            anim.SetBool("IsNear", false);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Only damage if we are in the toxic state
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Barrel_Toxic"))
            {
                if (Time.time >= nextDamageTime)
                {
                    PlayerHealth health = other.GetComponent<PlayerHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(damageAmount, transform.position);
                        nextDamageTime = Time.time + damageInterval;
                        Debug.Log("Poisoning the player!");
                    }
                }
            }
        }
    }
}