using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillTree : MonoBehaviour
{
    private PlayerController p_controller;
    [SerializeField] private List<PlayerSkillName> skill;
    [SerializeField] private List<PlayerSkillName> selectedSkill;
    [SerializeField] private List<PlayerSkillName> nonSelectedSkill;

    Color selectColor;
    Color unselectColor;
    void Start()
    {
        selectColor = GameManager.Instance.HexToColor("#FF8C80");
        unselectColor = GameManager.Instance.HexToColor("#00C8FF");
        p_controller = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
        skill = new List<PlayerSkillName>();
        skill.Clear();
        selectedSkill = new List<PlayerSkillName>();
        selectedSkill.Clear();
        nonSelectedSkill = new List<PlayerSkillName>();
        nonSelectedSkill.Clear();

    }
    public void setting()
    {
        p_controller.P_Skills.OnSkillMapUpdated += SkillMapUpdate;
        SkillSetting();
        SkillMapUpdate();
    }

    private PlayerSkillName nameToSkill(string namee)
    {
        foreach (KeyValuePair<string, PlayerSkillName> i in p_controller.P_Skills.skillMap)
        {
            if (namee == i.Key)
            {
                return i.Value;
            }
        }
        return null;
    }

    public void SelectSkill(PlayerSkillName curSkill)
    {
        PlayerSkillName curSkillName = curSkill.GetComponent<PlayerSkillName>();
        if (selectedSkill.Contains(curSkill))  // 이미 선택된 경우
        {
            selectedSkill.Remove(curSkill);
            nonSelectedSkill.Add(curSkill);
            curSkillName.skillData.isSelect = false;
            curSkillName.InputButton.image.color = unselectColor;
        }
        else
        {
            if (selectedSkill.Count >= 3)
            {
                PlayerSkillName firstSkillName = selectedSkill[0].GetComponent<PlayerSkillName>();
                firstSkillName.skillData.isSelect = false;
                firstSkillName.InputButton.image.color = unselectColor;
                nonSelectedSkill.Add(selectedSkill[0]);
                selectedSkill.RemoveAt(0);
            }
            selectedSkill.Add(curSkill);
            if (nonSelectedSkill.Contains(curSkill))
            {
                nonSelectedSkill.Remove(curSkill);
            }
            curSkillName.skillData.isSelect = true;
            curSkillName.InputButton.image.color = selectColor;
        }
    }

    public void SkillMapUpdate()
    {
        skill = p_controller.P_Skills.getskillMapToSkill();
    }

    public void selectSkillAddList()
    {
        foreach (PlayerSkillName i in skill)
        {
            if (i.skillData.isSelect)
            {
                selectedSkill.Add(i);
            }
            else
            {
                nonSelectedSkill.Add(i);
            }
        }
    }

    public void SkillSetting()
    {
        selectSkillAddList();
        if (selectedSkill.Count < 3)
        {
            for (int i = 0; i < selectedSkill.Count; i++)
            {
                p_controller._skillInfo.selectSkill[i] = selectedSkill[i];
            }
            int j = 0;
            for (int i = selectedSkill.Count; i < 3; i++)
            {
                p_controller._skillInfo.selectSkill[i] = nonSelectedSkill[j++];
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                p_controller._skillInfo.selectSkill[i] = selectedSkill[i];
            }
        }
    }
}
