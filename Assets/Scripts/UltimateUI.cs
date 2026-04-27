using UnityEngine;
using TMPro;

public class UltimateUI : MonoBehaviour
{
    public PlayerUltimate playerUltimate;
    public TextMeshProUGUI ultimateText;

    void Update()
    {
        if (playerUltimate == null || ultimateText == null) return;

        int percent = Mathf.RoundToInt(playerUltimate.GetChargePercent() * 100f);

        if (playerUltimate.IsUltimateReady())
        {
            ultimateText.text = "Ultimate: READY";
        }
        else if (playerUltimate.IsPraying())
        {
            ultimateText.text = "Praying... " + percent + "%";
        }
        else
        {
            ultimateText.text = "Ultimate: " + percent + "%";
        }
    }
}