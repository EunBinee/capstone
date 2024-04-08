using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.UI;

public class DialogueUI_info : MonoBehaviour
{
    public GameObject go_DialogueBar; //대화창 UI
    public TMP_Text Text_Dialogue; //대화 text
    public TMP_Text Text_Name; //이름 text
    public GameObject ObjectTextBox_Button01; //선택지 1번 UI
    public TMP_Text Text_Btn01; //선택지 1번 text
    public GameObject ObjectTextBox_Button02; //선택지 2번 UI
    public TMP_Text Text_Btn02; //선택지 2번 text
    public GameObject ObjectTextBox_Button03; //선택지 3번 UI
    public TMP_Text Text_Btn03; //선택지 3번 text
    public GameObject ObjectTextBox_Button04; //선택지 4번 UI
    public TMP_Text Text_Btn04; //선택지 4번 text
    public GameObject ObjectTextBox_Button05; //선택지 5번 UI
    public TMP_Text Text_Btn05; //선택지 5번 text

    public GameObject dialogueArrow; //대사 끝났을 경우 화살표ui
    public Image portrait;

    public GameObject Quest_Button01;
    public GameObject Go_QuestDetail;
    public TMP_Text Text_QuestGoal; //퀘스트 목표 text
    public TMP_Text Text_QuestDetailGoal; //퀘스트 목표 text
    public TMP_Text Text_QuestDetailTitle; //퀘스트 제목 text
    public TMP_Text Text_QuestDetailContent; //퀘스트 세부내용 text
    public GameObject Text_Alarm; //튜토리얼, 알람등을 알려주는 text 

    public TMP_Text objectText;

}
