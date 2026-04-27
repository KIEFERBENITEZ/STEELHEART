using UnityEngine;
using System; // Required for Action

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public int attackDamage = 1;

    [Header("Combo Settings")]
    public int maxCombo = 5;
    public float comboAttackSpeed = 0.2f;
    public float normalAttackSpeed = 0.5f;
    public float comboCooldown = 8f;
    public float comboResetTime = 1f;

    [Header("Fireball Skill")]
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireballCooldown = 1.0f;
    private float nextFireTime = 0f;

    [Header("Fireball Ammo")]
    public int currentFireballs = 5;
    public int maxFireballs = 5;
    // This event tells the UI to update whenever ammo changes
    public Action<int> OnFireballChanged;
    public int crouchAttackDamage = 1; // Set this to whatever you want
    public float crouchAttackCooldown = 1.5f; // Adjust this in the Inspector
    private float nextCrouchAttackTime = 0f;

    private int comboCounter = 0;
    private float nextAttackTime = 0f;
    private float cooldownEndTime = 0f;
    private float lastAttackTime = 0f;
    private bool inCooldown = false;
    private bool attackBuffered = false;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Initialize UI at start
        OnFireballChanged?.Invoke(currentFireballs);
    }

    void Update()
    {
        TryFinishCooldown();
        TryResetCombo();

        // Standard Attack Input
        if (Input.GetKeyDown(KeyCode.J))
        {
            attackBuffered = true;
        }

        // Fireball Attack Input
        if (Input.GetKeyDown(KeyCode.O) && Time.time >= nextFireTime)
        {
            if (currentFireballs > 0)
            {
                LaunchFireball();
            }
            else
            {
                Debug.Log("Out of Fireballs!");
            }
        }

        // EMERGENCY UNLOCK
        if (!attackBuffered && Time.time - lastAttackTime > 0.6f)
        {
            if (anim != null && anim.GetInteger("AttackIndex") != 0)
            {
                ResetAttackIndex();
                Debug.Log("Forced Reset: Animation Event missed.");
            }
        }

        if (attackBuffered && Time.time >= nextAttackTime)
        {
            HandleAttack();
            attackBuffered = false;
        }
    }

    void LaunchFireball()
    {
        if (anim == null) return;

        // Reduce ammo and update UI immediately
        currentFireballs--;
        OnFireballChanged?.Invoke(currentFireballs);

        anim.ResetTrigger("FireAttack");
        anim.SetTrigger("FireAttack");

        nextFireTime = Time.time + fireballCooldown;
    }

    // CALLED BY ANIMATION EVENT
    // THIS IS THE UPDATED FUNCTION IN PlayerAttack.cs
    public void CreateFireballInstance()
    {
        if (fireballPrefab != null && firePoint != null)
        {
            // 1. Instantiate the fireball
            GameObject ball = Instantiate(fireballPrefab, firePoint.position, firePoint.rotation);

            // 2. Get the Fireball script from the prefab
            Fireball ballScript = ball.GetComponent<Fireball>();

            if (ballScript != null)
            {
                // 3. Determine direction: 
                // We check the Player's scale. If localScale.x is positive, direction is 1.
                // If localScale.x is negative, direction is -1.
                float dir = transform.localScale.x > 0 ? 1f : -1f;

                // 4. Send that direction to the Fireball script!
                ballScript.SetDirection(dir);
            }
        }
    }

    void HandleAttack()
    {
        if (anim == null) return;

        lastAttackTime = Time.time;

        if (anim.GetBool("IsCrouching"))
        {
            // Check if the cooldown has passed
            if (Time.time >= nextCrouchAttackTime)
            {
                DoCrouchAttack();
            }
            else
            {
                Debug.Log("Crouch Attack is on Cooldown!");
            }
            return;
        }

        if (inCooldown)
        {
            DoCooldownAttack();
        }
        else
        {
            DoComboAttack();
        }
    }

    void DoCrouchAttack()
    {
        comboCounter = 0;

        // Set the next time the player can use this move
        nextCrouchAttackTime = Time.time + crouchAttackCooldown;

        PlayAttackAnimation(1);
        nextAttackTime = Time.time + normalAttackSpeed;
    }

    void DoComboAttack()
    {
        comboCounter++;
        if (comboCounter > maxCombo) comboCounter = 1;
        PlayAttackAnimation(comboCounter);

        if (comboCounter >= maxCombo)
        {
            StartComboCooldown();
        }
        else
        {
            nextAttackTime = Time.time + comboAttackSpeed;
        }
    }

    void DoCooldownAttack()
    {
        comboCounter = 0;
        PlayAttackAnimation(1);
        nextAttackTime = Time.time + normalAttackSpeed;
    }

    void StartComboCooldown()
    {
        inCooldown = true;
        cooldownEndTime = Time.time + comboCooldown;
        comboCounter = 0;
        nextAttackTime = Time.time + normalAttackSpeed;
    }

    void TryFinishCooldown()
    {
        if (inCooldown && Time.time >= cooldownEndTime)
        {
            inCooldown = false;
        }
    }

    void TryResetCombo()
    {
        if (!inCooldown && comboCounter > 0 && Time.time - lastAttackTime > comboResetTime)
        {
            comboCounter = 0;
            ResetAttackIndex();
        }
    }

    void PlayAttackAnimation(int attackIndex)
    {
        if (anim != null)
        {
            anim.SetInteger("AttackIndex", attackIndex);
        }
    }

    public void ResetAttackIndex()
    {
        if (anim != null)
        {
            anim.SetInteger("AttackIndex", 0);
        }
    }

    public void DoAttackDamage()
    {
        if (attackPoint == null) return;

        // This checks if we are crouching:
        // If true, it uses 'crouchAttackDamage'. If false, it uses 'attackDamage'.
        // (Make sure to add 'public int crouchAttackDamage = 2;' at the top of your script!)
        int finalDamage = (anim != null && anim.GetBool("IsCrouching")) ? crouchAttackDamage : attackDamage;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                // Now passing 'finalDamage' instead of 'attackDamage'
                enemyHealth.TakeDamage(finalDamage, knockbackDir * 2f);
                continue;
            }

            TrainingDummy dummy = enemy.GetComponent<TrainingDummy>();
            if (dummy != null)
            {
                // Now passing 'finalDamage' instead of 'attackDamage'
                dummy.TakeDamage(finalDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}