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
    public SkillButton skill_T;
    public SkillButton skill_F;
    public SkillButton skill_E;
    public SkillButton skill_R;
    public SkillButton skill_Q;

    public ScrollRect skillScrollWindow;

    [Space]
    [Header("Player 조준 카메라 관련")]
    public TMP_Text crosshairImage; // 조준점 이미지


    private PlayerController p_controller;
    [SerializeField]
    private List<string> SkillNameList;
    List<PlayerSkillName> playerSkillList;
    Color selectColor;
    public GameObject skillUIPrefab;  //리스트에 추가할 UI 프리팹
    public Transform content;

    //todo: 선택된 스킬 버튼 색 변경
    int curSceneIndex;
    public string curSelectSkillName = "";

    void Start()
    {
        SkillNameList = new List<string>();
        playerSkillList = new List<PlayerSkillName>();
        selectColor = GameManager.Instance.HexToColor("#FF8C80");
        p_controller = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
    }
    void Update()
    {
        if (p_controller.P_Skills.presetWin && !p_controller.P_Skills.once)
        {
            p_controller.P_Skills.once = true;
            SkillNameList = p_controller.P_Skills.getskillMap();

            if (SkillNameList.Count <= 0)   // list 갯수가 0 이하
            {
                Debug.Log("스킬 리스트 비어있음");
            }
            else    //리스트가 1개 이상
            {
                for (int i = 0; i < SkillNameList.Count; i++)   // 리스트 갯수 만큼 생성
                {
                    GameObject curObj = Instantiate(skillUIPrefab);
                    curObj.transform.SetParent(content);
                    curObj.transform.localPosition = new Vector3(0, -100 - (i * 150), 0);

                    PlayerSkillName curSkillName = curObj.GetComponent<PlayerSkillName>();
                    curSkillName.m_Index = i;
                    curSkillName.skillName.text = SkillNameList[i];
                    string name = curSkillName.skillName.text;
                    curSkillName.iconImg.sprite = p_controller.P_Skills.skillMap[name].icon;

                    playerSkillList.Add(curSkillName);

                    curSkillName.InputButton.onClick.AddListener(() =>
                    {
                        int curIndex = curSkillName.m_Index;
                        playerSkillList[curIndex].InputButton.image.color = selectColor;
                        playerSkillList[curSceneIndex].InputButton.image.color = Color.white;
                        curSceneIndex = curSkillName.m_Index;
                    });
                }
            }
        }
    }
}
