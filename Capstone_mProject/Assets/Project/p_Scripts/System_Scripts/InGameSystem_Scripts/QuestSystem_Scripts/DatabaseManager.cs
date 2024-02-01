using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    static public DatabaseManager instance;
    public DialogueParser dialogueParser;
    public string csvFileName_NPC;
    public string csvFileName_Quest;

    //npc
    public Dictionary<int, Dialogue> NPC_diaglogues_Dictionary = new Dictionary<int, Dialogue>(); //csvFileName_NPC
    public Dictionary<int, Quest> Quest_Dictionary = new Dictionary<int, Quest>(); //csvFileName_Quest

    int eventID;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            //NPC Dialogue
            DialogueParser(csvFileName_NPC, true);
            QuestParser(csvFileName_Quest, 0);

        }
    }

    static public DatabaseManager GetInstance()
    {
        return instance;
    }


    public void DialogueParser(string csvFileName, bool isNPC)
    {
        //NPC
        Dialogue[] dialogues = dialogueParser.DialogueParse(csvFileName);

        for (int i = 0; i < dialogues.Length; i++)
        {
            //1 01 001 01 01 00 
            //엔딩, npc id, 이벤트id, 대사단락번호, 퀘스트 번호
            int id = 0;
            //id = GameManager.Instance.dialogueInfo.id;
            string id_String = "";

            id_String += dialogues[i].endingNum.ToString();
            //id_String += dialogues[i].lineNum.ToString();

            if (dialogues[i].npcNum.ToString().Length == 1)
                id_String += "0" + dialogues[i].npcNum.ToString();
            else
                id_String += dialogues[i].npcNum.ToString();

            if (dialogues[i].eventNum.ToString().Length == 1)
                id_String += "00" + dialogues[i].eventNum.ToString();
            else if (dialogues[i].eventNum.ToString().Length == 2)
                id_String += "0" + dialogues[i].eventNum.ToString();
            else
                id_String += dialogues[i].eventNum.ToString();

            if (dialogues[i].dialogueNum.ToString().Length == 1)
                id_String += "0" + dialogues[i].dialogueNum.ToString();
            else
                id_String += dialogues[i].dialogueNum.ToString();

            if (dialogues[i].questNum.ToString().Length == 1)
            {
                id_String += "0"; //+ dialogues[i].questNum.ToString();
                //GameManager.Instance.dialogueManager.DoQuest = true;
            }
            else
                id_String += "0";
            //id_String += dialogues[i].questNum.ToString();

            id = int.Parse(id_String);


            if (isNPC)
            {
                //idList01.Add(id);   //Start() Debug용
                NPC_diaglogues_Dictionary[id] = dialogues[i];
            }

        }
        Debug.Log(csvFileName + "완료!");
    }

    public void QuestParser(string csvFileName, int questNum)
    {
        //DialogueParser dialogueParser = GetComponent<DialogueParser>();
        Quest[] quests = dialogueParser.QuestParse(csvFileName);

        for (int i = 0; i < quests.Length; i++)
        {
            int id = 0;
            id = quests[i].questId;
            Quest_Dictionary[id] = quests[i];

        }
        Debug.Log(csvFileName + "완료!");
    }
}
