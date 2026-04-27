using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public float parallaxAmount; // 0 = moves with camera, 1 = stays still
    private Transform cam;
    private Vector3 lastCameraPosition;

    void Start()
    {
        cam = Camera.main.transform;
        lastCameraPosition = cam.position;
    }

    void LateUpdate() // Use LateUpdate so it moves AFTER the camera moves
    {
        Vector3 deltaMovement = cam.position - lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxAmount, 0, 0);
        lastCameraPosition = cam.position;
    }
}