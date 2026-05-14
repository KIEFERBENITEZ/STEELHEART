using UnityEngine;
using UnityEngine.UI;

public class FloatingBossBar : MonoBehaviour
{
    public Slider slider;
    public EnemyHealth healthScript; // Drag Boss object here
    public Vector3 offset = new Vector3(0, 2.5f, 0); // Height above head

    void Start()
    {
        // Link the slider to the boss's health
        slider.maxValue = healthScript.maxHealth;
        slider.value = healthScript.currentHealth;
    }

    void Update()
    {
        if (healthScript == null) return;

        // Update value smoothly
        slider.value = Mathf.Lerp(slider.value, healthScript.currentHealth, Time.deltaTime * 10f);

        // Keep the bar at the right height and facing the camera
        transform.position = healthScript.transform.position + offset;
        transform.rotation = Quaternion.identity;

        // Hide if dead
        if (healthScript.isDead) gameObject.SetActive(false);
    }
}