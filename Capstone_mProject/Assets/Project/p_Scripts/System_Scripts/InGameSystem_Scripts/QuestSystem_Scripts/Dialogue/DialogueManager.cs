using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    //[SerializeField] GameObject go_DialogueBar;
    //[SerializeField] GameObject go_ChoiceBar;

    //[SerializeField] TextMeshProUGUI Text_Name;
    //[SerializeField] TextMeshProUGUI Text_Dialogue;

    //[SerializeField] Button FirstChoice;
    //[SerializeField] Button SecondChoice;
    //[SerializeField] Text Text_FirstChoice;
    //[SerializeField] Text Text_SecondChoice;

    //[Header("텍스트 출력 딜레이")]
    //[SerializeField] float textDelay;

    //Dialogue[] dialogues;

    //NpcAreaController theNC;

    //bool isDialogue = false; //대화중일 경우 true
    //bool isNext = false; //특정 키 입력 대기

    //int lineCount = 0; //대화 카운트 
    //int contextCount = 0; //대사 카운트 


    //private void Update()
    //{
    //    if (isDialogue)
    //    {
    //        if (isNext)
    //        {
    //            if (Input.GetKeyDown(KeyCode.Space))
    //            {
    //                isNext = false;
    //                Text_Dialogue.text = "";
    //                if (++contextCount < dialogues[lineCount].contexts.Length)
    //                {
    //                    StartCoroutine(TypeWriter());
    //                }
    //                else
    //                {
    //                    contextCount = 0;
    //                    if (++lineCount < dialogues.Length)
    //                    {
    //                        StartCoroutine(TypeWriter());
    //                    }
    //                    else
    //                    {
    //                        EndDialogue();
    //                    }
    //                }


    //            }
    //        }
    //    }
    //}
    //public void ShowDialogue(Dialogue[] p_dialogues) //대화 내용 보이게
    //{
    //    isDialogue = true; //대화시작
    //    Text_Dialogue.text = "";
    //    Text_Name.text = "";
    //    //theNC.SettingUI(false);
    //    dialogues = p_dialogues;

    //    StartCoroutine(TypeWriter());
    //}

    //public void ShowChoice(Dialogue[] p_dialogues) //대화 내용 보이게
    //{

    //    Text_FirstChoice.text = "";
    //    Text_SecondChoice.text = "";
    //    go_ChoiceBar.SetActive(true);


    //}

    //void EndDialogue()
    //{
    //    isDialogue = false; //대화 끝
    //    contextCount = 0;
    //    lineCount = 0;
    //    dialogues = null;
    //    isNext = false;
    //    //theNC.SettingUI(true);
    //    SettingUI(false);

    //}
    //void SettingUI(bool p_flag)
    //{
    //    go_DialogueBar.SetActive(p_flag); //대화 창 보이게 

    //}

    //IEnumerator TypeWriter()
    //{
    //    SettingUI(true);

    //    string t_ReplaceText = dialogues[lineCount].contexts[contextCount];
    //    t_ReplaceText = t_ReplaceText.Replace("'", ","); // '이 ,로 바꿔지게끔.
    //    t_ReplaceText = t_ReplaceText.Replace("\\n", "\n"); // \n이 줄바끔으로 바꿔지게끔.

    //    Text_Name.text = dialogues[lineCount].name; //이름 보여주게.
    //    for (int i = 0; i < t_ReplaceText.Length; i++)
    //    {
    //        Text_Dialogue.text += t_ReplaceText[i];
    //        yield return new WaitForSeconds(textDelay);
    //    }

    //    isNext = true;
    //    yield return null;
    //}


    //1차수정========================================================

    //GameInfo gameInfo;

    //[SerializeField] GameObject go_DialogueBar; //대화 ui
    //[SerializeField] GameObject go_ChoiceBar; //선택지 ui

    //[SerializeField] TextMeshProUGUI Text_Name; //이름 텍스트
    //[SerializeField] TextMeshProUGUI Text_Dialogue; //대화 텍스트

    //[SerializeField] GameObject FirstChoice;
    //[SerializeField] GameObject SecondChoice;
    //[SerializeField] Text Text_FirstChoice;
    //[SerializeField] Text Text_SecondChoice;

    //private void Start()
    //{
    //    gameInfo = GetComponent<GameInfo>();
    //}
    //public void Action_Npc(int id, Item interaction_Item)
    //{
    //    Debug.Log("Action Script NPC: " + id);  //디버깅용
    //    Dialogue dialogue = DatabaseManager.GetInstance().NPC_diaglogues_Dictionary[id];
    //    StartCoroutine(StartObjectTextBox(dialogue, interaction_Item));
    //}

    //IEnumerator StartObjectTextBox(Dialogue dialogue, Item interaction_Item)
    //{
    //    go_DialogueBar.SetActive(true); //대화창 ui 보이게 함.
    //    Text_Name.text = "";
    //    Text_Dialogue.text = "";

    //    bool AllFinish = false; //모든 대사가 끝났는지

    //    yield return new WaitForSeconds(0.5f);

    //    int curPart = 0; //Dialogue.cs에 lines[curPart][curLine]  B열
    //    int curLine = 0; // 캐릭터 이름 D열
    //    int curContext = 0; //lines[curPart][curLine].context[curContext] 대사 E열

    //    bool isFinish = false; //

    //    bool isChoice = false; //선택지 여부
    //    bool choiceSettingF = false; //선택지 가지고 있으면 선택지 버튼들의 텍스트 변환등 세팅을 끝냈는지
    //    bool ClickChoiceBtn = true; //선택지를 눌렀을 경우, 바로 Enter키 없이 바로 다음 대사로 넘어가도록 설정하는 bool값


    //    int curlineContextLen;  //현재 dialogue.line[curline]의 line안의 context의 길이


    //    bool choice_OpenObject = false; //대화를 끝마치고 새로운 씬이 열려야하는지 여부 //ex. 퀘스트
    //    string openObjectName = ""; //어떤 씬, 오브젝트를 열어야하는지, string으로 받음

    //    //ending이나 Ending의 변화가 았는지
    //    bool changeEvnetID = false;
    //    int eventIDToBeChange = 0; //업데이트할 이벤트 ID
    //    bool changeEndingID = false;
    //    int endingIDToBeChange = 0; //업데이트할 엔딩 ID

    //    string line = ""; //대사

    //    while (!AllFinish)
    //    {
    //        curlineContextLen = dialogue.lines[curPart][curLine].context.Length;

    //        if (curContext < curlineContextLen)
    //        {
    //            //아직 문장 안끝났다면
    //            if (ClickChoiceBtn)
    //            {
    //                //선택지 고르고 나서
    //                Text_Dialogue.text = "";
    //                Text_Name.text = "";

    //                curContext++;
    //                ClickChoiceBtn = false;

    //            }
    //            else if (Input.GetKeyDown(KeyCode.Space))
    //            {
    //                //선택지가 없다면
    //                Text_Dialogue.text = "";
    //                Text_Name.text = "";

    //                Text_Name.text = dialogue.lines[curPart][curLine].Name;
    //                line = dialogue.lines[curPart][curLine].context[curContext];

    //                curContext++;
    //            }
    //        }

    //        if (curContext == curlineContextLen) //마지막 context의 마지막 문장이 끝난 경우
    //        {
    //            isChoice = dialogue.lines[curPart][curLine].isChoice;
    //            isFinish = dialogue.lines[curPart][curLine].isFinishLine;

    //            if (isChoice)
    //            {
    //                //대사가 끝나고 선택지가 있는 경우
    //                if (!choiceSettingF)
    //                {
    //                    //선택지 버튼 활성화
    //                    FirstChoice.SetActive(true);
    //                    SecondChoice.SetActive(true);

    //                    int firstOptSkipLine = dialogue.lines[curPart][curLine].choice.firstSkipDialogNum;
    //                    int secondOptSkipLine = dialogue.lines[curPart][curLine].choice.secondSkipDialogNum;

    //                    bool firstChoice_OpenObject = dialogue.lines[curPart][curLine].choice.firstOpenQuest;
    //                    string firstOpenObjectName = dialogue.lines[curPart][curLine].choice.firstQuestName;
    //                    bool secondChoice_OpenObject = dialogue.lines[curPart][curLine].choice.secondOpenQuest;
    //                    string secondOpenObjectName = dialogue.lines[curPart][curLine].choice.secondQuestName;

    //                    Text_FirstChoice.text = dialogue.lines[curPart][curLine].choice.firstOption;
    //                    Text_SecondChoice.text = dialogue.lines[curPart][curLine].choice.secondOption;

    //                    Button btn01 = FirstChoice.GetComponent<Button>();
    //                    btn01.onClick.RemoveAllListeners();

    //                    btn01.onClick.AddListener(() =>
    //                    {
    //                        if (!Input.GetKeyDown(KeyCode.Space))
    //                        {
    //                            curPart = (firstOptSkipLine - 1);
    //                            curLine = 0;
    //                            curContext = 0;
    //                            FirstChoice.SetActive(false);
    //                            SecondChoice.SetActive(!false);

    //                            if (!choice_OpenObject)
    //                            {
    //                                choice_OpenObject = firstChoice_OpenObject;
    //                                openObjectName = firstOpenObjectName;
    //                            }
    //                            choiceSettingF = false;
    //                            ClickChoiceBtn = true;
    //                        }
    //                    });

    //                    Button btn02 = FirstChoice.GetComponent<Button>();
    //                    btn02.onClick.RemoveAllListeners();

    //                    btn02.onClick.AddListener(() =>
    //                    {
    //                        if (!Input.GetKeyDown(KeyCode.Space))
    //                        {
    //                            curPart = (firstOptSkipLine - 1);
    //                            curLine = 0;
    //                            curContext = 0;
    //                            FirstChoice.SetActive(false);
    //                            SecondChoice.SetActive(!false);

    //                            if (!choice_OpenObject)
    //                            {
    //                                choice_OpenObject = firstChoice_OpenObject;
    //                                openObjectName = firstOpenObjectName;
    //                            }
    //                            choiceSettingF = false;
    //                            ClickChoiceBtn = true;
    //                        }
    //                    });

    //                    choiceSettingF = true;
    //                }
    //            }
    //            else if (!isChoice && isFinish)
    //            {
    //                int nextDialogueNum = dialogue.lines[curPart][curLine].nextDialogueNum;
    //                interaction_Item.eventNum = nextDialogueNum;

    //                bool eventID = dialogue.lines[curPart][curLine].changeEvent;
    //                if (eventID) //변경할 이벤트가 있을 경우
    //                {
    //                    changeEndingID = eventID;
    //                    if (changeEndingID)
    //                    {
    //                        eventIDToBeChange = dialogue.lines[curPart][curLine].changeEventID;
    //                    }
    //                }

    //                bool endingID = dialogue.lines[curPart][curLine].changeEnding;
    //                if (endingID)
    //                {
    //                    changeEndingID = endingID;
    //                    if (changeEndingID)
    //                    {
    //                        endingIDToBeChange = dialogue.lines[curPart][curLine].changeEndingID;
    //                    }
    //                }
    //                if (Input.GetKeyDown(KeyCode.Space))
    //                {
    //                    AllFinish = true;
    //                }
    //            }

    //            else if (!isChoice && !isFinish)
    //            {
    //                curLine++;
    //                curContext = 0;
    //                ClickChoiceBtn = false;
    //            }
    //        }

    //    }
    //    if (choice_OpenObject)
    //    {
    //        //선택에 의해서 씬 등 다른게 열려야하는 경우
    //    }
    //    if (changeEndingID)
    //    {
    //        gameInfo.EndingNum = endingIDToBeChange;

    //    }

    //    go_DialogueBar.SetActive(false);
    //    //GameManager.GetInstance().player_InteractingFalse(); //플레이어 상호작용 허용
    //}


    //2차수정========================================================
    DialogueController dialogueController;
    GameInfo gameInfo; //게임의 전반적인 정보를 가진 스크립트
    public GameObject go_DialogueBar; //대사 UI
    public TMP_Text Text_Dialogue; //대사 텍스트
    public TMP_Text Text_Name; //이름 텍스트
    public GameObject ObjectTextBox_Button01; //버튼 1번 오브젝트
    public Text Text_Btn01; //버튼 1번 텍스트
    public GameObject ObjectTextBox_Button02; //버튼 2번 오브젝트
    public Text Text_Btn02; //버튼 2번 텍스트

    public bool endChat_inController = false;  //ChatController에서 Chat 애니메이션이 끝났는지, 확인용.

    void Start()
    {
        dialogueController = GetComponent<DialogueController>();
        gameInfo = GetComponent<GameInfo>();
    }

    public void Action_NPC(int id, Item interaction_Item)
    {
        //NPC의 대사를 가지고 온다.
        //DBManager.GetInstance().NPC_diaglogues_Dictionary[id]를 통해서 현재 id의 맞는 Dialogue를 가지고 온다.
        Dialogue dialogue = DatabaseManager.GetInstance().NPC_diaglogues_Dictionary[id];
        StartCoroutine(StartObjectTextBox(dialogue, interaction_Item));
    }


    IEnumerator StartObjectTextBox(Dialogue dialogue, Item interaction_Item)
    {
        //텍스트를 보여주는 코루틴

        go_DialogueBar.SetActive(true); //대사 UI 활성화
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
        int eventIDToBeChange = 0; //업데이트할 이벤트 ID
        bool changeEndingID = false;
        int endingIDToBeChange = 0; //업데이트할 엔딩 ID

        string line = ""; //대사

        endChat_inController = true; //ChatController에서 Chat 애니메이션이 끝났는지, 확인용.

        while (!AllFinish) 
        {
            curlineContextLen = dialogue.lines[curPart][curLine].context.Length; //현재 대사의 배열 길이

            if (curContext < curlineContextLen)
            {
                //아직 문장이 끝나지 않았다면..
                if (ClickChoiceBtn)
                {
                    //선택지를 고르고 난 직후면?
                    Text_Dialogue.text = "";
                    Text_Name.text = "";
                    endChat_inController = false; //chat 애니메이션 확인용.

                    Text_Name.text = dialogue.lines[curPart][curLine].Name;
                    line = dialogue.lines[curPart][curLine].context[curContext];

                    dialogueController.Chat_Obect(line);
                    curContext++;
                    ClickChoiceBtn = false;

                    //Debug.Log("d");

                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    //선택지 고름이 없고, 아직 문장이 끝나지 않았다면..
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
                        Debug.Log("ischoice 트루");
                        //1. 선택지의 버튼들을 비활성화 -> 활성화
                        ObjectTextBox_Button01.SetActive(true);
                        ObjectTextBox_Button02.SetActive(true);

                        //옵션을 누르면 어디로 갈지 결정
                        int firstOptDialogPart = dialogue.lines[curPart][curLine].choice.firstOptDialogNum;
                        int secondOptDialogPart = dialogue.lines[curPart][curLine].choice.secondOptDialogNum;


                        //선택지를 띄운다.
                        Text_Btn01.text = dialogue.lines[curPart][curLine].choice.firstOption;
                        Text_Btn02.text = dialogue.lines[curPart][curLine].choice.secondOption;

                        //버튼안에 내용물 넣어줌.
                        Button btn01 = ObjectTextBox_Button01.GetComponent<Button>();
                        btn01.onClick.RemoveAllListeners();
                        //AddListener에 함수를 만들어 넣어줄 수 있지만.. 동적으로 계속 curPart가 변해야하기에..
                        //람다를 이용해서 익명함수를 만들어주었다.
                        btn01.onClick.AddListener(() =>
                        {
                            if (!Input.GetKeyDown(KeyCode.Return))// 아직 문장이 끝나지 않았다면..
                            {
                                curPart = (firstOptDialogPart - 1); //curPart로 다음으로 넘어간다.
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

                    //이 대사후 다음 어떤 대사를 처야할지
                    int nextDialogueNum = dialogue.lines[curPart][curLine].nextDialogueNum;
                    interaction_Item.dialogueNum = nextDialogueNum;
 
                    // 현재 상호작용하고 있는 오브젝트의 Item에 그 값을 업데이트 해준다.
                    bool eventID = dialogue.lines[curPart][curLine].changeEvnetID; //현재 Event를 변경해야하는 가..
                    if (eventID) //변경할 이벤트가 있을 경우에만 변경
                    {
                        changeEvnetID = eventID;
                        if (changeEvnetID)
                        {
                            interaction_Item.dialogueNum = 1;
                            eventIDToBeChange = dialogue.lines[curPart][curLine].evnetIDToBeChange;
                        }
                        Debug.Log("이벤트 변화");

                    }

                    bool endingID = dialogue.lines[curPart][curLine].changeEndingID;
                    if (endingID)
                    {
                        changeEndingID = endingID;
                        if (changeEndingID)
                        {
                            interaction_Item.dialogueNum = 1;
                            endingIDToBeChange = dialogue.lines[curPart][curLine].endingIDToBeChange;
                        }
                        Debug.Log("엔딩 변화");
                    }


                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        AllFinish = true;
                        Debug.Log("d3");
                    }
                }

                else if (!isChoice && !isFinish)
                {
                    //선택지가 없고, 완전히 문단이 끝난 경우가 아닌 뒤에 다른 사람의 대사가 더 있는 경우
                    //계속 이어진다.
                    curLine++;
                    curContext = 0;
                    ClickChoiceBtn = false;

                    Debug.Log("대사 이어져야하는데..");
                }
            }
        }

        //엔딩에 변화가 있는지
        if (changeEndingID)
        {
            gameInfo.EndingNum = endingIDToBeChange;
            int nextEventNum = dialogue.lines[curPart][curLine].evnetIDToBeChange;
            interaction_Item.preEventNum = nextEventNum;

            Debug.Log(nextEventNum);
        }

        //이벤트 ID에 변화가 있는지
        if (changeEvnetID)
        {
            gameInfo.EventNum = eventIDToBeChange;
        }
        go_DialogueBar.SetActive(false); //대화 오브젝트를 비활성화 시킨다.
        GameManager.GetInstance().player_InteractingFalse();  //플레이어가 움직일 수 있도록 상호작용다시 허용
    
    }

}

