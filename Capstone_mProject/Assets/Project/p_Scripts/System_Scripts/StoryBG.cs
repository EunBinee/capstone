using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class StoryBG : MonoBehaviour
{
    public List<string> storyList;
    public TMP_Text storyText;
    public TMP_Text I_Text;

    public Image bgImg;

    void Start()
    {
        startStory();

    }

    public void startStory()
    {
        bgImg = GetComponent<Image>();
        Color color = bgImg.color;
        color.a = 1;
        bgImg.color = color;
        StartCoroutine(I_Blink_Text());
        StartCoroutine(StoryText());
    }

    void Update()
    {

    }

    IEnumerator StoryText()
    {
        yield return new WaitForSeconds(1.5f);
        int i = 0;
        string writerText = "";
        string sentence = "";
        while (i < storyList.Count)
        {
            if (storyText.text != "")
            {
                writerText = storyText.text;
                int length = storyText.text.Length;
                for (int j = 0; j < length; j++)
                {
                    writerText = writerText.Remove(writerText.Length - 1);
                    storyText.text = writerText;

                    yield return new WaitForSeconds(0.05f);
                }
                yield return new WaitForSeconds(2f);
            }

            storyText.text = "";
            writerText = "";
            sentence = storyList[i];
            for (int j = 0; j < sentence.Length; j++)
            {
                writerText += sentence[j];
                storyText.text = writerText;

                yield return new WaitForSeconds(0.1f);
            }

            storyText.text = storyList[i];
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => Input.anyKeyDown);

            i++;
            yield return null;
        }

        //* 글 지우기
        if (storyText.text != "")
        {
            writerText = storyText.text;
            int length = storyText.text.Length;
            for (int j = 0; j < length; j++)
            {
                writerText = writerText.Remove(writerText.Length - 1);
                storyText.text = writerText;

                yield return new WaitForSeconds(0.05f);
            }
            yield return new WaitForSeconds(2f);
        }

        //* 페이드 아웃

    }

    IEnumerator I_Blink_Text()
    {
        float time = 0;

        while (time < 20)
        {
            time += Time.deltaTime;
            I_Text.text = "  I";
            yield return new WaitForSeconds(0.5f);
            I_Text.text = "  ";
            yield return new WaitForSeconds(0.5f);
            yield return null;
        }
    }





}
