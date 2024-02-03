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
    public List<string> curSelectSceneNameList;
    List<mainSceneName> sceneList;
    Color selectColor;
    public GameObject scenePrefab;
    public ScrollView scrollView;
    public Transform content;

    public TMP_Text noticeText; // 어느씬으로 이동할지 알려줌.
    int curSceneIndex;
    public string curSelectSceneName = ""; //* 현재 이동하는 씬 (불러오기 X)
    public string defaultcurSelectSceneName = "StartScene 1";

    void Start()
    {
        SetButton();
        sceneList = new List<mainSceneName>();
        selectColor = GameManager.Instance.HexToColor("#FF8C80");

        //!! 리스트로 씬 이름 받기!!
        if (curSelectSceneNameList.Count <= 0)
        {
            GameObject curObj = Instantiate(scenePrefab);
            curObj.transform.SetParent(content);

            mainSceneName curSelectSceneNameDebug = curObj.GetComponent<mainSceneName>();
            curSelectSceneNameDebug.sceneName.text = defaultcurSelectSceneName;
            curSelectSceneName = defaultcurSelectSceneName;
            noticeText.text = $"이동할 씬 이름은 [ {curSelectSceneName} ]입니다.";

            sceneList.Add(curSelectSceneNameDebug);
        }
        else
        {
            int mainSceneIndex = -1;
            for (int i = 0; i < curSelectSceneNameList.Count; i++)
            {
                GameObject curObj = Instantiate(scenePrefab);
                curObj.transform.SetParent(content);

                mainSceneName curSelectSceneNameDebug = curObj.GetComponent<mainSceneName>();

                curSelectSceneNameDebug.sceneName.text = curSelectSceneNameList[i];
                curSelectSceneNameDebug.m_Index = i;
                sceneList.Add(curSelectSceneNameDebug);
                if (defaultcurSelectSceneName == curSelectSceneNameList[i])
                    mainSceneIndex = i;

                //* 버튼
                curSelectSceneNameDebug.InputButton.onClick.AddListener(() =>
                {
                    int curIndex = curSelectSceneNameDebug.m_Index;
                    Debug.Log(curIndex);
                    curSelectSceneName = sceneList[curIndex].sceneName.text;
                    noticeText.text = $"이동할 씬 이름은 [ {curSelectSceneName} ]입니다.";
                    sceneList[curIndex].bgImg.color = selectColor;
                    sceneList[curSceneIndex].bgImg.color = Color.white;
                    sceneList[curIndex].bgImg.color = selectColor;
                    curSceneIndex = curSelectSceneNameDebug.m_Index;
                });
            }

            if (mainSceneIndex != -1) //* defaultcurSelectSceneName과 똑같은 씬이 있는 것
            {
                curSelectSceneName = defaultcurSelectSceneName;
                noticeText.text = $"이동할 씬 이름은 [ {curSelectSceneName} ]입니다.";
                sceneList[mainSceneIndex].bgImg.color = selectColor;
                curSceneIndex = mainSceneIndex;
            }
            else
            {
                curSelectSceneName = sceneList[0].sceneName.text;
                noticeText.text = $"이동할 씬 이름은 [ {curSelectSceneName} ]입니다.";
                sceneList[0].bgImg.color = selectColor;
                curSceneIndex = 0;
            }
        }
    }

    public void SetButton()
    {
        Debug.Log($"{GameManager.instance.loadScene}");
        startBtn.onClick.AddListener(() => GameManager.instance.loadScene.LoadMainScene(curSelectSceneName));
        loadBtn.onClick.AddListener(() => GameManager.instance.loadScene.LoadDataScene());
    }

    public void InputcurSelectSceneName()
    {
        noticeText.text = $"이동할 씬 이름은 [ {curSelectSceneName} ]입니다.";
    }
}
