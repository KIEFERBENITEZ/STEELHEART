using UnityEngine;

public class BobbingUI : MonoBehaviour
{
    public float bobSpeed = 3f;      // How fast it moves
    public float bobAmount = 0.1f;   // How high it goes

    private Vector3 startPos;

    void Start()
    {
        // Remember where we started so we don't drift away
        startPos = transform.position;
    }

    void Update()
    {
        // Calculate the new Y position using a Sine wave
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;

        // Apply it
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}