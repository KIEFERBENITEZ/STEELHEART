using UnityEngine;

public class ArchdemonAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 15f;
    private Transform player;

    [Header("Shooting Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Tooltip("The base delay between attacks (in seconds)")]
    public float attackCooldown = 3f;

    [Tooltip("Adds random extra time so he isn't too predictable")]
    public float cooldownVariation = 1f;

    private float nextFireTime;

    [Header("Components")]
    public Animator anim;
    private EnemyHealth healthScript;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (anim == null) anim = GetComponent<Animator>();
        healthScript = GetComponent<EnemyHealth>();

        // Start with a slight delay so he doesn't shoot the millisecond the game starts
        nextFireTime = Time.time + 1f;
    }

    void Update()
    {
        if (healthScript != null && healthScript.isDead)
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            this.enabled = false;
            return;
        }

        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            LookAtPlayer();

            // --- COOLDOWN LOGIC ---
            if (Time.time >= nextFireTime)
            {
                StartAttackSequence();

                // Calculate next time to fire: Base Cooldown + a random bit of extra time
                float randomDelay = Random.Range(0, cooldownVariation);
                nextFireTime = Time.time + attackCooldown + randomDelay;
            }
        }
    }

    void LookAtPlayer()
    {
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(3f, 3f, 1f);
        else
            transform.localScale = new Vector3(-3f, 3f, 1f);
    }

    void StartAttackSequence()
    {
        if (anim != null)
        {
            anim.SetTrigger("attack");
        }
    }

    public void CreateHadouken()
    {
        if (healthScript != null && healthScript.isDead) return;

        if (projectilePrefab != null && firePoint != null && player != null)
        {
            GameObject ball = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Vector2 direction = (player.position - firePoint.position).normalized;
            ball.transform.right = direction;
            Debug.Log("Hadouken Released!");
        }
    }
}