using UnityEngine;
using System.Collections;

public class SamuraiAI : MonoBehaviour
{
    [Header("Platform Boundaries")]
    public float leftPoint;
    public float rightPoint;

    [Header("Detection & Movement")]
    public float detectionRange = 7f;
    public float runSpeed = 4.5f;

    [Header("Dash Attack")]
    public float attackRange = 3f;
    public float dashForce = 25f;
    public float dashDuration = 0.12f;
    public float attackCooldown = 1.5f;
    public int damage = 1;

    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private EnemyHealth health;
    private bool canAttack = true;
    private bool isDashing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<EnemyHealth>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (leftPoint == 0 && rightPoint == 0)
        {
            leftPoint = transform.position.x - 3f;
            rightPoint = transform.position.x + 3f;
        }
    }

    void Update()
    {
        // STOP LOGIC IF DEAD
        if (health != null && health.isDead)
        {
            rb.linearVelocity = Vector2.zero;
            if (anim != null) anim.SetBool("IsMoving", false);
            this.enabled = false; // "Kills" the brain
            return;
        }

        if (player == null || isDashing) return;

        float clampedX = Mathf.Clamp(transform.position.x, leftPoint, rightPoint);
        transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);

        float dist = Vector2.Distance(transform.position, player.position);
        bool playerOnPlatform = (player.position.x >= leftPoint && player.position.x <= rightPoint);

        if (dist <= detectionRange && playerOnPlatform)
        {
            if (dist <= attackRange)
            {
                if (canAttack) StartCoroutine(PerformAttack());
            }
            else ChasePlayer();
        }
        else StopMoving();
    }

    void ChasePlayer()
    {
        float dir = player.position.x > transform.position.x ? 1 : -1;
        if ((transform.position.x >= rightPoint - 0.1f && dir > 0) || (transform.position.x <= leftPoint + 0.1f && dir < 0))
        {
            StopMoving();
            return;
        }
        FlipSprite(dir);
        rb.linearVelocity = new Vector2(dir * runSpeed, rb.linearVelocity.y);
        anim.SetBool("IsMoving", true);
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("IsMoving", false);
    }

    IEnumerator PerformAttack()
    {
        canAttack = false;
        isDashing = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("IsMoving", false);
        yield return new WaitForSeconds(0.2f);

        Physics2D.IgnoreLayerCollision(gameObject.layer, player.gameObject.layer, true);
        anim.SetTrigger("Attack");
        float dashDir = player.position.x > transform.position.x ? 1 : -1;
        FlipSprite(dashDir);

        rb.linearVelocity = new Vector2(dashDir * dashForce, 0);
        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        Physics2D.IgnoreLayerCollision(gameObject.layer, player.gameObject.layer, false);
        isDashing = false;

        if (Vector2.Distance(transform.position, player.position) <= 3.5f)
            player.GetComponent<PlayerHealth>()?.TakeDamage(damage, transform.position);

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void FlipSprite(float dir)
    {
        if (dir > 0) transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
        else if (dir < 0) transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1);
    }
}