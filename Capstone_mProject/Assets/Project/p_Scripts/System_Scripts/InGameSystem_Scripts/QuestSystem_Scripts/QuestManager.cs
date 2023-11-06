using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

// public enum QuestType
// {
//     None = -1,
//     MainQuest = 0,
//     SubQuest,
// }

public class QuestManager : MonoBehaviour
{
    static public QuestManager instance;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    static public QuestManager GetInstance()
    {
        return instance;
    }
    public Quest quest_ = new Quest();
    public string text = "";


    private void Init()
    {
        quest_.currentQuestValue = 0;
        quest_.questClearValue = 0;
    }
    private void Update()
    {
        if (GameManager.Instance.dialogueManager.DoQuest == true)
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                quest_.currentQuestValue++;
                Debug.Log(quest_.currentQuestValue);
                TextUpdate();
            }
            Quest_ValueUpdate();
        }


    }

    public void QuestList(int questNum)
    {





    }

    //퀘스트 진행도 업데이트
    public void Quest_ValueUpdate()
    {

        if (quest_.currentQuestValue >= quest_.questClearValue)
        {
            Quest_Clear();
        }

    }

    protected void Quest_Clear()
    {
        Debug.Log("퀘스트 클리어");
        GameManager.instance.dialogueManager.DoQuest = false;
        quest_.currentQuestValue = 0;
        GameManager.GetInstance().dialogueManager.QuestGoal_UIFalse();
    }


    public void UpdateQuest(int id)
    {
        GameManager.instance.dialogueManager.DoQuest = true;
        Init();

        quest_ = DatabaseManager.GetInstance().Quest_Dictionary[GameManager.Instance.gameInfo.QuestNum];
        // switch (GameManager.Instance.gameInfo.QuestNum)
        // {
        //     case 1:
        //         Debug.Log("퀘스트 1번");
        //         quest_ = DatabaseManager.GetInstance().Quest_Dictionary[id];
        //         //QuestList(1);
        //         break;
        //     default:
        //         break;
        // }
        TextUpdate();

    }


    public void TextUpdate()
    {
        for (int i = 0; i < quest_.questContent.Count;)
        {
            text = quest_.questContent[i] + "(" + quest_.currentQuestValue + "/" + quest_.questClearValue + ")";
            if (++i != quest_.questContent.Count)
            {

                text += "\n";
            }
        }
        GameManager.GetInstance().dialogueManager.QuestGoal_UI(text);
        Debug.Log(GameManager.Instance.dialogueManager.DoQuest);

    }


}
