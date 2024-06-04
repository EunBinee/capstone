using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    Image img;
    Color initColor;

    Coroutine fadeIn_co = null;
    Coroutine fadeOut_co = null;

    public bool finishFadeIn = false;
    public bool finishFadeOut = false;

    void Start()
    {
        img = GetComponent<Image>();
        initColor = img.color;
    }
    public void FadeIn()
    {
        if (fadeIn_co == null)
            fadeIn_co = StartCoroutine(FadeIn_co());
    }

    IEnumerator FadeIn_co()
    {
        if (img == null)
            img = GetComponent<Image>();
        Color color = img.color;
        if (color.a != 0)
            color.a = 0;
        img.color = color;


        float duration = 1.0f;
        // 초기 알파 값 설정
        float alpha = 0;

        // 페이드 아웃 루프
        while (alpha < 1)
        {
            // 알파 값 계산
            alpha += Time.deltaTime / duration;

            color = img.color;
            color.a = alpha;
            img.color = color;

            yield return null;
        }

        color = img.color;
        color.a = 1;
        img.color = color;
        finishFadeIn = true;
        fadeIn_co = null;
    }


    public void FadeOut()
    {
        if (fadeOut_co == null)
            fadeOut_co = StartCoroutine(FadeOut_co());
    }

    IEnumerator FadeOut_co()
    {
        if (img == null)
            img = GetComponent<Image>();
        Color color = img.color;
        if (color.a != 1)
            color.a = 1;
        img.color = color;


        float duration = 1.0f;
        // 초기 알파 값 설정
        float alpha = 1;

        // 페이드 아웃 루프
        while (alpha > 0)
        {
            // 알파 값 계산
            alpha -= Time.deltaTime / duration;

            color = img.color;
            color.a = alpha;
            img.color = color;

            yield return null;
        }

        color = img.color;
        color.a = 0;
        img.color = color;

        finishFadeOut = true;
        this.gameObject.SetActive(false);
        fadeOut_co = null;
    }
}
