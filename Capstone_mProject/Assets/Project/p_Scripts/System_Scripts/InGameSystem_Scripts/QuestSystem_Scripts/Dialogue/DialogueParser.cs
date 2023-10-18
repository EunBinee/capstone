using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueParser : MonoBehaviour
{
    //    public Dialogue[] Parse(string _CSVFileName)
    //    {
    //        List<Dialogue> dialogueList = new List<Dialogue>(); //대화 리스트 생성
    //        TextAsset csvData = Resources.Load<TextAsset>(_CSVFileName); //csv파일 가져옴

    //        string[] data = csvData.text.Split(new char[] { '\n' }); //엔터 기준으로 쪼겜.

    //        for (int i = 1; i < data.Length;)
    //        {

    //            string[] row = data[i].Split(new char[] { ',' });

    //            Dialogue dialogue = new Dialogue(); //대사 리스트 생성

    //            dialogue.name = row[3]; //캐릭터 이름
    //            dialogue.skipnum = row[7]; //스킵번호
    //            dialogue.endingNum = row[9]; //엔딩번호

    //            List<string> contextList = new List<string>();

    //            if (row[4].ToString() == "1") //선택지 여부가 1이면 선택지 있음. 
    //            {
    //                dialogue.isChoice = true;
    //            }
    //            else
    //            {
    //                dialogue.isChoice = false;
    //            }

    //            do
    //            {
    //                Debug.Log(row[4]);
    //                if (row[3] != "") //대사가 공백이 아니면 추가 
    //                {
    //                    contextList.Add(row[4]); //대사
    //                }

    //                if (++i < data.Length)
    //                {
    //                    row = data[i].Split(new char[] { ',' });
    //                }
    //                else
    //                {
    //                    break;
    //                }
    //                if (row[5].ToString() != "") //선택대사가 공백이 아니면 추가
    //                {
    //                    contextList.Add(row[6]);
    //                }

    //            } while (row[2].ToString() == ""); //여백이면 다음 대사 추가

    //            dialogue.contexts = contextList.ToArray();
    //            dialogueList.Add(dialogue);

    //        }

    //        return dialogueList.ToArray();
    //    }
    //}

    
    int eventNum_in = 0;
    int npcNum_in = 0;
    int dialogueNum_in = 0;  //E열
    int endingNum_in = 0;
    int lineNum_in = 0;

    //Line.cs에 쓰임
    string name_in;
    int choice_OneTwo = 0;
    bool startChoice = false;
    bool finishBreak = false;

    void Initialization()
    {
        eventNum_in = 0;
        npcNum_in = 0;
        dialogueNum_in = 0;
        endingNum_in = 0;
        lineNum_in = 0;

        name_in = "";
        choice_OneTwo = 0;
        startChoice = false;
        finishBreak = false;
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
                dialogue.eventNum = eventNum_in;   //현재 진행되고있는 이벤트번호
                dialogue.npcNum = npcNum_in;     //Npc 번호
                dialogue.endingNum = endingNum_in;  //ending번호
                dialogue.lineNum = lineNum_in;
            }
            else
            {

                eventNum_in = int.Parse(row[1].ToString());
                npcNum_in = int.Parse(row[2].ToString());
                if (row[11] == "")
                    endingNum_in = endingNum_in;
                else
                    endingNum_in = int.Parse(row[11].ToString());
                lineNum_in= int.Parse(row[4].ToString());

                dialogue.eventNum = eventNum_in;   //현재 진행되고있는 이벤트
                dialogue.npcNum = npcNum_in;     //Npc 번호
                dialogue.endingNum = endingNum_in;  //ending번호
                dialogue.lineNum = lineNum_in;
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
            if (row[4].ToString() == "")
            {
                dialogue.lineNum = lineNum_in;
            }
            else
            {
                lineNum_in = int.Parse(row[4].ToString());
                dialogue.lineNum = lineNum_in;
            }
            if (row[5].ToString() == "")
            {
                dialogue.lineNum = lineNum_in;
            }
            else
            {
                lineNum_in = int.Parse(row[4].ToString());
                dialogue.lineNum = lineNum_in;
            }
            do
            {
                List<Line> LineList = new List<Line>();
                List<string> contextList = new List<string>();  //Line.cs의 context[]에 넣기 위함

                do
                {
                    Line line = new Line();

                    //라인 초기화
                    line.isChoice = false;
                    line.isFinishLine = false;  //대화가 끝났는지 여부    
                    line.nextDialogueNum = dialogueNum_in;
                    line.nextLineNum=lineNum_in;

                    do
                    {
                        if (!startChoice) //선택지가 없을 경우
                        {
                            if (row[5].ToString() != "")
                            {
                                name_in = row[5].ToString(); //캐릭터 이름 
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
                                //선택지가 있는 경우
                                startChoice = true;
                                line.isChoice = true;
                            }
                            else if (int.Parse(row[7].ToString()) == 0)
                            {
                                line.isFinishLine = true;
                                //다음 대사의 순서 
                                int nextDialogueNum = 0;
                                bool isNumeric = int.TryParse(row[3].ToString(), out nextDialogueNum);


                                if (isNumeric) //만약 숫자 변환이 가능하다면
                                {
                                    line.nextDialogueNum = nextDialogueNum;
                                    //Debug.Log(nextDialogueNum);
                                }
                                if (!isNumeric && row[3].ToString() == "")
                                {
                                    //다음 단락으로 
                                    line.nextDialogueNum = dialogueNum_in+1;

                                }
                         

                                //대사가 끝나고 Evnet의 변화가 있는지도 확인
                                if (!line.changeEvnetID) //false일때만 변경 한번 
                                {
                                    int changeEvnetID = 0;
                                    isNumeric = int.TryParse(row[12].ToString(), out changeEvnetID);

                                    if (isNumeric) //만약 숫자 변환이 가능하다면
                                    {
                                        line.changeEvnetID = true;
                                        line.evnetIDToBeChange = changeEvnetID;
                                        Debug.Log(line.evnetIDToBeChange);
                                    }
                                }
                                //대사가 끝나고 Ending의 변화가 있는지도 확인
                                if (!line.changeEndingID)
                                {
                                    int changeEndingID = 0;
                                    isNumeric = int.TryParse(row[11].ToString(), out changeEndingID);

                                    if (isNumeric) //만약 숫자 변환이 가능하다면
                                    {
                                        line.changeEndingID = true;
                                        line.endingIDToBeChange = changeEndingID;
                                    }
                                }

                            }
                        }
                                                
                        //-----------------------------------------------------------
                        //여기서 i를 ++ 해줌
                        if (++i < (data.Length))
                        {
                            row = data[i].Split(new char[] { ',' });
                        }
                        else
                        {
                            finishBreak = true;
                            break;
                        }
                    } while (row[5].ToString() == "" );    //이름이 비어있으면 계속 대사가 이어짐.

                    line.context = contextList.ToArray();
                    LineList.Add(line);

                    contextList.Clear();


                    if (finishBreak)
                    {
                        break;
                    }


                } while (row[4].ToString() == "");    //csv E열.

                lines.Add(LineList);

                if (finishBreak)
                {
                    break;
                }
            } while (row[3].ToString() == "");    //csv D열 


            dialogue.lines = lines;
            dialogues.Add(dialogue);
        }
        return dialogues.ToArray();
    }

}
