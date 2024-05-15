using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SettingUI : MonoBehaviour
{
    public SettingInfo settingInfo;

    void Start()
    {
        SettingInit();
    }
    public void SettingInit()
    {

    }

    public void ChangeSettingValue()
    {
        //* 닫을 때 변경된 세팅값을 적용시켜주는 함수

        GameManager.instance.cameraSensitivity = settingInfo.slider_CameraSensitivity.value;
        GameManager.instance.ChangeSettingValue();
    }


}
