using UnityEngine;
using System.Collections;

public class CrystalBossAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float attackRange = 2.5f;

    [Header("Attack Timing")]
    public float timeBetweenCombos = 2.0f;
    private float nextAttackTime = 0f;

    [Header("References")]
    public Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private EnemyHealth healthScript;

    private bool isAttacking = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        healthScript = GetComponent<EnemyHealth>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (healthScript != null && healthScript.isDead) return;
        if (isAttacking || player == null) return;

        // Use X-axis distance only to prevent "running in place" when heights differ
        float distanceX = Mathf.Abs(transform.position.x - player.position.x);

        if (distanceX <= attackRange && Time.time >= nextAttackTime)
        {
            ChooseAttack();
        }
        else if (distanceX > attackRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            // Within range but on cooldown
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            anim.SetBool("isRunning", false);
            FlipTowardsPlayer(); // Keep facing the player while waiting
        }
    }

    void MoveTowardsPlayer()
    {
        anim.SetBool("isRunning", true);

        float directionX = player.position.x - transform.position.x;
        float moveDir = directionX > 0 ? 1 : -1;

        rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);

        FlipTowardsPlayer();
    }

    void FlipTowardsPlayer()
    {
        float directionX = player.position.x - transform.position.x;

        // SWAP THE MINUS SIGNS HERE
        if (directionX > 0)
        {
            // If he was moonwalking, remove the '-' from Mathf.Abs
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            // If he was moonwalking, add the '-' here
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    void ChooseAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetBool("isRunning", false);
        FlipTowardsPlayer(); // Ensure one last flip before starting animation

        if (healthScript != null && healthScript.currentHealth > 4)
        {
            StartCoroutine(ComboPhaseOne());
        }
        else
        {
            StartCoroutine(ComboPhaseTwo());
        }
    }

    IEnumerator ComboPhaseOne()
    {
        anim.SetTrigger("A1");
        yield return new WaitForSeconds(1.2f);

        anim.SetTrigger("A2");
        yield return new WaitForSeconds(1.2f);

        FinishAttack();
    }

    IEnumerator ComboPhaseTwo()
    {
        anim.SetTrigger("A3");
        yield return new WaitForSeconds(1.5f);

        anim.SetTrigger("A4");
        yield return new WaitForSeconds(1.5f);

        FinishAttack();
    }

    void FinishAttack()
    {
        isAttacking = false;
        nextAttackTime = Time.time + timeBetweenCombos;
    }

    // --- ANIMATION EVENTS ---
    public void Event_StunPlayer() { Debug.Log("Crystal Boss: STUNNED PLAYER"); }
    public void Event_DealDamage() { CheckHit(1); }
    public void Event_DealSpecialDamage() { CheckHit(2); }

    void CheckHit(int dmg)
    {
        if (player == null) return;
        float dist = Mathf.Abs(transform.position.x - player.position.x);
        if (dist <= attackRange + 0.8f) // Slightly bigger window for damage
        {
            player.GetComponent<PlayerHealth>()?.TakeDamage(dmg);
        }
    }
}