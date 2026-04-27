using System.Collections;
using UnityEngine;

public class NightBorne : MonoBehaviour
{
    [Header("Combat Stats")]
    public int contactDamage = 1;
    public int explosionDamage = 2;
    public float explosionRadius = 3.5f;

    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float chaseRange = 10f;
    public float attackRange = 2.5f;      // Distance he starts swinging
    public float stoppingDistance = 1.8f; // Personal space (stops overlapping)
    public float scaleSize = 5f;

    [Header("Cooldowns")]
    public float attackCooldown = 3f;
    private float nextAttackTime = 0f;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isDead = false;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // 1. Attack logic
        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            StartCoroutine(AttackRoutine());
        }
        // 2. Chase logic (now includes distance check)
        else if (distance <= chaseRange && !isAttacking)
        {
            Chase(distance);
        }
        // 3. Idle logic
        else if (!isAttacking)
        {
            StopMoving();
        }
    }

    void Chase(float distance)
    {
        float moveDir = player.position.x > transform.position.x ? 1 : -1;

        // If he's further than stopping distance, he runs
        if (distance > stoppingDistance)
        {
            rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
            anim.SetBool("isRunning", true);
        }
        // If he's close enough, he stays still but faces the player
        else
        {
            StopMoving();
        }

        transform.localScale = new Vector3(moveDir * scaleSize, scaleSize, 1f);
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("isRunning", false);
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isRunning", false);
        anim.SetTrigger("Attack");

        // Face player for the strike
        float attackDir = player.position.x > transform.position.x ? 1 : -1;
        transform.localScale = new Vector3(attackDir * scaleSize, scaleSize, 1f);

        yield return new WaitForSeconds(1.2f); // Match your animation length
        isAttacking = false;
    }

    // --- DAMAGE LOGIC (Must be called by Animation Event) ---
    public void NightBorneStrike()
    {
        // Offsets the hit circle forward so it hits the player, not himself
        float lookDir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 attackCenter = new Vector2(transform.position.x + (lookDir * 1.5f), transform.position.y);

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackCenter, attackRange);
        foreach (Collider2D obj in hitPlayers)
        {
            if (obj.CompareTag("Player"))
            {
                obj.GetComponent<PlayerHealth>()?.TakeDamage(contactDamage, transform.position);
                Debug.Log("NightBorne hit the player with a strike!");
            }
        }
    }

    // --- DEATH & EXPLOSION ---
    public void StartDeathSequence()
    {
        if (isDead) return;
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        if (anim != null) anim.SetTrigger("Death");
        yield return new WaitForSeconds(0.8f);

        Explode();
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    void Explode()
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D obj in hitPlayers)
        {
            if (obj.CompareTag("Player"))
            {
                obj.GetComponent<PlayerHealth>()?.TakeDamage(explosionDamage, transform.position);
            }
        }
    }

    // Body Contact Damage
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isDead)
        {
            collision.GetComponent<PlayerHealth>()?.TakeDamage(contactDamage, transform.position);
        }
    }

    // Visual helper for the hit zone (Visible in Scene View)
    private void OnDrawGizmosSelected()
    {
        float lookDir = transform.localScale.x > 0 ? 1 : -1;
        Vector2 attackCenter = new Vector2(transform.position.x + (lookDir * 1.5f), transform.position.y);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackCenter, attackRange);
    }
}