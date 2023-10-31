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

    public bool endChat_inController = false;  //dialogueController 

    void Start()
    {
        dialogueController = GetComponent<DialogueController>();
        gameInfo = GetComponent<GameInfo>();
    }

    public void Action_NPC(int id, Item interaction_Item)
    {
        //NPC�� ��縦 ������ �´�.
        //DBManager.GetInstance().NPC_diaglogues_Dictionary[id]�� ���ؼ� ���� id�� �´� Dialogue�� ������ �´�.
        Dialogue dialogue = DatabaseManager.GetInstance().NPC_diaglogues_Dictionary[id];
        StartCoroutine(StartObjectTextBox(dialogue, interaction_Item));
    }


    IEnumerator StartObjectTextBox(Dialogue dialogue, Item interaction_Item)
    {
        //�ؽ�Ʈ�� �����ִ� �ڷ�ƾ

        go_DialogueBar.SetActive(true); //��� UI Ȱ��ȭ
        Text_Dialogue.text = "";
        Text_Name.text = "";
        bool AllFinish = false; //��� ��簡 �������� Ȯ�ο�

        int curPart = 0; //Dialogue.cs�� lines[curPart][curLine] => lines[curPart]   
        int curLine = 0; //lines[curPart][curPart] 
        int curContext = 0; //lines[curPart][curLine].context[curContext] 

        bool isFinish = false; //��簡 ����. ALLFinish���� �ٸ�
        //�״��� ���� ��� ���� ��������, ��縦 �� ��,���� ���͸� ġ�� �ʾƼ� ���� ������ �������� ���� ����

        bool isChoice = false; // �������� ������ �ִ���
        bool choiceSettingF = false; //������ ������ ������ ������ ��ư���� �ؽ�Ʈ ��ȯ�� ������ ���´���
        bool ClickChoiceBtn = true; //�������� ������ ���, �ٷ� EnterŰ ���� �ٷ� ���� ���� �Ѿ���� �����ϴ� bool��


        int curlineContextLen;  //���� dialogue.line[curline]�� line���� context�� ����


        //ending�̳� Ending�� ��ȭ�� �Ҵ���
        bool changeEvnetID = false;
        int eventIDToBeChange = 0; //������Ʈ�� �̺�Ʈ ID
        bool changeEndingID = false;
        int endingIDToBeChange = 0; //������Ʈ�� ���� ID

        string line = ""; //���

        endChat_inController = true; //ChatController���� Chat �ִϸ��̼��� ��������, Ȯ�ο�.

        while (!AllFinish)
        {
            curlineContextLen = dialogue.lines[curPart][curLine].context.Length; //���� ����� �迭 ����

            if (curContext < curlineContextLen)
            {
                //���� ������ ������ �ʾҴٸ�..
                if (ClickChoiceBtn)
                {
                    //�������� ������ �� ���ĸ�?
                    Text_Dialogue.text = "";
                    Text_Name.text = "";
                    endChat_inController = false; //chat �ִϸ��̼� Ȯ�ο�.

                    Text_Name.text = dialogue.lines[curPart][curLine].Name;
                    line = dialogue.lines[curPart][curLine].context[curContext];

                    dialogueController.Chat_Obect(line);
                    curContext++;
                    ClickChoiceBtn = false;

                    //Debug.Log("d");

                }
                else if (Input.GetKeyDown(KeyCode.Return))
                {
                    //������ ������ ����, ���� ������ ������ �ʾҴٸ�..
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

            //������ context�� ������ ������ ���� ��� Ȯ���ϱ�
            if (curContext == curlineContextLen)
            {
                //������ context�� ������ ������ ���� ��, ��ȭ�� ���� ������, �������� �ִ��� Ȯ��

                isChoice = dialogue.lines[curPart][curLine].isChoice;
                isFinish = dialogue.lines[curPart][curLine].isFinishLine;

                if (isChoice)
                {
                    //���� ��簡 ������ �������� �ִ� ���
                    if (!choiceSettingF)
                    {
                        Debug.Log("ischoice Ʈ��");
                        //1. �������� ��ư���� ��Ȱ��ȭ -> Ȱ��ȭ
                        ObjectTextBox_Button01.SetActive(true);
                        ObjectTextBox_Button02.SetActive(true);

                        //�ɼ��� ������ ���� ���� ����
                        int firstOptDialogPart = dialogue.lines[curPart][curLine].choice.firstOptDialogNum;
                        int secondOptDialogPart = dialogue.lines[curPart][curLine].choice.secondOptDialogNum;


                        //�������� ����.
                        Text_Btn01.text = dialogue.lines[curPart][curLine].choice.firstOption;
                        Text_Btn02.text = dialogue.lines[curPart][curLine].choice.secondOption;

                        //��ư�ȿ� ���빰 �־���.
                        Button btn01 = ObjectTextBox_Button01.GetComponent<Button>();
                        btn01.onClick.RemoveAllListeners();
                        //AddListener�� �Լ��� ����� �־��� �� ������.. �������� ��� curPart�� ���ؾ��ϱ⿡..
                        //���ٸ� �̿��ؼ� �͸��Լ��� ������־���.
                        btn01.onClick.AddListener(() =>
                        {
                            if (!Input.GetKeyDown(KeyCode.Return))// ���� ������ ������ �ʾҴٸ�..
                            {
                                curPart = (firstOptDialogPart - 1); //curPart�� �������� �Ѿ��.
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
                    //�������� ���� ������ ������ ���� ���

                    //�� ����� ���� � ��縦 ó������
                    int nextDialogueNum = dialogue.lines[curPart][curLine].nextDialogueNum;
                    interaction_Item.dialogueNum = nextDialogueNum;

                    // ���� ��ȣ�ۿ��ϰ� �ִ� ������Ʈ�� Item�� �� ���� ������Ʈ ���ش�.
                    bool eventID = dialogue.lines[curPart][curLine].changeEvnetID; //���� Event�� �����ؾ��ϴ� ��..
                    if (eventID) //������ �̺�Ʈ�� ���� ��쿡�� ����
                    {
                        changeEvnetID = eventID;
                        if (changeEvnetID)
                        {
                            interaction_Item.dialogueNum = 1;
                            eventIDToBeChange = dialogue.lines[curPart][curLine].evnetIDToBeChange;
                        }
                        Debug.Log("�̺�Ʈ ��ȭ");

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
                        Debug.Log("���� ��ȭ");
                    }


                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        AllFinish = true;
                        Debug.Log("d3");
                    }
                }

                else if (!isChoice && !isFinish)
                {
                    //�������� ����, ������ ������ ���� ��찡 �ƴ� �ڿ� �ٸ� ����� ��簡 �� �ִ� ���
                    //��� �̾�����.
                    curLine++;
                    curContext = 0;
                    ClickChoiceBtn = false;

                    Debug.Log("��� �̾������ϴµ�..");
                }
            }
        }

        //������ ��ȭ�� �ִ���
        if (changeEndingID)
        {
            gameInfo.EndingNum = endingIDToBeChange;
            int nextEventNum = dialogue.lines[curPart][curLine].evnetIDToBeChange;
            interaction_Item.preEventNum = nextEventNum;

            Debug.Log(nextEventNum);
        }

        //�̺�Ʈ ID�� ��ȭ�� �ִ���
        if (changeEvnetID)
        {
            gameInfo.EventNum = eventIDToBeChange;
        }
        go_DialogueBar.SetActive(false); //��ȭ ������Ʈ�� ��Ȱ��ȭ ��Ų��.
        //GameManager.GetInstance().player_InteractingFalse();  //�÷��̾ ������ �� �ֵ��� ��ȣ�ۿ�ٽ� ���

    }

}
