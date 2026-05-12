using UnityEngine;
using Unity.Cinemachine;

public class AreaTeleport : MonoBehaviour
{
    public CinemachineCamera vcam;
    public Transform cameraTarget;
    public Transform playerSpawnPoint;

    [Header("Gate Settings")]
    public string enemyTag = "Enemy";
    public float detectionRadius = 20f; // Only check for enemies near this portal

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Find all enemies in the WHOLE scene
            GameObject[] allEnemies = GameObject.FindGameObjectsWithTag(enemyTag);
            bool enemiesNearby = false;

            // 2. Only block the portal if the enemies are close to THIS portal
            foreach (GameObject enemy in allEnemies)
            {
                if (Vector2.Distance(transform.position, enemy.transform.position) < detectionRadius)
                {
                    enemiesNearby = true;
                    break;
                }
            }

            if (enemiesNearby)
            {
                Debug.Log("Portal Locked! Clear the enemies in this area first.");
                return;
            }

            // 3. Teleport Logic
            TeleportPlayer(other.transform);
        }
    }

    void TeleportPlayer(Transform player)
    {
        // Move Camera
        if (vcam != null && cameraTarget != null)
            vcam.Follow = cameraTarget;

        // Move Player
        player.position = playerSpawnPoint.position;

        // Stop momentum
        if (player.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.linearVelocity = Vector2.zero;
        }

        Debug.Log("Area Clear! Moving to next section.");
    }

    // Visualizes the detection range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}