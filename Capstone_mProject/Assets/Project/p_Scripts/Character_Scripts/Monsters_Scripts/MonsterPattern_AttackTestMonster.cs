using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPattern_AttackTestMonster : MonsterPattern
{
    bool first = false;
    public override void Init()
    {
        m_monster = GetComponent<Monster>();
        m_animator = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody>();
        playerTrans = GameManager.Instance.gameData.GetPlayerTransform();
        playerTargetPos = GameManager.Instance.gameData.playerTargetPos;

        m_monster.monsterPattern = this;

        playerlayerMask = 1 << playerLayerId; //플레이어 레이어

        ChangeMonsterState(MonsterState.Roaming);
        originPosition = transform.position;

        playerHide = false;
    }

    public override void Monster_Pattern()
    {
        if (curMonsterState != MonsterState.Death)
        {
            switch (curMonsterState)
            {
                case MonsterState.Roaming:
                    if (!first)
                    {
                        first = true;

                        if (m_monster.HPBar_CheckNull() == false)
                            m_monster.GetHPBar();

                        SetPlayerAttackList(true);
                    }
                    break;
                case MonsterState.Discovery:
                    break;
                case MonsterState.Tracing:

                    break;
                case MonsterState.Attack:
                    break;
                case MonsterState.GetHit:

                    break;
                case MonsterState.GoingBack:
                    break;
                default:
                    break;
            }
        }
    }


}