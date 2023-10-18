using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
