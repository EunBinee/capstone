using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    //! 애니메이션 효과
    [SerializeField] TMP_Text objectText;

    bool startChat = false;
    public bool stopChat = false;


    DialogueManager dialogueManager;

    void Start()
    {
        dialogueManager = GetComponent<DialogueManager>();
        SetUIVariable();
    }

    public void SetUIVariable()
    {
        if (CanvasManager.instance.dialogueUI == null)
        {
            CanvasManager.instance.dialogueUI = CanvasManager.instance.GetCanvasUI(CanvasManager.instance.dialogueUIName);
            if (CanvasManager.instance.dialogueUI == null)
                return;
        }
        DialogueUI_info dialogueUI_Info = CanvasManager.instance.dialogueUI.GetComponent<DialogueUI_info>();
        objectText = dialogueUI_Info.objectText;
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

    IEnumerator ObjectChat(string sentence)
    {

        //s        Debug.Log($"지금 문장 : {sentence}");
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
                yield return new WaitForSeconds(0.05f);
                break;

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
                yield return new WaitForSeconds(0.02f);

            }
            yield return null;
        }
        //모든 대사가 타이핑 되고 초기화
        dialogueManager.endChat_inController = true;
        startChat = false;
        stopChat = false;
    }

    //화살표 애니메이션 
    public void ArrowAnimation()
    {
        StartCoroutine(AnimateArrow());
    }

    private IEnumerator AnimateArrow()
    {
        dialogueManager.isArrowAnimating = true;

        while (dialogueManager.isArrowAnimating)
        {
            // 예시로 알파값을 조절하여 페이드 효과 구현
            float alpha = Mathf.PingPong(Time.time, 0.5f);
            Color arrowColor = dialogueManager.DialogueUI_info.dialogueArrow.GetComponent<Image>().color;
            arrowColor.a = alpha;
            dialogueManager.DialogueUI_info.dialogueArrow.GetComponent<Image>().color = arrowColor;

            yield return null;
        }
    }

}
