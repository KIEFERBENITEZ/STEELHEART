using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerUltimate : MonoBehaviour
{
    [Header("Prayer / Charge")]
    public float maxCharge = 100f;
    public float currentCharge = 0f;
    public float chargeTime = 5f;
    public KeyCode prayKey = KeyCode.P;

    [Header("Ultimate Dash")]
    public KeyCode ultimateKey = KeyCode.K;
    public float dashForce = 30f;
    public float dashDuration = 0.5f;

    [Header("Dash Damage")]
    public int dashDamage = 3;
    public LayerMask enemyLayer;
    public float dashHitRadius = 0.7f;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Color normalColor = Color.white;
    public Color chargingColor = Color.cyan;

    [Header("UI")]
    public Slider chargeSlider;

    private Rigidbody2D rb;
    private Animator anim;
    private PlayerHealth health;

    private bool isPraying = false;
    private bool isDashing = false;
    private bool facingRight = true;

    private float dashTimer = 0f;
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        health = GetComponent<PlayerHealth>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (chargeSlider != null)
        {
            chargeSlider.maxValue = maxCharge;
            chargeSlider.value = currentCharge;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }

    void Update()
    {
        if (health != null && health.IsDead())
        {
            UpdateAnimator();
            UpdateChargeUI();
            UpdateGlow();
            return;
        }

        HandlePrayerStart();
        HandleCharging();
        HandleUltimateInput();
        UpdateFacingDirection();
        UpdateAnimator();
        UpdateChargeUI();
        UpdateGlow();
    }

    void FixedUpdate()
    {
        if (health != null && health.IsDead())
        {
            return;
        }

        if (isDashing)
        {
            dashTimer -= Time.fixedDeltaTime;

            // Calculate direction
            float dir = facingRight ? 1f : -1f;

            // APPLY VELOCITY: We set Y to 0 so the player "hovers" slightly while dashing
            rb.linearVelocity = new Vector2(dir * dashForce, 0f);

            // --- DAMAGE LOGIC ---
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, dashHitRadius, enemyLayer);

            foreach (Collider2D enemy in hits)
            {
                if (!hitEnemies.Contains(enemy.gameObject))
                {
                    hitEnemies.Add(enemy.gameObject);

                    EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        Vector2 ultimateKnockback = new Vector2(dir * 20f, 2f);
                        enemyHealth.TakeDamage(dashDamage, ultimateKnockback);
                    }
                }
            }

            // --- END DASH ---
            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }
    }

    void HandlePrayerStart()
    {
        if (Input.GetKeyDown(prayKey) && !isPraying && !isDashing && currentCharge < maxCharge)
        {
            isPraying = true;

            if (anim != null)
            {
                anim.SetTrigger("StartPray");
            }
        }
    }

    void HandleCharging()
    {
        if (!isPraying)
            return;

        currentCharge += (maxCharge / chargeTime) * Time.deltaTime;
        currentCharge = Mathf.Clamp(currentCharge, 0f, maxCharge);

        if (currentCharge >= maxCharge)
        {
            currentCharge = maxCharge;
            isPraying = false;
            Debug.Log("Charge Complete!");
        }
    }

    void HandleUltimateInput()
    {
        // 1. Don't do anything if the player is dead
        if (health != null && health.IsDead())
            return;

        // 2. Check if: K is pressed, Charge is full, and we aren't already dashing
        if (Input.GetKeyDown(ultimateKey) && currentCharge >= maxCharge && !isDashing)
        {
            Debug.Log("Ultimate Key Pressed! Starting Dash...");

            // 3. Play the animation
            if (anim != null)
            {
                anim.SetTrigger("Ultimate");
            }

            // 4. TRIGGER THE PHYSICS (This is the line that was missing!)
            StartDash();
        }
    }

    void StartDash()
    {
        currentCharge = 0f;
        isPraying = false;
        isDashing = true;
        dashTimer = dashDuration;
        hitEnemies.Clear();

        Debug.Log("DASH STARTED");
    }

    public void StartUltimateDashFromAnimation()
    {
        if (health != null && health.IsDead())
            return;

        StartDash();
    }

    void EndDash()
    {
        isDashing = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void ResetUltimateState()
    {
        isPraying = false;
        isDashing = false;
        currentCharge = 0f;
        dashTimer = 0f;
        hitEnemies.Clear();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (anim != null)
        {
            anim.SetBool("IsPraying", false);
            anim.SetBool("IsDashing", false);
            anim.ResetTrigger("StartPray");
            anim.ResetTrigger("Ultimate");
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        if (chargeSlider != null)
        {
            chargeSlider.value = currentCharge;
        }
    }

    void UpdateFacingDirection()
    {
        if (isPraying || isDashing)
            return;

        float h = Input.GetAxisRaw("Horizontal");

        if (h > 0f)
            facingRight = true;
        else if (h < 0f)
            facingRight = false;
    }

    void UpdateAnimator()
    {
        if (anim == null)
            return;

        anim.SetBool("IsPraying", isPraying);
        anim.SetBool("IsDashing", isDashing);
    }

    void UpdateChargeUI()
    {
        if (chargeSlider != null)
        {
            chargeSlider.value = currentCharge;
        }
    }

    void UpdateGlow()
    {
        if (spriteRenderer == null)
            return;

        if (isPraying)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, chargingColor, Time.deltaTime * 8f);
        }
        else
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, normalColor, Time.deltaTime * 8f);
        }
    }

    public bool IsPraying()
    {
        return isPraying;
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    public float GetChargePercent()
    {
        return currentCharge / maxCharge;
    }

    public bool IsUltimateReady()
    {
        return currentCharge >= maxCharge;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashHitRadius);
    }
}