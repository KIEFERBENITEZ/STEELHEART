using UnityEngine;
using System.Collections;

public class Wizard2AI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float attackRange = 2.5f;
    public float attackCooldown = 5f;
    public float delayBetweenAttacks = 2f;

    [Header("Attack Hitboxes")]
    public GameObject attack1Hitbox;
    public GameObject attack2Hitbox;

    [Header("Components")]
    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private EnemyHealth healthScript; // Reference to your health script

    private bool isAttacking = false;
    private bool isDead = false;
    private float cooldownTimer = 0f;
    private bool waitingForSecondAttack = false;

    void Start()
    {
        gameObject.tag = "Enemy";
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        healthScript = GetComponent<EnemyHealth>(); // Link to health script

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (attack1Hitbox) attack1Hitbox.SetActive(false);
        if (attack2Hitbox) attack2Hitbox.SetActive(false);
    }

    void Update()
    {
        // --- THE FIX ---
        // If the EnemyHealth script says we are dead, but this script hasn't stopped yet:
        if (healthScript != null && healthScript.isDead && !isDead)
        {
            Die();
            return;
        }

        if (player == null || isDead) return;
        // ---------------

        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && cooldownTimer <= 0)
        {
            StartComboAttack();
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

    void StartComboAttack()
    {
        if (isDead) return;
        isAttacking = true;
        waitingForSecondAttack = true;
        StopMoving();
        anim.SetTrigger("Attack1");
    }

    // --- ANIMATION EVENTS ---
    public void StartAttack1() { if (attack1Hitbox && !isDead) attack1Hitbox.SetActive(true); }

    public void StopAttack1()
    {
        if (attack1Hitbox) attack1Hitbox.SetActive(false);
        if (waitingForSecondAttack && !isDead)
        {
            StartCoroutine(WaitAndShootSecondAttack());
        }
        else
        {
            isAttacking = false;
        }
    }

    IEnumerator WaitAndShootSecondAttack()
    {
        waitingForSecondAttack = false;
        yield return new WaitForSeconds(delayBetweenAttacks);

        if (!isDead)
        {
            anim.SetTrigger("Attack2");
        }
        else
        {
            isAttacking = false;
        }
    }

    public void StartAttack2() { if (attack2Hitbox && !isDead) attack2Hitbox.SetActive(true); }

    public void StopAttack2()
    {
        if (attack2Hitbox) attack2Hitbox.SetActive(false);
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
        waitingForSecondAttack = false;

        // Force Animator to stop fighting
        anim.ResetTrigger("Hurt");
        anim.ResetTrigger("Attack1");
        anim.ResetTrigger("Attack2");

        anim.SetBool("isDead", true);
        anim.SetBool("isRunning", false);
        anim.SetTrigger("Death");

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        if (attack1Hitbox) attack1Hitbox.SetActive(false);
        if (attack2Hitbox) attack2Hitbox.SetActive(false);

        this.enabled = false;
    }
}