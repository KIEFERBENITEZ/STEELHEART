using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFlash : MonoBehaviour
{
    public Image flashImage;

    public IEnumerator BlinkSequence()
    {
        // 1. Flash to White
        flashImage.color = Color.white;
        yield return SetAlpha(1f);
        yield return new WaitForSeconds(0.05f);

        // 2. Flash to Black
        flashImage.color = Color.black;
        yield return SetAlpha(1f);
        yield return new WaitForSeconds(0.05f);

        // 3. Fade out to transparent
        yield return SetAlpha(0f);
    }

    private IEnumerator SetAlpha(float targetAlpha)
    {
        float duration = 0.1f; // How fast the blink is
        float startAlpha = flashImage.color.a;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Color c = flashImage.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            flashImage.color = c;
            yield return null;
        }
    }
}