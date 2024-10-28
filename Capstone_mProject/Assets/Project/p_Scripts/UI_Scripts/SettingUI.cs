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

    private Selectable[] selectables; // UI에서 선택 가능한 버튼들
    private int currentSelection = 0; // 현재 선택된 메뉴 항목의 인덱스

    void Start()
    {
        settingUIAnim = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();
        SettingInit();
        SettingBtn();
        Michsky.UI.Reach.UIManagerAudio.instance.audioSource = SoundManager.instance.UIPlayer;

        // UI에서 선택 가능한 버튼을 가져옵니다.
        selectables = GetComponentsInChildren<Selectable>(); // 자식 오브젝트에서 Selectable 컴포넌트를 가져옴
         // Debug: selectables 배열에 포함된 요소 출력하여 확인
   
        if (selectables.Length > 0 && !isMainScene)
        {
            EventSystem.current.SetSelectedGameObject(selectables[currentSelection].gameObject); // 기본 선택 버튼 설정
        }
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
        HandleJoystickInput();
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

    private void HandleJoystickInput()
    {
        if(!isMainScene)
        {
            float vertical = Input.GetAxis("Vertical");

            float sliderValueAdjustment = 0.1f; // 슬라이더 조정 값

            if (Mathf.Abs(vertical) > 4.5f) // 입력 감도 설정
            {
                if (vertical > 0)
                {
                    // 위로 이동
                    currentSelection = (currentSelection - 1 + selectables.Length) % selectables.Length;
                    EventSystem.current.SetSelectedGameObject(selectables[currentSelection].gameObject);
                }
                else
                {
                    // 아래로 이동
                    currentSelection = (currentSelection + 1) % selectables.Length;
                    EventSystem.current.SetSelectedGameObject(selectables[currentSelection].gameObject);
                }
            }


            // B 버튼 입력 처리 (슬라이더 조정)
            if (Input.GetButton("Submit")) // B 버튼이 눌렸을 때
            {
                // 현재 선택된 UI 요소가 슬라이더인지 확인
                Slider currentSlider = selectables[currentSelection].GetComponentInChildren<Slider>();
                Debug.Log(currentSlider);
            
                if (currentSlider != null)
                {
                    // 슬라이더 값 조정
                    if (Input.GetAxis("Horizontal") > 0)
                    {
                        currentSlider.value += sliderValueAdjustment; // 값을 증가
                    }
                    else if (Input.GetAxis("Horizontal") < 0)
                    {
                        currentSlider.value -= sliderValueAdjustment; // 값을 감소
                    }

                    // 슬라이더의 범위를 제한 (예: 0에서 1까지)
                    currentSlider.value = Mathf.Clamp(currentSlider.value, currentSlider.minValue, currentSlider.maxValue);
                }

            
            }

        }
        
    }
}