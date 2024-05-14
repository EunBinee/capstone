using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SettingUI : MonoBehaviour
{
    public SettingInfo settingInfo;

    void Start()
    {
        Init();
    }
    public void Init()
    {

    }

    void OnDisable()
    {
        ChangeSettingValue();

    }

    void ChangeSettingValue()
    {
        //* 닫을 때 변경된 세팅값을 적용시켜주는 함수
        GameManager.instance.ChangeSettingValue();
    }


}
