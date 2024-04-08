using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI_info : MonoBehaviour
{
    [Header("Player Hit")]
    public TMP_Text hitNum;
    public GameObject hitUI;

    [Space]
    [Header("Player HP")]
    public Slider HPgauge;

    [Space]
    [Header("Player Skill")]
    public SkillButton skill_E;
    public SkillButton skill_Q;
    public SkillButton skill_R;

    public ScrollRect skillScrollWindow;

    [Space]
    [Header("Player 조준 카메라 관련")]
    public TMP_Text crosshairImage; // 조준점 이미지
}
