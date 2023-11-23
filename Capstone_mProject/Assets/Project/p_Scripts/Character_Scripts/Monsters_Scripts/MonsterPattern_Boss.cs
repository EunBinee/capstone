using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPattern_Boss : MonsterPattern
{
    // * 보스 몬스터 : 패턴
    public enum BossMonsterPhase
    {
        Phase1,
        Phase2,
        Phase3
    }

    protected BossMonsterPhase curBossPhase;

}
