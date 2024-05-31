using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Data;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[Serializable]
public class DialogueParser
{
    int eventNum_in = 0;
    int npcNum_in = 0;
    public int dialogueNum_in = 0;  //E
    int endingNum_in = 0;
    int questNum_in = 0;
    int reasoningNum_in = 0;
    //int lineNum_in = 0;

    //Line.cs에 쓰임
    string name_in;
    int choice_OneTwo = 0;
    bool startChoice = false;
    bool finishBreak = false;
    bool startReasoning = false; //추리
    //bool doQuest = false;

    //csv 
    int csv_Event = 1;
    int csv_Npc = 2;
    int csv_Context = 3; //대화단락
    int csv_Dialogue = 4; //몇번째대사인지
    int csv_Name = 5;
    int csv_Line = 6;//대사
    int csv_Portrait = 7; //초상화
    int csv_Choice = 8;
    int csv_ChoiceLine = 9; //선택지대사
    int csv_SkipLine = 10;
    int csv_Quest = 11;
    int csv_Reasoning = 12;
    int csv_Ending = 13; //12;
    int csv_AfterEnding = 14;//13;


    void Initialization() //초기화
    {
        eventNum_in = 0;
        npcNum_in = 0;
        dialogueNum_in = 0;
        endingNum_in = 0;
        questNum_in = 0;
        //lineNum_in = 0;

        name_in = "";
        choice_OneTwo = 0;
        startChoice = false;
        finishBreak = false;
        //doQuest = false;

        //csv
        csv_Event = 1;
        csv_Npc = 2;
        csv_Context = 3; //대화단락
        csv_Dialogue = 4; //몇번째대사인지
        csv_Name = 5;
        csv_Line = 6;//대사
        csv_Portrait = 7;
        csv_Choice = 8;
        csv_ChoiceLine = 9; //선택지대사
        csv_SkipLine = 10;
        csv_Quest = 11;
        csv_Reasoning = 12;
        csv_Ending = 13;
        csv_AfterEnding = 14;
    }
    public Dialogue[] DialogueParse(string csvFileName)
    {
        Initialization();

        List<Dialogue> dialogues = new List<Dialogue>();

        TextAsset csvData = Resources.Load<TextAsset>("Dialogues/" + csvFileName);
        string[] data = csvData.text.Split(new char[] { '\n' });

        for (int i = 1; i < (data.Length);)
        {
            string[] row = data[i].Split(new char[] { ',' });

            Dialogue dialogue = new Dialogue();
            List<List<Line>> lines = new List<List<Line>>();

            if (row[csv_Event].ToString() == "")
            {
                dialogue.eventNum = eventNum_in;   //이벤트id
                dialogue.npcNum = npcNum_in;     //Npcid
                dialogue.endingNum = endingNum_in;  //endingid
                dialogue.questNum = questNum_in;
                dialogue.reasoningNum = reasoningNum_in;
                //dialogue.lineNum = lineNum_in;
            }
            else
            {
                eventNum_in = int.Parse(row[csv_Event].ToString());
                npcNum_in = int.Parse(row[csv_Npc].ToString());
                if (row[csv_Ending] == "")
                {
                    //endingNum_in = endingNum_in;
                }
                else
                    endingNum_in = int.Parse(row[csv_Ending].ToString());
                if (row[csv_Quest] == "")
                {
                    //questNum_in = questNum_in; 
                }
                else
                    questNum_in = int.Parse(row[csv_Quest].ToString());
                //lineNum_in = int.Parse(row[4].ToString());
                if (row[csv_Portrait] == "")
                {
                    //dialogue.
                }
                if(row[csv_Reasoning]!="")
                {
                    //reasoningNum_in = reasoningNum_in;
                    reasoningNum_in = int.Parse(row[csv_Reasoning].ToString());
                }
                dialogue.eventNum = eventNum_in;   //이벤트 id
                dialogue.npcNum = npcNum_in;     //Npc id
                dialogue.endingNum = endingNum_in;  //ending id
                dialogue.questNum = questNum_in;
                dialogue.reasoningNum = reasoningNum_in;
                //dialogue.lineNum = lineNum_in;


            }

            if (row[csv_Context].ToString() == "")
            {
                dialogue.dialogueNum = dialogueNum_in;
            }
            else
            {
                dialogueNum_in = int.Parse(row[csv_Context].ToString());
                dialogue.dialogueNum = dialogueNum_in;
            }
            
            // if (row[4].ToString() == "")
            // {
            //     //dialogue.lineNum = lineNum_in;
            // }
            // else
            // {
            //     //lineNum_in = int.Parse(row[4].ToString());
            //     //dialogue.lineNum = lineNum_in;
            // }
            // if (row[5].ToString() == "")
            // {
            //     //dialogue.lineNum = lineNum_in;
            // }
            // else
            // {
            //     //lineNum_in = int.Parse(row[4].ToString());
            //     //dialogue.lineNum = lineNum_in;
            // }
            do
            {
                List<Line> LineList = new List<Line>();
                List<string> contextList = new List<string>();  //Line.cs의 context[]에 넣기 위함. 
                List<string> spriteList = new List<string>(); //초상화

                do
                {
                    Line line = new Line();

                    //라인 초기화
                    line.isChoice = false;
                    line.isFinishLine = false;  //대화 끝났는지 여부
                    line.nextDialogueNum = dialogueNum_in;
                    line.isReasoning = false;
                    //line.DoQuest = false;
                    //line.nextLineNum = lineNum_in;

                    do
                    {
                        if (!startChoice) //선택지가 없을 경우
                        {
                            if (row[csv_Name].ToString() != "")
                            {
                                name_in = row[csv_Name].ToString(); //npc 이름 
                            }

                            line.Name = name_in;
                            contextList.Add(row[csv_Line].ToString());
                            spriteList.Add(row[csv_Portrait].ToString());

                        }
                        else if (startChoice) //선택지가 있을 경우
                        {
                            choice_OneTwo++;

                            line.isChoice = true;

                            if (choice_OneTwo == 1)
                            {
                                //첫번째 질문인지
                                line.choice = new Choice();
                                line.choice.firstOption = row[csv_ChoiceLine].ToString();
                                line.choice.firstOptDialogNum = int.Parse(row[csv_SkipLine].ToString());

                            }
                            else if (choice_OneTwo == 2)
                            {
                                //두번째 질문인지
                                line.choice.secondOption = row[csv_ChoiceLine].ToString();
                                line.choice.secondOptDialogNum = int.Parse(row[csv_SkipLine].ToString());

                                choice_OneTwo = 0;
                                startChoice = false;
                            }

                        }


                        if(startReasoning)
                        {
                            line.isReasoning = true; 
                        }

                        if(row[csv_Reasoning].ToString()!="")
                        {
                            startReasoning = true;
                            line.isReasoning = true;
                        }

                        if (row[csv_Choice].ToString() != "")
                        {
                            if (int.Parse(row[csv_Choice].ToString()) == 1)
                            {
                                //선택지 있는 경우
                                startChoice = true;
                                line.isChoice = true;
                            }
                            else if (int.Parse(row[csv_Choice].ToString()) == 0)
                            {
                                line.isFinishLine = true;

                                //선택지 없이 끝나는 경우 다음 대사 정해야함. 
                                int nextDialogueNum = 0;
                                bool isNumeric = int.TryParse(row[csv_Npc].ToString(), out nextDialogueNum);


                                if (isNumeric) //만약 숫자 변환이 가능하다면 
                                {
                                    line.nextDialogueNum = nextDialogueNum;
                                }
                                if (!isNumeric && row[csv_Npc].ToString() == "")
                                {
                                    line.nextDialogueNum = dialogueNum_in + 1;
                                }
                                // if (row[12].ToString() != "")
                                // {
                                //     line.nextDialogueNum = 1;
                                // }

                                // line.nextDialogueNum = dialogueNum_in + 1;




                                //if (!isNumeric && row[9].ToString() == "")
                                //{
                                //    line.nextDialogueNum = dialogueNum_in;
                                //}


                                //대사가 끝나고 Evnet의 변화가 있는지
                                if (!line.changeEvnetID) //false일때 변화
                                {
                                    int changeEvnetID = 0;
                                    isNumeric = int.TryParse(row[csv_AfterEnding].ToString(), out changeEvnetID);

                                    if (isNumeric)
                                    {
                                        line.changeEvnetID = true;
                                        line.evnetIDToBeChange = changeEvnetID;
                                        //Debug.Log(line.evnetIDToBeChange);
                                        //Debug.Log("이벤트 변화 있음");
                                    }
                                }
                                //대사가 끝나고 엔딩의 변화가 있는지
                                if (!line.changeEndingID)
                                {
                                    int changeEndingID = 0;
                                    isNumeric = int.TryParse(row[csv_Ending].ToString(), out changeEndingID);

                                    if (isNumeric)
                                    {
                                        line.changeEndingID = true;
                                        line.endingIDToBeChange = changeEndingID;
                                    }
                                }

                                if (!line.changeQuestID)
                                {
                                    int changeQuestID = 0;
                                    isNumeric = int.TryParse(row[csv_Quest].ToString(), out changeQuestID);

                                    if (isNumeric)
                                    {
                                        line.changeQuestID = true;
                                        line.questIDToBeChange = changeQuestID;
                                    }

                                }

                            }
                        }
                        if(row[csv_Quest].ToString() != "")
                        {
                            int nextDialogueNum = 0;
                            bool isNumeric = int.TryParse(row[csv_Npc].ToString(), out nextDialogueNum);
                            if (!line.changeQuestID)
                                {
                                    int changeQuestID = 0;
                                    isNumeric = int.TryParse(row[csv_Quest].ToString(), out changeQuestID);

                                    if (isNumeric)
                                    {
                                        line.changeQuestID = true;
                                        line.questIDToBeChange = changeQuestID;
                                    }
                                }
                           //Debug.Log(line.questIDToBeChange);
                           if(row[csv_Event].ToString() != "")
                            {
                                int changeEvnetID = 0;
                                //int nextDialogueNum = 0;
                                //bool isNumeric = int.TryParse(row[csv_Npc].ToString(), out nextDialogueNum);
                                isNumeric = int.TryParse(row[csv_Event].ToString(), out changeEvnetID);

                                if (isNumeric && line.evnetIDToBeChange !=changeEvnetID)
                                {
                                    line.changeEvnetID = true;
                                    line.evnetIDToBeChange = changeEvnetID+1;
                                    //Debug.Log(line.evnetIDToBeChange);
                                    //Debug.Log("이벤트 변화 있음");
                                }
                            }
                        }
        

                        //-----------------------------------------------------------
                        //여기서 i를 ++
                        if (++i < (data.Length))
                        {
                            row = data[i].Split(new char[] { ',' });
                        }
                        else
                        {
                            finishBreak = true;
                            break;
                        }
                    } while (row[csv_Name].ToString() == "");    //csv f열 비어있으면 대화 이어짐. 

                    line.context = contextList.ToArray();
                    line.spriteName = spriteList.ToArray();
                    LineList.Add(line);

                    contextList.Clear();


                    if (finishBreak)
                    {
                        break;
                    }


                } while (row[csv_Dialogue].ToString() == "");    //csv E.

                lines.Add(LineList);

                if (finishBreak)
                {
                    break;
                }
            } while (row[csv_Npc].ToString() == "");    //csv D


            dialogue.lines = lines;
            dialogues.Add(dialogue);
        }
        return dialogues.ToArray();
    }

    public Quest[] QuestParse(string csvFileName)
    {
        List<Quest> quests = new List<Quest>();

        TextAsset csvData = Resources.Load<TextAsset>("Dialogues/" + csvFileName);
        string[] data = csvData.text.Split(new char[] { '\n' });

        for (int i = 1; i < data.Length;)
        {
            string[] row = data[i].Split(new char[] { ',' });
            Quest quest = new Quest();
            quest.questId = int.Parse(row[0].ToString());
            quest.questClearValue = int.Parse(row[5].ToString());
            quest.questClearString = row[6].ToString();

            if (quest.questClearString == "") //퀘목표(숫자x)가 비어있지않으먼 
            {
                quest.questClearString = "";
            }
            else
            {
                quest.questClearString = row[6].ToString(); ;
            }

            quest.questGoal = new List<string>();
            quest.questTitle = new List<string>();
            quest.questContent = new List<string>();

            do
            {
                quest.questGoal.Add(row[3].ToString()); //퀘스트 목표            
                quest.questTitle.Add(row[2].ToString()); //퀘스트 제목
                quest.questContent.Add(row[4].ToString()); //퀘스트 세부내용

                if (++i < data.Length)
                {
                    row = data[i].Split(new char[] { ',' });
                }
                else
                {
                    finishBreak = true;
                    break;
                }
            } while (row[0].ToString() == "");
            quests.Add(quest);
        }
        return quests.ToArray();
    }

    public Reasoning[] ReasoningParse(string csvFileName)
    {
        List<Reasoning> reasonings = new List<Reasoning>();

        TextAsset csvData = Resources.Load<TextAsset>("Dialogues/" + csvFileName);
        string[] data = csvData.text.Split(new char[] { '\n' });

        for (int i = 1; i < data.Length;)
        {
            string[] row = data[i].Split(new char[] { ',' });
            Reasoning reasoning = new Reasoning();

            reasoning.reasoningID = int.Parse(row[0].ToString());
            reasoning.question = row[1].ToString();
            reasoning.answer = row[4].ToString();

            reasoning.reasoningChoice = new List<string>();
            reasoning.reasoningDialogue = new List<string>();
            do
            {
                if(row[2].ToString()!="")
                {
                    reasoning.reasoningChoice.Add(row[2].ToString());
                    reasoning.reasoningDialogue.Add(row[3].ToString());
                }
               

                
                if(startReasoning)
                {
                    //! 여기서 부터 추리 시스템 하면됨.. 언젠가... csv지옥이야아ㅏ아아
                    //Debug.Log(reasoning.reasoningChoice.Count);
                
                }

                 if (++i < data.Length)
                {
                    row = data[i].Split(new char[] { ',' });
                }
                else
                {
                    finishBreak = true;
                    break;
                }

            } while (row[0].ToString() == "");
            reasonings.Add(reasoning);
        }
        return reasonings.ToArray();
    }
}