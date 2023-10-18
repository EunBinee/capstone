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
    public Transform playerTargetPos; //총알 과녁 (플레이어 몸)

    [Header("총알이 나가는 위치 : 인덱스 0번 L쪽 총, 인덱스 1번 R쪽 총")]
    public Transform[] muzzles;
    [Space]
    [Header("몬스터 회전 각도")]
    public int roamingAngle = 60;
    private Quaternion buttomOriginRotation;

    [Header("몬스터 공격 시간, 공격 중지 시간")]
    public int attackTime = 2;
    public int stopAttackTime = 2;

    [Header("몬스터 근거리 공격 범위 (실제 플레이어가 공격을 받는 범위)")]
    public float shortRangeAttack_Radius = 2;

    [Header("몬스터 근거리 공격 거리")]
    public float shortRangeAttackDistance = 3f;
    [Header("몬스터가 공격을 멈추는 거리")]
    public float stopAttackDistance = 20;


    public override void Init()
    {
        m_monster = GetComponentInParent<Monster>();
        m_animator = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody>();
        playerTrans = GameManager.Instance.gameData.GetPlayerTransform();

        m_monster.monsterPattern = this;

        navMeshAgent = null;

        playerlayerMask = 1 << playerLayerId; //플레이어 레이어

        ChangeMonsterState(MonsterState.Roaming);
        //ChangeMonsterState(MonsterState.Tracing);
        //Monster_Motion(MonsterMotion.Long_Range_Attack);

        originPosition = transform.position;
        buttomOriginRotation = buttomGameObject.transform.rotation;

        overlapRadius = m_monster.monsterData.overlapRadius; //플레이어 감지 범위.

        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        capsuleCollider.enabled = true;
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

    public override void SetAttackAnimation(MonsterAttackAnimation monsterAttackAnimation, int animIndex = 0)
    {
        switch (monsterAttackAnimation)
        {
            case MonsterAttackAnimation.ResetAttackAnim:
                break;
            case MonsterAttackAnimation.Short_Range_Attack:
                break;
            case MonsterAttackAnimation.Long_Range_Attack:
                m_animator.SetTrigger("m_l_Attack");
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
                if (m_monster.HPBar_CheckNull() == false)
                    m_monster.GetHPBar();
                break;
            case MonsterState.Attack:
                if (m_monster.HPBar_CheckNull() == false)
                    m_monster.GetHPBar();
                break;
            case MonsterState.GoingBack:
                break;
            default:
                break;
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
            StartCoroutine(Roam_Monster_co());
        }
    }

    IEnumerator Roam_Monster_co()
    {
        float rotationSpeed = 1f;
        float time = 0;
        float roamTime = 0;
        Quaternion targetRotation;
        int curAngle = 0;

        while (curMonsterState == MonsterState.Roaming)
        {
            time = 0;
            roamTime = UnityEngine.Random.Range(1, 3);
            Debug.Log("기다리는 중 " + roamTime + "초");
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
        }
    }

    public override void CheckPlayerCollider()
    {
        if (curMonsterState != MonsterState.Death)
        {
            //로밍중, 집돌아갈 때 플레이어 콜라이더 감지중
            Collider[] playerColliders = Physics.OverlapSphere(transform.position, overlapRadius, playerlayerMask);
            if (0 < playerColliders.Length)
            {
                if (isRoaming)
                {
                    StopCoroutine(Roam_Monster_co());
                    isRoaming = false;
                    ChangeMonsterState(MonsterState.Discovery);
                }
                if (isFinding)
                {
                    isFinding = false;
                    ChangeMonsterState(MonsterState.Attack);

                    float distance = Vector3.Distance(transform.position, playerTrans.position);
                    if (distance < shortRangeAttackDistance)
                        Monster_Motion(MonsterMotion.Short_Range_Attack);
                    else
                        Monster_Motion(MonsterMotion.Long_Range_Attack);
                }
            }
            else
            {
                if (isFinding)
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
            StartCoroutine(DiscoveryPlayer_co());
        }
    }

    IEnumerator DiscoveryPlayer_co()
    {
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

        CheckPlayerCollider();
    }
    // * ---------------------------------------------------------------------------------------------------------//
    // * 몬스터 공격 모션, 피격 모션, 죽음 모션
    public override void Monster_Motion(MonsterMotion monsterMotion)
    {
        switch (monsterMotion)
        {
            case MonsterMotion.Short_Range_Attack:
                //근거리 공격
                StartCoroutine(Short_Range_Attack_Monster01());
                break;
            case MonsterMotion.Long_Range_Attack:
                //원거리 공격
                StartCoroutine(Long_Range_Attack01_Monster02());
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
                break;
            default:
                break;
        }
    }
    // * ---------------------------------------------------------------------------------------------------------//
    // * 근거리 공격 01
    IEnumerator Short_Range_Attack_Monster01()
    {
        Effect effect01 = GameManager.Instance.objectPooling.ShowEffect("MC01_Red", m_monster.gameObject.transform);
        effect01.gameObject.transform.position = m_monster.gameObject.transform.position;
        effect01.gameObject.transform.position += new Vector3(0, 0.3f, 0);

        yield return new WaitForSeconds(1.5f);

        Effect effect02 = GameManager.Instance.objectPooling.ShowEffect("Spikes attack", m_monster.gameObject.transform);
        Vector3 effect02Pos = new Vector3(m_monster.gameObject.transform.position.x, 1f, m_monster.gameObject.transform.position.z);
        effect02.gameObject.transform.position = effect02Pos;

        yield return new WaitForSeconds(0.2f);

        bool playerGetDamage = CheckPlayerDamage(shortRangeAttack_Radius);

        if (playerGetDamage)
        {
            //카메라 흔들림
            GameManager.Instance.cameraShake.ShakeCamera(0.5f, 2, 2);
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
    IEnumerator Long_Range_Attack01_Monster02()
    {
        //좌 우 좌 우 발사 
        float time = 0;
        float curAttackTime = 0;

        float distance = Vector3.Distance(transform.position, playerTrans.position);

        bool useBack = false;
        bool useLeft = false;
        bool goingBack = false;
        bool canAttack = true;

        Quaternion originRotate = transform.rotation;

        while (distance > shortRangeAttackDistance && curMonsterState != MonsterState.GetHit)
        {

            useBack = true;
            // * 플레이어 쪽으로 고개 돌림--------------------------------//
            Vector3 targetPos = playerTrans.position;
            //몬스터 고개 돌리기
            Vector3 curPlayerPos = playerTrans.position;
            Vector3 curPlayerdirection = curPlayerPos - transform.position;
            Quaternion targetAngle = Quaternion.LookRotation(curPlayerdirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Time.deltaTime * 20.0f);

            yield return null;

            // * 몬스터 공격----------------------------------------------//

            curAttackTime += Time.deltaTime;

            if (curAttackTime < attackTime && canAttack)
            {
                time += Time.deltaTime;
                if (time > 0.25f) //n초마다 총알 발사
                {
                    time = 0;

                    SetAttackAnimation(MonsterAttackAnimation.Long_Range_Attack);

                    if (!useLeft)
                    {
                        useLeft = true;
                        FireBullet(playerTargetPos.position, muzzles[0]);
                    }
                    else
                    {
                        useLeft = false;
                        FireBullet(playerTargetPos.position, muzzles[1]);
                    }
                    StartCoroutine(Shake(0.1f));
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
                goingBack = true;
                break;
            }
        }

        if (curMonsterState != MonsterState.GetHit)
        {
            // 기울었던 몸 다시 원상 복귀
            if (useBack)
            {
                if (!goingBack)
                {
                    originRotate.y = transform.rotation.y;
                }
                time = 0;
                while (time < 1)
                {
                    time += Time.deltaTime;
                    transform.rotation = Quaternion.Slerp(transform.rotation, originRotate, Time.deltaTime * 5.0f);
                    yield return null;
                }
                if (goingBack)
                {
                    ChangeMonsterState(MonsterState.Roaming);
                }
                else
                {
                    //몬스터가 가까이 옴.
                    Monster_Motion(MonsterMotion.Short_Range_Attack);
                }
            }
        }
    }

    private void FireBullet(Vector3 targetPos, Transform muzzlePos)
    {
        //총알 발사
        StartCoroutine(Fire(targetPos, muzzlePos));
    }

    IEnumerator Fire(Vector3 targetPos, Transform muzzlePos)
    {
        GameObject bulletObj = GameManager.Instance.objectPooling.GetProjectilePrefab(bulletPrefabsName, bulletsParent);
        Rigidbody bulletRigid = bulletObj.GetComponent<Rigidbody>();

        //총알
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.Reset(m_monster, bulletPrefabsName, muzzlePos);

        bullet.OnHitPlayerEffect = (Vector3 bulletPos) =>
        {
            //플레이어가 총에 맞았을 경우, 이펙트
            Effect effect = GameManager.Instance.objectPooling.ShowEffect("Basic_Impact_01");

            effect.gameObject.transform.position = targetPos;
            Vector3 curDirection = targetPos - bulletObj.transform.position;
            effect.gameObject.transform.position += curDirection * 0.35f;
        };

        Vector3 curDirection = targetPos - bulletObj.transform.position;
        Quaternion targetAngle = Quaternion.LookRotation(curDirection);
        bulletObj.transform.rotation = targetAngle;
        //?--
        bullet.GetDistance(curDirection.normalized);
        //?--
        bulletRigid.velocity = curDirection.normalized * 50f;

        //총쏠때 이펙트
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Power_Impact_Fire_02");
        effect.gameObject.transform.position = muzzlePos.position;

        yield return null;
    }

    //총 쏘고나서 몬스터 몸 흔들리게
    IEnumerator Shake(float duration)
    {
        float shakeAmount = 0.02f;
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
                positionX = -transform.position.x * shakeAmount * 5f;
                positionY = -transform.position.y * shakeAmount;
                rotationX = -transform.position.x * shakeAmount;
                rotationY = -transform.position.y * shakeAmount * 0.2f;
            }
            if (curMonsterState == MonsterState.GetHit)
            {
                smoothAmount = 6;
                positionX = -transform.position.x * shakeAmount * 5f;
                // positionY = -transform.position.y * shakeAmount;
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
        yield return null;
    }
    // * ---------------------------------------------------------------------------------------------------------//
    // * 피격 모션

    IEnumerator GetHit_KnockBack_co()
    {
        //플레이어의 반대 방향으로 넉백
        float duration = 0.1f;

        //몬스터 상태 변경 >> 피격
        ChangeMonsterState(MonsterState.GetHit);

        StartCoroutine(Shake(duration));
        yield return new WaitForSeconds(duration);

        isGettingHit = false;

        ChangeMonsterState(MonsterState.Attack);
        Monster_Motion(MonsterMotion.Long_Range_Attack);
    }
    // * ---------------------------------------------------------------------------------------//
    // * 죽음 모션
    IEnumerator Death_co()
    {
        ChangeMonsterState(MonsterState.Death);
        SetAnimation(MonsterAnimation.Death);

        yield return new WaitForSeconds(5f);

        this.gameObject.SetActive(false);
    }

    // * ---------------------------------------------------------------------------------------//

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
}





