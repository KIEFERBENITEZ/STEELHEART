using UnityEngine;
using System.Collections;

public class DisappearingBlock : MonoBehaviour
{
    [Header("Timings")]
    public float activeTime = 3.0f;     // Stays solid for 3 seconds after touch
    public float respawnTime = 3.0f;    // Waits 3 seconds before coming back
    public float blinkStartTime = 1.0f; // Starts blinking when 1 second remains

    private SpriteRenderer sr;
    private BoxCollider2D col;
    private bool isSteppedOn = false;
    private Color originalColor;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        if (sr != null)
            originalColor = sr.color;
    }

    // This detects when the player's feet touch the block
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. Check if the object has the "Player" Tag
        // 2. Check if we aren't already counting down
        if (collision.gameObject.CompareTag("Player") && !isSteppedOn)
        {
            StartCoroutine(StartDisappearing());
        }
    }

    IEnumerator StartDisappearing()
    {
        isSteppedOn = true;

        // PHASE 1: Wait until it's time to start blinking
        // (If activeTime is 3 and blink is 1, it stays solid for 2 seconds first)
        yield return new WaitForSeconds(activeTime - blinkStartTime);

        // PHASE 2: Blink Warning
        float elapsed = 0;
        while (elapsed < blinkStartTime)
        {
            sr.enabled = !sr.enabled; // Toggle visibility
            yield return new WaitForSeconds(0.1f); // Blink speed
            elapsed += 0.1f;
        }

        // PHASE 3: Disappear
        sr.enabled = false;
        col.enabled = false;

        // PHASE 4: Respawn Cooldown
        yield return new WaitForSeconds(respawnTime);

        // PHASE 5: Reset for the next time
        sr.enabled = true;
        col.enabled = true;
        sr.color = originalColor;
        isSteppedOn = false;
    }
}