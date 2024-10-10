using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishUI : MonoBehaviour
{
    CanvasGroup canvasGroup;
    Animator settingUIAnim;
    string panelFadeIn = "Panel In";
    string panelFadeOut = "Panel Out";

    public Michsky.UI.Reach.ButtonManager gototheMainSceneBtn;
    public Michsky.UI.Reach.ButtonManager restartBtn;

    void Start()
    {
        settingUIAnim = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();
        SettingInit();
        SettingBtn();
        Michsky.UI.Reach.UIManagerAudio.instance.audioSource = SoundManager.instance.UIPlayer;
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
