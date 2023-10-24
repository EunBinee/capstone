using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.Animations;

public class MonsterPattern_Monster01 : MonsterPattern
{
    [Header("몬스터 무기 : 인덱스 0번 L쪽 무기, 인덱스 1번 R쪽 무기")]
    public Collider[] weapons;

    [Header("플레이어가 뒤에 있을때 몬스터가 눈치까는 거리")]
    public float findPlayerDistance = 6f;

    Coroutine roam_Monster_co = null;
    Coroutine short_Range_Attack_co = null;
    Coroutine long_Range_Attack_co = null;

    public override void Init()
    {
        m_monster = GetComponent<Monster>();
        m_animator = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody>();
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

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = false;
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.enabled = true;

        playerHide = true;
    }

    public override void SetAnimation(MonsterAnimation m_anim)
    {
        switch (m_anim)
        {
            case MonsterAnimation.Idle:
                m_animator.SetBool("m_Walk", false);
                m_animator.SetBool("m_Idle", true);
                break;
            case MonsterAnimation.Move:
                m_animator.SetBool("m_Walk", true);
                m_animator.SetBool("m_Idle", false);
                break;
            case MonsterAnimation.GetHit:
                m_animator.SetTrigger("m_GetHit");
                break;
            case MonsterAnimation.Death:
                m_animator.SetBool("m_Walk", false);
                m_animator.SetBool("m_Idle", false);
                m_animator.SetBool("m_Death", true);
                break;
            default:
                break;
        }
    }

    public override void SetAttackAnimation(MonsterAttackAnimation monsterAttackAnimation, int animIndex = 0)
    {
        switch (monsterAttackAnimation)
        {
            case MonsterAttackAnimation.ResetAttackAnim:
                m_animator.SetBool("m_Walk", false);
                m_animator.SetBool("m_Idle", true);
                m_animator.SetBool("m_l_WalkSpinAttack", false);
                break;
            case MonsterAttackAnimation.Short_Range_Attack:
                switch (animIndex)
                {
                    case 0:
                        m_animator.SetTrigger("m_s_Attack01");
                        break;
                    case 1:
                        m_animator.SetTrigger("m_s_Attack02");
                        break;
                    case 2:
                        m_animator.SetTrigger("m_s_Attack03");
                        break;
                    case 3:
                        m_animator.SetTrigger("m_s_Attack04");
                        break;
                    default:
                        break;
                }
                break;
            case MonsterAttackAnimation.Long_Range_Attack:
                m_animator.SetBool("m_Walk", false);
                m_animator.SetBool("m_Idle", false);
                m_animator.SetBool("m_l_WalkSpinAttack", true);
                break;
            default:
                break;
        }
    }

    public override void Monster_Pattern()
    {
        switch (curMonsterState)
        {
            case MonsterState.Roaming:
                if (m_monster.HPBar_CheckNull() == true)
                    m_monster.RetrunHPBar();
                Roam_Monster();
                CheckPlayerCollider();
                break;
            case MonsterState.Discovery:
                Discovery_Player();
                break;
            case MonsterState.Tracing:
                Tracing_Movement();
                if (m_monster.HPBar_CheckNull() == false)
                    m_monster.GetHPBar();
                break;
            case MonsterState.Attack:
                if (m_monster.HPBar_CheckNull() == false)
                    m_monster.GetHPBar();
                break;
            case MonsterState.GoingBack:
                GoingBack_Movement();
                break;
            default:
                break;
        }
    }
    // *---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 로밍
    public override void Roam_Monster()
    {
        if (!isRoaming)
        {
            isRoaming = true;
            //x와 Z주변을 배회하는 몬스터
            roam_Monster_co = StartCoroutine(Roam_Monster_co());
        }
    }

    IEnumerator Roam_Monster_co()
    {
        bool monsterRoaming = false;
        Vector3 randomPos = Vector3.zero;

        monsterRoaming = false;
        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
        navMeshAgent.updatePosition = false;

        while (curMonsterState == MonsterState.Roaming)
        {
            if (!monsterRoaming)
            {
                SetAnimation(MonsterAnimation.Idle);

                float roamTime = UnityEngine.Random.Range(2, 4);

                yield return new WaitForSeconds(roamTime);

                if ((isFinding == false) && (curMonsterState != MonsterState.Death))
                {
                    float distance = 0;
                    bool checkObstacle = false;
                    while (true)
                    {
                        randomPos = GetRandom_RoamingPos();
                        mRoaming_randomPos = randomPos;
                        distance = Vector3.Distance(transform.position, randomPos);
                        checkObstacle = CheckObstacleCollider(randomPos);
                        if (distance > 3f && checkObstacle)
                            break;
                    }
                    SetMove_AI(true);
                    navMeshAgent.SetDestination(randomPos);
                    SetAnimation(MonsterAnimation.Move);
                    monsterRoaming = true;
                }
            }
            else if (monsterRoaming)
            {
                if (Vector3.Distance(transform.position, randomPos) < 0.5f)
                {
                    monsterRoaming = false;
                    SetMove_AI(false);
                }
                yield return null;
            }
        }
    }

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
    // * ---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 발견
    public override void Discovery_Player()
    {
        if (!isFinding)
        {
            isFinding = true;
            StartCoroutine(DiscoveryPlayer_co());
        }
    }

    IEnumerator DiscoveryPlayer_co()
    {
        SetMove_AI(false);
        SetAnimation(MonsterAnimation.Idle);
        float time = 0f; //예비 탈출용
        Vector3 curPlayerPos = playerTrans.position;
        Vector3 curPlayerdirection = curPlayerPos - transform.position;
        Quaternion targetAngle = Quaternion.LookRotation(curPlayerdirection);

        while (time < 1.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Time.deltaTime * 5.0f);

            if (transform.rotation == targetAngle)
                break;
            else
            {
                time += Time.deltaTime;
                yield return null;
            }
        }

        //yield return new WaitForSeconds(2f);
        CheckPlayerCollider();
    }

    // * ---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 추적
    public override void Tracing_Movement()
    {
        //움직임.
        navMeshAgent.SetDestination(playerTrans.position);
        SetMove_AI(true);

        SetAnimation(MonsterAnimation.Move);

        //몬스터와 플레이어 사이의 거리 체크
        CheckDistance();
    }
    // * ---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 다시 자기자리로
    public override void GoingBack_Movement()
    {
        SetMove_AI(true);

        SetAnimation(MonsterAnimation.Move);

        navMeshAgent.SetDestination(originPosition);
        CheckDistance();       //계속 거리 체크
        CheckPlayerCollider();
    }

    public override void CheckDistance()
    {
        float distance = 0f;
        //해당 몬스터와 플레이어 사이의 거리 체크
        switch (curMonsterState)
        {
            case MonsterState.Roaming:
                break;

            case MonsterState.Tracing:
                distance = Vector3.Distance(transform.position, playerTrans.position);
                //만약 몬스터와 캐릭터의 거리가 멀어지면, 다시 원위치로.
                if (distance >= 12f)
                {
                    isGoingBack = true;
                    ChangeMonsterState(MonsterState.GoingBack);
                }

                if (distance <= 1.3f)
                {
                    //거리가 2.5만큼 가깝다.
                    //일반 공격
                    ChangeMonsterState(MonsterState.Attack);
                    Monster_Motion(MonsterMotion.Short_Range_Attack);
                }
                else if (distance >= 7f && distance < 12f)
                {
                    ChangeMonsterState(MonsterState.Attack);
                    Monster_Motion(MonsterMotion.Long_Range_Attack);
                }

                break;

            case MonsterState.Attack:
                break;

            case MonsterState.GoingBack:
                distance = Vector3.Distance(transform.position, originPosition);
                if (distance < 1f)
                {
                    isGoingBack = false;
                    ChangeMonsterState(MonsterState.Roaming);
                }
                break;
        }
    }
    // * ---------------------------------------------------------------------------------------------------------//
    // * 몬스터 공격 모션, 피격 모션, 죽음 모션
    public override void Monster_Motion(MonsterMotion monsterMotion)
    {
        switch (monsterMotion)
        {
            case MonsterMotion.Short_Range_Attack:
                //근거리 공격
                if (short_Range_Attack_co != null)
                    StopCoroutine(short_Range_Attack_co);
                short_Range_Attack_co = StartCoroutine(Short_Range_Attack_Monster01());
                break;
            case MonsterMotion.Long_Range_Attack:
                //원거리 공격
                if (short_Range_Attack_co != null)
                    StopCoroutine(long_Range_Attack_co);
                long_Range_Attack_co = StartCoroutine(Long_Range_Attack02_Monster01());
                break;
            case MonsterMotion.GetHit_KnockBack:
                //피격=>>넉백
                if (!isGettingHit)
                {
                    isGettingHit = true;
                    StartCoroutine(GetHit_KnockBack_co());
                }
                break;
            case MonsterMotion.Death:
                //죽음
                if (curMonsterState != MonsterState.Death)
                {
                    StartCoroutine(Death_co());
                }
                break;
            default:
                break;
        }
    }
    // * ---------------------------------------------------------------------------------------------------------//
    // * 근거리 공격 01
    IEnumerator Short_Range_Attack_Monster01()
    {
        SetMove_AI(false);
        SetAnimation(MonsterAnimation.Idle);

        int index = UnityEngine.Random.Range(0, m_monster.monsterData.shortAttack_Num);

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].enabled = true;
        }

        SetAttackAnimation(MonsterAttackAnimation.Short_Range_Attack, index);

        yield return new WaitUntil(() => (m_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")));


        float distance = Vector3.Distance(transform.position, playerTrans.position);
        if (distance < 1.3f)
        {
            yield return new WaitForSeconds(0.5f);
            Monster_Motion(MonsterMotion.Short_Range_Attack);
        }
        else
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                weapons[i].enabled = false;
            }

            ChangeMonsterState(MonsterState.Tracing);
            short_Range_Attack_co = null;
        }
    }

    // * 원거리 공격 01
    IEnumerator Long_Range_Attack_Monster01()
    {
        float defaultSpeed = navMeshAgent.speed;
        SetMove_AI(false);

        navMeshAgent.speed = 8f;

        SetAttackAnimation(MonsterAttackAnimation.Long_Range_Attack);
        SetMove_AI(true);

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].enabled = true;
        }

        float distance = 0;
        float time = 0;
        bool goingBack = false;

        while (time <= 10f)
        {
            navMeshAgent.SetDestination(playerTrans.position);
            distance = Vector3.Distance(transform.position, playerTrans.position);

            if (distance <= 1f)
                break;
            if (distance > 15)
            {
                goingBack = true;
                break;
            }
            yield return null;
            time += Time.deltaTime;
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].enabled = false;
        }
        navMeshAgent.speed = defaultSpeed;

        SetMove_AI(false);
        SetAttackAnimation(MonsterAttackAnimation.ResetAttackAnim);

        if (goingBack)
        {
            isGoingBack = true;
            ChangeMonsterState(MonsterState.GoingBack);
        }
        else
        {
            ChangeMonsterState(MonsterState.Tracing);
        }


    }

    //* 원거리 공격 02
    IEnumerator Long_Range_Attack02_Monster01()
    {
        float defaultSpeed = navMeshAgent.speed;
        SetMove_AI(false);

        navMeshAgent.speed = 15f;

        SetAttackAnimation(MonsterAttackAnimation.Long_Range_Attack);
        SetMove_AI(true);

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].enabled = true;
        }

        float distance = 0;
        float time = 0;
        bool goingBack = false;

        bool isAttack = true;
        noAttack = true;
        while (true)
        {
            if (time <= 5 && isAttack)
            {
                navMeshAgent.SetDestination(playerTargetPos.position);
                distance = Vector3.Distance(transform.position, playerTargetPos.position);
                if (distance <= 2f)
                    break;
                if (distance > 15)
                {
                    goingBack = true;
                    break;
                }
                yield return null;
                time += Time.deltaTime;
            }
            else if (time > 5 && isAttack)
            {
                // 공격 정지
                time = 0;
                isAttack = false;
                noAttack = false;

                SetMove_AI(false);
                SetAttackAnimation(MonsterAttackAnimation.ResetAttackAnim);

            }

            //* 공격 쉬는 시간.------------------------------------------------------------------//
            if (time <= 3 && !isAttack)
            {
                time += Time.deltaTime;
                yield return null;
            }
            else if (time > 3 && !isAttack)
            {
                //공격 다시 시작
                time = 0;
                isAttack = true;
                noAttack = true;

                SetAttackAnimation(MonsterAttackAnimation.Long_Range_Attack);
                SetMove_AI(true);

            }
        }


        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].enabled = false;
        }

        navMeshAgent.speed = defaultSpeed;
        noAttack = false;

        SetMove_AI(false);
        SetAttackAnimation(MonsterAttackAnimation.ResetAttackAnim);

        if (goingBack)
        {
            isGoingBack = true;
            ChangeMonsterState(MonsterState.GoingBack);
        }
        else
        {
            ChangeMonsterState(MonsterState.Tracing);
        }

        long_Range_Attack_co = null;
        yield return null;
    }

    // * 피격 모션01
    IEnumerator GetHit_KnockBack_co()
    {
        //플레이어의 반대 방향으로 넉백
        MonsterState preState = curMonsterState;
        ChangeMonsterState(MonsterState.GetHit);

        SetMove_AI(false);
        SetAnimation(MonsterAnimation.Idle);
        SetAnimation(MonsterAnimation.GetHit);

        Vector3 knockback_Dir = transform.position - playerTrans.position;
        knockback_Dir = knockback_Dir.normalized;
        Vector3 KnockBackPos = transform.position + knockback_Dir * 1.5f; // 넉백 시 이동할 위치
        float time = 0;
        while (time < 0.5f)
        {
            transform.position = Vector3.Lerp(transform.position, KnockBackPos, 5 * Time.deltaTime);
            if (transform.position == KnockBackPos)
                break;
            else
            {
                time += Time.deltaTime;
                yield return null;
            }
        }

        if ((preState == MonsterState.Roaming || preState == MonsterState.Discovery) || preState == MonsterState.Attack)
            ChangeMonsterState(MonsterState.Tracing);
        else
            ChangeMonsterState(preState);

        isGettingHit = false;
    }

    // * 죽음 모션
    IEnumerator Death_co()
    {
        ChangeMonsterState(MonsterState.Death);
        SetMove_AI(false);
        SetAnimation(MonsterAnimation.Death);

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = true;
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.enabled = false;


        yield return new WaitForSeconds(5f);

        this.gameObject.SetActive(false);
    }

    // * ---------------------------------------------------------------------------------------//
    private void OnDrawGizmos()
    {
        //몬스터 감지 범위 Draw
        //크기는  monsterData.overlapRadius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, overlapRadius);

        if (curMonsterState == MonsterState.Roaming)
        {
            //만약 로밍 중이면, 로밍 범위 그리기
            Gizmos.DrawLine(roam_vertex01, roam_vertex02);
            Gizmos.DrawLine(roam_vertex02, roam_vertex03);
            Gizmos.DrawLine(roam_vertex03, roam_vertex04);
            Gizmos.DrawLine(roam_vertex04, roam_vertex01);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(mRoaming_randomPos, 1);
    }

    //로밍 범위 체크
    private void CheckRoam_Range()
    {
        //사각형 왼쪽 가장 위
        roam_vertex01 = new Vector3(transform.position.x + ((roaming_RangeX / 2) * -1), transform.position.y, transform.position.z + (roaming_RangeZ / 2));
        //사각형 왼쪽 가장 아래
        roam_vertex02 = new Vector3(transform.position.x + ((roaming_RangeX / 2) * -1), transform.position.y, transform.position.z + ((roaming_RangeZ / 2) * -1));
        //사각형 오른쪽 가장 아래
        roam_vertex03 = new Vector3(transform.position.x + (roaming_RangeX / 2), transform.position.y, transform.position.z + ((roaming_RangeZ / 2) * -1));
        //사각형 오른쪽 가장 위
        roam_vertex04 = new Vector3(transform.position.x + (roaming_RangeX / 2), transform.position.y, transform.position.z + (roaming_RangeZ / 2));
    }
}
