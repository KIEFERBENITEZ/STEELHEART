using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum MovementType { LeftRight, UpDown }
    public enum DirectionMode { PositiveFirst, NegativeFirst }

    [Header("Movement Settings")]
    public MovementType movementType = MovementType.LeftRight;
    public DirectionMode startDirection = DirectionMode.PositiveFirst;
    public float speed = 3f;
    public float range = 5f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Multiplier: PositiveFirst = 1, NegativeFirst = -1
        float dirMultiplier = (startDirection == DirectionMode.PositiveFirst) ? 1f : -1f;

        // MATH: Creates a smooth back-and-forth oscillation
        float movement = Mathf.Sin(Time.time * speed) * range * dirMultiplier;

        if (movementType == MovementType.LeftRight)
        {
            transform.position = startPos + new Vector3(movement, 0, 0);
        }
        else
        {
            transform.position = startPos + new Vector3(0, movement, 0);
        }
    }

    // --- THE "STICKY" LOGIC ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }

    [ContextMenu("Reset Position")]
    void ResetPos()
    {
        transform.position = Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = Application.isPlaying ? startPos : transform.position;

        if (movementType == MovementType.LeftRight)
            Gizmos.DrawLine(center + Vector3.left * range, center + Vector3.right * range);
        else
            Gizmos.DrawLine(center + Vector3.up * range, center + Vector3.down * range);
    }
}