using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class mainStartScene : MonoBehaviour
{
    public UnityEngine.UI.Button startBtn;
    public UnityEngine.UI.Button loadBtn;

    [Header("Debug")]
    public List<string> sceneNameList;
    public GameObject scenePrefab;
    public ScrollView scrollView;
    public Transform content;

    public TMP_Text noticeText; // 어느씬으로 이동할지 알려줌.

    string sceneName = "";
    string defaultSceneName = "StartScene 1";

    void Start()
    {
        SetButton();
        GameObject curObj = Instantiate(scenePrefab);
        curObj.transform.SetParent(content)
        //!! 리스트로 씬 이름 받기!!
        if (sceneNameList.Count == 0)
        {
            mainSceneName sceneNameDebug = curObj.GetComponent<mainSceneName>();
            sceneNameDebug.sceneName.text = defaultSceneName;
            sceneName = defaultSceneName;
            noticeText.text = $"이동할 씬 이름은 [ {sceneName} ]입니다.";
        }


    }

    public void SetButton()
    {
        Debug.Log($"{GameManager.instance.loadScene}");
        startBtn.onClick.AddListener(() => GameManager.instance.loadScene.LoadMainScene());
        loadBtn.onClick.AddListener(() => GameManager.instance.loadScene.LoadDataScene());
    }

    public void InputSceneName()
    {
        noticeText.text = $"이동할 씬 이름은 [ {sceneName} ]입니다.";
    }
}
