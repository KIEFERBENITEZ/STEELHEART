using UnityEngine;
using System.Collections;

public class DreadwingAI : MonoBehaviour
{
    [Header("Detection & Movement")]
    public float detectionRange = 10f;
    public float walkSpeed = 2.5f;
    public float flySpeed = 4.5f;
    public float dashSpeed = 16f;

    [Header("Phase 1 Settings")]
    public float attack1Distance = 2.5f;
    public float attack1HitRadius = 1.8f;
    public float attack1Cooldown = 3f;
    public float attack1Duration = 0.6f;

    [Header("Phase 2 Settings")]
    public int phase2Threshold = 4;
    public float attack2Distance = 5f;
    public float attack2HitRadius = 3.5f;
    public float attack2Cooldown = 5f;
    public int attack2Damage = 2;
    public float attack2Duration = 0.8f;

    [Header("Combat General")]
    public LayerMask playerLayer;
    public Transform attackPoint;
    public float avoidDistance = 4f;

    private float nextAttackTime = 0f;
    private bool isPlayerDetected = false;
    private bool isDashing = false;
    private bool isAttacking = false;
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth health;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();
        sr = GetComponent<SpriteRenderer>();
        rb.freezeRotation = true;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (health == null || health.isDead || player == null) return;

        // CRITICAL: If he is attacking or dashing, DO NOT run any other movement or flip logic
        if (isAttacking || isDashing) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isPlayerDetected)
        {
            if (distanceToPlayer <= detectionRange) isPlayerDetected = true;
            else { StopMoving(); return; }
        }

        // --- FLIP LOGIC ---
        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            ApplyFlip(rb.linearVelocity.x > 0);
        }
        else
        {
            ApplyFlip(player.position.x > transform.position.x);
        }

        bool isP2 = (health.currentHealth <= phase2Threshold);

        if (Time.time >= nextAttackTime)
        {
            float targetDist = isP2 ? attack2Distance : attack1Distance;
            if (distanceToPlayer > targetDist) MoveTowards(player.position, isP2);
            else
            {
                if (isP2) StartCoroutine(DashAttack());
                else StartCoroutine(ExecuteAttack1());
            }
        }
        else
        {
            AvoidPlayer(distanceToPlayer, isP2);
        }
    }

    void ApplyFlip(bool lookRight)
    {
        if (sr != null) sr.flipX = false;
        float lookDir = lookRight ? 1 : -1;
        transform.localScale = new Vector3(lookDir * Mathf.Abs(transform.localScale.x),
                                           transform.localScale.y, transform.localScale.z);
    }

    void MoveTowards(Vector2 target, bool isFlying)
    {
        float speed = isFlying ? flySpeed : walkSpeed;
        Vector2 dir = ((Vector2)target - (Vector2)transform.position).normalized;

        if (!isFlying)
        {
            rb.linearVelocity = new Vector2(dir.x * speed, rb.linearVelocity.y);
            rb.gravityScale = 1;
        }
        else
        {
            rb.linearVelocity = dir * speed;
            rb.gravityScale = 0;
        }

        anim.SetBool("IsFlying", isFlying);
        anim.SetBool("IsWalking", !isFlying);
    }

    void AvoidPlayer(float dist, bool isFlying)
    {
        if (dist < avoidDistance)
        {
            Vector2 awayDir = ((Vector2)transform.position - (Vector2)player.position).normalized;
            float speed = isFlying ? flySpeed : walkSpeed;

            if (!isFlying)
            {
                rb.linearVelocity = new Vector2(awayDir.x * speed, rb.linearVelocity.y);
                rb.gravityScale = 1;
            }
            else
            {
                rb.linearVelocity = awayDir * speed;
                rb.gravityScale = 0;
            }
            anim.SetBool("IsWalking", !isFlying);
            anim.SetBool("IsFlying", isFlying);
        }
        else
        {
            StopMoving();
        }
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("IsWalking", false);
    }

    IEnumerator ExecuteAttack1()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        ApplyFlip(player.position.x > transform.position.x);

        anim.SetTrigger("Attack1");

        // --- EMERGENCY HIT CALL ---
        // If your animation doesn't have events, this forces a hit check halfway through
        Invoke("HitPlayer", attack1Duration / 2f);

        yield return new WaitForSeconds(attack1Duration);

        isAttacking = false;
        nextAttackTime = Time.time + attack1Cooldown;
    }

    IEnumerator DashAttack()
    {
        isAttacking = true;
        isDashing = true;
        anim.SetBool("IsFlying", true);
        rb.linearVelocity = Vector2.zero;

        ApplyFlip(player.position.x > transform.position.x);
        yield return new WaitForSeconds(0.3f);

        anim.SetTrigger("Attack2");
        Vector2 dashDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = dashDir * dashSpeed;

        // Damage check during the lunge
        Invoke("HitPlayer", 0.2f);

        yield return new WaitForSeconds(0.4f);
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(attack2Duration - 0.4f);

        isDashing = false;
        isAttacking = false;
        nextAttackTime = Time.time + attack2Cooldown;
    }

    public void HitPlayer()
    {
        if (attackPoint == null || health.isDead) return;

        bool p2 = (health.currentHealth <= phase2Threshold);
        float r = p2 ? attack2HitRadius : attack1HitRadius;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, r, playerLayer);
        foreach (Collider2D p in hits)
        {
            p.GetComponent<PlayerHealth>()?.TakeDamage(p2 ? attack2Damage : 1);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, attack1HitRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attack2HitRadius);
        }
    }
}