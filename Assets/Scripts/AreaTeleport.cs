using UnityEngine;
using Unity.Cinemachine;

public class AreaTeleport : MonoBehaviour
{
    public CinemachineCamera vcam;      // Drag your CinemachineCamera here
    public Transform cameraTarget;     // Drag Point2 here
    public Transform playerSpawnPoint; // Drag Area2_Spawn here

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the thing touching the portal is the Player
        if (other.CompareTag("Player"))
        {
            // 1. Switch the camera's focus to the new area
            vcam.Follow = cameraTarget;

            // 2. Physically move the player to the new area
            other.transform.position = playerSpawnPoint.position;

            // 3. Reset velocity so they don't carry speed from the jump/run
            if (other.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}