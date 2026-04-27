using UnityEngine;

public class FireballPickup : MonoBehaviour
{
    [Header("Movement Settings")]
    public float floatSpeed = 3f;      // How fast it bobs
    public float floatHeight = 0.5f;   // How far it travels (Up/Down)

    [Header("Ammo Settings")]
    public int ammoAmount = 1;

    private Vector3 startPos;

    void Start()
    {
        // Record where the item started so it bobs around THIS point
        startPos = transform.position;
    }

    void Update()
    {
        // MATH: Sin wave creates the smooth up-and-down loop
        // Time.time * floatSpeed = the horizontal progress of the wave
        // Mathf.Sin(...) * floatHeight = the vertical peak of the wave
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        // Apply the wave to the starting position
        transform.position = startPos + new Vector3(0, newY, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerAttack player = other.GetComponent<PlayerAttack>();
            if (player != null && player.currentFireballs < player.maxFireballs)
            {
                player.currentFireballs = Mathf.Min(player.currentFireballs + ammoAmount, player.maxFireballs);
                player.OnFireballChanged?.Invoke(player.currentFireballs);
                Destroy(gameObject);
            }
        }
    }
}