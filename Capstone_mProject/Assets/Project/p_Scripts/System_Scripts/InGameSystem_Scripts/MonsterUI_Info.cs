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
        m_slider.value = 1;
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

    public void UpdateHP()
    {
        float monsterHP_Value = (float)(m_Monster.monsterData.HP / m_Monster.monsterData.MaxHP);

        if (monsterHP_Value <= 0)
            monsterHP_Value = 0;

        if (m_slider.value > 0)
            StartCoroutine(UpdateHPBar_Anim(monsterHP_Value));

    }

    IEnumerator UpdateHPBar_Anim(float monsterHP_Value)
    {
        Debug.Log($"update : {monsterHP_Value}");
        float time = 0;
        while (time < 0.5f)
        {
            time += Time.deltaTime;

            m_slider.value = Mathf.Lerp(m_slider.value, monsterHP_Value, 0.5f);
            if (m_slider.value == monsterHP_Value)
            {
                Debug.Log($"끝~");
                break;
            }
            yield return null;
        }

        m_slider.value = monsterHP_Value;
    }


}
