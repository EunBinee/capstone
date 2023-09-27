using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class MonsterData
{
    public enum MonsterType
    {
        None,
        NomalMonster
    }

    public int monsterid;
    public MonsterType monsterType;
    public float overlapRadius;

    [Range(5f, 30f), Tooltip("몬스터 로밍 범위 x (가로)")]
    public int roaming_RangeX;
    [Range(5f, 30f), Tooltip("몬스터 로밍 범위 z (세로)")]
    public int roaming_RangeZ;

    public float MaxHP;
    public float HP;
    //몬스터 필요한 데이터 계속 추가.. 할 것.



}