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
    private string text_goal = "";
    private string text_title = "";
    private string text_content = "";
    public int currentQuestValue_;

    private void Init()
    {
        currentQuestValue_ = 0;
        quest_.currentQuestValue = 0;
        quest_.questClearValue = 0;
    }
    private void Update()
    {
        if (GameManager.Instance.dialogueManager.DoQuest == true)
        {
            //quest_.currentQuestValue = currentQuestValue_;
            // if (Input.GetKeyDown(KeyCode.J))
            // {
            //     currentQuestValue_++;
            //     quest_.currentQuestValue = currentQuestValue_;
            //     Debug.Log(quest_.currentQuestValue);
            //     TextUpdate();
            // }
            UpdateQuest(quest_.questId);

            //TextUpdate();
        }

        if (GameManager.Instance.dialogueManager.IsQuestDetail)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            {
                QuestExit();
                GameManager.Instance.dialogueManager.IsQuestDetail = false;
            }
        }

    }

    //퀘스트 버튼 클릭시에 퀘스트 상세내용 팝업창 띄우기
    public void QuestClick()
    {
        GameManager.GetInstance().dialogueManager.QuestDetailTitle_UI(text_title);
        GameManager.GetInstance().dialogueManager.QuestDetailContent_UI(text_content);
        GameManager.GetInstance().dialogueManager.QuestDetailGoal_UI(text_goal);
    }
    public void QuestExit()
    {
        GameManager.Instance.dialogueManager.QuestDeailFalse();
    }


    //퀘스트 진행도 업데이트
    public void Quest_ValueUpdate()
    {
        //현재 퀘스트 진행도와 퀘스트 목표치가 같으면 퀘스트 클리어.
        if (quest_.currentQuestValue >= quest_.questClearValue)
        {
            quest_.currentQuestValue = quest_.questClearValue;
            Quest_Clear();
        }

    }

    //퀘스트 클리어 함수
    protected void Quest_Clear()
    {

        GameManager.instance.dialogueManager.DoQuest = false;
    }

    //퀘스트 업데이트 함수 
    public void UpdateQuest(int id)
    {

        GameManager.instance.dialogueManager.DoQuest = true;
        quest_.currentQuestValue = GameManager.Instance.questManager.currentQuestValue_;//currentQuestValue_;

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
        Quest_ValueUpdate();
        TextUpdate();


    }

    //퀘스트 목표 업데이트 함수 
    public void TextUpdate()
    {

        for (int i = 0; i < quest_.questGoal.Count;)
        {
            text_goal = quest_.questGoal[i].Replace("'", ",").Replace("ⓨ", "<color=#ffff00>").Replace("ⓦ", "</color><color=#ffffff>" + "</color>") + "(" + quest_.currentQuestValue + "/" + quest_.questClearValue + ")";
            if (++i != quest_.questGoal.Count)
            {

                text_goal += "\n";
            }
        }

        for (int i = 0; i < quest_.questTitle.Count;)
        {
            text_title = quest_.questTitle[i].Replace("'", ",").Replace("ⓨ", "<color=#ffff00>").Replace("ⓦ", "</color><color=#ffffff>" + "</color>"); ;
            if (++i != quest_.questTitle.Count)
            {
                text_title += "\n";
            }
        }
        for (int i = 0; i < quest_.questContent.Count;)
        {
            text_content = quest_.questContent[i].Replace("'", ",").Replace("ⓨ", "<color=#ffff00>").Replace("ⓦ", "</color><color=#ffffff>" + "</color>"); ;
            if (++i != quest_.questContent.Count)
            {
                text_content += "\n";
            }
        }

        GameManager.GetInstance().dialogueManager.QuestGoal_UI(text_goal); //퀘스트 목표 UI 활성화

    }


}
