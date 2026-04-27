using UnityEngine;
using System.Collections;

public class EnemyChaseDash : MonoBehaviour
{
    public Transform player;
    private Animator anim; // NEW
    private Rigidbody2D rb; // NEW

    [Header("Patrol")]
    public float patrolSpeed = 2f;
    public float leftPoint = 2f;
    public float rightPoint = 6f;

    [Header("Detection")]
    public float detectionRange = 10f;

    [Header("Chase")]
    public float chaseSpeed = 3f;

    [Header("Dash")]
    public float dashRange = 4f;
    public float dashSpeed = 8f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1.5f;

    private bool movingRight = true;
    private bool isDashing = false;
    private bool canDash = true;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (player == null)
        {
            Debug.LogError("REAPER ERROR: Player is not assigned in the Inspector!");
            return;
        }

        FacePlayer();

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // This will tell us if the Reaper even sees you
        float currentMoveSpeed = isDashing ? 0 : (distanceToPlayer <= detectionRange ? chaseSpeed : patrolSpeed);
        if (anim != null) anim.SetFloat("Speed", currentMoveSpeed);

        if (isDashing) return;

        // FORCED TEST: If distance is less than 5, ATTACK!
        if (distanceToPlayer <= dashRange && canDash)
        {
            Debug.Log("DISTANCE CHECK PASSED! Starting Attack...");
            StartCoroutine(AttackRoutine());
        }
        else if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (movingRight)
        {
            transform.Translate(Vector2.right * patrolSpeed * Time.deltaTime);
            if (transform.position.x >= rightPoint) movingRight = false;
        }
        else
        {
            transform.Translate(Vector2.left * patrolSpeed * Time.deltaTime);
            if (transform.position.x <= leftPoint) movingRight = true;
        }
    }

    void FacePlayer()
    {
        if (player == null || isDashing) return; // Added safety check

        if (player.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(3, 3, 1);
        }
        else
        {
            transform.localScale = new Vector3(-3, 3, 1);
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)(direction * chaseSpeed * Time.deltaTime);
    }

    IEnumerator AttackRoutine()
    {
        canDash = false;

        // 1. Face the player FIRST, before we lock movement
        FacePlayer();

        isDashing = true; // Lock movement now

        // 2. Play the Animation
        if (anim != null) anim.SetTrigger("Attack");

        yield return new WaitForSeconds(0.8f);

        SpawnRoots();

        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    void SpawnRoots()
    {
        Debug.Log("Reaper summoned roots!");
        // Logic for spawning a Root Prefab would go here later
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashRange);
    }
}