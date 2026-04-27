using UnityEngine;
using UnityEngine.UI;

public class FireballUIManager : MonoBehaviour
{
    [Header("Setup")]
    public PlayerAttack playerAttack; // Drag player here
    public GameObject[] fireballIcons; // Drag your 5 UI images here

    void Start()
    {
        if (playerAttack != null)
        {
            // Subscribe to the ammo change event
            playerAttack.OnFireballChanged += UpdateUI;

            // Set initial state
            UpdateUI(playerAttack.currentFireballs);
        }
    }

    void UpdateUI(int currentAmmo)
    {
        for (int i = 0; i < fireballIcons.Length; i++)
        {
            // If the icon index is less than our ammo, turn it on
            if (i < currentAmmo)
            {
                fireballIcons[i].SetActive(true);
            }
            else
            {
                fireballIcons[i].SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (playerAttack != null)
            playerAttack.OnFireballChanged -= UpdateUI;
    }
}