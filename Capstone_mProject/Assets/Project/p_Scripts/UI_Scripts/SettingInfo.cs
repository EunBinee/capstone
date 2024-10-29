using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[Serializable]
public class SettingInfo
{
    //* General
    public Slider slider_CameraSensitivity;
    public Michsky.UI.Reach.UIManagerText windowModeText;
    public Michsky.UI.Reach.UIManagerText resolutionText;
    public string windowModeName;
    public string resolutionName;
    public Michsky.UI.Reach.UIManagerText restartHeaderText;
    public string restartHeaderName;

    public Michsky.UI.Reach.ButtonManager gototheMainSceneBtn;
    public Michsky.UI.Reach.ButtonManager restartBtn;

    //* Audio
    public Slider masterVolumeSlider;
    public Slider BGMVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider UIVolumeSlider;

}
