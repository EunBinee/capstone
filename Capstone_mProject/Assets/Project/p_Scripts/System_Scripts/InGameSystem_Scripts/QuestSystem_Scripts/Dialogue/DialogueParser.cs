using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class DialogueParser
{
    int eventNum_in = 0;
    int npcNum_in = 0;
    public int dialogueNum_in = 0;  //E
    int endingNum_in = 0;
    int questNum_in = 0;
    //int lineNum_in = 0;

    //Line.cs에 쓰임
    string name_in;
    int choice_OneTwo = 0;
    bool startChoice = false;
    bool finishBreak = false;
    //bool doQuest = false;

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

    }
    public Dialogue[] DialogueParse(string csvFileName)
    {
        Initialization();

        List<Dialogue> dialogues = new List<Dialogue>();

        TextAsset csvData = Resources.Load<TextAsset>(csvFileName);
        string[] data = csvData.text.Split(new char[] { '\n' });

        for (int i = 1; i < (data.Length);)
        {
            string[] row = data[i].Split(new char[] { ',' });

            Dialogue dialogue = new Dialogue();
            List<List<Line>> lines = new List<List<Line>>();

            if (row[1].ToString() == "")
            {
                dialogue.eventNum = eventNum_in;   //이벤트id
                dialogue.npcNum = npcNum_in;     //Npcid
                dialogue.endingNum = endingNum_in;  //endingid
                dialogue.questNum = questNum_in;
                //dialogue.lineNum = lineNum_in;
            }
            else
            {

                eventNum_in = int.Parse(row[1].ToString());
                npcNum_in = int.Parse(row[2].ToString());
                if (row[11] == "")
                {
                    //endingNum_in = endingNum_in;
                }
                else
                    endingNum_in = int.Parse(row[11].ToString());
                if (row[10] == "")
                {
                    //questNum_in = questNum_in; 
                }
                else
                    questNum_in = int.Parse(row[10].ToString());
                //lineNum_in = int.Parse(row[4].ToString());

                dialogue.eventNum = eventNum_in;   //이벤트 id
                dialogue.npcNum = npcNum_in;     //Npc id
                dialogue.endingNum = endingNum_in;  //ending id
                dialogue.questNum = questNum_in;
                //dialogue.lineNum = lineNum_in;


            }

            if (row[3].ToString() == "")
            {
                dialogue.dialogueNum = dialogueNum_in;
            }
            else
            {
                dialogueNum_in = int.Parse(row[3].ToString());
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

                do
                {
                    Line line = new Line();

                    //라인 초기화
                    line.isChoice = false;
                    line.isFinishLine = false;  //대화 끝났는지 여부
                    line.nextDialogueNum = dialogueNum_in;
                    //line.DoQuest = false;
                    //line.nextLineNum = lineNum_in;

                    do
                    {
                        if (!startChoice) //선택지가 없을 경우
                        {
                            if (row[5].ToString() != "")
                            {
                                name_in = row[5].ToString(); //npc 이름 
                            }

                            line.Name = name_in;
                            contextList.Add(row[6].ToString());

                        }
                        else if (startChoice) //선택지가 있을 경우
                        {
                            choice_OneTwo++;

                            line.isChoice = true;
                            if (choice_OneTwo == 1)
                            {
                                //첫번째 질문인지
                                line.choice = new Choice();
                                line.choice.firstOption = row[8].ToString();
                                line.choice.firstOptDialogNum = int.Parse(row[9].ToString());

                            }
                            else if (choice_OneTwo == 2)
                            {
                                //두번째 질문인지
                                line.choice.secondOption = row[8].ToString();
                                line.choice.secondOptDialogNum = int.Parse(row[9].ToString());

                                choice_OneTwo = 0;
                                startChoice = false;
                            }
                        }

                        if (row[7].ToString() != "")
                        {
                            if (int.Parse(row[7].ToString()) == 1)
                            {
                                //선택지 있는 경우
                                startChoice = true;
                                line.isChoice = true;
                            }
                            else if (int.Parse(row[7].ToString()) == 0)
                            {
                                line.isFinishLine = true;

                                //선택지 없이 끝나는 경우 다음 대사 정해야함. 
                                int nextDialogueNum = 0;
                                bool isNumeric = int.TryParse(row[3].ToString(), out nextDialogueNum);


                                if (isNumeric) //만약 숫자 변환이 가능하다면 
                                {
                                    line.nextDialogueNum = nextDialogueNum;
                                }
                                if (!isNumeric && row[3].ToString() == "")
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
                                    isNumeric = int.TryParse(row[12].ToString(), out changeEvnetID);

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
                                    isNumeric = int.TryParse(row[11].ToString(), out changeEndingID);

                                    if (isNumeric)
                                    {
                                        line.changeEndingID = true;
                                        line.endingIDToBeChange = changeEndingID;
                                    }
                                }

                                if (!line.changeQuestID)
                                {
                                    int changeQuestID = 0;
                                    isNumeric = int.TryParse(row[10].ToString(), out changeQuestID);

                                    if (isNumeric)
                                    {
                                        line.changeQuestID = true;
                                        line.questIDToBeChange = changeQuestID;
                                    }

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
                    } while (row[5].ToString() == "");    //csv f열 비어있으면 대화 이어짐. 

                    line.context = contextList.ToArray();
                    LineList.Add(line);

                    contextList.Clear();


                    if (finishBreak)
                    {
                        break;
                    }


                } while (row[4].ToString() == "");    //csv E.

                lines.Add(LineList);

                if (finishBreak)
                {
                    break;
                }
            } while (row[3].ToString() == "");    //csv D


            dialogue.lines = lines;
            dialogues.Add(dialogue);
        }
        return dialogues.ToArray();
    }

    public Quest[] QuestParse(string csvFileName)
    {
        List<Quest> quests = new List<Quest>();

        TextAsset csvData = Resources.Load<TextAsset>(csvFileName);
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
}
