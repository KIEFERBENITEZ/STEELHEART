using UnityEngine;

public class WorldArrow : MonoBehaviour
{
    public enum ArrowDirection { Right, Left, Up, Down }
    public ArrowDirection direction;

    void Start()
    {
        UpdateDirection();
    }

    // This runs when you change things in the Inspector!
    void OnValidate()
    {
        UpdateDirection();
    }

    void UpdateDirection()
    {
        switch (direction)
        {
            case ArrowDirection.Right: transform.eulerAngles = new Vector3(0, 0, 0); break;
            case ArrowDirection.Left: transform.eulerAngles = new Vector3(0, 0, 180); break;
            case ArrowDirection.Up: transform.eulerAngles = new Vector3(0, 0, 90); break;
            case ArrowDirection.Down: transform.eulerAngles = new Vector3(0, 0, 270); break;
        }
    }
}