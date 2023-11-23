using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.Animations;

public class MonsterPattern_Boss_Abyss : MonsterPattern_Boss
{
    //! 보스 몬스터 나락.
    public override void Init()
    {
        m_monster = GetComponent<Monster>();
        m_animator = GetComponent<Animator>();

        //rigid = GetComponent<Rigidbody>();

        playerTrans = GameManager.Instance.gameData.GetPlayerTransform();
        playerTargetPos = GameManager.Instance.gameData.playerTargetPos;
        m_monster.monsterPattern = this;
        if (m_monster.monsterData.movingMonster)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.updateRotation = false;
        }

        playerlayerMask = 1 << playerLayerId; //플레이어 레이어

        ChangeMonsterState(MonsterState.Roaming);
        originPosition = transform.position;

        overlapRadius = m_monster.monsterData.overlapRadius; //플레이어 감지 범위.
        roaming_RangeX = m_monster.monsterData.roaming_RangeX; //로밍 범위 x;
        roaming_RangeZ = m_monster.monsterData.roaming_RangeZ; //로밍 범위 y;
        CheckRoam_Range();

        playerHide = true;

        curBossPhase = BossMonsterPhase.Phase1;
    }


    public override void Monster_Pattern()
    {
        if (curMonsterState != MonsterState.Death)
        {
            switch (curMonsterState)
            {
                case MonsterState.Roaming:
                    Roam_Monster();
                    CheckPlayerCollider();
                    break;
                case MonsterState.Discovery:
                    break;
                case MonsterState.Tracing:
                case MonsterState.Attack:
                case MonsterState.GetHit:

                    break;
                case MonsterState.GoingBack:

                    break;
                default:
                    break;
            }
        }
    }

    // *---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 로밍(시네머신 등장 신.)
    public override void Roam_Monster()
    {
        if (!isRoaming)
        {
            isRoaming = true;
            //x와 Z주변을 배회하는 몬스터
            // roam_Monster_co = StartCoroutine(Roam_Monster_co());
        }
    }
    /*
        public override void CheckPlayerCollider()
        {
            if (curMonsterState != MonsterState.Death)
            {
                //로밍중, 집돌아갈 때 플레이어 콜라이더 감지중
                Collider[] playerColliders = Physics.OverlapSphere(transform.position, overlapRadius, playerlayerMask);

                Vector3 curDirection = GetDirection(playerTargetPos.position, transform.position);
                playerHide = HidePlayer(transform.position, curDirection.normalized);

                if (0 < playerColliders.Length)
                {
                    if (!playerHide) //*플레이어가 안숨었을 경우에만..
                    {
                        //몬스터의 범위에 들어옴
                        //로밍 코루틴 제거
                        if (isRoaming)
                        {
                            bool inFrontOf_Player = PlayerLocationCheck();
                            bool findPlayer = false;
                            if (!inFrontOf_Player)
                            {
                                //* 플레이어가 몬스터 뒤에 있음.
                                float distance = Vector3.Distance(transform.position, playerTrans.position);
                                if (distance < findPlayerDistance)
                                {
                                    //플레이어가 몬스터 뒤에 있지만 일정 거리 가까워졌을때.
                                    // >>>> 발견
                                    findPlayer = true;

                                }
                                else
                                    findPlayer = true;

                                if (findPlayer)
                                {
                                    StopCoroutine(roam_Monster_co);
                                    isRoaming = false;

                                    ChangeMonsterState(MonsterState.Discovery);
                                }
                            }
                            if (isFinding || isGoingBack)
                            {
                                //집돌아가는 도중이면 다시 추적 또는 찾은 후라면
                                ChangeMonsterState(MonsterState.Tracing);
                                isFinding = false;
                                isGoingBack = false;
                            }
                        }
                        else
                        {
                            if (isFinding) //* State : Discorvery
                            {
                                isFinding = false;
                                ChangeMonsterState(MonsterState.Roaming);

                            }
                        }
                    }
                    else
                    {
                        if (isFinding) //* State : Discorvery
                        {
                            //플레이어가 나갔을 경우
                            isFinding = false;
                            ChangeMonsterState(MonsterState.GoingBack);

                        }
                    }
                }
            }
    */
}
