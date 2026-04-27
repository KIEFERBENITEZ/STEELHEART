using UnityEngine;
using System.Collections;

public class SpeedBoost : MonoBehaviour
{
    [Header("Boost Settings")]
    public float boostMultiplier = 2f;
    public float duration = 5f;
    public float respawnDelay = 15f;

    [Header("Hover Settings")]
    public float floatAmplitude = 0.2f;
    public float floatFrequency = 3f;

    private Vector3 startPos;
    private bool isPickedUp = false;

    // Use a standard float to avoid the Nullable "Value" error
    private float savedOriginalSpeed;

    private SpriteRenderer itemSprite;
    private Collider2D itemCollider;

    void Start()
    {
        startPos = transform.position;
        itemSprite = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isPickedUp) return;

        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isPickedUp)
        {
            isPickedUp = true;
            StartCoroutine(ApplyBoost(collision.gameObject));
            StartCoroutine(RespawnTimer());

            itemSprite.enabled = false;
            itemCollider.enabled = false;
        }
    }

    IEnumerator ApplyBoost(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();

        if (controller != null && sr != null && health != null)
        {
            // 1. Save speed and apply boost
            savedOriginalSpeed = controller.moveSpeed;
            controller.moveSpeed = savedOriginalSpeed * boostMultiplier;
            health.SetInvincible(true);

            float timer = 0;
            while (timer < duration)
            {
                timer += Time.deltaTime;

                // 2. Force the color to Blue every frame so nothing else changes it
                sr.color = Color.blue;

                yield return null;
            }

            // 3. Reset everything
            if (controller != null)
            {
                controller.moveSpeed = savedOriginalSpeed;
                if (sr != null) sr.color = Color.white;
                if (health != null) health.SetInvincible(false);
            }
        }
    }

    IEnumerator RespawnTimer()
    {
        yield return new WaitForSeconds(respawnDelay);
        itemSprite.enabled = true;
        itemCollider.enabled = true;
        isPickedUp = false;
    }
}