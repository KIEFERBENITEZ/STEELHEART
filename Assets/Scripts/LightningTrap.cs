using System.Collections;
using UnityEngine;

public class LightningTrap : MonoBehaviour
{
    [Header("Timing Settings")]
    public float warningDuration = 0.7f;
    public float activeDuration = 0.5f;
    public int damageAmount = 1;

    private Animator anim;
    private bool isStriking = false;
    private bool playerInside = false;
    private PlayerHealth playerHealth;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // 1. DETECTION: This starts the animation sequence
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            playerHealth = other.GetComponent<PlayerHealth>();

            if (!isStriking)
            {
                StartCoroutine(TrapSequence());
            }
        }
    }

    // 2. EXIT: If the player runs away before it strikes
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    IEnumerator TrapSequence()
    {
        isStriking = true;

        // --- QUICK WARNING ---
        anim.SetTrigger("Trigger");
        yield return new WaitForSeconds(warningDuration); // Set to 0.2 in Inspector

        // --- INSTANT STRIKE ---
        anim.SetTrigger("Strike");

        if (playerInside && playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount, transform.position);
        }

        yield return new WaitForSeconds(activeDuration); // Set to 0.1 in Inspector

        // --- QUICK RESET ---
        anim.SetTrigger("Reset");

        yield return new WaitForSeconds(0.2f); // Short recovery time
        isStriking = false;

        // Re-trigger if player is still there
        if (playerInside) StartCoroutine(TrapSequence());
    }
}