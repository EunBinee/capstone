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

    public UnityEngine.UI.Image blinkImg;
    public float blinkSpeed = 0.1f; //클수록 느리고 작을수록 빠름. 
    public UnityEngine.UI.Image moveImg; // 이동하는 이미지
    public GameObject[] targetObjects; // 위치를 담을 빈 오브젝트 배열
    public Vector3[] positions; // 이동할 위치 배열
    public float moveDuration = 8f; // 이동 시간
    //public float fadeDuration = 6.5f; // 사라지는 시간
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
        // 빈 오브젝트의 위치를 positions 배열에 담기
        positions = new Vector3[targetObjects.Length];
        for (int i = 0; i < targetObjects.Length; i++)
        {
            positions[i] = targetObjects[i].transform.position;
        }
        StartCoroutine(HandleImageMovement());
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

        float alpha = (Mathf.Sin(Time.unscaledTime * blinkSpeed) + 1) / 2.0f; // 0 ~ 1로 변환
        Color color = blinkImg.color;
        color.a = alpha;
        blinkImg.color = color;
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

    private IEnumerator HandleImageMovement()
    {
        while (true) // 무한 루프
        {
            foreach (Vector3 targetPos in positions)
            {
                yield return StartCoroutine(MoveToPosition(targetPos));
            }
        }
    }

   private IEnumerator MoveToPosition(Vector3 targetPos)
    {
        Vector3 startPos = moveImg.transform.position;
        float elapsedTime = 0f;
        Color color = moveImg.color;

        // 이동 및 알파 값 조정
        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration; // [0, 1] 사이의 비율
            moveImg.transform.position = Vector3.Lerp(startPos, targetPos, t);
            
            // 이동하는 동안 알파 값을 조절 (최초에만 적용)
            if (targetPos != positions[0]) // 처음 위치가 아닐 때만 페이드 아웃
            {
                color.a = Mathf.Lerp(1, 0, t); // 알파 값을 이동하는 동안 조절
            }
            
            moveImg.color = color;

            elapsedTime += Time.unscaledDeltaTime; // 경과 시간 증가
            yield return null; // 다음 프레임까지 대기
        }

        // 최종 위치 설정
        moveImg.transform.position = targetPos;

        // 마지막 위치에 도달할 때
        if (targetPos == positions[positions.Length - 1]) // 마지막 위치에 도달할 때
        {
            // 즉시 알파 값을 0으로 설정하여 사라지게 함
            color.a = 0; 
            moveImg.color = color;
        }
        else
        {
            // 다른 위치에 도달했을 때는 알파 값을 1로 설정 (옵션)
            color.a = 1; 
            moveImg.color = color;
        }
    }
}