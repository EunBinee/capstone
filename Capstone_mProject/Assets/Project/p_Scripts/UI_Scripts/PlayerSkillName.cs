using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerSkillName : MonoBehaviour
{
    public Image iconImg;
    public SOSkill skillData;
    public TMP_Text skillName;
    public Button InputButton;
    public int m_Index;
    public bool isSelect;
    public bool isOpen;

    public void onClick()
    {
        if (isSelect)
        {
            isSelect = false;
        }
        else
        {
            isSelect = true;
        }
    }
}
