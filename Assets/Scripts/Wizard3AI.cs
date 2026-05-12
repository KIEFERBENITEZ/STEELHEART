using UnityEngine;
using System.Collections;

public class Wizard3AI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float attackRange = 8f;
    public float attackCooldown = 4f;

    [Header("Attack 1: Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Attack 2: Summon")]
    public GameObject monsterPrefab;
    public Transform summonPoint;

    [Header("Components")]
    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private EnemyHealth healthScript;

    private bool isAttacking = false;
    private bool isDead = false;
    private float cooldownTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        healthScript = GetComponent<EnemyHealth>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (healthScript != null && healthScript.isDead && !isDead)
        {
            Die();
            return;
        }

        if (player == null || isDead) return;

        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

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
        else if (distanceToPlayer > attackRange)
        {
            Move();
        }
        else
        {
            StopMoving();
        }
    }

    void Move()
    {
        anim.SetBool("isRunning", true);
        float direction = (player.position.x > transform.position.x) ? 1 : -1;
        transform.localScale = new Vector3(direction * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    void StopMoving()
    {
        anim.SetBool("isRunning", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void StartAttack()
    {
        isAttacking = true;
        StopMoving();

        // PHASE LOGIC:
        if (healthScript != null && healthScript.currentHealth > 5)
        {
            anim.SetTrigger("Attack1");
        }
        else
        {
            anim.SetTrigger("Attack2");
        }
    }

    // --- UPDATED SHOOT LOGIC ---
    public void ShootW3Projectile()
    {
        if (isDead) return;
        if (projectilePrefab && firePoint)
        {
            // 1. Spawn the projectile
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // 2. Determine Wizard's facing direction (1 for Right, -1 for Left)
            float dir = transform.localScale.x > 0 ? 1f : -1f;

            // 3. Find the script on the projectile and call Setup
            W3Projectile projScript = proj.GetComponent<W3Projectile>();
            if (projScript != null)
            {
                projScript.Setup(dir);
            }
        }
    }

    public void SummonMonster()
    {
        if (isDead) return;
        if (monsterPrefab && summonPoint)
        {
            Instantiate(monsterPrefab, summonPoint.position, Quaternion.identity);
        }
    }

    public void FinishAttack()
    {
        isAttacking = false;
        cooldownTimer = attackCooldown;
    }

    public void TakeDamage()
    {
        if (isDead) return;
        anim.SetBool("isRunning", false);
        anim.SetTrigger("Hurt");
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        StopAllCoroutines();
        isAttacking = false;

        anim.ResetTrigger("Hurt");
        anim.ResetTrigger("Attack1");
        anim.ResetTrigger("Attack2");

        anim.SetBool("isDead", true);
        anim.SetBool("isRunning", false);
        anim.SetTrigger("Death");

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        this.enabled = false;
    }
}