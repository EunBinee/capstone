using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
            if (Input.GetKeyDown(KeyCode.Return))
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


    IEnumerator ObjectChat(string sentence)
    {


        string writerText = "";

        for (int i = 0; i < sentence.Length; i++)
        {
            if (stopChat)
            {
                //Enter키를 누르면 애니메이션 중지하고, 바로 글씨 나오도록.
                writerText = sentence;
                objectText.text = writerText;
                break;
            }
            else
            {
                writerText += sentence[i];
                objectText.text = writerText;


                yield return new WaitForSeconds(0.07f);
            }
        }

        //모든 대사가 타이핑 됐다는 것을 알려야함.
        dialogueManager.endChat_inController = true;
        startChat = false;
        stopChat = false;

    }

}
