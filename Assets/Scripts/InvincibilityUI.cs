using UnityEngine;
using TMPro;

public class InvincibilityUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public TextMeshProUGUI invincibleText;


    void Update()
    {
        if (playerHealth == null || invincibleText == null) return;

        if (playerHealth.IsInvincible())
        {
            invincibleText.gameObject.SetActive(true);

            float t = Mathf.PingPong(Time.time * 5f, 1f);
            invincibleText.alpha = Mathf.Lerp(0.2f, 1f, t);
        }
        else
        {
            invincibleText.gameObject.SetActive(false);
        }
    }
}