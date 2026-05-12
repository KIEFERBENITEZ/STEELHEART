using UnityEngine;
using System.Collections;

public class FireTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 1;
    public float damageDelay = 0.3f;    // Time before fire becomes deadly
    public float damageDuration = 1.5f; // How long fire stays deadly
    public float cooldownTime = 3f;     // Time before trap can reset
    public float damageInterval = 0.5f; // Time between each heart loss

    private Animator anim;
    private float cooldownTimer = 0f;
    private float nextDamageTime = 0f;
    private bool isFireActive = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Handle the main trap cooldown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player stepped on it and trap is ready
        if (other.CompareTag("Player") && cooldownTimer <= 0)
        {
            StartCoroutine(TrapSequence());
        }
    }

    IEnumerator TrapSequence()
    {
        cooldownTimer = cooldownTime;

        // 1. Play Animation
        if (anim != null)
        {
            anim.SetTrigger("Activate");
            Debug.Log("Trap Animation Started");
        }

        // 2. Wait for fire to appear visually
        yield return new WaitForSeconds(damageDelay);
        isFireActive = true;
        Debug.Log("Fire is now DEADLY");

        // 3. Keep fire deadly for the duration
        yield return new WaitForSeconds(damageDuration);
        isFireActive = false;
        Debug.Log("Fire is safe now");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Only deal damage if fire is active, it's the player, and interval has passed
        if (isFireActive && other.CompareTag("Player") && Time.time >= nextDamageTime)
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
                nextDamageTime = Time.time + damageInterval; // Prevent instant death
                Debug.Log("Player hit! Health reduced.");
            }
            else
            {
                // This warning shows if you hit the player but your health script name is different
                Debug.LogWarning("Hit Player, but no 'PlayerHealth' script found!");
            }
        }
    }
}