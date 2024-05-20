using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SettingUI : MonoBehaviour
{
    public SettingInfo settingInfo;
    public bool isMainScene = false;
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
        if (Input.GetKeyDown(KeyCode.Escape) && canAccess)
        {
            if (hideSettingUI_co == null)
                HideSettingUI();
        }
    }

    public void SettingInit()
    {
        if (!isMainScene)
        {
            //* 메인 씬이 아닐때
            settingUIAnim.Play(panelFadeIn);
        }
    }

    public void ChangeSettingValue()
    {
        //* 닫을 때 변경된 세팅값을 적용시켜주는 함수

        GameManager.instance.cameraSensitivity = settingInfo.slider_CameraSensitivity.value;
        GameManager.instance.ChangeSettingValue();
    }

    public void ShowSettingUI()
    {
        //settingUIAnim.Play(panelFadeIn);
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
        hideSettingUI_co = null;
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
        this.gameObject.SetActive(false);
    }

}
