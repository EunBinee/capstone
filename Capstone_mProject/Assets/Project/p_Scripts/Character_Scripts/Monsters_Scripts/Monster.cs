using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    public MonsterData monsterData;
    public MonsterPattern monsterPattern;

    public AudioClip[] monsterSoundClips; //나중에 몬스터 스크립트로 빼도 괜찮을듯
    public enum monsterSound
    {
        Hit,
        GetDamage,
        Death
    }

    private void Awake()
    {
        monsterPattern = GetComponent<MonsterPattern>();
    }

    public virtual void OnHit()
    {
        //몬스터가 플레이어를 때리다.
    }

    public virtual void GetDamage()
    {
        //플레이어에게 맞다.
    }

    public virtual void Death()
    {
        //죽다.
    }

}
