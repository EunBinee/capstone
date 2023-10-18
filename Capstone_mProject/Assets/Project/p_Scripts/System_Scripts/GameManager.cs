using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Diagnostics.Tracing;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    public GameData gameData;


    public ObjectPooling objectPooling;

    //* 카메라 제어---------------------------------//
    public CameraShake cameraShake;
    //* --------------------------------------------//
    void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);

        objectPooling.InitPooling();
        cameraShake = GetComponent<CameraShake>();
    }


    //��ȭ �ý���
    DialogueManager dialogueManager;
    GameInfo gameInfo;

    static public GameManager GetInstance()
    {
        return instance;
    }

    public void StartInteraction(GameObject gameObject)
    {
        dialogueManager = GetComponent<DialogueManager>(); //��� �ý����� ���� ��ũ��Ʈ
        //���ӿ� ���� �������� ������ ������ �ִ� ��ũ��Ʈ. ex. ���� ������ ���� ��ȣ, �̺�Ʈ ��ȣ
        gameInfo = GetComponent<GameInfo>();
  
        Item interaction_Item = gameObject.GetComponent<Item>();

        if (interaction_Item != null)
        {
  
            if ((interaction_Item.preEndingNum != gameInfo.EndingNum) || (interaction_Item.preEventNum != gameInfo.EventNum))
            {
                interaction_Item.preEndingNum = gameInfo.EndingNum;
                interaction_Item.preEventNum = gameInfo.EventNum;
                interaction_Item.dialogueNum = 1;
            }

            player_InteractingTrue(); //�÷��̾� ĳ���Ͱ� ��ȣ�ۿ� ���ϵ��� ����.
            Debug.Log(interaction_Item.Name);


            //1 01 0001 01 
            //����, npc id, �̺�Ʈid, ���ܶ���ȣ 
            int id = 0;
            string id_String = "";

            id_String += gameInfo.EndingNum.ToString();
            id_String += gameInfo.LineNum.ToString();

            if (interaction_Item.id.ToString().Length == 1)
                id_String += "0" + interaction_Item.id.ToString();
            else
                id_String += interaction_Item.id.ToString();


            if (gameInfo.EventNum.ToString().Length == 1)
                id_String += "000" + gameInfo.EventNum.ToString();
            else if (gameInfo.EventNum.ToString().Length == 2)
                id_String += "00" + gameInfo.EventNum.ToString();
            else if (gameInfo.EventNum.ToString().Length == 3)
                id_String += "0" + gameInfo.EventNum.ToString();
            else
                id_String += gameInfo.EventNum.ToString();


            if (interaction_Item.dialogueNum.ToString().Length == 1)
                id_String += "0" + interaction_Item.dialogueNum.ToString();
            else
                id_String += interaction_Item.dialogueNum.ToString();

            id = int.Parse(id_String);


            Debug.Log(id.ToString());

            if (interaction_Item.isNpc)
            {
                //�����ۿ��� ������ NPC�� ���
                dialogueManager.Action_NPC(id, interaction_Item);
            }

        }

    }

    public void player_InteractingTrue()
    {
        //�÷��̾��� ��ȣ�ۿ��� ���´�.
        //playerController.interacting = true;
    }
    public void player_InteractingFalse()
    {
        //�÷��̾ �ٽ� ��ȣ�ۿ��� �� �ֵ��� Ǯ���ش�.
        //playerController.interacting = false;
    }

}

