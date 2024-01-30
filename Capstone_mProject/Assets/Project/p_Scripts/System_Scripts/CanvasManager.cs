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
    [Header("player UI")]
    [Space]
    public string playerUIName;
    public GameObject playerUI;

    //* player UI
    [Header("Dialogue UI")]
    [Space]
    //* dialogue UI
    public string dialogueUIName;
    public GameObject dialogueUI;

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
        //* 플레이어 UI
        if (playerUI == null)
            playerUI = GetCanvasUI(playerUIName);
        //* 다이어로그 UI
        if (dialogueUI == null)
            dialogueUI = GetCanvasUI(dialogueUIName);
    }

    public GameObject GetCanvasUI(string name)
    {
        GameObject curObj = Resources.Load<GameObject>("CanvasPrefabs/" + name);
        if (curObj == null)
        {
            Debug.LogError($"리소스 파일에 {name}프리펩 없음");
            return null;
        }
        curObj = Instantiate(curObj);
        RectTransform curObj_rect = curObj.GetComponent<RectTransform>();
        curObj.gameObject.transform.SetParent(this.transform);
        curObj_rect.anchoredPosition = Vector2.zero;

        return curObj;
    }
}
