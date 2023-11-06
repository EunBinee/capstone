using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Line
{
    public string Name; //npc 이름
    public string[] context; //대화내용들

    public bool isChoice;//선택지 여부
    public Choice choice;

    public bool isFinishLine;  //대화 끝났는지 여부
    public int nextDialogueNum; //대화가 끝나고 다음 대사 단락의 번호

    public bool changeEvnetID; //대화가 끝나고 이벤트번호 변경이 있는지 여부
    public int evnetIDToBeChange; //변경될 이벤트 ID

    public bool changeEndingID; //대화가 끝나고 ending 번호의 변경이 있는지 여부
    public int endingIDToBeChange; //변경될 엔딩ID

    public bool DoQuest; //퀘스트 진행중인지 여부
    public bool changeQuestID; //대화 끝나고 퀘스트 번호 변경 있는지 여부
    public int questIDToBeChange; //변경될 퀘스트 ID

    //public int nextLineNum;
}


[System.Serializable]
public class Dialogue
{
    public List<List<Line>> lines;

    //엔딩번호
    public int endingNum; //
    //NPCID
    public int npcNum; //C
    //이벤트ID
    public int eventNum; //B��
    //대화단락
    public int dialogueNum; //D��
    //퀘스트 번호
    public int questNum;
    //대사번호
    //ublic int lineNum;

}

[System.Serializable]
public class Choice
{
    public string firstOption; //1번 선택지 Text
    public string secondOption; //2번 선택지 Text

    public int firstOptDialogNum; //1번째 선택지를 선택했을 경우, 그다음 대사 단락 번호
    public int secondOptDialogNum;//2번째 선택지를 선택했을 경우, 그다음 대사 단락 번호

}
