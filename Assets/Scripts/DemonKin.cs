using UnityEngine;

public class DemonKin : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float chaseSpeed = 3.5f; // Faster when chasing
    public Transform ledgeCheck;
    public LayerMask groundLayer;

    [Header("Detection & Combat")]
    public float detectionRange = 5f; // How far he can "see"
    public float attackRange = 1.5f;
    public int damage = 1;
    public float attackCooldown = 2f;
    private float cooldownTimer = Mathf.Infinity;

    private Rigidbody2D rb;
    private bool movingRight = true;
    private bool isChasing = false;
    private Animator anim;
    private Transform player;
    private EnemyHealth health;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (health != null && health.isDead)
        {
            rb.linearVelocity = Vector2.zero;
            if (anim != null) anim.SetBool("IsWalking", false);
            this.enabled = false;
            return;
        }

        cooldownTimer += Time.deltaTime;
        if (player == null) return;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // DECISION TREE
        if (distToPlayer <= attackRange)
        {
            // 1. Close enough to hit? Attack.
            if (cooldownTimer >= attackCooldown) Attack();
            else StopMoving(); // Wait for cooldown
        }
        else if (distToPlayer <= detectionRange)
        {
            // 2. Player detected? Chase.
            ChasePlayer();
        }
        else
        {
            // 3. No player? Patrol.
            isChasing = false;
            Patrol();
        }
    }

    void Patrol()
    {
        rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);
        if (anim != null) anim.SetBool("IsWalking", true);

        // Check for ledges/walls to flip
        RaycastHit2D groundInfo = Physics2D.Raycast(ledgeCheck.position, Vector2.down, 1f, groundLayer);
        if (groundInfo.collider == false) Flip();
    }

    void ChasePlayer()
    {
        isChasing = true;
        if (anim != null) anim.SetBool("IsWalking", true);

        // Determine direction to player
        float direction = (player.position.x > transform.position.x) ? 1 : -1;

        // Face the player
        if ((direction > 0 && !movingRight) || (direction < 0 && movingRight))
        {
            Flip();
        }

        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (anim != null) anim.SetBool("IsWalking", false);
    }

    void Attack()
    {
        cooldownTimer = 0;
        StopMoving();
        if (anim != null) anim.SetTrigger("Attack");
    }

    public void ApplyDamage()
    {
        // Check distance again during the animation event to ensure player hasn't dodged
        if (player != null && Vector2.Distance(transform.position, player.position) < attackRange + 0.5f)
            player.GetComponent<PlayerHealth>()?.TakeDamage(damage, transform.position);
    }

    void Flip()
    {
        movingRight = !movingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, 1);
    }

    // VISUALIZE THE RANGES IN THE EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}