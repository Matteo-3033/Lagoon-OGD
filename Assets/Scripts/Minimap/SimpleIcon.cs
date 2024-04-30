using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleIcon : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private float _startAlpha = 1;

    // Start is called before the first frame update
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _startAlpha = _spriteRenderer.color.a;
    }


    public void MakeOpaque()
    {
        SetAlpha(_startAlpha);
    }

    public void MakeTransparent()
    {
        SetAlpha(0);
    }

    public void SetAlpha(float alpha)
    {
        Color color = _spriteRenderer.color;
        color.a = alpha;
        _spriteRenderer.color = color;
    }

    public void SetVisible(bool visible)
    {
        SetAlpha(visible ? _startAlpha : 0);
    }

    public void FadeOutIcon(float duration)
    {
        StartCoroutine(FadeOutIconCoroutine(duration));
    }

    public void FadeInIcon(float duration)
    {
        StartCoroutine(FadeInIconCoroutine(duration));
    }

    private IEnumerator FadeOutIconCoroutine(float duration)
    {
        float time = 0;

        MakeOpaque();

        while (time < duration)
        {
            SetAlpha(_startAlpha * (1 - time / duration));
            yield return null;

            time += Time.deltaTime;
        }
    }

    private IEnumerator FadeInIconCoroutine(float duration)
    {
        float time = 0;

        MakeTransparent();

        while (time < duration)
        {
            SetAlpha(_startAlpha * (time / duration));
            yield return null;

            time += Time.deltaTime;
        }
    }
}