using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [Header("메인화면이면 체크하세요!")]
    public bool isMainScene = false;

    [Space]
    public SettingInfo settingInfo;

    CanvasGroup canvasGroup;
    Animator settingUIAnim;
    string panelFadeIn = "Panel In";
    string panelFadeOut = "Panel Out";

    bool canAccess = true;
    Coroutine hideSettingUI_co = null;

    public string mainSceneName = "StartMainScene";

    private int currentIndex = 0;   // 현재 선택된 UI 요소의 인덱스
    private bool isSliderSelected = false;
    private Slider[] sliders; // 슬라이더 요소 저장

    void Start()
    {
        settingUIAnim = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();
        SettingInit();
        SettingBtn();
        Michsky.UI.Reach.UIManagerAudio.instance.audioSource = SoundManager.instance.UIPlayer;

         sliders = new Slider[]
        {
            settingInfo.slider_CameraSensitivity,
            settingInfo.masterVolumeSlider,
            settingInfo.BGMVolumeSlider,
            settingInfo.sfxVolumeSlider,
            settingInfo.UIVolumeSlider,
        };
         //초기 선택 요소 설정
        EventSystem.current.SetSelectedGameObject(sliders[currentIndex].gameObject);
    }

    private void Update()
    {
        if (!isMainScene)
        {
            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Setting")) && canAccess)
            {
                if (hideSettingUI_co == null)
                    HideSettingUI();
            }
        }
        else
        {
            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Setting")) && canvasGroup.alpha == 1)
            {
                settingUIAnim.Play(panelFadeOut);
            }
        }
        
        
        // 조이스틱 입력 처리
        HandleSelection();

    }

    public void SettingInit()
    {
        SettingText();
        if (!isMainScene)
        {
            //* 메인 씬이 아닐때
            StartCoroutine(ShowSettingUI_co());
        }
    }

    public void ChangeSettingValue()
    {
        //* 닫을 때 변경된 세팅값을 적용시켜주는 함수

        GameManager.instance.cameraSensitivity = settingInfo.slider_CameraSensitivity.value;
        GameManager.instance.ChangeSettingValue();

        SettingVolume();
    }



    public void ShowSettingUI()
    {
        StartCoroutine(ShowSettingUI_co());

    }
    public void HideSettingUI()
    {
        hideSettingUI_co = StartCoroutine(HideSettingUI_co());

    }
    IEnumerator ShowSettingUI_co()
    {
        canAccess = false;
        settingUIAnim.Play(panelFadeIn);

        while (canvasGroup.alpha == 1)
        {
            yield return null;
        }
        canAccess = true;
    }
    IEnumerator HideSettingUI_co()
    {
        canAccess = false;
        settingUIAnim.Play(panelFadeOut);

        while (canvasGroup.alpha == 0)
        {
            yield return null;
        }
        hideSettingUI_co = null;
        canAccess = true;


        ChangeSettingValue();
        if (UIManager.gameIsPaused == true)
        {
            //게임이 멈춰있으면 다시 재생.
            UIManager.Instance.Resume();
        }
        if (!isMainScene)
            this.gameObject.SetActive(false);
    }


    //* 오디오의 소리 조절
    public void SettingVolume()
    {
        float masterValue = settingInfo.masterVolumeSlider.value;

        float bgmValue = settingInfo.BGMVolumeSlider.value * masterValue;
        SoundManager.Instance.bgmPlayer.volume = bgmValue;

        float sfxValue = settingInfo.sfxVolumeSlider.value * masterValue;
        SoundManager.Instance.playerSoundPlayer.volume = sfxValue;
        foreach (AudioSource audioSource in SoundManager.Instance.mosterSoundPlayer)
        {
            audioSource.volume = sfxValue;
        }
        foreach (AudioSource audioSource in SoundManager.Instance.sfxPlayer)
        {
            audioSource.volume = sfxValue;
        }
        float uiValue = settingInfo.UIVolumeSlider.value * masterValue;
        SoundManager.Instance.UIPlayer.volume = uiValue;

        Debug.Log(masterValue);
    }

    void SettingBtn()
    {
        settingInfo.gototheMainSceneBtn.onClick.AddListener(() =>
        {
            //* 몬스터
            GameManager.instance.Stop_AllMonster();
            if (GameManager.instance.gameData.GetPlayerController()._currentState.isStrafing)
            {
                GameManager.instance.gameData.GetPlayerController()._currentState.isStrafing = false;
            }
            GameManager.instance.RemoveMonster();
            
            if (!isMainScene)
            {
                //* 메인씬이 아니면 세팅창 닫아주고 메인으로
                settingUIAnim.Play(panelFadeOut);
                ChangeSettingValue();
                if (UIManager.gameIsPaused == true)
                {
                    //게임이 멈춰있으면 다시 재생.
                    UIManager.Instance.Resume();
                }
                if (!isMainScene)
                    this.gameObject.SetActive(false);
            }

            LoadingSceneController.LoadScene(mainSceneName);
        });


        settingInfo.restartBtn.onClick.AddListener(() =>
        {
            if (!isMainScene)
            {
                //* 몬스터
                GameManager.instance.Stop_AllMonster();
                if (GameManager.instance.gameData.GetPlayerController()._currentState.isStrafing)
                {
                    GameManager.instance.gameData.GetPlayerController()._currentState.isStrafing = false;
                }
                GameManager.instance.RemoveMonster();

                //* 메인씬이 아니면 세팅창 닫아주고 다시 시작
                settingUIAnim.Play(panelFadeOut);
                ChangeSettingValue();
                if (UIManager.gameIsPaused == true)
                {
                    //게임이 멈춰있으면 다시 재생.
                    UIManager.Instance.Resume();
                }
                if (!isMainScene)
                    this.gameObject.SetActive(false);

                string curSceneName = CurSceneManager.instance.curSceneName;
                if (curSceneName == "")
                {
#if UNITY_EDITOR
                    Debug.Log("없는 씬 이름입니다");
#endif
                }
                GameManager.Instance.gameData.player.GetComponent<PlayerController>().PlayerSetting();
                LoadingSceneController.LoadScene(curSceneName);
            }
            //

        });
    }

    void SettingText()
    {
        settingInfo.windowModeText.objText.text = settingInfo.windowModeName;
        settingInfo.resolutionText.objText.text = settingInfo.resolutionName;
        settingInfo.restartHeaderText.objText.text = settingInfo.restartHeaderName;
    }

    public void resetPlayer()
    {
        GameManager.Instance.gameData.player.GetComponent<PlayerController>().PlayerSetting();
    }

    void HandleSelection()
    {
        if (Input.GetButtonDown("Submit"))
        {
            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

            if (selectedObject != null)
            {
                // 기본값으로 false 설정
                isSliderSelected = false;

                // 현재 선택된 슬라이더와 비교
                for (int i = 0; i < sliders.Length; i++)
                {
                    if (selectedObject.name == sliders[i].gameObject.transform.parent.name)
                    {
                        currentIndex = i; // 현재 선택된 슬라이더의 인덱스로 업데이트
                        isSliderSelected = true; // 슬라이더가 선택된 경우 true로 설정
                        break;
                    }
                }

                // 슬라이더가 선택된 경우에만 이벤트 실행
                if (isSliderSelected)
                {
                    ExecuteEvents.Execute(selectedObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
                }

                Debug.Log("Slider selected: " + isSliderSelected);
                Debug.Log("Selected object name: " + selectedObject.name);
            }
        }

        // 슬라이더 값 조정
        if (isSliderSelected)
        {
            HandleSliderAdjustment();
        }
    }

    void HandleSliderAdjustment()
    {
        // 슬라이더의 현재 인덱스를 사용
        Slider currentSlider = sliders[currentIndex];
        Debug.Log(currentSlider.transform.parent.name);
        if (currentSlider != null)
        {
            float adjustment = Input.GetAxis("Horizontal"); // 조이스틱 또는 D-Pad의 수평 입력
            
            // 조정 스케일 (조정 민감도)
            float sensitivityScale = 0.2f; // 필요에 따라 조정 가능
           // Debug.Log( currentSlider.value);
            // 슬라이더 값 업데이트 (최소값과 최대값을 설정)
            float newValue = currentSlider.value + adjustment * sensitivityScale;
            currentSlider.value = Mathf.Clamp(newValue, currentSlider.minValue, currentSlider.maxValue);

            Debug.Log(currentSlider.value);
        }
    }

}