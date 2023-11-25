using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.Animations;
using UnityEditor.Rendering;

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
                    break;
                case MonsterState.Discovery:
                    break;
                case MonsterState.Tracing:
                    break;
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

    public override void SetAnimation(MonsterAnimation m_anim)
    {
        switch (m_anim)
        {
            case MonsterAnimation.Idle:
                break;
            case MonsterAnimation.Move:
                break;
            case MonsterAnimation.GetHit:
                break;
            case MonsterAnimation.Death:
                break;
            default:
                break;
        }
    }

    public override void SetBossAttackAnimation(BossMonsterAttackAnimation bossMonsterAttackAnimation, int animIndex = 0)
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
                        m_animator.SetTrigger("isJumping");
                        break;
                    case 1:
                        //착지
                        m_animator.SetTrigger("isLanding");
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }
    // *---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 로밍(시네머신 등장 신.)
    public override void Roam_Monster()
    {
        if (!isRoaming)
        {
            isRoaming = true;
            //TODO: 나중에 범위안에 들어오면, 등장씬 나오도록 수정
            //* 일단은 바로 공격하도록

            isRoaming = false;
            isFinding = true;
            ChangeMonsterState(MonsterState.Tracing);

            StartCoroutine(BossAbyss_Skill01());
        }
    }
    //*----------------------------------------------------------------------------------------------------------//

    //* 스킬 01 내려찍기
    IEnumerator BossAbyss_Skill01()
    {
        yield return new WaitForSeconds(2f);
        Vector3 originPos = transform.position;

        Effect effect = GameManager.Instance.objectPooling.ShowEffect("HeartOfBattle_01");
        effect.transform.position = originPos;

        yield return new WaitForSeconds(1f);

        float time = 0f;
        float speed = 0;

        //* 점프 --------------------------------------------------------------------//
        SetBossAttackAnimation(BossMonsterAttackAnimation.Skill01, 0);
        yield return new WaitForSeconds(1f);
        // 점프전 잠깐 밑으로 내려감.
        speed = 30f;
        while (time < 0.1)
        {
            time += Time.deltaTime;
            transform.Translate(-Vector3.up * speed * Time.deltaTime);

            yield return null;
        }

        //? 연기이펙트-----------------------------------------------------------------------//
        effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_02");
        Vector3 effectPos = originPos;
        effectPos.y += 2.5f;
        effect.transform.position = effectPos;
        //------------------------------------------------------------------------------------//
        //점프
        time = 0;
        speed = 30;
        Vector3 targetPos = transform.position + (Vector3.up * 60);
        while (time < 5f)
        {
            time += Time.deltaTime;
            speed = Mathf.Lerp(90, 60, Time.time);
            transform.Translate(Vector3.up * speed * Time.deltaTime);

            if (transform.position.y >= targetPos.y)
                break;

            yield return null;
        }

        //*-------------------------------------------------------------------------------//
        //* 플레이어를 쫓아 다니는 이펙트 
        float duration = 5f;
        StartCoroutine(FollowPlayer_Effect_InSkill01(duration));

        yield return new WaitForSeconds(duration);
        Vector3 curPlayerPos = playerTrans.position;
        transform.position = new Vector3(curPlayerPos.x, transform.position.y, curPlayerPos.z);

        speed = 50f;
        time = 0;
        SetBossAttackAnimation(BossMonsterAttackAnimation.Skill01, 1);
        yield return new WaitForSeconds(1f);
        while (time < 5f)
        {
            time += Time.deltaTime;
            speed = Mathf.Lerp(50, 90, Time.time);
            transform.Translate(-Vector3.up * speed * Time.deltaTime);

            if (transform.position.y <= curPlayerPos.y)
                break;
            yield return null;
        }

        CheckPlayerDamage(6f, 20);
        //? 연기이펙트-----------------------------------------------------------------------//
        effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect");
        effect.transform.position = transform.position;


        //------------------------------------------------------------------------------------//
        yield return null;
    }

    IEnumerator FollowPlayer_Effect_InSkill01(float duration)
    {
        //* 스킬01 내려찍기 중, 플레이어를 쫒아다니는 이펙트 
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("PulseGrenade_01");
        effect.transform.position = playerTrans.position;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;

            effect.transform.position = playerTrans.position;
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        effect.StopEffect();
    }
    // *---------------------------------------------------------------------------------------------------------//
}
