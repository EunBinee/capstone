using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData

{
    public int eventNum;
    public int endingNum;
    public int questNum;
    public bool doQuest;
    public int dialogueNum;

    public int currentQuestValue;



    public SaveData()
    {
        eventNum = GameManager.Instance.gameInfo.eventNum;
        endingNum = GameManager.Instance.gameInfo.EndingNum;
        questNum = GameManager.Instance.gameInfo.QuestNum;
        doQuest = GameManager.Instance.dialogueManager.DoQuest;
        dialogueNum = GameManager.Instance.gameInfo.DialogueNum;
        currentQuestValue = GameManager.Instance.questManager.currentQuestValue_;
    }

}
