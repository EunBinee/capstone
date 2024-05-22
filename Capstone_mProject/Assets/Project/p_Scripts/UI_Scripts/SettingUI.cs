using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

    void Start()
    {
        settingUIAnim = GetComponent<Animator>();
        canvasGroup = GetComponent<CanvasGroup>();
        SettingInit();
    }

    private void Update()
    {
        if (!isMainScene)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && canAccess)
            {
                if (hideSettingUI_co == null)
                    HideSettingUI();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape) && canvasGroup.alpha == 1)
            {
                settingUIAnim.Play(panelFadeOut);
            }
        }
    }

    public void SettingInit()
    {
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

        /*
        bgm
        SoundManager.Instance.bgmPlayer

        sfx
        SoundManager.Instance.playerSoundPlayer
        SoundManager.Instance.mosterSoundPlayer
        SoundManager.Instance.sfxPlayer

        */
    }

}
