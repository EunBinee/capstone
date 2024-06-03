using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
//using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.UI;

[Serializable]
public class DialogueUI
{
    public GameObject go_DialogueBar; //대화창 UI
    public TMP_Text Text_Dialogue; //대화 text
    public TMP_Text Text_Name; //이름 text
    public GameObject ObjectTextBox_Button01; //선택지 1번 UI
    public TMP_Text Text_Btn01; //선택지 1번 text
    public GameObject ObjectTextBox_Button02; //선택지 2번 UI
    public TMP_Text Text_Btn02; //선택지 2번 text
    public GameObject dialogueArrow; //대사 끝났을 경우 화살표ui
    public Image portrait;
    public Button dialogueSkip; //대화 스킵 버튼 UI

    public GameObject Quest_Button01;
    public GameObject Go_QuestDetail;
    public TMP_Text Text_QuestGoal; //퀘스트 목표 text
    public TMP_Text Text_QuestDetailGoal; //퀘스트 목표 text
    public TMP_Text Text_QuestDetailTitle; //퀘스트 제목 text
    public TMP_Text Text_QuestDetailContent; //퀘스트 세부내용 text
    public GameObject Text_Alarm; //튜토리얼, 알람등을 알려주는 text 
    public GameObject Text_Tuto;

    public TMP_Text objectText;
}
