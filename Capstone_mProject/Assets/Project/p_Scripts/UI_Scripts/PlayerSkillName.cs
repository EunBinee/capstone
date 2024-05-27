using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerSkillName : MonoBehaviour
{
    public PlayerUI_info playerUI_Info;

    public Image iconImg;
    public SOSkill skillData;
    public TMP_Text skillName;
    public Button InputButton;
    public int skillCodeNum;
    //public bool m_isSelect;
    public bool isOpen;


    void Start()
    {
        skillName.text = skillData.skillName;
        skillCodeNum = skillData.skillCodeNum;
        //skillData.skillObj = this;
        Transform i = this.transform;
        playerUI_Info = i.GetComponent<PlayerUI_info>();
        while (playerUI_Info == null)
        {
            i = i.transform.parent;
            playerUI_Info = i.GetComponent<PlayerUI_info>();
        }
        playerUI_Info.p_controller.playerSkillTree.SelectSkill(this);
        playerUI_Info.p_controller.playerSkillTree.SelectSkill(this);
    }

    public void onClickSelect()
    {
        selecSkill();
    }

    public void selecSkill()
    {
        playerUI_Info.p_controller.playerSkillTree.SelectSkill(this);
    }
}
