using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class StoryBG : MonoBehaviour
{

    public List<string> storyList;
    public TMP_Text preStory_Text;
    public TMP_Text storyText;
    public TMP_Text I_Text;

    public Image bgImg;

    bool ing_story = false;

    void Start()
    {
        startStory();

    }

    //*처음 불러올때 이거 무조건 쓰기
    public void startStory()
    {
        //*  플레이어 정지
        GameManager.instance.StopGame();

        bgImg = GetComponent<Image>();

        //* 컬러 제대로 다시 제정비

        Color color = bgImg.color;
        color.a = 1;
        bgImg.color = color;

        color = preStory_Text.color;
        color.a = 1;
        preStory_Text.color = color;

        color = storyText.color;
        color.a = 1;
        storyText.color = color;

        color = I_Text.color;
        color.a = 1;
        I_Text.color = color;


        ing_story = true;
        StartCoroutine(I_Blink_Text());
        StartCoroutine(StoryText());
    }


    IEnumerator StoryText()
    {
        yield return new WaitForSeconds(1.5f);
        int i = 0;
        string writerText = "";
        string sentence = "";
        string preSentence = "";
        while (i < storyList.Count)
        {
            if (storyText.text != "")
            {
                if (preSentence != "")
                {
                    preSentence += "\n";
                }
                preSentence += storyText.text;
                preStory_Text.text = preSentence;
            }

            storyText.text = "";
            writerText = "";
            sentence = storyList[i];
            for (int j = 0; j < sentence.Length; j++)
            {
                writerText += sentence[j];
                storyText.text = writerText;

                yield return new WaitForSeconds(0.05f);
            }

            storyText.text = storyList[i];
            yield return new WaitForSeconds(0.5f);
            yield return new WaitUntil(() => Input.anyKeyDown);

            i++;
            yield return null;
        }


        ing_story = false;

        //* 페이드 아웃

        float duration = 1.0f;
        // 초기 알파 값 설정
        float alpha = 1;

        // 페이드 아웃 루프
        while (alpha > 0)
        {
            // 알파 값 계산
            alpha -= Time.deltaTime / duration;

            Color color = bgImg.color;
            color.a = alpha;
            bgImg.color = color;

            color = preStory_Text.color;
            color.a = alpha;
            preStory_Text.color = color;

            color = storyText.color;
            color.a = alpha;
            storyText.color = color;

            color = I_Text.color;
            color.a = alpha;
            I_Text.color = color;

            yield return null;
        }

        GameManager.instance.StartGame();
        this.gameObject.SetActive(false);
    }



    IEnumerator I_Blink_Text()
    {
        while (ing_story)
        {
            I_Text.text = " I";
            yield return new WaitForSeconds(0.5f);
            I_Text.text = " ";
            yield return new WaitForSeconds(0.5f);
            yield return null;
        }
    }





}
