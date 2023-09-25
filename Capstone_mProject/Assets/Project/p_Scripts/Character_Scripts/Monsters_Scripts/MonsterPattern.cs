using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

public class MonsterPattern : MonoBehaviour
{
    Monster m_monster;

    public Rigidbody rigid;
    public Vector3 originPosition; //원래 캐릭터 position

    private int playerLayerId = 3;
    private int playerlayerMask; //플레이어 캐릭터 레이어 마스크
    [SerializeField] private Transform playerTrans;
    private NavMeshAgent navMeshAgent;
    private MonsterState curMonsterState;
    public enum MonsterState
    {
        Roaming,
        Tracing,
        Attack,
        GoingBack,
    }
    private float overlapRadius;

    public enum MonsterMotion
    {
        Attack,
        KnockBack,
    }
    void Start()
    {
        Init();
    }

    public void Init()
    {
        m_monster = GetComponent<Monster>();
        rigid = GetComponent<Rigidbody>();
        playerTrans = GameManager.Instance.gameData.GetPlayerTransform();
        navMeshAgent = GetComponent<NavMeshAgent>();

        playerlayerMask = 1 << playerLayerId; //플레이어 레이어

        curMonsterState = MonsterState.Roaming;

        originPosition = transform.position;
        overlapRadius = m_monster.monsterData.overlapRadius;
    }

    public void Update()
    {
        Monster_Pattern();
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

    public void Monster_Pattern()
    {
        switch (curMonsterState)
        {
            case MonsterState.Roaming:
                CheckPlayerCollider();
                break;
            case MonsterState.Tracing:
                Tracing_Movement();
                break;
            case MonsterState.GoingBack:
                GoingBack_Movement();
                break;
        }
    }

    public void CheckPlayerCollider()
    {
        //로밍중, 집돌아갈 때 플레이어 콜라이더 감지중
        Collider[] playerColliders = Physics.OverlapSphere(transform.position, overlapRadius, playerlayerMask);

        int i = 0;
        while (i < playerColliders.Length)
        {
            //몬스터의 범위에 들어옴
            ChangeMonsterState(MonsterState.Tracing);
            i++;
        }
    }

    public virtual void Tracing_Movement()
    {
        //움직임.
        navMeshAgent.SetDestination(playerTrans.position);

        //몬스터와 플레이어 사이의 거리 체크
        CheckDistance();
    }

    public void GoingBack_Movement()
    {
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
                if (distance > 9f)
                {
                    ChangeMonsterState(MonsterState.GoingBack);
                }
                break;

            case MonsterState.Attack:
                break;

            case MonsterState.GoingBack:
                distance = Vector3.Distance(transform.position, originPosition);
                if (distance < 0.3f)
                {
                    ChangeMonsterState(MonsterState.Roaming);
                }
                break;
        }
    }

    public virtual void Monster_Motion(MonsterMotion monsterMotion)
    {
        switch (monsterMotion)
        {
            case MonsterMotion.Attack:
                break;
            case MonsterMotion.KnockBack:
                break;
        }
    }

    private void ChangeMonsterState(MonsterState monsterState)
    {
        curMonsterState = monsterState;
    }

    private void OnDrawGizmos()
    {
        //몬스터 감지 범위 Draw
        //크기는  monsterData.overlapRadius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, overlapRadius);
    }
}
