using System;
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
    public enum BossMonsterMotion
    {
        Skill01,
        Skill02,
        Skill03,
        Skill04,
        GetHit,
        Death
    }

    protected BossMonsterPhase curBossPhase;
    protected float Phase1_BossHP = 0;
    protected float Phase2_BossHP = 0;
    protected float Phase3_BossHP = 0;

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

    public virtual void Monster_Motion(BossMonsterMotion monsterMotion)
    {
        switch (monsterMotion)
        {
            case BossMonsterMotion.Skill01:
                break;
            case BossMonsterMotion.Skill02:
                break;
            case BossMonsterMotion.Skill03:
                break;
            case BossMonsterMotion.Skill04:
                break;
            case BossMonsterMotion.GetHit:
                break;
            case BossMonsterMotion.Death:
                break;
            default:
                break;
        }
    }

    public virtual void ChangeBossPhase(BossMonsterPhase bossMonsterPhase)
    {
        curBossPhase = bossMonsterPhase;

        switch (curBossPhase)
        {
            case BossMonsterPhase.Phase1:
                break;
            case BossMonsterPhase.Phase2:
                break;
            case BossMonsterPhase.Phase3:
                break;
        }
    }

    protected void CheckBossHP()
    {
        Phase1_BossHP = (float)m_monster.monsterData.MaxHP;
        Phase2_BossHP = (float)(m_monster.monsterData.MaxHP * 0.7f);
        Phase3_BossHP = (float)(m_monster.monsterData.MaxHP * 0.2f);
    }

    public virtual void Base_Phase_HP()
    {
        //HP로 나누는 페이즈


    }
}
