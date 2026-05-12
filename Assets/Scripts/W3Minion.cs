using UnityEngine;

public class W3_Minion : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float attackRange = 1.5f; // How close to get before attacking
    public float attackCooldown = 2f;
    public float lifeSpan = 10f;

    [Header("Attack Hitbox")]
    public GameObject attackHitbox; // Assign the child object with the collider

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth healthScript;

    private bool canMove = true; // Set to true if you don't have a spawn animation
    private bool isDead = false;
    private bool isAttacking = false;
    private float cooldownTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        healthScript = GetComponent<EnemyHealth>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (attackHitbox) attackHitbox.SetActive(false);

        Destroy(gameObject, lifeSpan);
    }

    void Update()
    {
        // 1. Sync with EnemyHealth without changing the Health script
        if (healthScript != null && healthScript.isDead && !isDead)
        {
            Die();
            return;
        }

        if (player == null || isDead) return;

        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        // Don't move while attacking
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && cooldownTimer <= 0)
        {
            StartAttack();
        }
        else if (canMove)
        {
            MoveTowardsPlayer();
        }
    }

    void MoveTowardsPlayer()
    {
        anim.SetBool("isRunning", true);
        float direction = (player.position.x > transform.position.x) ? 1 : -1;
        transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    void StartAttack()
    {
        isAttacking = true;
        anim.SetBool("isRunning", false);
        anim.SetTrigger("Attack");
    }

    // --- ANIMATION EVENTS (You must add these to the Attack Clip) ---

    // 1. Put this event on the frame where the minion actually hits
    public void StartMinionAttack()
    {
        if (attackHitbox && !isDead) attackHitbox.SetActive(true);
    }

    // 2. Put this event on the frame where the hit is finished
    public void StopMinionAttack()
    {
        if (attackHitbox) attackHitbox.SetActive(false);
    }

    // 3. Put this event at the VERY LAST frame of the attack animation
    public void FinishMinionAttack()
    {
        isAttacking = false;
        cooldownTimer = attackCooldown;
    }

    public void StartMoving() { canMove = true; }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        if (attackHitbox) attackHitbox.SetActive(false);

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // Add this line to force the bool off immediately
        anim.SetBool("isRunning", false);

        anim.SetBool("isDead", true); // Matches your screenshot parameter name
        anim.SetTrigger("Death");

        this.enabled = false;
        Destroy(gameObject, 1.5f);
    }
}