using UnityEngine;

public class BetterJump : MonoBehaviour
{
    public float fallMultiplier = 2.5f; // Gravity multiplier when falling
    public float lowJumpMultiplier = 2f; // Gravity multiplier when you let go of jump early

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // If we are falling (velocity.y < 0)
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = fallMultiplier;
        }
        // If we are jumping UP but NOT holding the jump button
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.gravityScale = lowJumpMultiplier;
        }
        // Normal gravity while rising or grounded
        else
        {
            rb.gravityScale = 1f;
        }
    }
}