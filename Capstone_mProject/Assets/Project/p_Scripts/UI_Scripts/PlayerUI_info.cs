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
    private List<int> selectedSkillsIndex;  // 선택된 스킬의 인덱스를 저장하는 리스트
    Color selectColor;
    Color unselectColor;
    public GameObject skillUIPrefab;  //리스트에 추가할 UI 프리팹
    public Transform content;

    int curSceneIndex;
    public string curSelectSkillName = "";

    void Start()
    {
        SkillNameList = new List<string>();
        playerSkillList = new List<PlayerSkillName>();
        selectedSkillsIndex = new List<int>();
        selectColor = GameManager.Instance.HexToColor("#FF8C80");
        unselectColor = GameManager.Instance.HexToColor("#CEFDFF");
        p_controller = GameManager.Instance.gameData.player.GetComponent<PlayerController>();

        //skillWinSetting();
        //StartCoroutine(detectListChange());
    }

    void Update() {
        skillWinSetting();
    }

    public void skillWinSetting(){
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
                    curObj.transform.localPosition = new Vector3(0, -180 - (i * 100), 0);

                    PlayerSkillName curSkillName = curObj.GetComponent<PlayerSkillName>();
                    curSkillName.m_Index = i;
                    curSkillName.skillName.text = SkillNameList[i];
                    string name = curSkillName.skillName.text;
                    curSkillName.iconImg.sprite = p_controller.P_Skills.skillMap[name].icon;

                    playerSkillList.Add(curSkillName);

                    curSkillName.InputButton.onClick.AddListener(() =>
                    {
                        int curIndex = curSkillName.m_Index;
                        
                        if (selectedSkillsIndex.Contains(curIndex)) // 이미 선택된 경우, 선택 해제
                        {
                            playerSkillList[curIndex].InputButton.image.color = unselectColor;
                            playerSkillList[curIndex].isSelect = false;
                            selectedSkillsIndex.Remove(curIndex);
                        }
                        else
                        {
                            // 선택된 스킬이 이미 3개인 경우
                            if (selectedSkillsIndex.Count >= 3)
                            {
                                // FIFO 방식으로 첫 번째 선택된 스킬 해제
                                int firstSelectedIndex = selectedSkillsIndex[0];
                                playerSkillList[firstSelectedIndex].InputButton.image.color = unselectColor;
                                playerSkillList[firstSelectedIndex].isSelect = false;
                                selectedSkillsIndex.RemoveAt(0); // 리스트에서 첫 번째 요소 제거
                            }

                            // 새로운 스킬 선택
                            playerSkillList[curIndex].InputButton.image.color = selectColor;
                            playerSkillList[curIndex].isSelect = true;
                            selectedSkillsIndex.Add(curIndex); // 새로운 선택 추가
                        }
                    });
                }
            }
        }
    }

    IEnumerator detectListChange()
    {
        while (playerSkillList.Count < SkillNameList.Count)
        {// UI 오브젝트보다 스킬 이름 리스트가 더 많은 경우
            //todo: 추가된 스킬들 새로 생성
            for (int i = playerSkillList.Count-1; i < SkillNameList.Count-1; i++)
            {
                GameObject curObj = Instantiate(skillUIPrefab);
                curObj.transform.SetParent(content);
                curObj.transform.localPosition = new Vector3(0, -180 - (i * 100), 0);

                PlayerSkillName curSkillName = curObj.GetComponent<PlayerSkillName>();
                curSkillName.m_Index = i;
                curSkillName.skillName.text = SkillNameList[i];
                string name = curSkillName.skillName.text;
                curSkillName.iconImg.sprite = p_controller.P_Skills.skillMap[name].icon;

                playerSkillList.Add(curSkillName);
            }
            yield return null;
        }
        yield return null;
    }
}
