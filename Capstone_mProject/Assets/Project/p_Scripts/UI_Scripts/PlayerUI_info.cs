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
    public SkillButton skill_T;
    public SkillButton skill_F;
    public SkillButton skill_E;
    public SkillButton skill_R;
    public SkillButton skill_Q;

    [Header("Skill Slot")]
    public GameObject skillTree;
    [SerializeField] public List<Image> slot = new List<Image>(3);

    [Space]
    [Header("Player 조준 카메라 관련")]
    public TMP_Text crosshairImage; // 조준점 이미지


    private PlayerController p_controller;
    //[SerializeField] private List<string> SkillNameList;
    [SerializeField] public List<SOSkill> playerSkillList;  // 스킬 프리팹 저장 리스트
    [SerializeField] private List<int> selectedSkillsIndex;  // 선택된 스킬의 인덱스를 저장하는 리스트
    [SerializeField] List<SOSkill> sskill;
    [SerializeField] List<SOSkill> non_sskill;

    public Transform content;

    void Start()
    {
        //SkillNameList = new List<string>();
        //SkillNameList.Clear();
        playerSkillList = new List<SOSkill>();
        //selectedSkillsIndex = new List<int>();
        sskill = new List<SOSkill>();
        non_sskill = new List<SOSkill>();
        p_controller = GameManager.Instance.gameData.player.GetComponent<PlayerController>();

        // 스킬 맵 업데이트시 호출할 이벤트에 메서드 구독
        //p_controller.P_Skills.OnSkillMapUpdated += SkillMapUpdated;

        //SkillMapUpdated(); // 초기 UI 설정을 위해 메서드 호출
        Invoke("Setting", 0.3f);
    }
    void Setting()
    {
        playerSkillList = p_controller.P_Skills.getskillMapToSkill();
        skillPresetting();
    }

    /*private void SkillMapUpdated()
    {
        List<string> updatedSkills = p_controller.P_Skills.getskillMapToName(); // 업데이트된 스킬 목록 가져오기
        if (SkillNameList.Count != updatedSkills.Count) // 새 스킬이 추가되었는지 확인
        {
            //for (int i = SkillNameList.Count; i < updatedSkills.Count; i++) // 새 스킬만큼 UI 요소 생성
            {
                //GameObject curObj = Instantiate(skillUIPrefab);
                //curObj.transform.SetParent(content);

                //curObj.transform.localPosition = new Vector3(((i % 6) * 110), (i / 6), 0);
                //curObj.transform.localScale = Vector3.one;

                //PlayerSkillName curSkillName = curObj.GetComponent<PlayerSkillName>(); // 스킬 이름 컴포넌트 접근
                //playerSkillList.Add(curSkillName); // 리스트에 추가 nameToSkill
                //curSkillName.InputButton.onClick.AddListener(() => SelectSkill(curSkillName)); // 선택 이벤트 리스너 추가
            }
            SkillNameList = new List<string>(updatedSkills); // 스킬 목록 업데이트
        }
    }*/

    public void SelectSkill(SOSkill curSkill)
    {
        int curIndex = curSkill.skillCodeNum; // 현재 스킬의 인덱스
        if (selectedSkillsIndex.Contains(curIndex)) // 이미 선택된 경우
        {
            selectedSkillsIndex.Remove(curIndex); // 인덱스 리스트에서 제거
            sskill.RemoveAt(curIndex);  // 선택한 거에 최근 인덱스 스킬 삭제
            non_sskill.Add(curSkill);   // 선택안한 거에 받아온 스킬 추가
            curSkill.isSelect = false;
        }
        else
        {
            if (selectedSkillsIndex.Count >= 3) // 선택된 스킬이 3개 이상인 경우
            {
                selectedSkillsIndex.RemoveAt(0); // 인덱스 리스트에서 첫 번째 요소 제거
                non_sskill.Add(sskill[0]);  //선택안한 거에 지울 스킬 추가
                sskill.RemoveAt(0);     //선택한 거에 처음 거 삭제
                sskill[0].isSelect = false;
            }
            selectedSkillsIndex.Add(curIndex); // 새로운 선택 추가
            sskill.Add(curSkill);   // 선택한 거에 받아온 스킬 추가
            non_sskill.Remove(curSkill);  // 선택안한 거에 받아온 스킬 삭제
            curSkill.isSelect = true;
        }
        //selectSkillAddList();
    }

    public void selectSkillAddList()
    {
        if (sskill.Count >= 3)
        {
            sskill.Clear();
            non_sskill.Clear();
        }
        foreach (SOSkill i in playerSkillList)
        {
            if (i.isSelect)
            {
                sskill.Add(i);
            }
            else
            {
                non_sskill.Add(i);
            }
        }
    }
    public void skillPresetting()
    {
        selectSkillAddList();
        if (sskill.Count < 3)   // 3개 미만 선택 했다면
        {
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

    public void confirm()
    {
        //선택한 스킬 저장
        skillPresetting();
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