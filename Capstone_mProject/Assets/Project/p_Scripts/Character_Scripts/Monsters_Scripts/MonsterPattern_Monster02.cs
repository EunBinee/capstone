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
                switch (animIndex)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    default:
                        break;
                }
                break;
            case MonsterAttackAnimation.Long_Range_Attack:
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

    //---------------------------------------------------------------------------------------------------------//
    //몬스터 상태 =>> 로밍
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
        //float rotationSpeed = 60.0f;
        //float time = 0;
        //float roamTime = 0;
        //
        //while (curMonsterState == MonsterState.Roaming)
        //{
        //    time = 0;
        //
        //    roamTime = UnityEngine.Random.Range(3, 8);
        //    Debug.Log("기다리는 중 " + roamTime + "초");
        //    yield return new WaitForSeconds(roamTime);
        //
        //    float rotTime = UnityEngine.Random.Range(3, 8);
        //
        //
        //    int rotDirect = UnityEngine.Random.Range(0, 2);
        //    if (rotDirect == 0)
        //        rotationSpeed = Mathf.Abs(rotationSpeed);
        //    else
        //        rotationSpeed = Mathf.Abs(rotationSpeed) * -1;
        //
        //    Debug.Log(rotTime + "초동안 돈다. 그리고 " + rotationSpeed);
        //
        //    while (time <= rotTime)
        //    {
        //        time += Time.deltaTime;
        //        buttomGameObject.transform.Rotate(new Vector3(0, rotationSpeed * Time.deltaTime, 0));
        //        yield return null;
        //    }
        //}
        //----------------------------------------------------------------------------------//

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

        //----------------------------------------------------------------------------------//
        //float rotationSpeed = 0.5f;
        //float time = 0;
        //float roamTime = 0;
        //Quaternion targetRotation;
        //float totalRotationTime = 0.0f;
        //
        //while (curMonsterState == MonsterState.Roaming)
        //{
        //    time = 0;
        //    roamTime = UnityEngine.Random.Range(4, 8);
        //    Debug.Log("기다리는 중 " + roamTime + "초");
        //    yield return new WaitForSeconds(roamTime);
        //
        //
        //    int randomAngle = UnityEngine.Random.Range(90, 180);
        //    Debug.Log("randomAngle도 만큼 회전 " + randomAngle + "도");
        //    // randomAngle 도 만큼 회전
        //    targetRotation = buttomGameObject.transform.rotation * Quaternion.Euler(0, randomAngle, 0);
        //    while (time <= 10)
        //    {
        //        time += Time.deltaTime;
        //        // Slerp를 사용하여 부드럽게 회전합니다.
        //        buttomGameObject.transform.rotation = Quaternion.Slerp(buttomGameObject.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        //        if (transform.rotation == targetRotation)
        //            break;
        //
        //        yield return null;
        //    }
        //}


    }

    public override void CheckPlayerCollider()
    {

    }

    //---------------------------------------------------------------------------------------//
    private void OnDrawGizmos()
    {
        //몬스터 감지 범위 Draw
        //크기는  monsterData.overlapRadius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, overlapRadius);
    }
}





