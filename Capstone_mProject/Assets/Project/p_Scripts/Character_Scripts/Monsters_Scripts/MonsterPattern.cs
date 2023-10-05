using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.Animations;

public class MonsterPattern : MonoBehaviour
{
    Monster m_monster;
    Animator m_animator;

    public Rigidbody rigid;
    public Vector3 originPosition; //원래 캐릭터 position

    private int playerLayerId = 3;
    private int playerlayerMask; //플레이어 캐릭터 레이어 마스크
    private Transform playerTrans;

    private NavMeshAgent navMeshAgent;
    private MonsterState curMonsterState;
    private MonsterAnimation curMonsterAnimation; //현재 진행중인 애니메이션 (기본모션만. IDLE MOVE DEATH)

    [Header("몬스터 무기 : 인덱스 0번 L쪽 무기, 인덱스 1번 R쪽 무기")]
    public Collider[] weapons;

    public enum MonsterState
    {
        Roaming,
        Discovery,
        Tracing,
        Attack,
        GetHit,
        GoingBack,
        Death
    }

    public enum MonsterAnimation
    {
        Idle,
        Move,
        GetHit,
        Death
    }

    public enum MonsterAttackAnimation
    {
        ResetAttackAnim,
        Short_Range_Attack,
        Long_Range_Attack,
    }

    private float overlapRadius;
    private int roaming_RangeX;
    private int roaming_RangeZ;
    Vector3 roam_vertex01; //사각형 왼쪽 가장 위
    Vector3 roam_vertex02; //사각형 왼쪽 가장 아래
    Vector3 roam_vertex03; //사각형 오른쪽 가장 아래
    Vector3 roam_vertex04; //사각형 오른쪽 가장 위

    Vector3 mRoaming_randomPos = Vector3.zero;

    private bool isRoaming = false;
    private bool isFinding = false;
    private bool isTracing = false;
    private bool isGoingBack = false;
    private bool isGettingHit = false;

    public enum MonsterMotion
    {
        Short_Range_Attack,
        Long_Range_Attack,
        GetHit_KnockBack,
        Death
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        m_monster = GetComponent<Monster>();
        m_animator = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody>();
        playerTrans = GameManager.Instance.gameData.GetPlayerTransform();
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;

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


    }

    public void Update()
    {
        Monster_Pattern();
        UpdateRotation();
    }

    private void FixedUpdate()
    {
        FreezeVelocity();
    }

    private void FreezeVelocity()
    {
        rigid.velocity = Vector3.zero;
        rigid.angularVelocity = Vector3.zero;
    }

    private void UpdateRotation()
    {
        if (navMeshAgent.desiredVelocity.sqrMagnitude >= 0.1f * 0.1f)
        {
            //적 ai의 이동방향
            Vector3 direction = navMeshAgent.desiredVelocity;
            //회전 각도 산출 후, 선형 보간 함수로 부드럽게 회전
            Quaternion targetAngle = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Time.deltaTime * 8.0f);
        }
    }

    private void SetAnimation(MonsterAnimation m_anim)
    {
        switch (m_anim)
        {
            case MonsterAnimation.Idle:
                curMonsterAnimation = MonsterAnimation.Idle;
                m_animator.SetBool("m_Walk", false);
                m_animator.SetBool("m_Idle", true);
                break;
            case MonsterAnimation.Move:
                curMonsterAnimation = MonsterAnimation.Move;
                m_animator.SetBool("m_Walk", true);
                m_animator.SetBool("m_Idle", false);
                m_animator.SetBool("m_Death", false);
                break;
            case MonsterAnimation.GetHit:
                m_animator.SetTrigger("m_GetHit");
                break;
            case MonsterAnimation.Death:
                curMonsterAnimation = MonsterAnimation.Death;
                m_animator.SetBool("m_Walk", false);
                m_animator.SetBool("m_Idle", false);
                m_animator.SetBool("m_Death", true);
                break;
            default:
                break;
        }
    }

    public virtual void SetAttackAnimation(MonsterAttackAnimation monsterAttackAnimation, int animIndex = 0)
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

    private void SetMove_AI(bool moveAI)
    {
        if (moveAI)
        {
            //움직임.
            navMeshAgent.isStopped = false;
            navMeshAgent.updatePosition = true;
        }
        else if (!moveAI)
        {
            //멈춤
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.updatePosition = false;
        }
    }

    private void ChangeMonsterState(MonsterState monsterState)
    {
        curMonsterState = monsterState;

#if UNITY_EDITOR
        Debug.Log("상태 변경 " + monsterState);
#endif

    }

    public void Monster_Pattern()
    {
        switch (curMonsterState)
        {
            case MonsterState.Roaming:
                Roam_Monster();
                CheckPlayerCollider();
                break;
            case MonsterState.Discovery:
                Discovery_Player();
                break;
            case MonsterState.Tracing:
                Tracing_Movement();
                break;
            case MonsterState.GoingBack:
                GoingBack_Movement();
                break;
            default:
                break;
        }
    }

    private void Roam_Monster()
    {
        if (!isRoaming)
        {
            isRoaming = true;
            //x와 Z주변을 배회하는 몬스터
            StartCoroutine(Roam_Monster_co());
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

                float roamTime = UnityEngine.Random.Range(3, 7);

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

    private Vector3 GetRandom_RoamingPos()
    {
        //로밍시, 랜덤한 위치 생성
        float randomX = UnityEngine.Random.Range(originPosition.x + ((roaming_RangeX / 2) * -1), originPosition.x + (roaming_RangeX / 2));
        float randomZ = UnityEngine.Random.Range(originPosition.z + ((roaming_RangeZ / 2) * -1), originPosition.z + (roaming_RangeZ / 2));
        return new Vector3(randomX, transform.position.y, randomZ);
    }

    public bool CheckObstacleCollider(Vector3 randomPos)
    {
        //몬스터 로밍시 지정된 장소에 장애물이 있는지 확인
        Collider[] colliders = Physics.OverlapSphere(randomPos, 1);
        if (colliders.Length > 1)
        {
            //장애물 존재 (1은 바닥 콜라이더)
            return false;
        }
        return true;
    }

    public void CheckPlayerCollider()
    {
        if (curMonsterState != MonsterState.Death)
        {
            //로밍중, 집돌아갈 때 플레이어 콜라이더 감지중
            Collider[] playerColliders = Physics.OverlapSphere(transform.position, overlapRadius, playerlayerMask);

            if (0 < playerColliders.Length)
            {
                //몬스터의 범위에 들어옴
                //로밍 코루틴 제거
                if (isRoaming)
                {
                    StopCoroutine(Roam_Monster_co());
                    isRoaming = false;

                    ChangeMonsterState(MonsterState.Discovery);
                }
                if (isFinding || isGoingBack)
                {
                    //집돌아가는 도중이면 다시 추적 또는 찾은 후라면
                    ChangeMonsterState(MonsterState.Tracing);
                    isTracing = true;
                    isFinding = false;
                    isGoingBack = false;
                }
            }
            else
            {
                if (isFinding)
                {
                    //플레이어가 나갔을 경우
                    isFinding = false;
                    isTracing = false;
                    ChangeMonsterState(MonsterState.GoingBack);

                }
            }
        }

    }

    private void Discovery_Player()
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

        while (time < 2f)
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

        yield return new WaitForSeconds(2f);
        CheckPlayerCollider();
    }

    public virtual void Tracing_Movement()
    {
        //움직임.
        navMeshAgent.SetDestination(playerTrans.position);
        SetMove_AI(true);

        SetAnimation(MonsterAnimation.Move);

        //몬스터와 플레이어 사이의 거리 체크
        CheckDistance();
    }

    public void GoingBack_Movement()
    {
        SetMove_AI(true);

        SetAnimation(MonsterAnimation.Move);

        navMeshAgent.SetDestination(originPosition);
        CheckDistance();       //계속 거리 체크
        CheckPlayerCollider();
    }

    private void CheckDistance()
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
                    isTracing = false;
                    isGoingBack = true;
                    ChangeMonsterState(MonsterState.GoingBack);
                }

                if (distance <= 1.3f)
                {
                    //거리가 2.5만큼 가깝다.
                    //일반 공격
                    isTracing = false;
                    ChangeMonsterState(MonsterState.Attack);
                    Monster_Motion(MonsterMotion.Short_Range_Attack);
                }
                else if (distance >= 7f && distance < 12f)
                {
                    isTracing = false;
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
                    isTracing = false;
                    isGoingBack = false;
                    ChangeMonsterState(MonsterState.Roaming);
                }
                break;
        }
    }

    public virtual void Monster_Motion(MonsterMotion monsterMotion)
    {
        switch (monsterMotion)
        {
            case MonsterMotion.Short_Range_Attack:
                StartCoroutine(Short_Range_Attack_Monster01());
                break;
            case MonsterMotion.Long_Range_Attack:
                StartCoroutine(Long_Range_Attack_Monster01());
                break;
            case MonsterMotion.GetHit_KnockBack:
                if (!isGettingHit)
                {
                    isGettingHit = true;
                    StartCoroutine(GetHit_KnockBack_co());
                }
                break;
            case MonsterMotion.Death:
                if (curMonsterState != MonsterState.Death)
                {
                    StartCoroutine(Death_co());
                }
                break;
            default:
                break;
        }
    }

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
        }
    }

    IEnumerator Long_Range_Attack_Monster01()
    {
        float defaultSpeed = navMeshAgent.speed;
        SetMove_AI(false);

        navMeshAgent.speed = 8f;

        SetAttackAnimation(MonsterAttackAnimation.Long_Range_Attack);
        SetMove_AI(true);

        float distance = 0;

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].enabled = true;
        }

        while (true)
        {
            navMeshAgent.SetDestination(playerTrans.position);
            distance = Vector3.Distance(transform.position, playerTrans.position);

            if (distance <= 0.5)
                break;

            yield return null;
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].enabled = false;
        }
        navMeshAgent.speed = defaultSpeed;

        SetMove_AI(false);
        SetAttackAnimation(MonsterAttackAnimation.ResetAttackAnim);

        ChangeMonsterState(MonsterState.Tracing);
    }

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

    //---------------------------------------------------------------------------------------//
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
