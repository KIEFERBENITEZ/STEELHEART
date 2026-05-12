using UnityEngine;

public class Wizard1AI : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 2.5f;
    public float attackRange = 4f;
    public float attackCooldown = 3f;
    public GameObject fireHitbox;

    [Header("Components")]
    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;

    private bool isAttacking = false;
    private bool isDead = false;
    private float cooldownTimer = 0f;

    void Start()
    {
        // Ensure the Wizard itself is tagged as Enemy
        gameObject.tag = "Enemy";

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // We still search for the "Player" tag to know who to follow
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Wizard1AI: Player tag not found! Make sure your Player object is tagged 'Player'.");
        }

        if (fireHitbox) fireHitbox.SetActive(false);
    }

    void Update()
    {
        if (player == null || isDead) return;

        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > attackRange || cooldownTimer > 0)
        {
            Move();
        }
        else
        {
            StartAttack();
        }
    }

    void Move()
    {
        anim.SetBool("isWalking", true);
        float direction = (player.position.x > transform.position.x) ? 1 : -1;

        // Flip logic
        transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    void StartAttack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("isWalking", false);
        anim.SetTrigger("Attack");

        isAttacking = true;
        cooldownTimer = attackCooldown;
    }

    // --- ANIMATION EVENTS ---
    public void StartFlamethrower()
    {
        if (fireHitbox) fireHitbox.SetActive(true);
    }

    public void StopFlamethrower()
    {
        if (fireHitbox) fireHitbox.SetActive(false);
        isAttacking = false;
    }

    public void TakeDamage()
    {
        // Only trigger hit if not already dead
        if (!isDead) anim.SetTrigger("Hit");
    }

    public void Die()
    {
        if (isDead) return; // Prevent double trigger

        isDead = true;
        anim.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;

        // Disable this script so it stops moving/attacking
        this.enabled = false;
    }
}