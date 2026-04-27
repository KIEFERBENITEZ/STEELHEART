using UnityEngine;
using System.Collections;

public class EnemyGate : MonoBehaviour
{
    [Header("Settings")]
    public string enemyTag = "Enemy";
    public float checkInterval = 0.5f;
    public float fadeSpeed = 1.5f; // How fast it vanishes

    private SpriteRenderer sr;
    private BoxCollider2D col;
    private bool isOpening = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        // Start checking for enemies
        InvokeRepeating("CheckForEnemies", 1f, checkInterval);
    }

    void CheckForEnemies()
    {
        if (isOpening) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        if (enemies.Length == 0)
        {
            StartCoroutine(FadeAndDestroy());
        }
    }

    IEnumerator FadeAndDestroy()
    {
        isOpening = true;
        CancelInvoke("CheckForEnemies");

        // Disable the collider immediately so the player can walk through
        if (col != null) col.enabled = false;

        Color objectColor = sr.color;

        // Loop until the alpha (A) is 0
        while (objectColor.a > 0)
        {
            float fadeAmount = objectColor.a - (fadeSpeed * Time.deltaTime);
            objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
            sr.color = objectColor;
            yield return null; // Wait for the next frame
        }

        Debug.Log("Gate vanished!");
        Destroy(gameObject);
    }
}