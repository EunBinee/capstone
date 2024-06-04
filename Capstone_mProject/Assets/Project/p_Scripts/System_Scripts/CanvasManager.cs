using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CanvasManager : MonoBehaviour
{
    //! 필수로 있어야하는 canvas
    public static CanvasManager instance = null;
    public static CanvasManager Instance
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

    [Space]
    [Header("player UI")]
    public string playerUIName;
    public GameObject playerUI;

    //* player UI
    [Space]
    [Header("Dialogue UI")]
    //* dialogue UI
    public string dialogueUIName;
    public GameObject dialogueUI;
    [Space]
    [Header("LoadingImg")]
    public string loadingImgName;
    public GameObject loadingImg;
    [Space]
    [Header("몬스터 HP 모음")]
    public string monster_HPBarName;
    public GameObject monster_HPBarUI;
    [Space]
    [Header("몬스터 데미지 UI 모음")]
    public string monsterDamageName;
    public GameObject monsterDamageUI;

    [Space]
    [Header("인벤토리 UI 모음")]
    public string inventoryUIName;
    public GameObject inventoryUI;

    [Space]
    [Header("버튼 UI 모음")]
    public string btnsUIName;
    public GameObject btnsUI;

    public CameraResolution cameraResolution;

    [Space]
    [Header("조사 UI 모음")]
    public string inspectionName;
    public GameObject inspectionUI;

    [Space]
    [Header("fadeInfadeOut")]
    public string fadeImgName;
    public GameObject fadeImg;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
        Init();
    }

    public void Init()
    {
        //* 메인씬

        //* 플레이어 UI
        if (playerUI == null)
            playerUI = GetCanvasUI(playerUIName);
        //* 다이어로그 UI
        if (dialogueUI == null)
            dialogueUI = GetCanvasUI(dialogueUIName);

        //* 로딩 이미지
        if (loadingImg == null)
        {
            loadingImg = GetCanvasUI(loadingImgName);
        }

        //* 몬스터 HP UI모아두는 곳
        if (monster_HPBarUI == null)
        {
            monster_HPBarUI = GetCanvasUI(monster_HPBarName);
        }
        //* 몬스터 데미지 UI 모아두는 곳
        if (monsterDamageUI == null)
        {
            monsterDamageUI = GetCanvasUI(monsterDamageName);
        }
        //* 인벤토리 UI
        if (inventoryUI == null)
        {
            inventoryUI = GetCanvasUI(inventoryUIName);
        }

        //* 버튼 UI
        if (btnsUI == null)
        {
            btnsUI = GetCanvasUI(btnsUIName);
        }

        cameraResolution = GetComponent<CameraResolution>();

        //* 조사 UI
        if (inspectionUI == null)
        {
            inspectionUI = GetCanvasUI(inspectionName);
        }

        if (fadeImg == null)
        {
            fadeImg = GetCanvasUI(fadeImgName);
        }
    }

    public GameObject GetCanvasUI(string name)
    {
        GameObject curObj = Resources.Load<GameObject>("CanvasPrefabs/" + name);
        if (curObj == null)
        {
            //Debug.LogError($"리소스 파일에 {name}프리펩 없음");
            return null;
        }
        curObj = Instantiate(curObj);
        RectTransform curObj_rect = curObj.GetComponent<RectTransform>();
        curObj.gameObject.transform.SetParent(this.transform);
        curObj_rect.anchoredPosition = Vector2.zero;

        return curObj;
    }
}
