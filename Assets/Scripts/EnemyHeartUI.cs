using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class EnemyHeartUI : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject canvasPrefab; // Drag 'EnemyHealthCanvas' Prefab here
    public GameObject heartPrefab;   // Drag 'HeartIcon' Prefab here

    [Header("Sprites")]
    public Sprite fullHeart;
    public Sprite emptyHeart;

    private EnemyHealth healthScript;
    private Transform heartGroup;
    private List<Image> heartImages = new List<Image>();

    void Start()
    {
        healthScript = GetComponent<EnemyHealth>();

        // 1. Spawn the Canvas as a child of the enemy
        GameObject uiInstance = Instantiate(canvasPrefab, transform);
        uiInstance.transform.localPosition = new Vector3(0, 0.6f, 0);

        // 2. Find the HeartGroup inside the spawned prefab
        heartGroup = uiInstance.transform.Find("HeartGroup");

        // 3. Spawn hearts based on enemy's Max Health
        for (int i = 0; i < healthScript.maxHealth; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartGroup);
            heartImages.Add(newHeart.GetComponent<Image>());
        }
    }

    void Update()
    {
        if (healthScript == null || heartGroup == null) return;

        // 4. Show Full or Empty hearts based on current HP
        for (int i = 0; i < heartImages.Count; i++)
        {
            heartImages[i].sprite = (i < healthScript.currentHealth) ? fullHeart : emptyHeart;
        }

        // 5. IMPORTANT: Keep the UI from flipping when the enemy turns
        heartGroup.parent.rotation = Quaternion.identity;

        if (healthScript.isDead) heartGroup.parent.gameObject.SetActive(false);
    }
}