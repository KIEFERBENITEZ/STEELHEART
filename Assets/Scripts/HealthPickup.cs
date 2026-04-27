using UnityEngine;
using System.Collections;

public class HealthPickup : MonoBehaviour
{
    public int healAmount = 1;
    public float floatSpeed = 2f;
    public float floatHeight = 0.1f;

    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        // Making it "Float"
        float newY = startY + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector2(transform.position.x, newY);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth healthScript = collision.GetComponent<PlayerHealth>();

            // Only proceed if health script exists AND health is not full
            if (healthScript != null && healthScript.currentHealth < healthScript.maxHealth)
            {
                // 1. Give Health
                healthScript.Heal(healAmount);

                // 2. TRIGGER YOUR ANIMATION
                Animator anim = collision.GetComponent<Animator>();
                if (anim != null)
                {
                    // "Heal" must match the name of the Trigger in your Animator window
                    anim.SetTrigger("Heal");
                }

                // 3. Destroy the heart
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Health is full! Heart stays on the ground.");
            }
        }
    }
}