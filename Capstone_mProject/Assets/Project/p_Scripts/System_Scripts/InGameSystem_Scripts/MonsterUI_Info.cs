using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MonsterUI_Info : MonoBehaviour
{
    public TMP_Text monsterName;
    public Slider m_slider;
    public Monster m_Monster;
    public Transform m_HPBarPos;
    private double monsterMaxHP = 0;

    public bool isReset = false;

    public void Reset(double _monsterMaxHP, Monster _monster)
    {
        monsterMaxHP = _monsterMaxHP;
        m_Monster = _monster;
        m_HPBarPos = m_Monster.monsterData.HPBarPos;

        Debug.Log("text" + _monster.monsterData.monsterName);
        monsterName.text = _monster.monsterData.monsterName;

        isReset = true;
    }

    private void OnDisable()
    {
        //비활성화 될때마다.
        isReset = false;
    }

}
