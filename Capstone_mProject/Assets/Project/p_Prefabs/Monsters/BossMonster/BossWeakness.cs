using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossWeakness : MonoBehaviour
{
    public Monster m_monster;
    //! 보스 약점
    public bool destroy_BossWeakness = false; //* false 아직 공격안당한 보스 약점. true 이미 공격당한 보스 약점

    public void SetMonster(Monster _monster)
    {
        m_monster = _monster;
    }

    public void WeaknessGetDamage()
    {
        //* 공격 당했을 때 연출
        this.gameObject.SetActive(false);
    }


}
