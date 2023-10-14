using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.Animations;

public class MonsterPattern : MonoBehaviour
{
    protected Monster m_monster;
    protected Animator m_animator;

    protected Rigidbody rigid;
    protected Vector3 originPosition; //원래 캐릭터 position

    protected int playerLayerId = 3;
    protected int playerlayerMask; //플레이어 캐릭터 레이어 마스크
    protected Transform playerTrans;

    protected NavMeshAgent navMeshAgent;
    protected MonsterState curMonsterState;

    protected bool drawDamageCircle = false;

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


    protected float overlapRadius; //플레이어 발견 범위
    // * --------------------------------------------------------//
    // * 로밍관련 변수들
    protected int roaming_RangeX;
    protected int roaming_RangeZ;

    protected Vector3 roam_vertex01; //사각형 왼쪽 가장 위
    protected Vector3 roam_vertex02; //사각형 왼쪽 가장 아래
    protected Vector3 roam_vertex03; //사각형 오른쪽 가장 아래
    protected Vector3 roam_vertex04; //사각형 오른쪽 가장 위

    protected Vector3 mRoaming_randomPos = Vector3.zero;
    //* --------------------------------------------------------//
    protected bool isRoaming = false;
    protected bool isFinding = false;
    protected bool isGoingBack = false;
    protected bool isGettingHit = false;

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

    public virtual void Init()
    {
        m_monster = GetComponent<Monster>();
        m_animator = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody>();
        playerTrans = GameManager.Instance.gameData.GetPlayerTransform();

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
    }

    public void Update()
    {
        Monster_Pattern();
        if (m_monster.monsterData.movingMonster)
        {
            UpdateRotation();
        }
    }

    private void FixedUpdate()
    {
        if (m_monster.monsterData.movingMonster)
        {
            FreezeVelocity();
        }
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

    public virtual void SetAnimation(MonsterAnimation m_anim)
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

    public virtual void SetAttackAnimation(MonsterAttackAnimation monsterAttackAnimation, int animIndex = 0)
    {
        switch (monsterAttackAnimation)
        {
            case MonsterAttackAnimation.ResetAttackAnim:
                break;
            case MonsterAttackAnimation.Short_Range_Attack:
                break;
            case MonsterAttackAnimation.Long_Range_Attack:
                break;
            default:
                break;
        }
    }

    protected void SetMove_AI(bool moveAI)
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

    protected void ChangeMonsterState(MonsterState monsterState)
    {
        curMonsterState = monsterState;

#if UNITY_EDITOR
        Debug.Log("상태 변경 " + monsterState);
#endif
    }

    public virtual void Monster_Pattern()
    {
        switch (curMonsterState)
        {
            case MonsterState.Roaming:
                break;
            case MonsterState.Discovery:
                break;
            case MonsterState.Tracing:
                break;
            case MonsterState.GoingBack:
                break;
            default:
                break;
        }
    }
    // * -------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 로밍
    public virtual void Roam_Monster()
    {
        if (!isRoaming)
        {
            isRoaming = true;
        }
    }
    //로밍할 랜덤 좌표
    public Vector3 GetRandom_RoamingPos()
    {
        //로밍시, 랜덤한 위치 생성
        float randomX = UnityEngine.Random.Range(originPosition.x + ((roaming_RangeX / 2) * -1), originPosition.x + (roaming_RangeX / 2));
        float randomZ = UnityEngine.Random.Range(originPosition.z + ((roaming_RangeZ / 2) * -1), originPosition.z + (roaming_RangeZ / 2));
        return new Vector3(randomX, transform.position.y, randomZ);
    }
    //랜덤 좌표에 장애물이 있는지 확인하는 함수
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
    // * -------------------------------------------------------------------------//
    // * 몬스터의 발견범위에 플레이어가 들어왔는지 확인하는 함수.
    public virtual void CheckPlayerCollider()
    {
        if (curMonsterState != MonsterState.Death)
        {
            //로밍중, 집돌아갈 때 플레이어 콜라이더 감지중
            Collider[] playerColliders = Physics.OverlapSphere(transform.position, overlapRadius, playerlayerMask);

            if (0 < playerColliders.Length)
            {
                //몬스터의 범위에 들어옴
            }
        }
    }
    // * -------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 발견
    public virtual void Discovery_Player()
    {
        if (!isFinding)
        {
            isFinding = true;
        }
    }
    // * ------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 추적
    public virtual void Tracing_Movement()
    {
    }
    // * ------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 다시 자기자리로
    public virtual void GoingBack_Movement()
    {
        SetMove_AI(true);

        SetAnimation(MonsterAnimation.Move);

        navMeshAgent.SetDestination(originPosition);
        CheckDistance();       //계속 거리 체크
        CheckPlayerCollider();
    }

    // * 몬스터 추격, 자기자리로 돌아갈 때 ==>>> 몬스터와 플레이어 사이의 거리 체크
    public virtual void CheckDistance()
    {
        //해당 몬스터와 플레이어 사이의 거리 체크
        switch (curMonsterState)
        {
            case MonsterState.Roaming:
                break;

            case MonsterState.Tracing:
                break;

            case MonsterState.Attack:
                break;

            case MonsterState.GoingBack:
                break;
        }
    }
    // * -----------------------------------------------------------------------//
    // * 몬스터 공격 모션, 피격 모션, 죽음 모션 등등
    public virtual void Monster_Motion(MonsterMotion monsterMotion)
    {
        switch (monsterMotion)
        {
            case MonsterMotion.Short_Range_Attack:
                break;
            case MonsterMotion.Long_Range_Attack:
                break;
            case MonsterMotion.GetHit_KnockBack:
                break;
            case MonsterMotion.Death:
                break;
            default:
                break;
        }
    }
    // * ---------------------------------------------------------------------------------------//
    //! 특정 범위안에 플레이어가 있는지 파악하고, 데미지 주는 함수
    public void CheckPlayerDamage(float _overlapRadius, float damage = 0)
    {
        drawDamageCircle = true;
        Collider[] playerColliders = Physics.OverlapSphere(transform.position, overlapRadius, playerlayerMask);
        if (0 < playerColliders.Length)
        {
            m_monster.OnHit(damage);
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