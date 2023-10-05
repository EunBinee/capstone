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

    [Header("몬스터 정보")]
    public int monsterid;
    public MonsterType monsterType;

    [Header("몬스터 체력")]
    public double MaxHP;
    public double HP;

    [Space]
    [Header("몬스터 플레이어 탐지 범위 (반지름)")]
    public float overlapRadius;

    [Header("몬스터 로밍 범위 (x , z)")]
    [Range(5f, 30f), Tooltip("몬스터 로밍 범위 x (가로)")]
    public int roaming_RangeX;
    [Range(5f, 30f), Tooltip("몬스터 로밍 범위 z (세로)")]
    public int roaming_RangeZ;

    [Space]
    [Header("단/원거리 능력 개수")]
    public int shortAttack_Num = 0;
    public int LongAttack_Num = 0;

}