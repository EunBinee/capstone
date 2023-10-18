using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public HPBarManager hPBarManager;

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
    }


}
