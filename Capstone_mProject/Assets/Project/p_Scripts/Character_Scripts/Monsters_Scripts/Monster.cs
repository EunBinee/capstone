using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    public MonsterData monsterData;
    public MonsterPattern monsterPattern;

    public AudioClip[] monsterSoundClips;

    public enum monsterSound
    {
        Hit,
        GetDamage,
        Death
    }

    private void Awake()
    {
        monsterPattern = GetComponent<MonsterPattern>();
        Init();
    }
    private void Update()
    {
        //임시 테스트 코드---===================//
        if (Input.GetKeyDown(KeyCode.P))
        {
            GetDamage(3);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            Death();
        }
        //---====================================//
    }

    private void Init()
    {
        monsterData.HP = monsterData.MaxHP;
    }

    private void Reset()
    {

    }

    public virtual void OnHit()
    {
        //몬스터가 플레이어를 때리다.


    }

    public virtual void GetDamage(double Damage)//플레이어에게 공격 당함.
    {
        monsterData.HP -= Damage;
        //플레이어의 반대 방향으로 넉백

        if (monsterData.HP <= 0)
        {
            //죽음
            Death();
        }
        else
        {
            //아직 살아있음.
            monsterPattern.Monster_Motion(MonsterPattern.MonsterMotion.GetHit_KnockBack);
        }

    }

    public virtual void Death()
    {
        //죽다.
        monsterData.HP = 0;
        monsterPattern.Monster_Motion(MonsterPattern.MonsterMotion.Death);
    }

    private void SoundPlay(monsterSound m_sound)
    {
        SoundManager.Instance.Play_MonsterSound(monsterSoundClips[(int)m_sound]);
    }

}
