using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI_info : MonoBehaviour
{
    [Header("Player Hit")]
    public TMP_Text hitNum;
    public GameObject hitUiGuide;
    public GameObject hitUI;
    //public GameObject chargingImg;

    [Space]
    [Header("Player HP")]
    public GameObject portrait;
    public Slider HPgauge;

    [Space]
    [Header("Player Skill")]
    public SkillButton skill_V;
    public SkillButton skill_Q;
    public SkillButton skill_E;
    public SkillButton skill_R;
    //public SkillButton skill_T;
    /**/
    [Header("Skill Slot")]
    public GameObject skillTree;
    //[SerializeField] public List<Image> slot = new List<Image>(3);

    [Space]
    [Header("Player 조준 카메라 관련")]
    public TMP_Text crosshairImage; // 조준점 이미지
    public Image crosshair; // 조준점 이미지
    public Image killImg; //몬스터 킬시 나오는 이미지 

    public PlayerController p_controller;
    //public PlayerSkillTree playerSkillTree;

    void Start()
    {
        p_controller = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
        p_controller.playerSkillTree = this.GetComponent<PlayerSkillTree>();
    }

    public void confirm()
    {
        //선택한 스킬 저장
        p_controller.playerSkillTree.SkillSetting();
        p_controller.P_Skills.CloseSkillWindow();
        Debug.Log("confirm()");
    }
    public void cancel()
    {
        //선택 하기 전 스킬들로 저장
        p_controller.P_Skills.CloseSkillWindow();
        Debug.Log("cancel()");
    }
}