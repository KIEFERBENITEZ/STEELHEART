using UnityEngine;

public class GiantAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float stoppingDistance = 1.8f;
    public bool facingRightByDefault = false;

    [Header("Attack Settings")]
    public float attackCooldown = 5f; // Updated to 5 seconds
    private float nextAttackTime = 0f;

    [Header("Combat Detection")]
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public LayerMask playerLayer;
    public int damage = 2; // Updated to 2 damage

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
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
            return;
        }

        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (distance <= stoppingDistance && Time.time >= nextAttackTime)
            {
                StartAttack();
            }
            else if (distance > stoppingDistance)
            {
                MoveTowardsPlayer();
            }
            else
            {
                StopMoving();
                FlipTowardsPlayer();
            }
        }
    }

    void MoveTowardsPlayer()
    {
        float direction = (player.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        FlipTowardsPlayer();
        anim.SetBool("IsWalking", true);
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("IsWalking", false);
    }

    void StartAttack()
    {
        StopMoving();
        FlipTowardsPlayer();
        anim.SetTrigger("Attack");
        nextAttackTime = Time.time + attackCooldown;
    }

    // This function MUST be called by the Animation Event in the Giant_Attack clip
    public void HitPlayer()
    {
        if (attackPoint == null) return;

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayer);

        foreach (Collider2D p in hitPlayers)
        {
            // Note: This looks for a script called PlayerHealth on your player
            var pHealth = p.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                pHealth.TakeDamage(damage);
                Debug.Log("Giant hit for " + damage + " damage!");
            }
        }
    }

    void FlipTowardsPlayer()
    {
        float direction = (player.position.x > transform.position.x) ? 1 : -1;
        float modifier = facingRightByDefault ? 1 : -1;
        transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x) * modifier,
                                           transform.localScale.y,
                                           transform.localScale.z);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}