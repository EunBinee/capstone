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
    public GameObject chargingImg;

    [Space]
    [Header("Player HP")]
    public GameObject portrait;
    public Slider HPgauge;

    [Space]
    [Header("Player Skill")]
    public SkillButton skill_T;
    public SkillButton skill_F;
    public SkillButton skill_E;
    public SkillButton skill_R;
    public SkillButton skill_Q;

    [Header("Skill Slot")]
    public GameObject skillScrollWindow;
    public Image slot1Img;
    public Image slot2Img;
    public Image slot3Img;
    public List<Image> slotImg;

    [Space]
    [Header("Player 조준 카메라 관련")]
    public TMP_Text crosshairImage; // 조준점 이미지


    private PlayerController p_controller;
    [SerializeField]
    private List<string> SkillNameList;
    List<PlayerSkillName> playerSkillList;
    private List<int> selectedSkillsIndex;  // 선택된 스킬의 인덱스를 저장하는 리스트
    [SerializeField] List<SOSkill> sskill;
    [SerializeField] List<SOSkill> non_sskill;

    Color selectColor;
    Color unselectColor;
    public GameObject skillUIPrefab;  //리스트에 추가할 UI 프리팹
    public Transform content;

    void Start()
    {
        SkillNameList = new List<string>();
        SkillNameList.Clear();
        playerSkillList = new List<PlayerSkillName>();
        selectedSkillsIndex = new List<int>();
        slotImg = new List<Image>();
        slotImg.Clear();
        selectColor = GameManager.Instance.HexToColor("#FF8C80");
        unselectColor = GameManager.Instance.HexToColor("#CEFDFF");
        p_controller = GameManager.Instance.gameData.player.GetComponent<PlayerController>();

        // 스킬 맵 업데이트시 호출할 이벤트에 메서드 구독
        p_controller.P_Skills.OnSkillMapUpdated += SkillMapUpdated;

        SkillMapUpdated(); // 초기 UI 설정을 위해 메서드 호출
    }

    private void SkillMapUpdated()
    {
        List<string> updatedSkills = p_controller.P_Skills.getskillMap(); // 업데이트된 스킬 목록 가져오기
        if (SkillNameList.Count != updatedSkills.Count) // 새 스킬이 추가되었는지 확인
        {
            for (int i = SkillNameList.Count; i < updatedSkills.Count; i++) // 새 스킬만큼 UI 요소 생성
            {
                GameObject curObj = Instantiate(skillUIPrefab);
                curObj.transform.SetParent(content);
                //curObj.transform.localPosition = new Vector3(0, -180 - (i * 100), 0);
                //curObj.transform.position = new Vector3(350+(i * 100), i / 6, 0);
                curObj.transform.localPosition = new Vector3(-260 + ((i % 6) * 100), -180 + (i / 6), 0);
                curObj.transform.localScale = Vector3.one;

                PlayerSkillName curSkillName = curObj.GetComponent<PlayerSkillName>(); // 스킬 이름 컴포넌트 접근r
                curSkillName.m_Index = i; // 인덱스 설정
                curSkillName.skillName.text = updatedSkills[i]; // 스킬 이름 설정
                curSkillName.iconImg.sprite = p_controller.P_Skills.skillMap[updatedSkills[i]].icon; // 스킬 아이콘 설정
                playerSkillList.Add(curSkillName); // 리스트에 추가
                curSkillName.InputButton.onClick.AddListener(() => SelectSkill(curSkillName)); // 선택 이벤트 리스너 추가
                slotImg[i] = curSkillName.iconImg;
            }
            SkillNameList = new List<string>(updatedSkills); // 스킬 목록 업데이트

            slot1Img.sprite = slotImg[0].sprite;
            slot2Img.sprite = slotImg[1].sprite;
            slot3Img.sprite = slotImg[2].sprite;
        }
    }

    private void SelectSkill(PlayerSkillName curSkillName)
    {
        int curIndex = curSkillName.m_Index; // 현재 스킬의 인덱스
        if (selectedSkillsIndex.Contains(curIndex)) // 이미 선택된 경우
        {
            curSkillName.InputButton.image.color = unselectColor; // 색상 변경
            curSkillName.isSelect = false; // 선택 상태 변경
            selectedSkillsIndex.Remove(curIndex); // 인덱스 리스트에서 제거
        }
        else
        {
            if (selectedSkillsIndex.Count >= 3) // 선택된 스킬이 3개 이상인 경우
            {
                int firstSelectedIndex = selectedSkillsIndex[0]; // 가장 먼저 선택된 스킬의 인덱스
                playerSkillList[firstSelectedIndex].InputButton.image.color = unselectColor; // 색상 변경
                playerSkillList[firstSelectedIndex].isSelect = false; // 선택 상태 변경
                selectedSkillsIndex.RemoveAt(0); // 인덱스 리스트에서 첫 번째 요소 제거
            }
            curSkillName.InputButton.image.color = selectColor; // 새로운 스킬의 색상 변경
            curSkillName.isSelect = true; // 선택 상태로 변경
            selectedSkillsIndex.Add(curIndex); // 새로운 선택 추가
        }
        skillPresetting();
    }

    void OnDestroy()
    {
        // 메모리 누수 방지를 위해 이벤트 구독 해제
        p_controller.P_Skills.OnSkillMapUpdated -= SkillMapUpdated;
    }

    public void skillPresetting()
    {
        sskill = new List<SOSkill>();
        non_sskill = new List<SOSkill>();
        foreach (PlayerSkillName i in playerSkillList)
        {
            if (i.isSelect)
            {
                sskill.Add(nameToSkill(i.skillName.text));
            }
            else
            {
                non_sskill.Add(nameToSkill(i.skillName.text));
            }
        }
        if (sskill.Count < 3)   // 3개 미만 선택 했다면
        {
            int needSkillCnt = 3 - sskill.Count;
            for (int i = 0; i < sskill.Count; i++)
            {
                p_controller._skillInfo.selectSkill[i] = sskill[i];
            }
            //* 나머지 넣기(skillMap 앞에서부터)
            int j = 0;
            for (int i = sskill.Count; i < 3; i++)
            {
                p_controller._skillInfo.selectSkill[i] = non_sskill[j++];
            }
            //p_controller._skillInfo.selectSkill = sskill;
        }
        else p_controller._skillInfo.selectSkill = sskill;

        //* slot1~3에 아이콘 사진 넣기
        slot1Img.sprite = p_controller._skillInfo.selectSkill[0].icon;
        slot2Img.sprite = p_controller._skillInfo.selectSkill[1].icon;
        slot3Img.sprite = p_controller._skillInfo.selectSkill[2].icon;
    }
    private SOSkill nameToSkill(string namee)
    {
        foreach (KeyValuePair<string, SOSkill> i in p_controller.P_Skills.skillMap)
        {
            if (namee == i.Key)
            {
                return i.Value;
            }
        }
        return null;
    }
}
