using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [SerializeField] TMP_Text objectText;


    bool startChat = false;
    public bool stopChat = false;


    DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager = GetComponent<DialogueManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (startChat)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            {
                //Enter키를 누르면 애니메이션 중지하고, 바로 글씨 나오도록 하기 위함.
                stopChat = true;
            }
        }
    }



    //타이핑 애니메이션
    public void Chat_Obect(string sentence)
    {
        startChat = true;
        StartCoroutine(ObjectChat(sentence));
    }

    public static IEnumerator WaitForRealTime(float delay)
    {
        while (true)
        {
            float pauseEndTime = Time.realtimeSinceStartup + delay;
            while (Time.realtimeSinceStartup < pauseEndTime)
            {
                yield return 0;
            }
            break;
        }
    }

    IEnumerator ObjectChat(string sentence)
    {
        string writerText = "";
        bool t_white = false, t_yellow = false;
        bool t_ignore = false;

        for (int i = 0; i < sentence.Length; i++)
        {
            if (stopChat)
            {
                //Enter키를 누르면 애니메이션 중지하고, 바로 글씨 나오도록.

                switch (sentence[i])
                {
                    case 'ⓦ': t_white = true; t_yellow = false; t_ignore = true; break;
                    case 'ⓨ': t_white = false; t_yellow = true; t_ignore = true; break;
                }



                writerText = sentence.Replace("'", ",").Replace("ⓨ", "<color=#ffff00>").Replace("ⓦ", "</color><color=#ffffff>" + "</color>");
                objectText.text = writerText;
                //writerText = sentence.Replace("\\n", "\n");

                //break;

            }
            else
            {
                switch (sentence[i])
                {
                    case 'ⓦ': t_white = true; t_yellow = false; t_ignore = true; break;
                    case 'ⓨ': t_white = false; t_yellow = true; t_ignore = true; break;
                }

                string t_letter = sentence[i].ToString();

                if (!t_ignore)
                {
                    if (t_white) { t_letter = "<color=#ffffff>" + t_letter + "</color>"; }
                    else if (t_yellow) { t_letter = "<color=#ffff00>" + t_letter + "</color>"; }
                    writerText += t_letter;
                }
                //writerText += sentence[i];
                objectText.text = writerText.Replace("'", ",");
                objectText.text = writerText.Replace("\\n", "\n");
                t_ignore = false;
                yield return new WaitForSecondsRealtime(0.02f);

                //yield return new WaitForSeconds(0.02f);

            }
        }
        //모든 대사가 타이핑 되고 초기화
        dialogueManager.endChat_inController = true;
        startChat = false;
        stopChat = false;

    }

}
