using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class Quest
{
    public int questId; //퀘스트 아이디
    public List<string> questContent; //퀘스트 세부 내용(설명?) 
    public List<string> questTitle; //퀘스트 제목
    public List<string> questGoal; //퀘스트 목표
    public int currentQuestValue = 0; //현재 퀘스트 진행도
    public int questClearValue = 0; //퀘스트 클리어 조건
    public string questClearString = "";
    //퀘스트 변경 시 보상 초기화
    public void InitMainQuestValue()
    {
        this.currentQuestValue = 0;
        this.questClearValue = 0;

    }

}
