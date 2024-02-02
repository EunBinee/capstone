using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMainSetting : MonoBehaviour
{
    //TODO: csv파일로 이름 적고 받아와서 배열안에 넣어주는 형식으로 변환할 예정.
    [SerializeField] private string canvasName;
    public GameObject canvasObj;
    [SerializeField] private string dialogueName;
    public GameObject dialogueObj;
    [SerializeField] private string gameManagerName;
    public GameObject gameManagerObj;
    [SerializeField] private string soundManagerName;
    public GameObject soundManagerObj;
    [SerializeField] private string uiManagerName;
    public GameObject uiManagerObj;
    [SerializeField] private string playerName;
    public GameObject playerObj;

    void Awake()
    {
        bool checkObj = CheckObject();

        if (!checkObj)
        {
            //생성.
            ObjInit();
        }
    }

    //*Dont Destroy Object가 없는지 있는지 체크---------------------//
    bool CheckObject()
    {
        GameObject obj = GameObject.Find(gameManagerName);
        if (obj != null)
        {
            return true; //* 이미 생성되어있음
        }
        else if (obj == null)
        {
            return false; //* 생성 안되어있음
        }
        return false;
    }
    //*--------------------------------------------------------------//
    public void ObjInit()
    {
        uiManagerObj = GetDontDestroyObj(uiManagerName);
        gameManagerObj = GetDontDestroyObj(gameManagerName);
        soundManagerObj = GetDontDestroyObj(soundManagerName);
        dialogueObj = GetDontDestroyObj(dialogueName);

        playerObj = GetDontDestroyObj(playerName);

        canvasObj = GetDontDestroyObj(canvasName);
        Setting();
    }

    public GameObject GetDontDestroyObj(string name)
    {
        GameObject curObj = Resources.Load<GameObject>("DontDestroyPrefabs/" + name);
        if (curObj == null)
        {
            Debug.LogError($"리소스 파일에 {name}프리펩 없음");
            return null;
        }
        curObj = Instantiate(curObj);
        Transform curObjTrans = curObj.GetComponent<Transform>();
        curObjTrans.position = Vector3.zero;

        return curObj;
    }

    public void Setting()
    {
        //* GameManager 내용
        GameManager.instance.gameData.player = playerObj;
        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        GameManager.instance.gameData.playerTargetPos = playerController._playerComponents.playerTargetPos;
        GameManager.instance.gameData.playerHeadPos = playerController._playerComponents.playerHeadPos;
        GameManager.instance.gameData.playerBackPos = playerController._playerComponents.playerBackPos;
        GameManager.instance.gameInfo = GameManager.instance.gameObject.GetComponent<GameInfo>();
        GameManager.instance.m_canvas = canvasObj.GetComponent<Canvas>();
        GameManager.instance.startInit();
        //* Player
        //playerObj.SetActive(false);
    }

}
