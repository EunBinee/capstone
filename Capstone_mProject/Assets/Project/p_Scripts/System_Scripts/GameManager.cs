using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.VersionControl.Asset;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
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
    public HPBarManager hPBarManager;

    //대화
    public DialogueInfo dialogueInfo;
    public DialogueManager dialogueManager;
    public GameInfo gameInfo;

    //*---------------------------------------------//
    public Canvas m_canvas;
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
        hPBarManager = GetComponent<HPBarManager>();
        cameraShake = GetComponent<CameraShake>();

        //instance = this;
        dialogueInfo = new DialogueInfo();
        dialogueManager = GetComponent<DialogueManager>(); //대사 시스템을 위한 스크립트
        //게임에 대한 전반적인 정보를 가지고 있는 스크립트. ex. 현재 게임의 엔딩 번호, 이벤트 번호
        gameInfo = GetComponent<GameInfo>();
    }

    static public GameManager GetInstance()
    {
        return instance;
    }


}

