using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerSkillName : MonoBehaviour
{
    //public PlayerUI_info playerUI_Info;

    public Image iconImg;
    public SOSkill skillData;
    public TMP_Text skillName;
    public Button InputButton;
    public int skillCodeNum;
    public bool m_isSelect;
    //public bool isOpen;

    //Color selectColor;
    //Color unselectColor;

    //void Start()
    //{
    //selectColor = GameManager.Instance.HexToColor("#FF8C80");
    //unselectColor = GameManager.Instance.HexToColor("#00C8FF");
    //skillName.text = skillData.skillName;
    //skillCodeNum = skillData.skillCodeNum;
    //Transform i = this.transform;
    //playerUI_Info = i.GetComponent<PlayerUI_info>();
    //while (playerUI_Info == null)
    //{
    //    i = i.transform.parent;
    //    playerUI_Info = i.GetComponent<PlayerUI_info>();
    //}
    //}
    //private void FixedUpdate()
    //{
    //    m_isSelect = skillData.isSelect;
    //}

    //public void onClickSelect()
    //{
    //    if (m_isSelect)
    //    {
    //        Debug.Log($"[skill test] skillName {skillName.text}");
    //        Debug.Log($"[skill test] m_isSelect {m_isSelect}");
    //        skillData.isSelect = m_isSelect = false;
    //        InputButton.image.color = unselectColor;
    //    }
    //    else
    //    {
    //        Debug.Log($"[skill test] skillName {skillName.text}");
    //        Debug.Log($"[skill test] m_isSelect {m_isSelect}");
    //        skillData.isSelect = m_isSelect = true;
    //        InputButton.image.color = selectColor;
    //    }
    //    //selecSkill();
    //}

    //public void selecSkill()
    //{
    //    playerUI_Info.SelectSkill(skillData);
    //}
}
