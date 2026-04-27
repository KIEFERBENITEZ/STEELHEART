using UnityEngine;

public class PortalPulse : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;
    Vector3 startScale;

    void Start()
    {
        startScale = transform.localScale;
    }

    void Update()
    {
        // This math makes the scale go up and down like a wave
        float scaleOffset = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = startScale + new Vector3(scaleOffset, scaleOffset, 0);
    }
}