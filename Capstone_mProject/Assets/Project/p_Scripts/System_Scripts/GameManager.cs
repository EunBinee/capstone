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

    public MonoBehaviour monoBehaviour { get; private set; }

    public ObjectPooling objectPooling;

    [SerializeField] private Button poolingBtn;

    void Awake()
    {
        InitGameManager();
    }

    private void InitGameManager()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);

        objectPooling.InitPooling();

        poolingBtn.onClick.RemoveAllListeners();
        poolingBtn.onClick.AddListener(() =>
        {
            objectPooling.ShowEffect("Prefab");

        });
    }

    //대화 시스템
    //DialogueManager dialogueManager;
    //GameInfo gameInfo;

    //static public GameManager GetInstance()
    //{
    //    return instance;
    //}

    //private void Start()
    //{
    //    dialogueManager = GetComponent<DialogueManager>();
    //    gameInfo = GetComponent<GameInfo>();
    //}

    //public void StartInteraction(GameObject gameObject)
    //{

    //    Item interaction_Item = gameObject.GetComponent<Item>();

    //    if(interaction_Item!=null)
    //    {
    //        if ((interaction_Item.preEndingNum != gameInfo.EndingNum) || (interaction_Item.preEventNum != gameInfo.EventNum))
    //        {
    //            //엔딩이나 이벤트가 바뀌었을 때
    //            interaction_Item.preEndingNum = gameInfo.EndingNum;
    //            interaction_Item.preEventNum = gameInfo.EventNum;
    //        }

    //        player_InteractingTrue(); //플레이어 상호작용 못하도록 제한

    //        int id = 0;
    //        string id_String = "";

    //        id_String += gameInfo.EndingNum.ToString();

    //        if (gameInfo.EventNum.ToString().Length == 1)
    //            id_String += "000" + gameInfo.EventNum.ToString();
    //        else if (gameInfo.EventNum.ToString().Length == 2)
    //            id_String += "00" + gameInfo.EventNum.ToString();
    //        else if (gameInfo.EventNum.ToString().Length == 3)
    //            id_String += "0" + gameInfo.EventNum.ToString();
    //        else
    //            id_String += gameInfo.EventNum.ToString();

    //        id = int.Parse(id_String);


    //        Debug.Log(id.ToString());

    //        if (interaction_Item.isNpc)
    //        {
    //            dialogueManager.Action_Npc(id, interaction_Item);
    //        }

    //    }
    //}
    //public void player_InteractingTrue()
    //{
    //    //플레이어의 상호작용을 막는다.

    //}
    //public void player_InteractingFalse()
    //{
    //    //플레이어가 다시 상호작용할 수 있도록 풀어준다.

    //}

    //2차수정========================================================

    //대화
    DialogueManager dialogueManager;
    GameInfo gameInfo;

    static public GameManager GetInstance()
    {
        return instance;
    }

    public void StartInteraction(GameObject gameObject)
    {
        dialogueManager = GetComponent<DialogueManager>(); //대사 시스템을 위한 스크립트
        //게임에 대한 전반적인 정보를 가지고 있는 스크립트. ex. 현재 게임의 엔딩 번호, 이벤트 번호
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

            player_InteractingTrue(); //플레이어 캐릭터가 상호작용 못하도록 제한.
            Debug.Log(interaction_Item.Name);


            //1 01 0001 01 
            //엔딩, npc id, 이벤트id, 대사단락번호 
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
                //상조작용이 가능한 NPC인 경우
                dialogueManager.Action_NPC(id, interaction_Item);
            }

        }

    }

    public void player_InteractingTrue()
    {
        //플레이어의 상호작용을 막는다.
        //playerController.interacting = true;
    }
    public void player_InteractingFalse()
    {
        //플레이어가 다시 상호작용할 수 있도록 풀어준다.
        //playerController.interacting = false;
    }

}

