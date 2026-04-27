using UnityEngine;

public class TutorialKey : MonoBehaviour
{
    public KeyCode keyToPress; // Set this to W, A, S, or D in the Inspector
    public float bobSpeed = 3f;
    public float bobAmount = 0.1f;

    private Vector3 startPos;
    private SpriteRenderer sr;
    private bool isPressed = false;

    void Start()
    {
        startPos = transform.position;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. Make it bob up and down
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 2. Check if player hit the key
        if (Input.GetKeyDown(keyToPress))
        {
            isPressed = true;
        }

        // 3. Fade out and destroy
        if (isPressed)
        {
            Color c = sr.color;
            c.a -= Time.deltaTime * 4f; // Fades out quickly
            sr.color = c;

            if (c.a <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}