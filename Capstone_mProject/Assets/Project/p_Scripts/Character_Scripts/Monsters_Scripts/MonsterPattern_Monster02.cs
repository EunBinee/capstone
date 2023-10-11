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
    public GameObject bullet; //총알
    public Transform bulletsParent;
    public Transform playerTargetPos; //총알 과녁 (플레이어 몸)

    [Header("총알이 나가는 위치 : 인덱스 0번 L쪽 총, 인덱스 1번 R쪽 총")]
    public Transform[] muzzles;
    private bool using_LBullet = false;
    private bool using_RBullet = false; //총알쏘고있는지 확인.


    public override void Init()
    {
        m_monster = GetComponentInParent<Monster>();
        m_animator = GetComponent<Animator>();

        rigid = GetComponent<Rigidbody>();
        playerTrans = GameManager.Instance.gameData.GetPlayerTransform();

        m_monster.monsterPattern = this;

        navMeshAgent = null;

        playerlayerMask = 1 << playerLayerId; //플레이어 레이어

        //ChangeMonsterState(MonsterState.Roaming);
        ChangeMonsterState(MonsterState.Tracing);
        Monster_Motion(MonsterMotion.Long_Range_Attack);

        originPosition = transform.position;

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
                Roam_Monster();
                CheckPlayerCollider();
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
        Quaternion originRotation = buttomGameObject.transform.rotation;
        int angle = 60;
        int curAngle = 0;

        while (curMonsterState == MonsterState.Roaming)
        {
            time = 0;
            roamTime = UnityEngine.Random.Range(1, 3);
            Debug.Log("기다리는 중 " + roamTime + "초");
            yield return new WaitForSeconds(roamTime);


            if (curAngle == 0)
            {
                targetRotation = originRotation * Quaternion.Euler(0, angle, 0);
                curAngle = 1;
            }
            else
            {
                targetRotation = originRotation * Quaternion.Euler(0, -angle, 0);
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

    }
    // * ---------------------------------------------------------------------------------------------------------//
    // * 몬스터 공격 모션, 피격 모션, 죽음 모션
    public override void Monster_Motion(MonsterMotion monsterMotion)
    {
        switch (monsterMotion)
        {
            case MonsterMotion.Short_Range_Attack:
                //근거리 공격
                break;
            case MonsterMotion.Long_Range_Attack:
                //원거리 공격
                StartCoroutine(Long_Range_Attack_Monster02());
                break;
            case MonsterMotion.GetHit_KnockBack:
                break;
            case MonsterMotion.Death:
                //죽음
                break;
            default:
                break;
        }
    }
    // * ---------------------------------------------------------------------------------------------------------//
    // * 원거리 공격 01
    IEnumerator Long_Range_Attack_Monster02()
    {
        //좌 우 좌 우 발사 
        float time = 0;
        float distance = Vector3.Distance(transform.position, playerTrans.position);

        bool useBack = false;

        Quaternion originRotate = transform.rotation;

        while (distance > 1.5f)
        {
            useBack = true;
            time += Time.deltaTime;

            Vector3 targetPos = playerTrans.position;
            //몬스터 고개 돌리기
            Vector3 curPlayerPos = playerTrans.position;
            Vector3 curPlayerdirection = curPlayerPos - transform.position;
            Quaternion targetAngle = Quaternion.LookRotation(curPlayerdirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Time.deltaTime * 20.0f);

            yield return null;

            if (time > 0.3f) //n초마다 총알 발사
            {
                time = 0;

                SetAttackAnimation(MonsterAttackAnimation.Long_Range_Attack);
                StartCoroutine(Shake(0.1f));
                FireBullet(playerTargetPos.position, muzzles[0]);
            }

            distance = Vector3.Distance(transform.position, playerTrans.position);
        }

        if (useBack)
        {
            originRotate.y = transform.rotation.y;
            Debug.Log("originRotate " + originRotate);
            Debug.Log("transform.rotation " + transform.rotation);
            time = 0;
            while (time < 5)
            {
                time += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(transform.rotation, originRotate, Time.deltaTime * 5.0f);
                yield return null;
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
        GameObject bulletObj = Instantiate(bullet, muzzlePos.position, muzzlePos.rotation, bulletsParent);
        Rigidbody bulletRigid = bulletObj.GetComponent<Rigidbody>();

        Vector3 curDirection = targetPos - bulletObj.transform.position;
        Quaternion targetAngle = Quaternion.LookRotation(curDirection);
        bulletObj.transform.rotation = targetAngle;

        bulletRigid.velocity = curDirection.normalized * 20;

        yield return null;
    }

    IEnumerator Shake(float duration)
    {
        float shakeAmount = 0.02f;
        float smoothAmount = 1f;
        float maxAmount = 0.06f;

        float time = 0;
        Vector3 originalPosition = transform.localPosition;
        Quaternion originalRotation = transform.localRotation;


        while (time < duration)
        {
            time += Time.deltaTime;

            float positionX = -transform.position.x * shakeAmount * 5f;
            float positionY = -transform.position.y * shakeAmount;
            float rotationX = -transform.position.x * shakeAmount;
            float rotationY = -transform.position.y * shakeAmount * 0.2f;

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

    // * ---------------------------------------------------------------------------------------//

    private void OnDrawGizmos()
    {
        //몬스터 감지 범위 Draw
        //크기는  monsterData.overlapRadius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, overlapRadius);
    }
}





