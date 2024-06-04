using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;


public class QuestManager : MonoBehaviour
{
    public Quest quest_;
    private string text_goal = "";
    private string text_title = "";
    private string text_content = "";
    public int currentQuestValue_;
    public bool isTutorial = false;
    private bool isQuestCompleted = false; // 플래그 확인
    DialogueController dialogueController;

    public static QuestManager instance = null;
        public static QuestManager Instance
        {
            get
            {
                if (null == instance)
                {
                    return null;
                }
                return instance;
            }
        }
    void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
                Destroy(this.gameObject);

        
    }
    private void Start()
    {
        if (isTutorial)
        {
            dialogueController = GetComponent<DialogueController>();
            GameManager.Instance.gameInfo.QuestNum = 1;
        }
    }

    private void Init()
    {
        currentQuestValue_ = 0;
        quest_.currentQuestValue = 0;
        quest_.questClearValue = 0;
    }

    private void Update()
    {
        if (DialogueManager.instance.DoQuest == true) 
        {
            UpdateQuest(quest_.questId);

        }

        if (DialogueManager.instance.isQuestDetail)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
            {
                QuestExit();
                DialogueManager.instance.isQuestDetail = false;
            }
        }

        if (isTutorial)
        {
            UpdateAlarm(quest_.questId);
        }


    }

    //퀘스트 버튼 클릭시에 퀘스트 상세내용 팝업창 띄우기
    public void QuestClick()
    {
        UpdateQuest(quest_.questId);
        DialogueManager.instance.QuestDetailTitle_UI(text_title);
        DialogueManager.instance.QuestDetailContent_UI(text_content);
        DialogueManager.instance.QuestDetailGoal_UI(text_goal);
    }
    public void QuestExit()
    {
        DialogueManager.instance.QuestDeailFalse();
    }


    //퀘스트 진행도 업데이트
    public void Quest_ValueUpdate()
    {
        //현재 퀘스트 진행도와 퀘스트 목표치가 같으면 퀘스트 클리어.
        //if(!isTutorial)
        //{
            if (quest_.currentQuestValue >= quest_.questClearValue)
            {
                quest_.currentQuestValue = quest_.questClearValue;
                Quest_Clear();
            }
    }

    //퀘스트 클리어 함수
    protected void Quest_Clear()
    {
        // if(isTutorial)
        // else if(!isTutorial)
        DialogueManager.instance.DoQuest = false;

    }

    //퀘스트 업데이트 함수 
    public void UpdateQuest(int id)
    {
        DialogueManager.instance.DoQuest = true;
        quest_.currentQuestValue = currentQuestValue_;

        if (GameManager.Instance.gameInfo.QuestNum != 0)
        {
            quest_ = DatabaseManager.GetInstance().Quest_Dictionary[GameManager.Instance.gameInfo.QuestNum];
            Quest_ValueUpdate();
            TextUpdate();
        }
    }

    //퀘스트 목표 업데이트 함수 
    public void TextUpdate()
    {
        if(!isTutorial)
        {
            for (int i = 0; i < quest_.questGoal.Count;)
            {
                text_goal = quest_.questGoal[i].Replace("'", ",").Replace("ⓨ", "<color=#ffff00>").Replace("ⓦ", "</color><color=#ffffff>" + "</color>") + "(" + quest_.currentQuestValue + "/" + quest_.questClearValue + ")";
                if (++i != quest_.questGoal.Count)
                {
                    text_goal += "\n";
                }
            }
        }
        else if(isTutorial)
        {
            for (int i = 0; i < quest_.questGoal.Count;)
            {
                text_goal = quest_.questGoal[i].Replace("'", ",").Replace("ⓨ", "<color=#ffff00>").Replace("ⓦ", "</color><color=#ffffff>" + "</color>");
                if (++i != quest_.questGoal.Count)
                {
                    text_goal += "\n";
                }
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
        if (!isTutorial)
        {
            DialogueManager.instance.QuestGoal_UI(text_goal); //퀘스트 목표 UI 활성화
            DialogueManager.instance.QuestTitle_Alarm(text_title);
        }
        else
        {
           DialogueManager.instance.QuestGoal_UI(text_goal); 
        }

    }

    //튜토리얼 
    public void UpdateAlarm(int id)
    {
        //Debug.Log(GameManager.Instance.gameInfo.QuestNum);
        //Debug.Log(DatabaseManager.GetInstance().Quest_Dictionary.ContainsKey(quest_.questId));
        if (GameManager.Instance.gameInfo.QuestNum != 0 && DatabaseManager.GetInstance().Quest_Dictionary.ContainsKey(quest_.questId))
        {
            quest_ = DatabaseManager.GetInstance().Quest_Dictionary[GameManager.Instance.gameInfo.QuestNum];
            TextAlarm();
            //NextTextAlarm();
            StartCoroutine(NextTextAlarmCoroutine());
        }
        else
        {
            DialogueManager.instance.TutorialUIFalse(text_goal); //퀘스트 목표 UI 비활성화
            //isTutorial = false;
        }

    }
    //튜토 알람 텍스트 
    public void TextAlarm()
    {
        for (int i = 0; i < quest_.questGoal.Count;)
        {
            text_goal = quest_.questGoal[i].Replace("'", ",").Replace("ⓨ", "<color=#ffff00>").Replace("ⓦ", "</color><color=#ffffff>" + "</color>");
            if (++i != quest_.questGoal.Count)
            {
                text_goal += "\n";
            }
        }
        DialogueManager.instance.TutorialUI(text_goal); //퀘스트 목표 UI 활성화

        //GameManager.Instance.gameInfo.QuestNum = quest_.questId;
    }
     private IEnumerator NextTextAlarmCoroutine()
    {
        yield return new WaitForSeconds(1.0f);
        NextTextAlarm();
    }
    public void NextTextAlarm()
    {
        if (isQuestCompleted) return;  // 플래그 확인

        string[] data = quest_.questClearString.ToString().Split(new char[] { '\'' });
        string stringdata = quest_.questClearString.ToString();

        if (data != null && data.Length >= 1)
        {
            bool[] keysPressed = new bool[data.Length];
            bool allKeysPressed = false;

            for (int i = 0; i < data.Length; i++)
            {
                KeyCode key;// = (KeyCode)Enum.Parse(typeof(KeyCode), data[i]);
                bool keyParsed = Enum.TryParse(data[i], out key);

                if(keyParsed)
                {
                    // 키가 눌렸을 때만 keysPressed[i]를 true로 설정
                    keysPressed[i] = Input.GetKeyDown(key);

                    if (keysPressed[i])
                    {
                        allKeysPressed = true;
                    }
                }
                else if (stringdata == "좌클릭")
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        StartCoroutine(DelayToNextQuest());
                    }
                }
                else if(stringdata=="우클릭")
                {
                    if(Input.GetMouseButtonDown(1)||Input.GetKeyDown(KeyCode.LeftShift))
                    {
                       StartCoroutine(DelayToNextQuest());
                    }
                }
                else if(stringdata=="")
                {
                    //Debug.Log(quest_.currentQuestValue);
                    quest_.currentQuestValue = currentQuestValue_;
                    if (quest_.currentQuestValue >= quest_.questClearValue)
                    {
                        quest_.currentQuestValue = quest_.questClearValue;
                        //Quest_Clear();
                       StartCoroutine(DelayToNextQuest());
                    }

                }

            }
            //Debug.Log("Keys Pressed: " + string.Join(", ", keysPressed));
            // 모든 키가 한 번 이상 눌렸을 때 수행할 동작
            if (allKeysPressed)
            {
               StartCoroutine(DelayToNextQuest());
            }
        }
    }
    private IEnumerator DelayToNextQuest()
    {
        isQuestCompleted = true;  // 퀘스트 완료 플래그 설정
        yield return new WaitForSeconds(0.8f);
        isEnd();
        isQuestCompleted = false;  // 퀘스트 완료 플래그 초기화
    }

    public void isEnd()
    {
        if (DatabaseManager.GetInstance().Quest_Dictionary.ContainsKey(quest_.questId)) //퀘스트 번호가 딕셔너리 범위를 벗어나면 ui비활성화가 되게 해야하는뎅...
        {
            quest_.questId++;
            GameManager.Instance.gameInfo.QuestNum = quest_.questId;

            //Debug.Log(text_goal);
            DialogueManager.instance.TutorialUI(text_goal); //퀘스트 목표 UI 활성화
        }
        else
        {
            isTutorial = false;
            Quest_Clear();
            
        }
    }
}

