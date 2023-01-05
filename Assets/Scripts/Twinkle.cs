using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Twinkle : MonoBehaviour
{
    private float fadeTime = 1.0f;
    private float minFadeTime = 1.0f;
    private float maxFadeTime = 2.0f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        fadeTime = Random.Range(minFadeTime, maxFadeTime);

        StartCoroutine("TwinkleLoop");
    }

    private IEnumerator TwinkleLoop()
    {
        while (true)
        {
            //Alpha ���� 1���� 0����: Fade Out
            yield return StartCoroutine(FadeEffect(1, 0));
            //Alpha ���� 0���� 1����: Fade In
            yield return StartCoroutine(FadeEffect(0, 1));
        }
    }

    private IEnumerator FadeEffect(float start, float end)
    {
        float currentTime = 0.0f;
        float percent = 0.0f;

        while(percent < 1)
        {
            //fadeTime �ð����� while() �ݺ��� ����
            currentTime += Time.deltaTime;
            percent = currentTime / fadeTime;

            //percent�� 0 -> 1�� �����ϰ� �� ���� ���� Alpha ���� ��ȭ��Ŵ
            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(start, end, percent);
            spriteRenderer.color = color;

            yield return null;
        }
    }
}
