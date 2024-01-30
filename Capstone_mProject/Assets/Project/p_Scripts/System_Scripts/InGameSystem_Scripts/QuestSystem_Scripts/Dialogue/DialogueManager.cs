using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;
using UnityEngine.EventSystems;     //UI 클릭시 터치 이벤트 발생 방지.
using UnityEditor;
using System.Runtime.CompilerServices;
using System.Text;

public class DialogueManager : MonoBehaviour
{
    //! 전체 대화 관리
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
    public GameObject dialogueArrow; //대사 끝났을 경우 화살표ui


    public GameObject Quest_Button01;
    public GameObject Go_QuestDetail;
    public TMP_Text Text_QuestGoal; //퀘스트 목표 text
    public TMP_Text Text_QuestDetailGoal; //퀘스트 목표 text
    public TMP_Text Text_QuestDetailTitle; //퀘스트 제목 text
    public TMP_Text Text_QuestDetailContent; //퀘스트 세부내용 text
    public GameObject Text_Alarm; //튜토리얼, 알람등을 알려주는 text 

    public bool DoQuest;
    public bool IsQuestDetail;
    public bool isDialogue;
    //화살표애니메이션
    bool isArrowAnimating = false;

    private Transform text_tuto;
    private TMP_Text textComponent;

    void Start()
    {
        dialogueController = GetComponent<DialogueController>();
        SetUIVariable();
        gameInfo = GetComponent<GameInfo>();
        DoQuest = false;
        IsQuestDetail = false;
        isDialogue = false;

        text_tuto = Text_Alarm.transform.GetChild(0);
        textComponent = text_tuto.GetComponent<TMP_Text>();
    }

    public void SetUIVariable()
    {
        if (CanvasManager.instance.dialogueUI == null)
        {
            CanvasManager.instance.dialogueUI = CanvasManager.instance.GetCanvasUI(CanvasManager.instance.dialogueUIName);
            if (CanvasManager.instance.dialogueUI == null)
                return;
        }
        DialogueUI_info dialogueUI_Info = CanvasManager.instance.dialogueUI.GetComponent<DialogueUI_info>();
        go_DialogueBar = dialogueUI_Info.go_DialogueBar;
        Text_Dialogue = dialogueUI_Info.Text_Dialogue;
        Text_Name = dialogueUI_Info.Text_Name;
        ObjectTextBox_Button01 = dialogueUI_Info.ObjectTextBox_Button01;
        Text_Btn01 = dialogueUI_Info.Text_Btn01;
        ObjectTextBox_Button02 = dialogueUI_Info.ObjectTextBox_Button02;
        Text_Btn02 = dialogueUI_Info.Text_Btn02;
        dialogueArrow = dialogueUI_Info.dialogueArrow;

        Quest_Button01 = dialogueUI_Info.Quest_Button01;
        Go_QuestDetail = dialogueUI_Info.Go_QuestDetail;
        Text_QuestGoal = dialogueUI_Info.Text_QuestGoal;
        Text_QuestDetailGoal = dialogueUI_Info.Text_QuestDetailGoal;
        Text_QuestDetailTitle = dialogueUI_Info.Text_QuestDetailTitle;
        Text_QuestDetailContent = dialogueUI_Info.Text_QuestDetailContent;
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
    //화살표 애니메이션 
    private IEnumerator AnimateArrow()
    {
        isArrowAnimating = true;

        while (isArrowAnimating)
        {
            // 예시로 알파값을 조절하여 페이드 효과 구현
            float alpha = Mathf.PingPong(Time.time, 0.5f);
            Color arrowColor = dialogueArrow.GetComponent<Image>().color;
            arrowColor.a = alpha;
            dialogueArrow.GetComponent<Image>().color = arrowColor;

            yield return null;
        }
    }
    IEnumerator StartObjectTextBox(Dialogue dialogue, Item interaction_Item)
    {
        isDialogue = true;
        GameManager.instance.cameraController.stopRotation = true;
        GameManager.Instance.gameData.player.GetComponent<PlayerController>().StopToFalse();
        //yield return new WaitForSecondsRealtime(0.35f);
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
            //* 게임 멈춤 = 참
            player_InteractingTrue(); //플레이어 캐릭터가 상호작용 못하도록 제한.
                                      //UIManager.gameIsPaused = true;

            GameManager.GetInstance().dialogueManager.QuestGoal_UIFalse(); //퀘스트 완료시 ui 비활성화
            curlineContextLen = dialogue.lines[curPart][curLine].context.Length; //현재대사 배열 길이

            //대화 스킵 버튼
            if (Input.GetKeyDown(KeyCode.J))
            {
                curContext = curlineContextLen;
            }

            if (curContext < curlineContextLen)
            {
                //아직 문장이 끝나지 않은 경우
                if (ClickChoiceBtn)
                {
                    //선택지를 고르고 나면
                    Text_Dialogue.text = "";
                    Text_Name.text = "";
                    endChat_inController = false;
                    dialogueArrow.SetActive(false);

                    Text_Name.text = dialogue.lines[curPart][curLine].Name;
                    line = dialogue.lines[curPart][curLine].context[curContext].Replace("'", ",");

                    dialogueController.Chat_Obect(line);
                    curContext++;
                    ClickChoiceBtn = false;

                }
                else if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
                {
                    //선택지가 없고 아직 문장이 끝나지 않은경우
                    Text_Dialogue.text = "";
                    Text_Name.text = "";
                    endChat_inController = false;
                    dialogueArrow.SetActive(false);

                    Text_Name.text = dialogue.lines[curPart][curLine].Name;

                    line = dialogue.lines[curPart][curLine].context[curContext].Replace("'", ",");
                    dialogueController.Chat_Obect(line);
                    curContext++;
                }
            }
            yield return new WaitUntil(() => endChat_inController == true);
            //yield return StartCoroutine(DialogueManager.WaitForRealTime(0.01f));

            //대사가 끝나면 밑에 화살표ui 띄우기
            if (!dialogueArrow.activeSelf)
            {
                dialogueArrow.SetActive(true);
                StartCoroutine(AnimateArrow());
            }

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
                        dialogueArrow.SetActive(false);
                        //Debug.Log("선택지 있음");
                        //선택지 버튼 활성화
                        ObjectTextBox_Button01.SetActive(true);
                        ObjectTextBox_Button02.SetActive(true);

                        //선택지 버튼 누르면 어디로 갈지 결정
                        int firstOptDialogPart = dialogue.lines[curPart][curLine].choice.firstOptDialogNum;
                        int secondOptDialogPart = dialogue.lines[curPart][curLine].choice.secondOptDialogNum;


                        //선택지 대사 출력
                        Text_Btn01.text = dialogue.lines[curPart][curLine].choice.firstOption.Replace("'", ",");
                        Text_Btn02.text = dialogue.lines[curPart][curLine].choice.secondOption.Replace("'", ","); ;

                        //버튼안에 내용물 넣어줌.
                        UnityEngine.UI.Button btn01 = ObjectTextBox_Button01.GetComponent<UnityEngine.UI.Button>();
                        btn01.onClick.RemoveAllListeners();

                        //AddListener에 함수를 만들어 넣어줄 수 있지만..동적으로 계속 curPart가 변해야하기에..
                        //람다를 이용해서 익명함수를 만들어주었다.
                        btn01.onClick.AddListener(() =>
                        {
                            if (!Input.GetMouseButtonDown(0) || !Input.GetKeyDown(KeyCode.Return))// 아직 문장이 끝나지 않았다면
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

                        UnityEngine.UI.Button btn02 = ObjectTextBox_Button02.GetComponent<UnityEngine.UI.Button>();
                        btn02.onClick.RemoveAllListeners();
                        btn02.onClick.AddListener(() =>
                        {
                            if (!Input.GetMouseButtonDown(0) || !Input.GetKeyDown(KeyCode.Return))
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
                    //interaction_Item.dialogueNum = nextDialogueNum;
                    GameManager.Instance.gameInfo.DialogueNum = nextDialogueNum;


                    bool eventID = dialogue.lines[curPart][curLine].changeEvnetID; //이벤트ID 변경해야하는지
                    if (eventID) //변경할 이벤트가 있을 경우
                    {
                        changeEvnetID = eventID;
                        if (changeEvnetID)
                        {
                            //interaction_Item.dialogueNum = 1;
                            GameManager.Instance.gameInfo.DialogueNum = 1;
                            eventIDToBeChange = dialogue.lines[curPart][curLine].evnetIDToBeChange;
                        }
                        //Debug.Log("이벤트 변경 o");
                    }
                    else
                    {
                        changeEvnetID = false;
                        //Debug.Log("이벤트 변경 x");
                    }

                    bool endingID = dialogue.lines[curPart][curLine].changeEndingID;
                    if (endingID)
                    {
                        changeEndingID = endingID;
                        if (changeEndingID)
                        {
                            //interaction_Item.dialogueNum = 1;
                            GameManager.Instance.gameInfo.DialogueNum = 1;
                            endingIDToBeChange = dialogue.lines[curPart][curLine].endingIDToBeChange;
                        }
                        //Debug.Log("엔딩 변경 o");
                    }

                    bool questID = dialogue.lines[curPart][curLine].changeQuestID;
                    if (questID)
                    {
                        changeQuestID = questID;
                        if (changeQuestID)
                        {
                            //GameManager.Instance.gameInfo.DialogueNum = 1;
                            questIDToBeChange = dialogue.lines[curPart][curLine].questIDToBeChange;
                            //DoQuest = true;
                        }
                        //Debug.Log("퀘스트 변경 o");

                    }

                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0))
                    {
                        dialogueArrow.SetActive(false);
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

                    //Debug.Log("대사 이어지는 중..");
                }

            }


            yield return new WaitForSecondsRealtime(0.01f);
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
            //QuestManager.GetInstance().UpdateQuest(gameInfo.QuestNum);
        }
        go_DialogueBar.SetActive(false); //대화 UI 비활성화
                                         //GameManager.Instance.dialogueInfo.player_InteractingFalse();

        //yield return new WaitForSecondsRealtime(0.3f);
        player_InteractingFalse();


        isDialogue = false;
        GameManager.instance.cameraController.stopRotation = false;
        GameManager.Instance.gameData.player.GetComponent<PlayerController>().StopToFalse();
    }

    //퀘스트 디테일에서 퀘스트 제목 ui 활성화
    public void QuestDetailTitle_UI(string text)
    {
        Go_QuestDetail.SetActive(true);
        IsQuestDetail = true;
        //player_InteractingTrue();
        if (Text_QuestDetailTitle.text != text)
        {
            Text_QuestDetailTitle.text = text;
        }

    }
    //퀘스트 디테일에서 퀘스트 세부내용 ui 활성화
    public void QuestDetailContent_UI(string text)
    {
        //Go_QuestDetail.SetActive(true);
        //player_InteractingTrue();

        if (Text_QuestDetailContent.text != text)
        {
            Text_QuestDetailContent.text = text;
        }
    }
    //퀘스트 디테일에서 퀘스트 목표 ui 활성화
    public void QuestDetailGoal_UI(string text)
    {
        if (Text_QuestDetailGoal.text != text)
        {
            Text_QuestDetailGoal.text = text;
        }
    }
    //퀘스트 디테일 비활성화
    public void QuestDeailFalse()
    {
        Go_QuestDetail.SetActive(false);
        //player_InteractingFalse();

    }
    //퀘스트 목표 UI 출력 활성화
    public void QuestGoal_UI(string text)
    {
        Quest_Button01.SetActive(true);
        if (Text_QuestGoal.text != text)
        {
            Text_QuestGoal.text = text;
        }
    }
    //퀘스트 목표 UI 출력 비활성화
    public void QuestGoal_UIFalse()
    {
        Quest_Button01.SetActive(false);
    }
    //튜토리얼 ui 활성화
    public void TutorialUI(string text)
    {
        if (textComponent.text != text)
        {
            textComponent.text = text;
        }
        GameManager.Instance.PadeIn_Alpha(Text_Alarm, true, 255, 0.7f, true);
        GameManager.Instance.PadeIn_Alpha(textComponent.gameObject, true, 255, 0.7f, false);
    }
    //튜토리얼 ui 비활성화
    public void TutorialUIFalse(string text)
    {
        //Text_QuestGoal.enabled = false;
        GameManager.Instance.PadeIn_Alpha(Text_Alarm, false, 0, 1f, true);
        GameManager.Instance.PadeIn_Alpha(textComponent.gameObject, false, 0, 1f, false);
        //Text_Alarm.gameObject.SetActive(false);
    }

    //플레이어 움직임, 몬스터 등 상호작용 멈추게 함.
    public void player_InteractingTrue()
    {
        UIManager.Instance.Pause(false);
        GameManager.Instance.Stop_AllMonster();
        GameManager.Instance.gameData.player.GetComponent<PlayerController>().StopToFalse();
    }

    //멈춰있던 플레이어, 몬스터 등 원래대로 움직이도록 함. 
    public void player_InteractingFalse()
    {
        isDialogue = false;
        GameManager.Instance.gameData.player.GetComponent<PlayerController>().StopToFalse();
        UIManager.Instance.Resume();

        GameManager.Instance.Start_AllMonster();
    }
}
