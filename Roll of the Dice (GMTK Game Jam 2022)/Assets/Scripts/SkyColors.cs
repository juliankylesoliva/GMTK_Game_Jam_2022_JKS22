using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyColors : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    [SerializeField] Color[] skyColors;

    private bool isChanging = false;

    void Awake()
    {
        spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
    }

    public void ChangeSkyColor(int index)
    {
        StartCoroutine(LerpColorChange(spriteRenderer.color, skyColors[index]));
    }

    private IEnumerator LerpColorChange(Color startColor , Color endColor)
    {
        while (isChanging)
        {
            yield return null;
        }

        isChanging = true;
        float lerpValue = 0f;

        spriteRenderer.color = startColor;
        while (lerpValue <= 1f)
        {
            lerpValue += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, endColor, lerpValue);
            yield return null;
        }
        spriteRenderer.color = endColor;

        isChanging = false;
        yield return null;
    }
}
