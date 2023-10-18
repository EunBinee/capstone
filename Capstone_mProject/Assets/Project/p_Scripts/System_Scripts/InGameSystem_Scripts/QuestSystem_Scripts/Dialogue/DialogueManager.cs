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

