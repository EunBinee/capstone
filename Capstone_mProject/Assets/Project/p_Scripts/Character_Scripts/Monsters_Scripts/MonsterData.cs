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

    [SerializeField] private int monsterid;
    [SerializeField] private MonsterType monsterType;

    //몬스터 필요한 데이터 계속 추가.. 할 것.



}