using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FinishUI : MonoBehaviour
{
    CanvasGroup canvasGroup;
    Animator settingUIAnim;
    string panelFadeIn = "Panel In";
    string panelFadeOut = "Panel Out";

    public Michsky.UI.Reach.ButtonManager gototheMainSceneBtn;
    public Michsky.UI.Reach.ButtonManager restartBtn;

    private int currentSelection = 0; // 현재 선택된 버튼의 인덱스

    void Start()
    {
        settingUIAnim = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();
        SettingInit();
        SettingBtn();
        Michsky.UI.Reach.UIManagerAudio.instance.audioSource = SoundManager.instance.UIPlayer;
    }

      void Update()
    {
        // 조이스틱 입력을 확인하여 포커스를 이동
        float verticalInput = Input.GetAxis("JVertical");
        if (verticalInput > 0.5f)
        {
            currentSelection = 0; // 첫 번째 버튼으로 선택
            EventSystem.current.SetSelectedGameObject(gototheMainSceneBtn.gameObject);
        }
        else if (verticalInput < -0.5f)
        {
            currentSelection = 1; // 두 번째 버튼으로 선택
            EventSystem.current.SetSelectedGameObject(restartBtn.gameObject);
        }

        // B 버튼으로 클릭 처리
        if (Input.GetButtonDown("Submit"))
        {
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                EventSystem.current.currentSelectedGameObject.GetComponent<Michsky.UI.Reach.ButtonManager>().onClick.Invoke();
            }
        }
    }

    void SettingBtn()
    {
        GameManager.instance.Stop_AllMonster();

        if (GameManager.instance.gameData.GetPlayerController()._currentState.isStrafing)
        {
            GameManager.instance.gameData.GetPlayerController()._currentState.isStrafing = false;
        }

        GameManager.instance.RemoveMonster();

        gototheMainSceneBtn.onClick.AddListener(() =>
        {
            settingUIAnim.Play(panelFadeOut);
            string mainSceneName = GameManager.instance.gameData.mainSceneName;
            LoadingSceneController.LoadScene(mainSceneName);
            // Destroy(this);
        });

        if (restartBtn != null)
        {
            restartBtn.onClick.AddListener(() =>
                    {
                        //* 메인씬이 아니면 세팅창 닫아주고 다시 시작
                        settingUIAnim.Play(panelFadeOut);

                        if (UIManager.gameIsPaused == true)
                        {
                            //게임이 멈춰있으면 다시 재생.
                            UIManager.Instance.Resume();
                        }

                        string curSceneName = CurSceneManager.instance.curSceneName;
                        if (curSceneName == "")
                        {
#if UNITY_EDITOR
                            Debug.Log("없는 씬 이름입니다");
#endif
                        }

                        resetPlayer();
                        LoadingSceneController.LoadScene(curSceneName);

                        //Debug.Log("HI");
                        // this.gameObject.SetActive(false);
                        // Destroy(this);
                    });
        }

    }
    void SettingInit()
    {
        settingUIAnim.Play(panelFadeIn);
    }

    public void ShowFinishUI()
    {
        SettingInit();
    }

    public void resetPlayer()
    {
        GameManager.Instance.gameData.player.GetComponent<PlayerController>().PlayerSetting();
    }
}
