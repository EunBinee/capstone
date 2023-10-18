using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[System.Serializable]
//public class Dialogue
//{
//    [Tooltip("대사 치는 캐릭터 이름")]
//    public string name;

//    [Tooltip("대사 내용")]
//    public string[] contexts;

//    public bool isChoice;
//    public string skipnum;
//    public string endingNum;
//    public string firstChoice; //첫번째 선택지
//    public string secondChoice; //두번째 선택지
//}

//[System.Serializable]
//public class DialogueEvent
//{
//    public string name; //이벤트 이름
//    public bool isChoice; //선택지 여부
//    public string endingNum; //엔딩번호

//    public Vector2 line;
//    public Dialogue[] dialogues;
//}

//1차수정========================================================
//[System.Serializable]
//public class Line
//{
//    //대사를 치는 인물
//    public string Name;
//    //대사 내용
//    public string[] context;

//    //선택지가 있는지 여부
//    public bool isChoice;
//    public Choice choice;

//    //대화가 끝났는지 여부    
//    public bool isFinishLine;
//    public int nextDialogueNum; //끝났으면 다음 대사

//    //대화 끝나고 이벤트 번호 변경 
//    public bool changeEvent;
//    public int changeEventID; //변경될 이벤트 ID


//    //대화가 끝나고 퀘스트 여부
//    //public bool isQuest;
//    //public int QuestId;

//    //엔딩 여부
//    public bool changeEnding;
//    public int changeEndingID; //변경될 엔딩 ID

//}


//[System.Serializable]
//public class Dialogue
//{
//    //csv F열을 기준으로.. 나누었다.   
//    public List<List<Line>> lines;
//    //lines[i][j] => lines[i]    F열 기준
//    //lines[i][j] => lines[i][g] G열 기준

//    //이벤트 번호
//    public int eventNum;
//    //NPC ID = 대사 번호
//    public int dialogueNum;
//    //퀘스트 번호
//    //public string questNum;
//    //엔딩 번호
//    public int endingNum;
//}

//[System.Serializable]
//public class Choice
//{
//    public string firstOption; //1번째 선택지의 Text
//    public string secondOption; //2번째 선택지의 Text

//    public int firstSkipDialogNum; //1번째 선택지를 선택했을 경우 그 다음 대사 g열
//    public int secondSkipDialogNum;//2번째 선택지를 선택했을 경우 그 다음 대사

//    public bool firstOpenQuest; //선택후 대화를 마치고 오브젝트를 열어야하는지 
//    public string firstQuestName; //열어야하는 오브젝트의 이름. 여기에 나중에 퀘스트 추가할듯?

//    public bool secondOpenQuest; //선택후 대화를 마치고 씬이동이 이루어져야하는지 
//    public string secondQuestName; //열어야하는 오브젝트의 이름. 
//}


//2차수정========================================================
[System.Serializable]
public class Line
{
    public string Name; //npc 이름
    public string[] context; //대사들

    public bool isChoice;//선택지 여부
    public Choice choice;

    public bool isFinishLine;  //대화가 끝났는지 여부    
    public int nextDialogueNum; //끝났다면, 이 대사 다음 대사(대사단락).
    
    public bool changeEvnetID; //대화가 끝나고 이벤트번호의 변경이 있는지 여부
    public int evnetIDToBeChange; //변경될 이벤트 ID

    public bool changeEndingID; //대화가 끝나고 ending 변경이 있는지 여부
    public int endingIDToBeChange; //변경될 엔딩 ID

    public int nextLineNum;
}


[System.Serializable]
public class Dialogue
{
    public List<List<Line>> lines;

    //엔딩 
    public int endingNum; //L열
    //NPC번호
    public int npcNum; //C열
    //이벤트 번호
    public int eventNum; //B열
    //대사 문단의 번호
    public int dialogueNum; //D열

    public int lineNum;

}

[System.Serializable]
public class Choice
{
    public string firstOption; //1번째 선택지의 Text
    public string secondOption; //2번째 선택지의 Text

    public int firstOptDialogNum; //1번째 선택지를 선택했을 경우, 그다음 대사 번호
    public int secondOptDialogNum;//2번째 선택지를 선택했을 경우, 그다음 대사 번호

}