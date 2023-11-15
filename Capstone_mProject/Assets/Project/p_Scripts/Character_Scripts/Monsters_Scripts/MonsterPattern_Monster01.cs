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
    private List<MonsterWeapon_CollisionCheck> weaponsChecks;

    [Header("플레이어가 뒤에 있을때 몬스터가 눈치까는 거리")]
    public float findPlayerDistance = 6f;

    [Space]
    public Transform attackEffectPos;
    public string shortAttackEffectName;
    public string LongAttackEffectName;
    Vector3 effectRotation;

    Effect curEffect = null;

    Coroutine roam_Monster_co = null;
    Coroutine short_Range_Attack_co = null;
    Coroutine long_Range_Attack_co = null;

    Action GetHit_duringLongRangeAttack = null;

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

        weaponsChecks = new List<MonsterWeapon_CollisionCheck>();

        for (int i = 0; i < weapons.Length; ++i)
        {
            weaponsChecks.Add(weapons[i].gameObject.GetComponent<MonsterWeapon_CollisionCheck>());
        }

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
                        //왼손 공격
                        effectRotation = new Vector3(-35, -145, 35);
                        m_animator.SetTrigger("m_s_Attack01");
                        break;
                    case 1:
                        effectRotation = new Vector3(-30, 50, 190);
                        m_animator.SetTrigger("m_s_Attack02");
                        break;
                    case 2:
                        effectRotation = new Vector3(0, 0, 0);
                        m_animator.SetTrigger("m_s_Attack03");
                        break;
                    case 3:
                        effectRotation = new Vector3(0, 0, 0);
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
        if (curMonsterState != MonsterState.Death)
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
                case MonsterState.GetHit:

                    break;
                case MonsterState.GoingBack:
                    GoingBack_Movement();
                    break;
                default:
                    break;
            }
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
        if (!isTracing)
        {
            isTracing = true;
            SetPlayerAttackList(true);
        }
        //움직임.
        if (navMeshAgent.isStopped == true)
            SetMove_AI(true);
        navMeshAgent.SetDestination(playerTrans.position);
        SetAnimation(MonsterAnimation.Move);
        //몬스터와 플레이어 사이의 거리 체크
        CheckDistance();
    }
    // * ---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 다시 자기자리로
    public override void GoingBack_Movement()
    {
        if (isTracing)
        {
            isTracing = false;
            SetPlayerAttackList(false);
        }

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
                else if (distance >= 8f && distance < 12f)
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
                //long_Range_Attack_co = StartCoroutine(Long_Range_Attack02_Monster01());
                long_Range_Attack_co = StartCoroutine(Long_Range_Attack03_Monster01());
                break;
            case MonsterMotion.GetHit_KnockBack:
                //피격=>>넉백
                GetHit();
                if (!isGettingHit) //넉백
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
        SetAttackAnimation(MonsterAttackAnimation.Short_Range_Attack, index);

        yield return new WaitForSeconds(0.5f);

        Effect effect = GameManager.Instance.objectPooling.ShowEffect(shortAttackEffectName, attackEffectPos);
        curEffect = effect;

        effect.finishAction = () =>
        {
            curEffect = null;
        };

        effect.transform.localEulerAngles = effectRotation;
        effect.transform.position = attackEffectPos.position;

        EnabledWeaponsCollider(true);

        yield return new WaitUntil(() => (m_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")));


        float distance = Vector3.Distance(transform.position, playerTrans.position);
        if (distance < 1.3f)
        {
            yield return new WaitForSeconds(1f);
            Monster_Motion(MonsterMotion.Short_Range_Attack);
        }
        else
        {
            EnabledWeaponsCollider(false);

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

        EnabledWeaponsCollider(true);

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

        EnabledWeaponsCollider(false);

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

        EnabledWeaponsCollider(true);

        float distance = 0;
        float time = 0;
        bool goingBack = false;

        bool isAttack = true;
        noAttack = true;

        yield return new WaitForSeconds(0.5f);
        Effect effect = null;

        // TODO : 나중에 원거리 이펙트 정해진 후, LongAttackEffectName != "" 이 부분 전부 없애기
        if (LongAttackEffectName != "")
        {
            effect = GameManager.Instance.objectPooling.ShowEffect(LongAttackEffectName, attackEffectPos);
            if (effect != null)
            {
                effect.transform.localEulerAngles = new Vector3(0, 0, 0);
                effect.transform.position = attackEffectPos.position;
            }
        }


        while (true)
        {
            if (time <= 5 && isAttack)
            {
                navMeshAgent.SetDestination(playerTargetPos.position);
                distance = Vector3.Distance(transform.position, playerTargetPos.position);
                if (LongAttackEffectName != "" && effect != null)
                {
                    effect.transform.position = attackEffectPos.position;
                }
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
                if (LongAttackEffectName != "" && effect != null)
                {
                    effect.StopEffect();
                }
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
                //* 이펙트

                yield return new WaitForSeconds(0.5f);
                if (LongAttackEffectName != "")
                {
                    effect = GameManager.Instance.objectPooling.ShowEffect(LongAttackEffectName, attackEffectPos);
                    if (effect != null)
                    {
                        effect.transform.localEulerAngles = new Vector3(0, 0, 0);
                        effect.transform.position = attackEffectPos.position;
                    }
                }
            }
        }

        if (LongAttackEffectName != "" && effect.gameObject.activeSelf && effect != null)
            effect.StopEffect();

        EnabledWeaponsCollider(false);

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

    //TODO: 원거리 공격 03
    IEnumerator Long_Range_Attack03_Monster01()
    {
        float defaultSpeed = navMeshAgent.speed;
        float defaultAcceleration = navMeshAgent.acceleration;
        SetMove_AI(false);
        SetAnimation(MonsterAnimation.Idle);
        navMeshAgent.speed = 120f; //*엄청 빠른 속도!
        navMeshAgent.acceleration = 90f;

        PlayerController playerController = GameManager.instance.gameData.player.GetComponent<PlayerController>();
        Vector3 curPlayerPos = playerTrans.position;

        noAttack = true;
        bool isGoingBack = false;

        NavMeshHit hit;
        int attackNum = 0;
        float distance;
        //* 몬스터가 갈 수 있는 위치일 경우에만 넉백~!
        while (attackNum < 3)
        {
            curPlayerPos = playerTrans.position;
            SetAttackAnimation(MonsterAttackAnimation.ResetAttackAnim);

            noAttack = false;

            if (NavMesh.SamplePosition(curPlayerPos, out hit, 20f, NavMesh.AllAreas))
            {
                //TODO: 플레이어 아래에 이펙트 ----//
                //** 이펙트가 계속 플레이어 따라가도록...(공격 이펙트 아님)
                //아마 새로 코루틴 파야할듯. 밑에 딜레이 1초 때문에..
                //계속 curPlayerPos 새로 업데이트..
                //-----------------------------//
            }

            //TODO: 2초가 지나기 전에 몬스터가 공격 받았을 경우 
            // => 이펙트 풀리기
            // =>
            GetHit_duringLongRangeAttack += () =>
            {
                //*원거리 공격 도중에 플레이어가 맞는다면!??
                SetAttackAnimation(MonsterAttackAnimation.ResetAttackAnim);

                navMeshAgent.speed = defaultSpeed;
                navMeshAgent.acceleration = defaultAcceleration;

                noAttack = false;



                //TODO :  이펙트 사라지게!(공격 이펙트 아님)
            };

            yield return new WaitForSeconds(1f);
            curPlayerPos = playerTrans.position;

            float m_distance = Vector3.Distance(curPlayerPos, transform.position);
            //*플레이어와 몬스터의 거리가 n이하일 경우에만 공격, 멀어졌다면 공격 중지..
            if (m_distance <= 25f)
            {
                if (NavMesh.SamplePosition(curPlayerPos, out hit, 20f, NavMesh.AllAreas))
                {
                    //원거리 공격 애니메이션션
                    noAttack = true;
                    SetAttackAnimation(MonsterAttackAnimation.Long_Range_Attack);
                    EnabledWeaponsCollider(true);

                    navMeshAgent.SetDestination(curPlayerPos); //특정 위치로 이동.
                    SetMove_AI(true);

                    while (true)
                    {
                        //공격
                        navMeshAgent.SetDestination(curPlayerPos);
                        distance = Vector3.Distance(transform.position, curPlayerPos);

                        if (playerController._currentState.isGettingHit && playerController.Get_CurHitEnemy() == m_monster)
                        {
                            //*원거리 공격에 플레이어가 맞았을 경우 
                            attackNum = 3; //공격 끝
                            break;
                        }
                        if (distance <= 0.5f)
                        {
                            break;
                        }

                        yield return null;
                    }
                    attackNum++;
                }
                else
                {
                    //갈 수 없는 위치면??
#if UNITY_EDITOR
                    Debug.LogError("몬스터 01 원거리 공격 : 갈 수 없는 곳");
#endif
                    //!단거리 공격으로 변환! 로밍 상태로 돌아가는 거 아님
                    isGoingBack = false;
                    break;
                }
            }
            else
            {
                //플레이어와 거리가 멀어서 집으로 돌아감
                isGoingBack = true;
                break;
            }
        }

        EnabledWeaponsCollider(false);


        navMeshAgent.speed = defaultSpeed;
        navMeshAgent.acceleration = defaultAcceleration;

        noAttack = false;

        SetMove_AI(false);
        SetAttackAnimation(MonsterAttackAnimation.ResetAttackAnim);

        if (isGoingBack)
        {
            ChangeMonsterState(MonsterState.GoingBack);
        }
        else
            ChangeMonsterState(MonsterState.Tracing);

        long_Range_Attack_co = null;

        yield return null;
    }


    // * 피격 모션01 :플레이어의 반대 방향으로 넉백
    private void GetHit()
    {
        //? 피격 이펙트
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Power_Impact_Fire_02_01", attackEffectPos);
        float x = UnityEngine.Random.Range(-1.0f, 1.0f);
        float y = UnityEngine.Random.Range(-1.0f, 1.0f);
        float z = UnityEngine.Random.Range(-1.0f, 1.0f);
        Vector3 randomPos = new Vector3(x, y, z);
        effect.transform.position = attackEffectPos.position + randomPos;
    }

    IEnumerator GetHit_KnockBack_co()
    {
        StopAtackCoroutine();

        MonsterState preState = curMonsterState;
        ChangeMonsterState(MonsterState.GetHit);

        if (curEffect != null)
        {
            //* 공격 이펙트 없앰.
            curEffect.StopEffect();
            curEffect = null;
        }

        SetMove_AI(false);
        SetAnimation(MonsterAnimation.Idle);
        SetAnimation(MonsterAnimation.GetHit);

        Vector3 knockback_Dir = transform.position - playerTrans.position;
        knockback_Dir = knockback_Dir.normalized;
        Vector3 KnockBackPos = transform.position + knockback_Dir * 2f; // 넉백 시 이동할 위치
        float time = 0;

        NavMeshHit hit;
        //* 몬스터가 갈 수 있는 위치일 경우에만 넉백~!
        if (NavMesh.SamplePosition(KnockBackPos, out hit, 20f, NavMesh.AllAreas))
        {
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
        }

        navMeshAgent.Warp(transform.position);
        yield return new WaitForSeconds(1.5f);

        SetMove_AI(true);
        ChangeMonsterState(MonsterState.Tracing);

        isGettingHit = false;
    }

    // * 죽음 모션
    IEnumerator Death_co()
    {
        StopAtackCoroutine();

        if (isTracing)
        {
            isTracing = false;
            SetPlayerAttackList(false);
        }

        SetAnimation(MonsterAnimation.Idle);
        ChangeMonsterState(MonsterState.Death);
        SetMove_AI(false);

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = true;
        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.enabled = false;

        yield return new WaitForSeconds(0.5f);

        m_monster.RetrunHPBar();
        SetAnimation(MonsterAnimation.Death);

        yield return new WaitForSeconds(5f);

        this.gameObject.SetActive(false);
    }

    public override void StopAtackCoroutine()
    {
        // * 죽음, 넉백에서 사용.
        if (short_Range_Attack_co != null)
        {
            StopCoroutine(short_Range_Attack_co);
            short_Range_Attack_co = null;
        }
        if (long_Range_Attack_co != null)
        {
            StopCoroutine(long_Range_Attack_co);
            GetHit_duringLongRangeAttack?.Invoke();

            GetHit_duringLongRangeAttack = null;
            long_Range_Attack_co = null;
        }

        EnabledWeaponsCollider(false);
    }

    private void EnabledWeaponsCollider(bool enable)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].enabled = enable;
            weaponsChecks[i].onEnable = enable;
        }
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
