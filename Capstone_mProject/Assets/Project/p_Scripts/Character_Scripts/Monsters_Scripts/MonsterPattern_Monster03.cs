using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterPattern_Monster03 : MonsterPattern
{
    [Header("방패")]
    public GameObject shield;
    // [Header("어그로 해제 후 다시 어그로가능한 거리 ")]
    // private float roundDistance = 1.0f;
    [Header("플레이어가 뒤에 있을때 몬스터가 눈치까는 거리")]
    public float findPlayerDistance = 6f;
    [Header("다시 어그로까지의 쿨탐")]
    private float coolTime=5.0f;

    SoundObject soundObject;
    GameObject soundObjectGameObject;

    Coroutine roam_Monster_co = null;
    Coroutine discovery_Monster_co = null;
    Coroutine tracing_Movement_co = null;

    public override void Init()
    {
        m_monster = GetComponentInParent<Monster>();
        m_animator = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody>();
        playerTrans = GameManager.Instance.gameData.GetPlayerTransform();
        playerTargetPos = GameManager.Instance.gameData.playerTargetPos;
        m_monster.monsterPattern = this;

        navMeshAgent = null;

        playerlayerMask = 1 << playerLayerId; //플레이어 레이어

        ChangeMonsterState(MonsterState.Roaming);

        originPosition = transform.position;
        //originRotatation = transform.rotation;

        overlapRadius = m_monster.monsterData.overlapRadius;
        Collider capsuleCollider = GetComponent<Collider>();
        capsuleCollider.enabled = true;
        
        playerHide = true;
        StartMonster();

        coolTime = 5.0f;

        soundObjectGameObject = GameObject.FindGameObjectWithTag("SoundObject");
        soundObject = soundObjectGameObject.GetComponent<SoundObject>();
        isShield = false;
    }

    public override void Monster_Pattern()
    {
        if (curMonsterState != MonsterState.Death)
        {
            switch (curMonsterState)
            {
                case MonsterState.Roaming:
                    if (m_monster.HPBar_CheckNull() == true){
                        m_monster.RetrunHPBar();
                    }
                    Roam_Monster();
                    if (!forcedReturnHome)
                    {
                        CheckPlayerCollider();
                    }
                    break;
                case MonsterState.Discovery:
                    if (m_monster.HPBar_CheckNull() == false)
                        m_monster.GetHPBar();
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

    // * ---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 로밍
    public override void Roam_Monster()
    {
        if (!isRoaming)
        {
            isRoaming = true;
            //x와 Z주변을 배회하는 몬스터
            //roam_Monster_co = StartCoroutine(Roam_Monster_co());
        }
    }

    // IEnumerator Roam_Monster_co()
    // {
    //     float time = 0;
    //     float roamTime = 0;

    //     time = 0;
    //     Quaternion startRotation = transform.rotation;

    //     // while (time < 3)
    //     // {
    //     //     time += Time.deltaTime;
    //     //     //transform.rotation = Quaternion.Slerp(startRotation, originRotatation, time * 2f);

    //     //     yield return null;
    //     // }
    //     //transform.rotation = originRotatation;

    //     while (curMonsterState == MonsterState.Roaming)
    //     {
    //         time = 0;
    //         roamTime = UnityEngine.Random.Range(1, 3);

    //        // yield return new WaitForSeconds(roamTime);

            

    //         while (time <= 10)
    //         {
    //             time += Time.deltaTime;
    //             // Slerp를 사용하여 부드럽게 회전합니다.
    
    //             yield return null;
    //         }
    //     }
    // }

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
                    if (isRoaming)
                    {
                        //* 플레이어가 뒤에 있는지 체크
                        bool inFrontOf_Player = PlayerLocationCheck_BackForth();
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
                            if (roam_Monster_co != null)
                                StopCoroutine(roam_Monster_co);
                            isRoaming = false;
                            ChangeMonsterState(MonsterState.Discovery);
                        }

                    }
                    if (isFinding) //* State : Discorvery
                    {
                        isFinding = false;
                        ChangeMonsterState(MonsterState.Attack);

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
            else //* playerColliders 영역 안에 없을 경우.
            {
                if (isFinding) //* State : Discorvery
                {
                    isFinding = false;
                    ChangeMonsterState(MonsterState.Roaming);
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

           
        }
        if (discovery_Monster_co != null)
        {
            StopCoroutine(discovery_Monster_co);
        }
        discovery_Monster_co = StartCoroutine(DiscoveryPlayer_co());
        ChangeMonsterState(MonsterState.Tracing);
    }

    IEnumerator DiscoveryPlayer_co()
    {

        float time = 0f; // 경과 시간
        Quaternion targetAngle;//= Quaternion.LookRotation(curPlayerdirection); // 플레이어를 바라보는 각도
        RaycastHit raycastHit;
      
        // 플레이어가 움직이는 동안 계속 플레이어를 바라보도록 함
        while (true)
        {
            Debug.DrawRay(transform.position, playerTargetPos.position - transform.position , Color.yellow);

            if(Physics.Raycast(transform.position, playerTargetPos.position - transform.position, out raycastHit))
            {
                //Debug.Log($"{raycastHit.collider.name}");
                if(raycastHit.collider.CompareTag("Shield"))
                {
                    //실드가 앞에 있으면 true, 없으면 false
                    //GameManager.Instance.gameData.player.GetComponentInChildren<PlayerAttackCheck>().isShield = true; 
                    //Debug.Log(GameManager.Instance.gameData.player.GetComponentInChildren<PlayerAttackCheck>().isShield);
                    isShield = true;


                }
                else
                {
                    //isShield = false;
                }
                yield return null;
            }
            
           
            if (time >= 60.0f)
            {
                Debug.Log("60초 지남/ BOOM~");
                time = 0;
                //! 몬스터 폭발, 죽음, 타임 초기화 해야함. 
                Monster_Motion(MonsterMotion.Death);
                yield break;
            }

            if(!soundObject.attackSoundObj)
            {
                time += Time.deltaTime;
                Vector3 curPlayerPos = playerTrans.position;
                Vector3 curPlayerdirection = curPlayerPos - transform.position;

                curPlayerdirection.y = 0f;

                targetAngle = Quaternion.LookRotation(curPlayerdirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Time.deltaTime * 5.0f);

                yield return null;
            }
            else
            {
                
                Vector3 curSoundObjPos = soundObject.collisionPos;
                Vector3 curSoundObjdirection = curSoundObjPos - transform.position;

                //curPlayerdirection.y = 0f;

                targetAngle = Quaternion.LookRotation(curSoundObjdirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Time.deltaTime * 5.0f);
                //soundObject.attackSoundObj = false;
                yield return new WaitForSeconds(coolTime);
                soundObject.attackSoundObj = false;
            }
    
            
        }

        // if(!soundObject.attackSoundObj)
        // {
        //     time += Time.deltaTime;
        //     Vector3 curPlayerPos = playerTrans.position;
        //     Vector3 curPlayerdirection = curPlayerPos - transform.position;

        //     curPlayerdirection.y = 0f;

        //     targetAngle = Quaternion.LookRotation(curPlayerdirection);
        //     transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Time.deltaTime * 5.0f);
            
        //     if(time>=60.0f)
        //     {
        //         Debug.Log("60초 지남/ BOOM~");
        //         //! 몬스터 폭발, 죽음, 타임 초기화 해야함. 
        //         yield break;
        //     }

        //     yield return null;
        // }
        // else
        // {
        //     Vector3 curSoundObjPos = soundObject.collisionPos;
        //     Vector3 curSoundObjdirection = curSoundObjPos - transform.position;

        //     //curPlayerdirection.y = 0f;

        //     targetAngle = Quaternion.LookRotation(curSoundObjdirection);
        //     transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Time.deltaTime * 5.0f);
        //     Debug.Log("sad");

        //    //yield return null;
        // }
    }

// * ---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 추적
    public override void Tracing_Movement()
    {
        if (!isTracing)
        {
            isTracing = true;
        }
     
        //! 여기서 플레이어 쫒게추가함. 
        // 플레이어를 쫒기 위해 Coroutine 시작
        if (tracing_Movement_co != null)
        {
            StopCoroutine(tracing_Movement_co);
        }
        tracing_Movement_co = StartCoroutine(TracingMovement_co());

        //SetAnimation(MonsterAnimation.Move);
        //몬스터와 플레이어 사이의 거리 체크
        //CheckDistance();
    
    }

    IEnumerator TracingMovement_co()
    {
        // 플레이어와 몬스터 사이의 거리 계산
        float distanceToPlayer = Vector3.Distance(transform.position, playerTrans.position);

        float tracingDistance = 3.0f; //거리
        float tracingSpeed = 1.0f;  //속도 


        if (distanceToPlayer > tracingDistance)
        {
            //일정 범위 밖이라면 플레이어를 향해 이동
            Vector3 directionToPlayer = (playerTrans.position - transform.position).normalized;
            directionToPlayer.y = 0; // y축 이동을 막음

            if (!soundObject.attackSoundObj)
            {
       
                transform.position += directionToPlayer * tracingSpeed * Time.deltaTime;
            }
            else if (soundObject.attackSoundObj)
            {
               tracingSpeed = 0.0f; // 이동을 멈추도록 함
                yield return new WaitForSeconds(coolTime); // 5초 대기 후에

                // 움직임을 다시 시작
                tracingSpeed = 1.0f;
            }
        }

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
                if (distance >= findPlayerDistance)
                {
    
                }

                if (distance < findPlayerDistance)
                {
                  
                }

                break;

            case MonsterState.Attack:
                break;

            case MonsterState.GoingBack:
              
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
                break;
            case MonsterMotion.Long_Range_Attack:
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
    // * 피격 모션
    private void GetHit()
    {
        //? 피격 이펙트
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Power_Impact_Fire_02_01");
        //TODO: 나중에 플레이어 방향쪽으로 변경.
        float x = UnityEngine.Random.Range(-1.5f, 1.5f);
        float y = UnityEngine.Random.Range(-1.5f, 1.5f);
        float z = UnityEngine.Random.Range(-1.5f, 1.5f);
        Vector3 randomPos = new Vector3(x, y, z);

        effect.transform.position = transform.position + randomPos;

        StartCoroutine(electricity_Damage(0.8f));
    }

    IEnumerator electricity_Damage(float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;

            float x = UnityEngine.Random.Range(-1.5f, 1.5f);
            float y = UnityEngine.Random.Range(-1.5f, 1.5f);
            float z = UnityEngine.Random.Range(-1.5f, 1.5f);
            Vector3 randomPos = new Vector3(x, y, z);
            randomPos = transform.position + randomPos;
            GetDamage_electricity(randomPos);

            float randomTime = UnityEngine.Random.Range(0, 0.5f);
            yield return new WaitForSeconds(randomTime);
            time += randomTime;
        }
    }

    IEnumerator GetHit_KnockBack_co()
    {
        //플레이어의 반대 방향으로 넉백
        float duration = 0.1f;

        //몬스터 상태 변경 >> 피격
        ChangeMonsterState(MonsterState.GetHit);

        yield return new WaitForSeconds(duration);


        isGettingHit = false;

        ChangeMonsterState(MonsterState.Tracing);
        //Monster_Motion(MonsterMotion.Long_Range_Attack);
    }

     // * 죽음 모션
    IEnumerator Death_co()
    {
        StopCoroutine(discovery_Monster_co);

        if (isTracing)
        {
            isTracing = false;
            SetPlayerAttackList(false);
        }

       //SetAnimation(MonsterAnimation.Idle);
        ChangeMonsterState(MonsterState.Death);
        //SetMove_AI(false);

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.enabled = true;

        yield return new WaitForSeconds(0.5f);
        //! 사운드
        m_monster.SoundPlay(Monster.monsterSound.Death, false);
        m_monster.RetrunHPBar();
        //SetAnimation(MonsterAnimation.Death);

        yield return new WaitForSeconds(5f);

        this.gameObject.SetActive(false);
    }


     public override void StopMonster()
    {
        //상태는 그대로.
        // 몬스터 is도 그대로.
        //모든 코루틴 정지

        //각자의 자리로 가기
        base.StopMonster();

        StopAtackCoroutine();

        if ((curMonsterState != MonsterState.Death || curMonsterState != MonsterState.Roaming) || curMonsterState != MonsterState.GoingBack)
        {
            if (curMonsterState == MonsterState.Discovery)
            {
                isRoaming = false;
                isFinding = false;
                isTracing = false;
                isGoingBack = false;
                isGettingHit = false;
                //로밍 으로 변경
                if (discovery_Monster_co != null)
                {
                    StopCoroutine(discovery_Monster_co);
                    discovery_Monster_co = null;
                }

                ChangeMonsterState(MonsterState.Roaming);
            }
            else if (curMonsterState == MonsterState.GetHit)
            {
                isRoaming = false;
                isFinding = false;
                isTracing = false;
                isGoingBack = true;
                isGettingHit = false;
                SetPlayerAttackList(false);
            }
            else
            {
                isRoaming = false;
                isFinding = false;
                isTracing = false;
                isGoingBack = true;
                isGettingHit = false;
                ChangeMonsterState(MonsterState.GoingBack);
                SetPlayerAttackList(false);
            }
        }

    }
    public override void StartMonster()
    {
        forcedReturnHome = false;
    }

}
