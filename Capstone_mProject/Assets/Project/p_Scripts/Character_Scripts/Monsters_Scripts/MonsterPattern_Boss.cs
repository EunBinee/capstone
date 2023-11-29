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

    public enum BossMonsterAttackAnimation
    {
        ResetAttackAnim,
        Skill01,
        Skill02,
        Skill03,
        Skill04
    }

    protected BossMonsterPhase curBossPhase;


    public virtual void SetBossAttackAnimation(BossMonsterAttackAnimation bossMonsterAttackAnimation, int animIndex = 0)
    {
        switch (bossMonsterAttackAnimation)
        {
            case BossMonsterAttackAnimation.ResetAttackAnim:
                break;
            case BossMonsterAttackAnimation.Skill01:
                switch (animIndex)
                {
                    case 0:
                        //점프
                        break;
                    case 1:
                        //착지
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }
}
