using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData

{
    public int eventNum; //이벤트번호
    public int endingNum; //엔딩번호
    public int questNum; //퀘스트번호
    public bool doQuest; //퀘스트중인지 아닌지 
    public int dialogueNum; //대화번호
    public int currentQuestValue; //현재 퀘스트 진행도
    public string nickname; //플레이어 닉네임



    // public SaveData()
    // {
    //     eventNum = GameManager.Instance.gameInfo.eventNum;
    //     endingNum = GameManager.Instance.gameInfo.EndingNum;
    //     questNum = GameManager.Instance.gameInfo.QuestNum;
    //     doQuest = GameManager.Instance.dialogueManager.DoQuest;
    //     dialogueNum = GameManager.Instance.gameInfo.DialogueNum;
    //     currentQuestValue = GameManager.Instance.questManager.currentQuestValue_;
    //     nickname = GameManager.Instance.gameInfo.Nickname;
    // }

}
