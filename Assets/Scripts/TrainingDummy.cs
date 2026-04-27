using UnityEngine;

public class TrainingDummy : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [Header("Visual Feedback")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage)
    {
        // 1. Play the Animation
        if (anim != null)
        {
            anim.SetTrigger("Hit");
        }

        // 2. Start the Flash Effect
        StopAllCoroutines();
        StartCoroutine(FlashRed());

        Debug.Log("Dummy took " + damage + " damage and wobbled!");
    }

    System.Collections.IEnumerator FlashRed()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }
}