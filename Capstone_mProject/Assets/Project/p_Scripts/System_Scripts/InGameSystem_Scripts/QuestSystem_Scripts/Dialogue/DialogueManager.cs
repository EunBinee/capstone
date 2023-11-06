using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    DialogueController dialogueController; //대화 텍스트 출력 애니메이션 구현 스크립트
    GameInfo gameInfo; //게임의 전반적인 정보 
    public GameObject go_DialogueBar; //대화창 UI
    public TMP_Text Text_Dialogue; //대화 text
    public TMP_Text Text_Name; //이름 text
    public GameObject ObjectTextBox_Button01; //선택지 1번 UI
    public TMP_Text Text_Btn01; //선택지 1번 text
    public GameObject ObjectTextBox_Button02; //선택지 2번 UI
    public TMP_Text Text_Btn02; //선택지 2번 text
    public bool endChat_inController = false;  //dialogueController 타이핑 애니메이션

    public GameObject go_QuestBar;
    public TMP_Text Text_questGoal; //퀘스트 목표 text

    public bool DoQuest;

    void Start()
    {
        dialogueController = GetComponent<DialogueController>();
        gameInfo = GetComponent<GameInfo>();
        DoQuest = false;

    }

    public void Action_NPC(int id, Item interaction_Item)
    {
        //NPC의 대사 가지고 옴.
        //DatabaseManager.GetInstance().NPC_diaglogues_Dictionary[id]를 통해서 현재 id의 맞는 Dialogue를 가지고 온다.
        Dialogue dialogue = DatabaseManager.GetInstance().NPC_diaglogues_Dictionary[id];
        StartCoroutine(StartObjectTextBox(dialogue, interaction_Item));


    }
    public static IEnumerator WaitForRealTime(float delay)
    {
        while (true)
        {
            float pauseEndTime = Time.realtimeSinceStartup + delay;
            while (Time.realtimeSinceStartup < pauseEndTime)
            {
                yield return 0;
            }
            break;
        }
    }

    IEnumerator StartObjectTextBox(Dialogue dialogue, Item interaction_Item)
    {

        //텍스트를 보여주는 코루틴 
        go_DialogueBar.SetActive(true); //텍스트 UI 활성화
        Text_Dialogue.text = "";
        Text_Name.text = "";
        bool AllFinish = false; //모든 대사가 끝났는지 확인용

        int curPart = 0; //Dialogue.cs의 lines[curPart][curLine] => lines[curPart]   
        int curLine = 0; //lines[curPart][curPart] 
        int curContext = 0; //lines[curPart][curLine].context[curContext] 

        bool isFinish = false; //대사가 끝남. ALLFinish랑은 다름
        //그다음 대사는 없어서 대사는 끝났지만, 대사를 본 후,아직 엔터를 치지 않아서 아직 완전히 꺼지지는 않은 상태

        bool isChoice = false; // 선택지를 가지고 있는지
        bool choiceSettingF = false; //선택지 가지고 있으면 선택지 버튼들의 텍스트 변환등 세팅을 끝냈는지
        bool ClickChoiceBtn = true; //선택지를 눌렀을 경우, 바로 Enter키 없이 바로 다음 대사로 넘어가도록 설정하는 bool값


        int curlineContextLen;  //현재 dialogue.line[curline]의 line안의 context의 길이


        //ending이나 Ending의 변화가 았는지
        bool changeEvnetID = false;
        int eventIDToBeChange = 0; //업데이트할 이벤트ID
        bool changeEndingID = false;
        int endingIDToBeChange = 0; //업데이트할 엔딩ID

        bool changeQuestID = false;
        int questIDToBeChange = 0;

        string line = ""; //대사 공백으로 초기화

        endChat_inController = true; //Chat 애니메이션이 끝났는지, 확인용.

        while (!AllFinish && !DoQuest)
        {

            curlineContextLen = dialogue.lines[curPart][curLine].context.Length; //현재대사 배열 길이

            if (curContext < curlineContextLen)
            {
                //아직 문장이 끝나지 않은 경우
                if (ClickChoiceBtn)
                {
                    //선택지를 고르고 나면
                    Text_Dialogue.text = "";
                    Text_Name.text = "";
                    endChat_inController = false;

                    Text_Name.text = dialogue.lines[curPart][curLine].Name;
                    line = dialogue.lines[curPart][curLine].context[curContext];

                    dialogueController.Chat_Obect(line);
                    curContext++;
                    ClickChoiceBtn = false;

                    //Debug.Log("d");

                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    //선택지가 없고 아직 문장이 끝나지 않은경우
                    Text_Dialogue.text = "";
                    Text_Name.text = "";
                    endChat_inController = false;

                    Text_Name.text = dialogue.lines[curPart][curLine].Name;

                    line = dialogue.lines[curPart][curLine].context[curContext];
                    dialogueController.Chat_Obect(line);
                    curContext++;

                    //Debug.Log("d1");

                }
            }

            yield return new WaitUntil(() => endChat_inController == true);
            //yield return StartCoroutine(DialogueManager.WaitForRealTime(0.1f));
            //마지막 context의 마지막 문장이 끝난 경우 확인하기
            if (curContext == curlineContextLen)
            {
                //마지막 context의 마지막 문장이 끝난 후, 대화가 끝이 났는지, 선택지가 있는지 확인

                isChoice = dialogue.lines[curPart][curLine].isChoice;
                isFinish = dialogue.lines[curPart][curLine].isFinishLine;

                if (isChoice)
                {
                    //만약 대사가 끝났고 선택지가 있는 경우
                    if (!choiceSettingF)
                    {
                        Debug.Log("선택지 있음");
                        //선택지 버튼 활성화
                        ObjectTextBox_Button01.SetActive(true);
                        ObjectTextBox_Button02.SetActive(true);

                        //선택지 버튼 누르면 어디로 갈지 결정
                        int firstOptDialogPart = dialogue.lines[curPart][curLine].choice.firstOptDialogNum;
                        int secondOptDialogPart = dialogue.lines[curPart][curLine].choice.secondOptDialogNum;


                        //선택지 대사 출력
                        Text_Btn01.text = dialogue.lines[curPart][curLine].choice.firstOption;
                        Text_Btn02.text = dialogue.lines[curPart][curLine].choice.secondOption;

                        //버튼안에 내용물 넣어줌.
                        Button btn01 = ObjectTextBox_Button01.GetComponent<Button>();
                        btn01.onClick.RemoveAllListeners();

                        //AddListener에 함수를 만들어 넣어줄 수 있지만..동적으로 계속 curPart가 변해야하기에..
                        //람다를 이용해서 익명함수를 만들어주었다.
                        btn01.onClick.AddListener(() =>
                        {
                            if (!Input.GetKeyDown(KeyCode.Return))// 아직 문장이 끝나지 않았다면
                            {
                                curPart = (firstOptDialogPart - 1); //curPart로 다음으로 넘어감. 
                                curLine = 0;
                                curContext = 0;
                                ObjectTextBox_Button01.SetActive(false);
                                ObjectTextBox_Button02.SetActive(false);

                                choiceSettingF = false;
                                ClickChoiceBtn = true;
                            }

                        });

                        Button btn02 = ObjectTextBox_Button02.GetComponent<Button>();
                        btn02.onClick.RemoveAllListeners();
                        btn02.onClick.AddListener(() =>
                        {
                            if (!Input.GetKeyDown(KeyCode.Return))
                            {
                                curPart = (secondOptDialogPart - 1);
                                curLine = 0;
                                curContext = 0;
                                ObjectTextBox_Button01.SetActive(false);
                                ObjectTextBox_Button02.SetActive(false);

                                choiceSettingF = false;
                                ClickChoiceBtn = true;
                            }
                        });


                        choiceSettingF = true;

                    }

                }


                else if (!isChoice && isFinish)
                {
                    //선택지가 없고 완전히 문단이 끝난 경우

                    //대사후 다음 대사
                    int nextDialogueNum = dialogue.lines[curPart][curLine].nextDialogueNum;
                    interaction_Item.dialogueNum = nextDialogueNum;


                    bool eventID = dialogue.lines[curPart][curLine].changeEvnetID; //이벤트ID 변경해야하는지
                    if (eventID) //변경할 이벤트가 있을 경우
                    {
                        changeEvnetID = eventID;
                        if (changeEvnetID)
                        {
                            interaction_Item.dialogueNum = 1;
                            eventIDToBeChange = dialogue.lines[curPart][curLine].evnetIDToBeChange;
                        }
                        Debug.Log("이벤트 변경 o");
                    }
                    else
                    {
                        changeEvnetID = false;
                        Debug.Log("이벤트 변경 x");
                    }

                    bool endingID = dialogue.lines[curPart][curLine].changeEndingID;
                    if (endingID)
                    {
                        changeEndingID = endingID;
                        if (changeEndingID)
                        {
                            //interaction_Item.dialogueNum = 1;
                            endingIDToBeChange = dialogue.lines[curPart][curLine].endingIDToBeChange;
                        }
                        Debug.Log("엔딩 변경 o");
                    }

                    bool questID = dialogue.lines[curPart][curLine].changeQuestID;
                    if (questID)
                    {
                        changeQuestID = questID;
                        if (changeQuestID)
                        {
                            //interaction_Item.dialogueNum = interaction_Item.dialogueNum;
                            questIDToBeChange = dialogue.lines[curPart][curLine].questIDToBeChange;
                            //DoQuest = true;
                            Debug.Log(questIDToBeChange);

                        }
                        Debug.Log("퀘스트 변경 o");

                    }



                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        AllFinish = true;
                    }
                }

                else if (!isChoice && !isFinish)
                {
                    //선택지가 없고, 완전히 문단이 끝난 경우가 아닌 뒤에 다른 사람의 대사가 더 있는 경우
                    //계속 이어진다.
                    curLine++;
                    curContext = 0;
                    ClickChoiceBtn = false;

                    Debug.Log("대사 이어지는 중..");
                }
            }
        }

        //엔딩 변화 있는 경우
        if (changeEndingID)
        {
            gameInfo.EndingNum = endingIDToBeChange;
            //int nextEventNum = dialogue.lines[curPart][curLine].evnetIDToBeChange;
            //interaction_Item.preEventNum = nextEventNum;
        }

        //이벤트 id변화 있는 경우
        if (changeEvnetID)
        {
            gameInfo.EventNum = eventIDToBeChange;
        }

        if (changeQuestID)
        {
            DoQuest = true;
            gameInfo.QuestNum = questIDToBeChange;
            QuestManager.GetInstance().UpdateQuest(gameInfo.QuestNum);
        }
        go_DialogueBar.SetActive(false); //��ȭ ������Ʈ�� ��Ȱ��ȭ ��Ų��.
        GameManager.Instance.dialogueInfo.player_InteractingFalse();

    }

    public void QuestGoal_UI(string text)
    {
        go_QuestBar.SetActive(true);
        if (Text_questGoal.text != text)
        {
            Text_questGoal.text = text;
        }
    }
    public void QuestGoal_UIFalse()
    {
        go_QuestBar.SetActive(false);
    }

}
