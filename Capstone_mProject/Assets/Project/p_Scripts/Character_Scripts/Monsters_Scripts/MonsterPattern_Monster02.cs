using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.Animations;

public class MonsterPattern_Monster02 : MonsterPattern
{

    //원거리
    [Header("바닥부분")]
    public GameObject buttomGameObject;
    [Header("총알")]
    public string bulletPrefabsName = "Monster02_Bullet"; //총알
    public Transform bulletsParent;


    [Header("총알이 나가는 위치 : 인덱스 0번 L쪽 총, 인덱스 1번 R쪽 총")]
    public Transform[] muzzles;

    [Space]
    [Header("몬스터 회전 각도")]
    public int roamingAngle = 60;
    private Quaternion buttomOriginRotation;
    private Quaternion originRotatation;

    [Header("몬스터 공격 시간, 공격 중지 시간")]
    public int attackTime = 2;
    public int stopAttackTime = 10;//2;

    [Header("몬스터 근거리 공격 범위 (실제 플레이어가 공격을 받는 범위)")]
    public float shortRangeAttack_Radius = 3;

    [Header("몬스터 근거리 공격 거리")]
    public float shortRangeAttackDistance = 3f;
    [Header("몬스터가 공격을 멈추는 거리")]
    public float stopAttackDistance = 20;

    [Header("플레이어가 뒤에 있을때 몬스터가 눈치까는 거리")]
    public float findPlayerDistance = 6f;

    Coroutine roam_Monster_co = null;
    Coroutine discovery_Monster_co = null;
    Coroutine short_Range_Attack_co = null;
    Coroutine long_Range_Attack_co = null;
    Coroutine hidePlayer_waitMonster_co = null;
    Coroutine shake_co = null;
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
        //ChangeMonsterState(MonsterState.Tracing);
        //Monster_Motion(MonsterMotion.Long_Range_Attack);

        originPosition = transform.position;
        originRotatation = transform.rotation;
        buttomOriginRotation = buttomGameObject.transform.rotation;


        overlapRadius = m_monster.monsterData.overlapRadius; //플레이어 감지 범위.

        Collider capsuleCollider = GetComponent<Collider>();
        capsuleCollider.enabled = true;

        playerHide = true;
        StartMonster();
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
                break;
            case MonsterAttackAnimation.Short_Range_Attack:
                break;
            case MonsterAttackAnimation.Long_Range_Attack:
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
                    if (!forcedReturnHome)
                        CheckPlayerCollider();
                    break;
                case MonsterState.Discovery:
                    Discovery_Player();
                    break;
                case MonsterState.Tracing:
                    // 몬스터 02 Tracing X
                    break;
                case MonsterState.Attack:
                    if (m_monster.HPBar_CheckNull() == false)
                        m_monster.GetHPBar();
                    if (!isTracing)
                    {
                        isTracing = true;
                        SetPlayerAttackList(true);
                    }

                    //* 공격 중에 숨으면..
                    if (playerHide && hidePlayer_waitMonster_co == null)
                    {
                        //못움직이는 몬스터인데 플레이어가 숨었다면??
                        hidePlayer_waitMonster_co = StartCoroutine(HidePlayer_waitMonster(2f));
                    }
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
            roam_Monster_co = StartCoroutine(Roam_Monster_co());
        }
    }

    IEnumerator Roam_Monster_co()
    {
        float rotationSpeed = 1f;
        float time = 0;
        float roamTime = 0;
        Quaternion targetRotation;
        int curAngle = 0;

        time = 0;
        Quaternion startRotation = transform.rotation;
        Quaternion startButtomRotation = buttomGameObject.transform.rotation;

        while (time < 3)
        {
            time += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, originRotatation, time * 2f);
            buttomGameObject.transform.rotation = Quaternion.Slerp(startButtomRotation, buttomOriginRotation, time * 2f);

            if (buttomOriginRotation == buttomGameObject.transform.rotation && originRotatation == transform.rotation)
                break;

            yield return null;
        }
        transform.rotation = originRotatation;
        buttomGameObject.transform.rotation = buttomOriginRotation;

        while (curMonsterState == MonsterState.Roaming)
        {
            time = 0;
            roamTime = UnityEngine.Random.Range(1, 3);

            yield return new WaitForSeconds(roamTime);

            if (curAngle == 0)
            {
                targetRotation = buttomOriginRotation * Quaternion.Euler(0, roamingAngle, 0);
                curAngle = 1;
            }
            else
            {
                targetRotation = buttomOriginRotation * Quaternion.Euler(0, -roamingAngle, 0);
                curAngle = 0;
            }

            while (time <= 10)
            {
                time += Time.deltaTime;
                // Slerp를 사용하여 부드럽게 회전합니다.
                buttomGameObject.transform.rotation = Quaternion.Slerp(buttomGameObject.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                if (transform.rotation == targetRotation)
                    break;

                yield return null;
            }

            yield return new WaitUntil(() => isRestraint == false);
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

                        float distance = Vector3.Distance(transform.position, playerTargetPos.position);
                        if (distance < shortRangeAttackDistance)
                            Monster_Motion(MonsterMotion.Short_Range_Attack);
                        else
                            Monster_Motion(MonsterMotion.Long_Range_Attack);
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
            if (discovery_Monster_co != null)
                StopCoroutine(discovery_Monster_co);
            discovery_Monster_co = StartCoroutine(DiscoveryPlayer_co());
        }
    }

    IEnumerator DiscoveryPlayer_co()
    {
        float time = 0f; //예비 탈출용
        Vector3 curPlayerdirection = GetDirection(playerTargetPos.position, transform.position);
        Quaternion targetAngle = Quaternion.LookRotation(curPlayerdirection);

        while (time < 1.5f)
        {
            yield return new WaitUntil(() => isRestraint == false);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Time.deltaTime * 5.0f);

            if (transform.rotation == targetAngle)
                break;
            else
            {
                time += Time.deltaTime;
                yield return new WaitUntil(() => isRestraint == false);
                //yield return null;
            }
        }

        CheckPlayerCollider();
    }

    //*-----------------------------------------------------------------------------------------------------------//
    //* goingBack => 로밍
    public override void GoingBack_Movement()
    {
        if (isGoingBack)
        {
            if (isTracing)
            {
                isTracing = false;
                SetPlayerAttackList(false);
            }
            ChangeMonsterState(MonsterState.Roaming);

            isGoingBack = false;
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
                short_Range_Attack_co = StartCoroutine(Short_Range_Attack_Monster01());
                break;
            case MonsterMotion.Long_Range_Attack:
                //원거리 공격
                long_Range_Attack_co = StartCoroutine(Long_Range_Attack_Monster02());
                break;
            case MonsterMotion.GetHit_KnockBack:
                //피격=>>넉백
                GetHit();
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
        yield return new WaitUntil(() => isRestraint == false);
        Effect effect01 = GameManager.Instance.objectPooling.ShowEffect("MC01_Red", m_monster.gameObject.transform);
        effect01.gameObject.transform.position = m_monster.gameObject.transform.position;
        effect01.gameObject.transform.position += new Vector3(0, 0.3f, 0);
        //! 사운드
        //m_monster.SoundPlay(Monster.monsterSound.Alarm, false);
        m_monster.SoundPlay("Monster02_Alarm", false);

        yield return new WaitForSeconds(1.2f);

        Effect effect02 = GameManager.Instance.objectPooling.ShowEffect("Spikes attack", m_monster.gameObject.transform);
        Vector3 effect02Pos = new Vector3(m_monster.gameObject.transform.position.x, 1f, m_monster.gameObject.transform.position.z);
        effect02.gameObject.transform.position = effect02Pos;
        //! 사운드
        // m_monster.SoundPlay(Monster.monsterSound.Hit_Long, false);
        m_monster.SoundPlay("Monster02_ShortAttack", false);

        yield return new WaitForSeconds(0.2f);

        bool playerGetDamage = CheckPlayerDamage(shortRangeAttack_Radius, transform.position, 10, true);

        if (playerGetDamage)
        {
            //카메라 흔들림
            GameManager.Instance.cameraController.cameraShake.ShakeCamera(0.5f, 2, 2);
            //이펙트
            Effect effect = GameManager.Instance.objectPooling.ShowEffect("Power_Impact_Fire_02_01");

            effect.gameObject.transform.position = playerTargetPos.position;
            Vector3 curDirection = m_monster.transform.position - playerTargetPos.position;
            effect.gameObject.transform.position += curDirection * 0.1f;
        }

        yield return new WaitForSeconds(1.5f);

        float distance = Vector3.Distance(transform.position, playerTrans.position);
        if (distance < shortRangeAttackDistance)
            Monster_Motion(MonsterMotion.Short_Range_Attack);
        else
            Monster_Motion(MonsterMotion.Long_Range_Attack);

        drawDamageCircle = false;
    }

    // * ---------------------------------------------------------------------------------------------------------//
    // * 원거리 공격 01
    IEnumerator Long_Range_Attack_Monster02()
    {
        //좌 우 좌 우 발사 
        float time = 0;
        float curAttackTime = 0;

        float distance = Vector3.Distance(transform.position, playerTrans.position);

        bool useBack = false;
        bool useLeft = false;
        bool canAttack = true;

        while (distance > shortRangeAttackDistance && curMonsterState != MonsterState.GetHit)
        {
            if (curMonsterState == MonsterState.Attack)
            {
                useBack = true;
                // * 플레이어 쪽으로 고개 돌림--------------------------------//
                Vector3 targetPos = playerTrans.position;
                //몬스터 고개 돌리기
                Vector3 curPlayerdirection = playerTrans.position - transform.position;
                Quaternion targetAngle = Quaternion.LookRotation(curPlayerdirection);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Time.deltaTime * 20.0f);

                yield return null;

                // * 몬스터 공격----------------------------------------------//

                curAttackTime += Time.deltaTime;

                if (curAttackTime < attackTime && canAttack)
                {
                    time += Time.deltaTime;
                    if (time > 0.2f) //n초마다 총알 발사
                    {
                        time = 0;

                        if (!useLeft)
                        {
                            useLeft = true;
                            StartCoroutine(Fire(playerTargetPos.position, muzzles[0]));
                        }
                        else
                        {
                            useLeft = false;
                            StartCoroutine(Fire(playerTargetPos.position, muzzles[1]));
                        }

                    }
                }
                else if (curAttackTime >= attackTime && canAttack)
                {
                    time = 0;
                    curAttackTime = 0;
                    canAttack = false;
                }


                if (curAttackTime > stopAttackTime && !canAttack)
                {
                    canAttack = true;
                    curAttackTime = 0;
                }

                distance = Vector3.Distance(transform.position, playerTrans.position);

                if (distance >= stopAttackDistance)
                {
                    //거리가 13만큼 떨어진다면
                    //어택 멈추기
                    isGoingBack = true;
                    break;
                }
            }

        }

        if (curMonsterState != MonsterState.GetHit)
        {
            // 기울었던 몸 다시 원상 복귀
            if (useBack)
            {
                if (!isGoingBack)
                {
                    originRotatation.y = transform.rotation.y;
                }
                time = 0;
                while (time < 1)
                {
                    time += Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(transform.rotation, originRotatation, Time.deltaTime * 5.0f);
                    //buttomGameObject.transform.rotation = Quaternion.Slerp(buttomGameObject.transform.rotation, buttomOriginRotation, Time.deltaTime * 5.0f);

                    yield return null;
                }
                if (isGoingBack)
                {
                    ChangeMonsterState(MonsterState.GoingBack);
                }
                else
                {
                    //몬스터가 가까이 옴.
                    Monster_Motion(MonsterMotion.Short_Range_Attack);
                }
            }
        }
    }

    IEnumerator Fire(Vector3 targetPos, Transform muzzlePos)
    {
        yield return new WaitUntil(() => isRestraint == false);
        //* 발사체가 나가가는 부분 (총구)에서 플레이어로 향하는 방향 벡터
        Vector3 curDirection = GetDirection(targetPos, muzzlePos.position);
        playerHide = HidePlayer(muzzlePos.position, curDirection.normalized);

        if (!playerHide)
        {
            //* 어택 에니메이션
            SetAttackAnimation(MonsterAttackAnimation.Long_Range_Attack);
            //* 총알 //
            GameObject bulletObj = GameManager.Instance.objectPooling.GetProjectilePrefab(bulletPrefabsName, bulletsParent);
            Rigidbody bulletRigid = bulletObj.GetComponent<Rigidbody>();

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            bullet.Reset(m_monster, bulletPrefabsName, muzzlePos);
            // 플레이어에게  총알이 맞았을 경우
            bullet.OnHitPlayerEffect = () =>
            {
                //플레이어가 총에 맞았을 경우, 이펙트
                //Effect effect = GameManager.Instance.objectPooling.ShowEffect("Basic_Impact_01");

                //effect.gameObject.transform.position = targetPos;
                //Vector3 curDirection = targetPos - bulletObj.transform.position;
                //effect.gameObject.transform.position += curDirection * 0.35f;
            };

            //총알 방향//

            Quaternion targetAngle = Quaternion.LookRotation(curDirection);
            bulletObj.transform.rotation = targetAngle;

            bullet.SetInfo(curDirection.normalized, "FX_Shoot_08_hit"); //* Bullet.cs에 방향 벡터 보냄
                                                                        //총알 발사.
            bulletRigid.velocity = curDirection.normalized * 80f;
            //총쏠때 이펙트
            Effect effect = GameManager.Instance.objectPooling.ShowEffect("FX_Shoot_08_muzzle");
            effect.gameObject.transform.position = muzzlePos.position;
            effect.transform.rotation = targetAngle;

            //! 사운드 
            // m_monster.SoundPlay(Monster.monsterSound.Hit_Close, false);
            m_monster.SoundPlay("Monster02_LongAttack", false);
            //몬스터 몸 흔들리는 연출//
            if (shake_co == null)
                shake_co = StartCoroutine(Shake(0.1f));
        }
        yield return null;
    }

    IEnumerator Shake(float duration)
    {
        float shakeAmount = 0.01f;
        float smoothAmount = 1f;
        float maxAmount = 0.06f;

        float time = 0;



        Vector3 originalPosition = transform.localPosition;
        Quaternion originalRotation = transform.localRotation;

        float positionX = transform.localPosition.x;
        float positionY = transform.localPosition.y;
        float rotationX = transform.localRotation.x;
        float rotationY = transform.localRotation.y;

        while (time < duration)
        {
            time += Time.deltaTime;

            if (curMonsterState == MonsterState.Attack)
            {
                //* 일반 총 쏠때 흔들림
                positionX = -transform.localPosition.x * shakeAmount * 5f;
                positionY = -transform.localPosition.y * shakeAmount;
                rotationX = -transform.localPosition.x * shakeAmount;
                rotationY = -transform.localPosition.y * shakeAmount * 0.2f;
            }
            if (curMonsterState == MonsterState.GetHit)
            {
                //* 데미지 얻었을때 흔들림
                smoothAmount = 6;
                shakeAmount = 1f;
                positionX = -transform.localPosition.x * shakeAmount * 5f;
                positionY = -transform.localPosition.y * shakeAmount;
                rotationX = -transform.localPosition.x * shakeAmount * 0.5f;
                rotationY = -transform.localPosition.y * shakeAmount * 0.5f;

            }

            Mathf.Clamp(positionX, -maxAmount, maxAmount);
            Mathf.Clamp(positionY, -maxAmount, maxAmount);

            Vector3 shakePosition = new Vector3(positionX, positionY, 0);
            Quaternion shackRotation = new Quaternion(rotationX, rotationY, 0, 1);


            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition + shakePosition, Time.deltaTime * smoothAmount);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, shackRotation, Time.deltaTime * smoothAmount);

            yield return null;
        }

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;


        shake_co = null;
        yield return null;
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
        if (shake_co == null)
            shake_co = StartCoroutine(Shake(duration));

        yield return new WaitForSeconds(duration);


        isGettingHit = false;

        ChangeMonsterState(MonsterState.Attack);
        Monster_Motion(MonsterMotion.Long_Range_Attack);
    }
    // * ---------------------------------------------------------------------------------------------------------//
    // * 죽음 모션

    IEnumerator Death_co()
    {
        StopAtackCoroutine();

        if (isTracing)
        {
            isTracing = false;
            SetPlayerAttackList(false);
        }

        ChangeMonsterState(MonsterState.Death);

        yield return new WaitForSeconds(0.5f);

        m_monster.RetrunHPBar();
        //SetAnimation(MonsterAnimation.Death);

        //TODO: 죽었을때 수정.(애니메이터 문제)
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Eff_Burst_1_oneShot");

        effect.transform.position = transform.position;
        effect.finishAction = () =>
        {
            m_monster.gameObject.SetActive(false);
        };
        //! 사운드 => 터지는 소리
        // m_monster.SoundPlay(Monster.monsterSound.Death, false);
        m_monster.SoundPlay("Monster02_Death", false);

        //yield return new WaitForSeconds(5f);

        //m_monster.gameObject.SetActive(false);
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
            long_Range_Attack_co = null;
        }
    }

    // * ---------------------------------------------------------------------------------------------------------//
    IEnumerator HidePlayer_waitMonster(float hideDuration)
    {
        //* hideDuration초만큼 플레이어가 숨어있으면 공격 멈추는 함수.
        float time = 0;
        if (playerHide)
        {
            while (true)
            {
                time += Time.deltaTime;
                if (!playerHide && time < hideDuration)
                {
                    break;
                }
                if (playerHide && time >= hideDuration)
                {
                    if (short_Range_Attack_co != null)
                        StopCoroutine(short_Range_Attack_co);
                    if (long_Range_Attack_co != null)
                        StopCoroutine(long_Range_Attack_co);

                    //*공격 멈춤
                    ChangeMonsterState(MonsterState.Roaming);

                    if (isTracing)
                    {
                        isTracing = false;
                        SetPlayerAttackList(false);
                    }

                    break;
                }
                yield return null;
            }
        }

        hidePlayer_waitMonster_co = null;
    }


    // * ---------------------------------------------------------------------------------------------------------//
    private void OnDrawGizmos()
    {
        //몬스터 감지 범위 Draw
        //크기는  monsterData.overlapRadius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, overlapRadius);

        if (drawDamageCircle)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(m_monster.gameObject.transform.position, shortRangeAttack_Radius);
        }
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





