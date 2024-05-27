using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SOSkill : ScriptableObject
{
    public float damage;
    public float cool;
    public bool isTwice;
    public bool isSelect;

    public string animationName;
    public Sprite icon;
    //public PlayerSkillName skillObj;

    public int skillCodeNum; //스킬 코드
    public string skillName; //스킬 이름
    public string skillDetail; //스킬 설명 
}