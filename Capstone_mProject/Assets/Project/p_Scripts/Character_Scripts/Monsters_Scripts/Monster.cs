using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    public MonsterData monsterData;
    private int playerLayerId = 3;
    private int playerlayerMask; //플레이어 캐릭터 레이어 마스크

    public Vector3 originPosition; //원래 캐릭터 position
    public Transform playerTrans;
    private NavMeshAgent navMeshAgent;

    [SerializeField] private bool isTracing; //플레이어 추적중.
    [SerializeField] private bool isGoingBack; //자기자리로 돌아가는 중
    [SerializeField] private bool isRoaming;   //로밍중


    void Awake()
    {
        Init();
    }

    public void Init()
    {
        playerlayerMask = 1 << playerLayerId;
        isRoaming = true;
        isTracing = false;
        isGoingBack = false;

        originPosition = transform.position;
        playerTrans = GameManager.Instance.gameData.player.GetComponent<Transform>();
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void Update()
    {
        MonsterPattern();
    }

    public void MonsterPattern()
    {
        if (isRoaming)
        {
            CheckPlayerCollider();
        }
        if (isTracing)
        {
            Movement();
        }
        if (isGoingBack)
        {
            Movement_GoToBack();
        }
    }

    public void CheckPlayerCollider()
    {
        Collider[] playerColliders = Physics.OverlapSphere(transform.position, monsterData.overlapRadius, playerlayerMask);

        int i = 0;

        while (i < playerColliders.Length)
        {
            //몬스터의 범위에 들어옴.
            isRoaming = false;
            isTracing = true;
            isGoingBack = false;

            i++;
        }
    }

    public virtual void Movement()
    {
        //움직임.
        navMeshAgent.SetDestination(playerTrans.position);

        //몬스터와 플레이어 사이의 거리 체크
        CheckDistance();
    }

    private void CheckDistance()
    {
        //해당 몬스터와 플레이어 사이의 거리 체크
        //Debug.Log(Vector3.Distance(transform.position, playerTrans.position));
        if (isTracing)
        {
            if (Vector3.Distance(transform.position, playerTrans.position) > 9f)
            {
                isTracing = false;
                isGoingBack = true;
            }
        }
        if (isGoingBack)
        {
            Debug.Log(Vector3.Distance(transform.position, originPosition));
            if (Vector3.Distance(transform.position, originPosition) < 0.3f)
            {
                isGoingBack = false;
                isRoaming = true;
            }
        }

    }

    public void Movement_GoToBack()
    {
        navMeshAgent.SetDestination(originPosition);
        CheckDistance();
        CheckPlayerCollider();
    }

    public virtual void Hit()
    {

    }
    public virtual void Death()
    {

    }
    private void OnDrawGizmos()
    {
        //몬스터 감지 범위 Draw
        //크기는  monsterData.overlapRadius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, monsterData.overlapRadius);
    }
}
