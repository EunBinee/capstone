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

    string yellow_Color = "<#FFD559>";
    string murkyYellow_Color = "<#A4CD8D>";

    public bool isStart = false;
    void Start()
    {
        isStart = false;
        //startStory();

    }

    void Update()
    {
        if (!isStart)
            startStory();
    }

    //*처음 불러올때 이거 무조건 쓰기
    public void startStory()
    {
        isStart = true;
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
        yield return new WaitUntil(() => Input.anyKeyDown);
        int i = 0;
        string writerText = "";
        string sentence = "";
        string preSentence = "";
        while (i < storyList.Count)
        {
            storyList[i] = storyList[i].Replace("\\n", "\n");


            if (storyText.text != "")
            {
                if (preSentence != "")
                {
                    preSentence += "\n";
                }
                string changeString = storyText.text.Replace(yellow_Color, murkyYellow_Color);
                preSentence += changeString;
                preStory_Text.text = preSentence;
            }

            storyText.text = "";
            writerText = "";

            string[] divideSentence = DivideSentence(storyList[i]);
            for (int j = 0; j < divideSentence.Length; j++)
            {
                writerText += divideSentence[j];
                storyText.text = writerText;
                yield return new WaitForSeconds(0.05f);
            }
            // sentence = storyList[i];
            // for (int j = 0; j < sentence.Length; j++)
            // {
            //     writerText += sentence[j];
            //     storyText.text = writerText;
            //
            //     yield return new WaitForSeconds(0.05f);
            // }

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






    string[] DivideSentence(string sentence)
    {
        //* 리치 텍스트에 맞게 분리된 배열 리턴
        //*------------------------------------------//
        string bold = "<b>";
        string endBold = "</b>";
        string yellowColor = yellow_Color;
        string endYellowColor = "</color>";
        //*------------------------------------------//
        List<string> textList = new List<string>();
        List<string> reachTextStack = new List<string>();
        string[] words = sentence.Split(' ');


        bool have_Bold = false;
        int bold_Index = 0;
        bool have_endBold = false;
        int endBold_Index = 0;

        bool have_color_yellow = false;
        int color_yellow_Index = 0;
        bool have_endColor_yellow = false;
        int endcolor_yellow_Index = 0;

        foreach (string word in words)
        {
            bool newLine = false;

            //* 리치 텍스트의 순서 찾는 ---------------------------------------//
            List<string> checkOrder = new List<string>();
            List<int> indexList = new List<int>();
            //* bold 리치 텍스트
            if (word.Contains(bold))
            {
                have_Bold = true;
                bold_Index = word.IndexOf(bold);
                indexList.Add(bold_Index);
            }
            if (word.Contains(endBold))
            {
                have_endBold = true;
                endBold_Index = word.IndexOf(endBold);
                indexList.Add(endBold_Index);
            }
            //*color red 리치 텍스트
            if (word.Contains(yellowColor))
            {
                have_color_yellow = true;
                color_yellow_Index = word.IndexOf(yellowColor);
                indexList.Add(color_yellow_Index);
            }
            if (word.Contains(endYellowColor))
            {
                have_endColor_yellow = true;
                endcolor_yellow_Index = word.IndexOf(endYellowColor);
                indexList.Add(endcolor_yellow_Index);
            }

            indexList.Sort();

            foreach (int index in indexList)
            {
                if (have_Bold)
                {
                    if (bold_Index == index)
                    {
                        checkOrder.Add(bold);
                    }
                }
                if (have_endBold)
                {
                    if (endBold_Index == index)
                    {
                        checkOrder.Add(endBold);
                    }
                }
                if (have_color_yellow)
                {
                    if (color_yellow_Index == index)
                    {
                        checkOrder.Add(yellowColor);
                    }
                }
                if (have_endColor_yellow)
                {
                    if (endcolor_yellow_Index == index)
                    {
                        checkOrder.Add(endYellowColor);
                    }
                }

            }
            //* ---------------------------------------//


            for (int i = 0; i < word.Length; i++)
            {
                while (checkOrder.Count != 0)
                {
                    if (checkOrder[0] == bold)
                    {
                        if (bold_Index == i)
                        {
                            i += bold.Length;
                            reachTextStack.Add(bold);
                            have_Bold = false;
                            checkOrder.RemoveAt(0);
                        }
                        else
                            break;

                        if (checkOrder.Count == 0)
                            break;
                    }
                    if (checkOrder[0] == endBold)
                    {
                        if (endBold_Index == i)
                        {
                            i += endBold.Length;
                            reachTextStack.Remove(bold);
                            have_endBold = false;
                            checkOrder.RemoveAt(0);
                        }
                        else
                            break;

                        if (checkOrder.Count == 0)
                            break;
                    }
                    if (checkOrder[0] == yellowColor)
                    {
                        if (color_yellow_Index == i)
                        {
                            i += yellowColor.Length;
                            reachTextStack.Add(yellowColor);
                            have_color_yellow = false;
                            checkOrder.RemoveAt(0);
                        }
                        else
                            break;

                        if (checkOrder.Count == 0)
                            break;
                    }
                    if (checkOrder[0] == endYellowColor)
                    {
                        if (endcolor_yellow_Index == i)
                        {
                            i += endYellowColor.Length;
                            reachTextStack.Remove(yellowColor);
                            have_endColor_yellow = false;
                            checkOrder.RemoveAt(0);
                        }
                        else
                            break;

                        if (checkOrder.Count == 0)
                            break;
                    }
                }
                if (i >= word.Length)
                {
                    break;
                }
                string text = "";
                string endText = "";
                if (reachTextStack.Count > 0)
                {
                    for (int j = 0; j < reachTextStack.Count; j++)
                    {
                        text += reachTextStack[j];
                    }

                    for (int j = reachTextStack.Count - 1; j >= 0; j--)
                    {
                        if (reachTextStack[j] == bold)
                        {
                            endText += endBold;
                        }
                        else if (reachTextStack[j] == yellowColor)
                        {
                            endText += endYellowColor;
                        }
                    }
                }
                text += word[i];
                if (endText != "")
                    text += endText;
                textList.Add(text);
                if (text == "\n")
                {
                    newLine = true;
                }
            }

            if (!newLine)
                textList.Add(" ");
        }

        return textList.ToArray();
    }



}
