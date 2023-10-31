using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup_Window : UIBase
{
    public PopupWindowUI_Info windowUI_Info;

    public Action cancelAction;
    public Action okAction;
    public override void Init()
    {
        base.Init();
    }


    public void SetButtonValue(string _title, string _content, Action _okAction, Action _cancelAction = null)
    {
        //TODO: title과 내용을 지금은 직접 받지만 나중에는 동적으로 파일을 읽어와서 하는 형식으로 변경해야함.
        //* 사용법   
        /*
        SetWindowBtn(() =>
        {
            Debug.Log("HI");
        }, () =>
        {
            Debug.Log("HI");
        });
        */

        windowUI_Info.txt_title.text = _title;
        windowUI_Info.txt_content.text = _content;

        cancelAction = _cancelAction;
        okAction = _okAction;

        //* 닫기 버튼
        windowUI_Info.closeBtn.onClick.RemoveAllListeners();
        windowUI_Info.closeBtn.onClick.AddListener(() =>
        {
            CloseBtn();
        });

        //* okay버튼
        windowUI_Info.btn_okay.onClick.RemoveAllListeners();
        windowUI_Info.btn_okay.onClick.AddListener(() => okAction());

        //* 취소 버튼
        windowUI_Info.btn_cancel.onClick.RemoveAllListeners();
        if (_cancelAction != null)
        {
            windowUI_Info.btn_cancel.onClick.AddListener(() => cancelAction());
        }
        else
        {
            windowUI_Info.btn_cancel.onClick.AddListener(() =>
            {
                CloseBtn();
            });
        }

    }


}
