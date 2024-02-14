using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

[Serializable]
public class DialogueInfo
{
    //public Item interaction_Item;

    //public int id;
    public void StartInteraction(GameObject _gameObject)
    {



        Npc interaction_Item = _gameObject.GetComponent<Npc>();


        if (interaction_Item != null)
        {
            if ((interaction_Item.preEndingNum != GameManager.Instance.gameInfo.EndingNum) || (interaction_Item.preEventNum != GameManager.Instance.gameInfo.EventNum))
            {
                // interaction_Item.preEndingNum = GameManager.Instance.gameInfo.EndingNum;
                // interaction_Item.preEventNum = GameManager.Instance.gameInfo.EventNum;
                //interaction_Item.dialogueNum = 1;
                //GameManager.Instance.gameInfo.DialogueNum = 1;
                // interaction_Item.questNum = GameManager.Instance.gameInfo.QuestNum;

            }
            //player_InteractingTrue(); //플레이어 캐릭터가 상호작용 못하도록 제한.
            //Debug.Log(interaction_Item.Name);
            //! 여기 고치기 
            if (GameManager.Instance.gameInfo.DialogueNum == 0)
            {
                interaction_Item.dialogueNum = 1;
            }
            else
            {
                interaction_Item.dialogueNum = GameManager.Instance.gameInfo.DialogueNum;
            }


            //if (interaction_Item.dialogueNum != 1)


            //1 01 001 01 01 00 
            //엔딩, npc id, 이벤트id, 대사단락번호, 퀘스트 번호
            int id = 0;
            //id = 0;
            string id_String = "";

            id_String += GameManager.Instance.gameInfo.EndingNum.ToString();
            //id_String += GameManager.Instance.gameInfo.LineNum.ToString();

            if (interaction_Item.id.ToString().Length == 1)
                id_String += "0" + interaction_Item.id.ToString();
            else
                id_String += interaction_Item.id.ToString();

            if (GameManager.Instance.gameInfo.EventNum.ToString().Length == 1)
                id_String += "00" + GameManager.Instance.gameInfo.EventNum.ToString();
            else if (GameManager.Instance.gameInfo.EventNum.ToString().Length == 2)
                id_String += "0" + GameManager.Instance.gameInfo.EventNum.ToString();
            else
                id_String += GameManager.Instance.gameInfo.EventNum.ToString();


            if (interaction_Item.dialogueNum.ToString().Length == 1)
                id_String += "0" + interaction_Item.dialogueNum.ToString();
            else
                id_String += interaction_Item.dialogueNum.ToString();

            if (interaction_Item.questNum.ToString().Length == 1)
                id_String += "0"; //+ interaction_Item.questNum.ToString();
            else
                id_String += "0";
            //id_String += interaction_Item.questNum.ToString();

            id = int.Parse(id_String);


            //Debug.Log(id.ToString());
            //interaction_Item.dialogueNum = GameManager.Instance.gameInfo.DialogueNum;

            if (interaction_Item.isNpc)
            {
                //상조작용이 가능한 NPC인 경우
                DialogueManager.instance.Action_NPC(id, interaction_Item);
                //GameManager.GetInstance().dialogueManager.Action_NPC(id, interaction_Item);
            }

        }

    }
    // //플레이어 움직임, 몬스터 등 상호작용 멈추게 함.
    // public void player_InteractingTrue()
    // {
    //     UIManager.Instance.Pause();

    // }
    // //멈춰있던 플레이어, 몬스터 등 원래대로 움직이도록 함. 
    // public void player_InteractingFalse()
    // {
    //     UIManager.Instance.Resume();
    // }
}