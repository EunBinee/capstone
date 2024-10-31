using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class mainStartScene : MonoBehaviour
{
    //public UnityEngine.UI.Button startBtn;
    //public UnityEngine.UI.Button loadBtn;

    public Michsky.UI.Reach.ButtonManager startBtnManager;
    public Michsky.UI.Reach.ButtonManager loadBtnManager;
    public Michsky.UI.Reach.ButtonManager settingBtnManager;
    public Michsky.UI.Reach.ButtonManager exitBtnManager;

    public SettingUI settingUI;
    public Animator mainStartSceneAnim;
    public Animator settingSceneAnim;

    string panelFadeIn = "Panel In";
    string panelFadeOut = "Panel Out";

    bool showSettingPanel = false;


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
    //public string defaultcurSelectSceneName = "StartScene 1";
    public string defaultcurSelectSceneName = "FieldMap01";

    private GameObject[] buttons;   // 네비게이션 할 버튼 배열
    private int currentIndex = 0;   // 현재 선택된 버튼의 인덱스

    void Start()
    {
        mainStartSceneAnim.Play(panelFadeIn);
        showSettingPanel = false;
        SetButton();
        sceneList = new List<mainSceneName>();
        selectColor = GameManager.Instance.HexToColor("#FF8C80");

        buttons = new GameObject[] { startBtnManager.gameObject,settingBtnManager.gameObject, exitBtnManager.gameObject};
        // 초기 선택 버튼 설정
        EventSystem.current.SetSelectedGameObject(buttons[currentIndex]);

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
                    //                    Debug.Log(curIndex);
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

    void Update()
    {
        if (showSettingPanel)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Setting"))
            {
                showSettingPanel = false;
                settingUI.ChangeSettingValue();
                settingSceneAnim.Play(panelFadeOut);
                mainStartSceneAnim.Play(panelFadeIn);
            }
        }
        
    }

    public void SetButton()
    {
        startBtnManager.onClick.AddListener(() =>
        {
            // CanvasManager.instance.mainStartScene.SetActive(false);
            LoadingSceneController.LoadScene(curSelectSceneName);
        });
        loadBtnManager.onClick.AddListener(() => GameManager.instance.loadScene.LoadDataScene());

        settingBtnManager.onClick.AddListener(() =>
        {
            mainStartSceneAnim.Play(panelFadeOut);
            settingSceneAnim.Play(panelFadeIn);
            showSettingPanel = true;
        });

        exitBtnManager.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    public void InputcurSelectSceneName()
    {
        noticeText.text = $"이동할 씬 이름은 [ {curSelectSceneName} ]입니다.";
    }
}
