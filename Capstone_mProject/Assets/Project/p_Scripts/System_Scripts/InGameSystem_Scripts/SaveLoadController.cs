using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadController : MonoBehaviour
{
    // public SaveData loadData;
    // private void Start()
    // {
    //     //SaveSystem.Instance.LoadGameData(); //불러오기

    // }
    // void Update()
    // {
    //     if (Input.GetKeyDown("k"))
    //     {
    //         //SaveSystem.Instance.SaveGameData(); //저장

    //         // SaveData dialogue = new SaveData();
    //         // SaveSystem.Save(dialogue, "GameData");
    //     }

    //     if (Input.GetKeyDown("l"))
    //     {
    //         //SaveSystem.Instance.LoadGameData(); //불러오기
    //         // loadData = SaveSystem.Load("GameData");
    //         // DialogueLoad();
    //         //Debug.Log(string.Format("LoadData Result => dialoguenum : {0}", loadData.dialogueNum));
    //         //Debug.Log(string.Format("LoadData Result => qeustnum : {0}", loadData.questNum));


    //     }
    //     if (Input.GetKeyDown("n"))
    //     {
    //         //Debug.Log(GameManager.Instance.gameInfo.DialogueNum);
    //         //Debug.Log(GameManager.Instance.questManager.currentQuestValue_);

    //     }
    // }
    // public void DialogueLoad()
    // {
    //     GameManager.Instance.gameInfo.eventNum = loadData.eventNum;
    //     GameManager.Instance.gameInfo.EndingNum = loadData.endingNum;
    //     GameManager.Instance.gameInfo.QuestNum = loadData.questNum;
    //     GameManager.Instance.dialogueManager.DoQuest = loadData.doQuest;
    //     GameManager.Instance.gameInfo.DialogueNum = loadData.dialogueNum;
    //     GameManager.Instance.questManager.currentQuestValue_ = loadData.currentQuestValue;
    //     //GameManager.Instance.questManager.UpdateQuest(GameManager.Instance.gameInfo.QuestNum);

    // }


}
