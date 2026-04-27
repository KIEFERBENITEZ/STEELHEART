using UnityEngine;

public class DesertBoss : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3f;
    public float rushSpeed = 7f;
    public float meleeRange = 2.2f;
    public float stopDistance = 8f;

    [Header("Attack 1 (Single Slash)")]
    public float meleeCooldown = 3f;
    private float nextMeleeTime;

    [Header("Attack 2 (Combo/Whirlwind Phase)")]
    public int phase2HealthThreshold = 5;
    public GameObject whirlwindPrefab;
    public Transform firePoint;
    public float comboCooldown = 10f;
    private float nextComboTime;

    [Header("Debug Tools")]
    public bool debugSpawnWhirlwind = false;

    [Header("Detection & Logic")]
    public LayerMask playerLayer;
    public Transform meleePoint;
    public bool isFacingRight = true;
    public bool spriteFacingLeftByDefault = false;

    private Transform player;
    private PlayerController playerScript;
    private Animator anim;
    private Rigidbody2D rb;
    private EnemyHealth healthScript;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        healthScript = GetComponent<EnemyHealth>();

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj)
        {
            player = pObj.transform;
            playerScript = pObj.GetComponent<PlayerController>();
        }

        if (spriteFacingLeftByDefault) isFacingRight = false;
    }

    void Update()
    {
        if (debugSpawnWhirlwind)
        {
            debugSpawnWhirlwind = false;
            SummonWhirlwind();
        }

        // --- DEATH CHECK FIX ---
        if (healthScript != null && healthScript.isDead)
        {
            // Freeze him in place so the death animation plays perfectly
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (!player) return;

        var state = anim.GetCurrentAnimatorStateInfo(0);
        // Added "Death" to the check so he doesn't move while dying
        bool isAttacking = state.IsName("Attack1") || state.IsName("Attack2") || state.IsName("Hit") || state.IsName("Death");

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        bool playerStunned = (playerScript != null && playerScript.isStunned);

        FlipLogic();

        if (playerStunned)
        {
            if (dist > meleeRange) Move(rushSpeed);
            else
            {
                Move(0);
                if (Time.time >= nextMeleeTime) TriggerAttack1();
            }
        }
        else if (healthScript != null && healthScript.currentHealth <= phase2HealthThreshold
                 && Time.time >= nextComboTime && dist <= stopDistance)
        {
            Move(0);
            TriggerAttack2();
        }
        else if (dist <= meleeRange)
        {
            Move(0);
            if (Time.time >= nextMeleeTime) TriggerAttack1();
        }
        else
        {
            Move(walkSpeed);
        }
    }

    void Move(float s)
    {
        float dir = (player.position.x > transform.position.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(dir * s, rb.linearVelocity.y);
        anim.SetFloat("Speed", Mathf.Abs(s));
    }

    void TriggerAttack1()
    {
        anim.SetTrigger("Attack1");
        nextMeleeTime = Time.time + meleeCooldown;
    }

    void TriggerAttack2()
    {
        anim.SetTrigger("Attack2");
        nextComboTime = Time.time + comboCooldown;
    }

    // --- ANIMATION EVENTS ---
    public void PerformMeleeDamage()
    {
        Collider2D hit = Physics2D.OverlapCircle(meleePoint.position, 1.5f, playerLayer);
        if (hit) hit.GetComponent<PlayerHealth>()?.TakeDamage(1, transform.position);
    }

    public void PerformComboHit()
    {
        Collider2D hit = Physics2D.OverlapCircle(meleePoint.position, 2f, playerLayer);
        if (hit)
        {
            playerScript?.ApplyStun(2f);
            hit.GetComponent<PlayerHealth>()?.TakeDamage(1, transform.position);
        }
    }

    public void SummonWhirlwind()
    {
        if (whirlwindPrefab && firePoint)
        {
            Instantiate(whirlwindPrefab, firePoint.position, Quaternion.identity);
        }
    }

    void FlipLogic()
    {
        float directionToPlayer = player.position.x - transform.position.x;
        if (directionToPlayer > 0 && !isFacingRight) Flip();
        else if (directionToPlayer < 0 && isFacingRight) Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (meleePoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(meleePoint.position, 1.5f);
        }
    }
}