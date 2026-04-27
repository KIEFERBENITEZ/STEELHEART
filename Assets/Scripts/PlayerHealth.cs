using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    public float invincibilityTime = 1f;
    public int currentHealth;

    [Header("Checkpoint System")]
    [Tooltip("The position the player will return to on death.")]
    public Vector3 respawnPoint;

    [Header("Knockback Settings")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.2f;
    public float knockbackUpForce = 4f;

    [Header("UI Settings")]
    public GameObject heartPrefab;
    public Transform heartParent;

    private bool isInvincible = false;
    private bool isKnockedBack = false;
    private bool isDead = false;

    private Animator anim;
    private Rigidbody2D rb;
    private Coroutine knockbackRoutine;
    private Coroutine invincibleRoutine;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Set starting position as the first checkpoint
        respawnPoint = transform.position;

        UpdateUI();
    }

    // --- DETECTION LOGIC ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detect the Pit (DeathZone)
        if (other.CompareTag("Respawn"))
        {
            if (!isDead)
            {
                TakePitDamage();
            }
        }

        // Detect Checkpoints
        if (other.CompareTag("Checkpoint"))
        {
            UpdateCheckpoint(other.transform.position);
        }
    }

    public void UpdateCheckpoint(Vector3 newCheckpoint)
    {
        respawnPoint = newCheckpoint;
        Debug.Log("Checkpoint Updated!");
    }

    // --- DAMAGE & HEALING LOGIC ---

    private void TakePitDamage()
    {
        currentHealth--; // Lose exactly 1 heart
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0)
        {
            Die(true); // Total Death (0 hearts)
        }
        else
        {
            Die(false); // Quick Respawn (still has hearts)
        }
    }

    // Overload for simple damage (Fixes EnemyDamage error)
    public bool TakeDamage(int damage)
    {
        return TakeDamage(damage, transform.position);
    }

    // Full damage with knockback
    public bool TakeDamage(int damage, Vector3 hitSourcePosition)
    {
        if (isInvincible || isDead || isKnockedBack) return false;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();

        isInvincible = true;
        if (anim != null) anim.SetTrigger("Hurt");

        if (knockbackRoutine != null) StopCoroutine(knockbackRoutine);
        knockbackRoutine = StartCoroutine(KnockbackCoroutine(hitSourcePosition));

        if (currentHealth <= 0)
        {
            Die(true);
        }
        else
        {
            if (invincibleRoutine != null) StopCoroutine(invincibleRoutine);
            invincibleRoutine = StartCoroutine(InvincibilityCoroutine());
        }

        return true;
    }

    // Fixes HealthPickup error
    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateUI();
    }

    // --- DEATH & RESPAWN ---

    void Die(bool isGameOver)
    {
        isDead = true;
        isKnockedBack = false;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Reset Ultimate charge if player has that script
        if (TryGetComponent(out MonoBehaviour ultimate))
        {
            ultimate.SendMessage("ResetUltimateState", SendMessageOptions.DontRequireReceiver);
        }

        if (isGameOver && anim != null)
            anim.SetTrigger("Die");
        else if (anim != null)
            anim.SetTrigger("Hurt");

        StartCoroutine(DeathRoutine(isGameOver));
    }

    IEnumerator DeathRoutine(bool isGameOver)
    {
        // Disable movement while dead
        MonoBehaviour controller = GetComponent("PlayerController") as MonoBehaviour;
        if (controller != null) controller.enabled = false;

        // Wait longer if they actually died (0 hearts)
        yield return new WaitForSeconds(isGameOver ? 1.5f : 0.4f);

        // Teleport to Checkpoint
        transform.position = respawnPoint;

        if (isGameOver)
        {
            currentHealth = maxHealth; // Reset hearts on true Game Over
            UpdateUI();
        }

        isDead = false;

        if (anim != null) anim.Play("Idle");
        if (controller != null) controller.enabled = true;
    }

    // --- COROUTINES & UI ---

    IEnumerator KnockbackCoroutine(Vector3 hitSourcePosition)
    {
        isKnockedBack = true;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            float direction = transform.position.x >= hitSourcePosition.x ? 1f : -1f;
            rb.AddForce(new Vector2(direction * knockbackForce, knockbackUpForce), ForceMode2D.Impulse);
        }
        yield return new WaitForSeconds(knockbackDuration);
        isKnockedBack = false;
    }

    IEnumerator InvincibilityCoroutine()
    {
        yield return new WaitForSeconds(invincibilityTime);
        isInvincible = false;
    }

    public void UpdateUI()
    {
        if (heartParent == null || heartPrefab == null) return;

        foreach (Transform child in heartParent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < currentHealth; i++)
        {
            Instantiate(heartPrefab, heartParent);
        }
    }

    // --- EXTERNAL HELPERS (Fixes Ultimate & SpeedBoost Errors) ---

    public bool IsDead() => isDead;
    public void SetInvincible(bool status) => isInvincible = status;
    public bool IsInvincible() => isInvincible;
    public bool IsKnockedBack() => isKnockedBack;
    public int GetCurrentHealth() => currentHealth;
}