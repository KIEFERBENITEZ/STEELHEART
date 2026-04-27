using UnityEngine;

public class ArcherBoss : MonoBehaviour
{
    [Header("Stats")]
    public float walkSpeed = 3.5f;
    public float rushSpeed = 6.5f;
    public float meleeDist = 1.8f;
    public float stopDist = 7f;
    public float bowCooldown = 3f;
    public float meleeCooldown = 1.5f;
    public bool isFacingRight = true;
    public float stunTime = 2f; // This is the duration in seconds

    [Header("Refs")]
    public GameObject arrowPrefab;
    public Transform Firepoint, Meleepoint, edgeCheck;
    public LayerMask groundLayer, playerLayer;

    private Transform player;
    private PlayerController playerScript;
    private Animator anim;
    private Rigidbody2D rb;
    private float nextBowTime;
    private float nextMeleeTime;
    private bool isRetreating;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.mass = 100f;
        rb.freezeRotation = true;

        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj)
        {
            player = pObj.transform;
            playerScript = pObj.GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        // SAFETY GATE: Prevents "UnassignedReference" errors if slots are empty
        if (!player || edgeCheck == null) return;

        // 1. DATA GATHERING
        var state = anim.GetCurrentAnimatorStateInfo(0);
        bool isMeleeing = state.IsName("ArcherBoss_Melee");
        bool isShooting = state.IsName("ArcherBoss_Bow");
        bool isHurt = state.IsName("ArcherBoss_Hit"); // Ensure your hit clip is named this!

        float dist = Vector2.Distance(transform.position, player.position);
        bool atEdge = !Physics2D.Raycast(edgeCheck.position, Vector2.down, 1.5f, groundLayer);
        bool playerIsStunned = (playerScript != null && playerScript.isStunned);

        FlipLogic();

        // 2. PRIORITY LOCKS (Prevents movement/logic from overriding animations)

        // LOCK A: Hurt/Hit takes top priority. He must stay still while flinching.
        if (isHurt)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // LOCK B: Attack Locks. 
        // If shooting, only stay frozen if the player is NOT stunned (to allow the rush).
        bool moveLock = isMeleeing || (isShooting && !playerIsStunned);
        if (moveLock)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // 3. BRAIN LOGIC

        // COMBO PHASE: Player is STUNNED -> Rush and Melee
        if (playerIsStunned)
        {
            isRetreating = false;

            if (dist > meleeDist)
            {
                if (!atEdge) Move(rushSpeed, 1.8f);
                else Move(0, 0);
            }
            else
            {
                Move(0, 0);
                if (Time.time >= nextMeleeTime) TriggerMelee();
            }
        }
        // RETREAT PHASE: Back away after hitting the player
        else if (isRetreating)
        {
            if (dist < stopDist - 2f && !atEdge) Move(walkSpeed, -1.2f);
            else { isRetreating = false; Move(0, 0); }
        }
        // MELEE PHASE: Player is close but NOT stunned
        else if (dist <= meleeDist)
        {
            Move(0, 0);
            if (Time.time >= nextMeleeTime) TriggerMelee();
        }
        // RANGE PHASE: Shoot the bow to start the combo
        else if (dist <= stopDist)
        {
            Move(0, 0);
            if (Time.time >= nextBowTime) TriggerBow();
        }
        // CHASE PHASE: Get closer to the player
        else
        {
            if (!atEdge) Move(walkSpeed, 1f);
            else Move(0, 0);
        }
    }

    void TriggerMelee()
    {
        var state = anim.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("ArcherBoss_Melee")) return;

        anim.Play("ArcherBoss_Melee", 0, 0f);
        nextMeleeTime = Time.time + meleeCooldown;
        isRetreating = true;
    }

    void TriggerBow()
    {
        var state = anim.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("ArcherBoss_Bow")) return;

        anim.Play("ArcherBoss_Bow", 0, 0f);
        nextBowTime = Time.time + bowCooldown;
    }

    void Move(float currentSpeed, float dirMult)
    {
        float direction = (player.position.x > transform.position.x ? 1 : -1) * dirMult;
        rb.linearVelocity = new Vector2(direction * currentSpeed, rb.linearVelocity.y);

        // Drive animator speed parameter
        anim.SetFloat("Speed", Mathf.Abs(dirMult));
    }

    void FlipLogic()
    {
        var state = anim.GetCurrentAnimatorStateInfo(0);
        // Don't flip while attacking or hurt
        if (state.IsName("ArcherBoss_Melee") || state.IsName("ArcherBoss_Bow") || state.IsName("ArcherBoss_Hit")) return;

        if ((player.position.x > transform.position.x && !isFacingRight) ||
            (player.position.x < transform.position.x && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
    }

    // --- ANIMATION EVENTS ---
    public void BossFireArrow()
    {
        if (arrowPrefab && Firepoint)
        {
            Instantiate(arrowPrefab, Firepoint.position, Quaternion.Euler(0, 0, isFacingRight ? 0 : 180));
        }
    }

    public void PerformMeleeDamage()
    {
        Collider2D hit = Physics2D.OverlapCircle(Meleepoint.position, 1.5f, playerLayer);
        if (hit) hit.GetComponent<PlayerHealth>()?.TakeDamage(1, transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        if (Meleepoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Meleepoint.position, 1.5f);
        }
    }
}