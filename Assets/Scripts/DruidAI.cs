using UnityEngine;

public class DruidAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public Transform ledgeCheck;
    public LayerMask groundLayer;
    private bool movingRight = true;

    // ANTI-SHAKE VARIABLES
    private float flipCooldown = 0.5f;
    private float lastFlipTime;

    [Header("Detection & Timing")]
    public float attackRange = 10f;
    public float attackCooldown = 3.5f;
    private float nextAttackTime;
    private Transform player;

    [Header("Summon Settings")]
    public GameObject earthSpikePrefab;

    private Animator anim;
    private EnemyHealth health;
    private Rigidbody2D rb;

    void Start()
    {
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody2D>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (rb != null) rb.freezeRotation = true;
    }

    void Update()
    {
        if (health != null && health.isDead)
        {
            StopMovement();
            this.enabled = false;
            return;
        }

        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            StopAndAttack();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        anim.SetBool("IsWalking", true);
        rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);
        UpdateFacingDirection(movingRight);

        if (ledgeCheck != null)
        {
            RaycastHit2D groundInfo = Physics2D.Raycast(ledgeCheck.position, Vector2.down, 1.5f, groundLayer);

            Vector2 forwardDir = movingRight ? Vector2.right : Vector2.left;
            RaycastHit2D wallInfo = Physics2D.Raycast(ledgeCheck.position, forwardDir, 0.5f, groundLayer);

            Debug.DrawRay(ledgeCheck.position, Vector2.down * 1.5f, Color.red);

            // ADDED: Time.time check to prevent rapid flickering
            if ((groundInfo.collider == null || wallInfo.collider != null) && Time.time > lastFlipTime + flipCooldown)
            {
                Flip();
            }
        }
    }

    void StopAndAttack()
    {
        StopMovement();
        bool shouldFaceRight = player.position.x > transform.position.x;
        UpdateFacingDirection(shouldFaceRight);

        if (Time.time >= nextAttackTime)
        {
            anim.SetTrigger("Summon");
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    void StopMovement()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("IsWalking", false);
    }

    void Flip()
    {
        movingRight = !movingRight;
        lastFlipTime = Time.time; // Mark the time he flipped
    }

    void UpdateFacingDirection(bool faceRight)
    {
        float xRotate = faceRight ? Mathf.Abs(transform.localScale.x) : -Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(xRotate, transform.localScale.y, transform.localScale.z);
    }

    public void SpawnEarthSpike()
    {
        if (player != null && earthSpikePrefab != null)
        {
            Vector3 spawnPos = new Vector3(player.position.x, player.position.y, 0);
            Instantiate(earthSpikePrefab, spawnPos, Quaternion.identity);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the thing we bumped into is the Player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Calculate direction to knock the player back (away from the druid)
            Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;

            // Try to deal damage
            // Change '1' to how much damage the Druid's body should do
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(1, knockbackDir * 5f);
        }
    }
}