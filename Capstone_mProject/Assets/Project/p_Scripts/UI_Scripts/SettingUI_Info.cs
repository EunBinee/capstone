using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SettingUI_Info
{
    public Button closeBtn;
    public Button settingPanelBtn; //TODO: 나중에 setting UI 버튼 이름 변경되면 이 변수 명도 바꾸기
    public Button savePanelBtn;

    public GameObject settingPanel;
    public GameObject savePanel;

    public Button saveBtn;
    public Button loadBtn;
    public Button quitBtn;

    public Slider musicBtn;
    public Slider vlumeBtn;
}