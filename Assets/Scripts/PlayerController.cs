using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 6f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    [Header("Slow Settings")]
    private float currentSlowMultiplier = 1f;

    [Header("Ledge Settings")]
    public Transform ledgeCheck;
    public Transform ledgeEmptyCheck;
    public float ledgeCheckDistance = 0.4f;

    [Header("Ledge Snap Offsets")]
    public float xSnapOffset = 0.05f;
    public float ySnapOffset = 0.6f;

    private bool isHanging = false;

    [Header("Air Smash Settings")]
    public float slamSpeed = 25f;
    public float slamHangingTime = 0.2f;
    private bool isSlamming = false;

    [Header("Stun Settings")]
    public bool isStunned = false;
    private float stunTimer = 3f;

    [Header("Roll Settings")]
    public float rollSpeed = 8f;

    [Header("Collider Settings")]
    public float crouchHeightPercent = 0.5f;
    private BoxCollider2D col;
    private Vector2 standbySize;
    private Vector2 standbyOffset;

    [Header("Climb Settings")]
    public float climbSpeed = 5f;
    private bool isClimbing = false;
    private bool canClimb = false;
    private float verticalInput;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private PlayerHealth health;
    private PlayerUltimate ultimate;

    private float moveInput;
    private bool isGrounded;
    private bool facingRight = true;
    private bool isAttacking = false;
    private bool isCrouching = false;
    private float originalGravity;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<PlayerHealth>();
        ultimate = GetComponent<PlayerUltimate>();
        originalGravity = rb.gravityScale;

        col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            standbySize = col.size;
            standbyOffset = col.offset;
        }
    }

    void Update()
    {
        // --- STUN LOGIC ---
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            moveInput = 0;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            UpdateAnimationStates();

            if (stunTimer <= 0)
            {
                isStunned = false;
                if (sr != null) sr.color = Color.white;
            }
            return;
        }

        if (ultimate != null && ultimate.IsPraying())
        {
            moveInput = 0;
            UpdateAnimationStates();
            return;
        }

        if (IsActionsLocked()) return;

        moveInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        isAttacking = anim.GetInteger("AttackIndex") != 0 || anim.GetCurrentAnimatorStateInfo(0).IsName("Player_Fireball_Attack");

        // --- CLIMBING LOGIC ---
        if (canClimb && Mathf.Abs(verticalInput) > 0.1f)
        {
            isClimbing = true;
        }

        // FIX: If we touch ANY ground layer (platforms included), we stop the climbing state immediately.
        if (isGrounded && isClimbing)
        {
            StopClimbing();
        }

        if (isClimbing)
        {
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(0, verticalInput * climbSpeed * currentSlowMultiplier);
        }

        if (!isGrounded && Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.J))
        {
            if (!isSlamming) StartCoroutine(PerformAirSmash());
        }

        HandleCrouchLogic();

        // --- JUMP LOGIC ---
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || isClimbing) && !isAttacking && !isSlamming && !isCrouching)
        {
            // Explicitly kill climbing before jumping so the physics and animation reset
            if (isClimbing)
            {
                StopClimbing();
            }
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        UpdateAnimationStates();

        if (!isAttacking && !isSlamming && !isCrouching)
            HandleFlip();
    }

    // Central method to ensure gravity and animation are ALWAYS reset when leaving a ladder
    void StopClimbing()
    {
        isClimbing = false;
        rb.gravityScale = originalGravity;

        if (anim != null)
        {
            anim.SetBool("IsClimbing", false);
            anim.SetFloat("ClimbVerticalSpeed", 0);
        }
    }

    void HandleCrouchLogic()
    {
        if (Input.GetKey(KeyCode.S) && isGrounded && !isSlamming)
        {
            isCrouching = true;
            if (col != null)
            {
                float newHeight = standbySize.y * crouchHeightPercent;
                col.size = new Vector2(standbySize.x, newHeight);
                col.offset = new Vector2(standbyOffset.x, newHeight / 2f);
            }
        }
        else
        {
            isCrouching = false;
            if (col != null)
            {
                col.size = standbySize;
                col.offset = standbyOffset;
            }
        }
    }

    void HandleLedgeInput()
    {
        rb.linearVelocity = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.S))
        {
            isHanging = false;
            rb.gravityScale = originalGravity;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isHanging = false;
            rb.gravityScale = originalGravity;
            float jumpDirection = facingRight ? 1 : -1;
            rb.linearVelocity = new Vector2(jumpDirection * (moveSpeed * 0.8f), jumpForce);
            anim.SetBool("IsHanging", false);
        }
        UpdateAnimationStates();
    }

    void CheckForLedge()
    {
        if (ledgeCheck == null || ledgeEmptyCheck == null) return;

        Vector2 direction = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallHit = Physics2D.Raycast(ledgeCheck.position, direction, ledgeCheckDistance, groundLayer);
        RaycastHit2D emptyHit = Physics2D.Raycast(ledgeEmptyCheck.position, direction, ledgeCheckDistance, groundLayer);

        if (wallHit.collider != null && emptyHit.collider == null)
        {
            isHanging = true;
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;

            float ledgeTopY = wallHit.collider.bounds.max.y;
            float playerHalfWidth = col.bounds.extents.x;

            float snapY = ledgeTopY - ySnapOffset;
            float snapX = wallHit.point.x - (direction.x * (playerHalfWidth - xSnapOffset));

            transform.position = new Vector2(snapX, snapY);
            anim.SetBool("IsHanging", true);
        }
    }

    void FixedUpdate()
    {
        if (ultimate != null && ultimate.IsDashing()) return;

        if (IsActionsLocked())
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (isHanging || isClimbing) return;

        if (isSlamming)
        {
            rb.linearVelocity = new Vector2(0f, -slamSpeed);
            if (isGrounded) FinishSlam();
            return;
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Player_CrouchAttack"))
        {
            float rollDir = facingRight ? 1 : -1;
            rb.linearVelocity = new Vector2(rollDir * rollSpeed, rb.linearVelocity.y);
            return;
        }

        if (isAttacking || isCrouching)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed * currentSlowMultiplier, rb.linearVelocity.y);
        }
    }

    private void UpdateAnimationStates()
    {
        if (anim == null) return;

        if (IsActionsLocked())
        {
            anim.SetBool("IsGrounded", true);
            anim.SetBool("IsCrouching", false);
            anim.SetFloat("Speed", 0);

            if (isHanging)
            {
                isHanging = false;
                rb.gravityScale = originalGravity;
            }
            return;
        }

        anim.SetBool("IsGrounded", isGrounded);
        anim.SetBool("IsCrouching", isCrouching);

        anim.SetBool("IsClimbing", isClimbing);
        anim.SetFloat("ClimbVerticalSpeed", Mathf.Abs(verticalInput));

        float finalSpeed = (isAttacking || isSlamming || isCrouching || isHanging || isClimbing) ? 0 : Mathf.Abs(moveInput);
        anim.SetFloat("Speed", finalSpeed);
    }

    private bool IsActionsLocked()
    {
        return (health != null && health.IsKnockedBack()) ||
                (ultimate != null && (ultimate.IsPraying() || ultimate.IsDashing()));
    }

    private void HandleFlip()
    {
        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    IEnumerator PerformAirSmash()
    {
        isSlamming = true;
        anim.SetTrigger("DownSlam");
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(slamHangingTime);
        if (!isClimbing) rb.gravityScale = originalGravity;
    }

    void FinishSlam()
    {
        isSlamming = false;
        anim.SetTrigger("SlamImpact");
        rb.linearVelocity = Vector2.zero;
    }

    void OnDrawGizmos()
    {
        if (ledgeCheck != null && ledgeEmptyCheck != null)
        {
            Vector3 dir = facingRight ? Vector3.right : Vector3.left;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(ledgeCheck.position, dir * ledgeCheckDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(ledgeEmptyCheck.position, dir * ledgeCheckDistance);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        if (sr != null) sr.color = Color.green;
    }

    public void ApplySlow(float[] data)
    {
        float amount = data[0];
        float duration = data[1];

        StopCoroutine("SlowRoutine");
        StartCoroutine(SlowRoutine(amount, duration));
    }

    private IEnumerator SlowRoutine(float amount, float duration)
    {
        currentSlowMultiplier = amount;
        if (sr != null) sr.color = new Color(0.5f, 0.5f, 1f); // Tint blue/slow

        yield return new WaitForSeconds(duration);

        currentSlowMultiplier = 1f;
        if (sr != null && !isStunned) sr.color = Color.white;
    }

    public void SetCanClimb(bool value)
    {
        canClimb = value;
        // If we leave the trigger (even by walking out), reset state
        if (!value && isClimbing)
        {
            StopClimbing();
        }
    }
}