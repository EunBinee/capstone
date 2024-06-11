using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComboAttack : MonoBehaviour
{

    public PlayerController _controller;
    private PlayerController P_Controller => _controller;
    private PlayerMovement P_Movement => P_Controller.P_Movement;
    private PlayerComponents P_Com => P_Controller._playerComponents;
    private CurrentState P_States => P_Controller._currentState;
    private CurrentValue P_Value => P_Controller._currentValue;
    private PlayerPhysicsCheck P_PhysicsCheck;// => P_Controller.P_PhysicsCheck;
    private PlayerInputHandle P_InputHandle;

    List<Collider> playerColliderList = new List<Collider>();
    List<PlayerAttackCheck> playerAttackCheckList = new List<PlayerAttackCheck>();
    public float comboClickTime = 0.5f;
    //[Header("플레이어 공격 콜라이더 : 인덱스 0번 칼, 1번 L발, 2번 R발")]
    public Collider[] attackColliders;
    private List<PlayerAttackCheck> playerAttackChecks;

    string comboName01 = "Attack_Combo_1";
    string comboName02 = "Attack_Combo_2";
    string comboName03 = "Attack_Combo_3";
    string comboName04 = "Attack_Combo_4";
    string comboName05 = "Attack_Combo_5";

    void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }

    void Start()
    {
        P_Controller.AttackEffectName[0] = comboName01;
        P_Controller.AttackEffectName[1] = comboName02;
        P_Controller.AttackEffectName[2] = comboName03;
        P_Controller.AttackEffectName[3] = comboName04;
        P_Controller.AttackEffectName[4] = comboName05;

        P_PhysicsCheck = P_Controller.P_PhysicsCheck;
        P_InputHandle = P_Controller.P_InputHandle;
        attackColliders = P_Movement.attackColliders;
        P_States.hadAttack = false; //* 공격 여부 비활성화

        playerAttackChecks = new List<PlayerAttackCheck>();
        for (int i = 0; i < attackColliders.Length; i++)
        {
            PlayerAttackCheck attackCheck = attackColliders[i].gameObject.GetComponent<PlayerAttackCheck>();
            playerAttackChecks.Add(attackCheck);
        }
    }

    public void inAttackClick()
    {
        Debug.Log("[attack test] inAttackClick()");
        //P_Com.animator.SetTrigger("onAttackCombo");

        P_Value.index = Mathf.Clamp(P_Value.index+1, 1, (P_Value.index + 1) % 6);
        Debug.Log($"[attack test] P_Value.index {P_Value.index}");
        
        AttackIndexColliderSet();
        
    }

    public void Attacking_co()
    {
        StartCoroutine(Attacking());
    }

    private void AttackIndexColliderSet()
    {
        AttackColliderOff();
        Debug.Log("[attack test] AttackIndexColliderSet()");
        switch (P_Value.index)
        {
            case 1:
                //검
                //Debug.Log("[attack test]플레이어 공격 콜라이더 활성화 : 검1");
                playerColliderList.Add(attackColliders[0]);
                playerAttackCheckList.Add(playerAttackChecks[0]);

                for (int i = 0; i < playerColliderList.Count; ++i)
                {
                    playerColliderList[i].enabled = true;
                    playerAttackCheckList[i].isEnable = true;
                }

                //P_Value.curAnimName = comboName01;
                break;
            case 2:
                //검
                //Debug.Log("[attack test]플레이어 공격 콜라이더 활성화 : 검2");
                playerColliderList.Add(attackColliders[0]);
                playerAttackCheckList.Add(playerAttackChecks[0]);

                for (int i = 0; i < playerColliderList.Count; ++i)
                {
                    playerColliderList[i].enabled = true;
                    playerAttackCheckList[i].isEnable = true;
                }

                //P_Value.curAnimName = comboName02;
                break;
            case 3:
                //오른쪽 다리
                //Debug.Log("[attack test]플레이어 공격 콜라이더 활성화 : 오른쪽 다리3");
                playerColliderList.Add(attackColliders[2]);
                playerAttackCheckList.Add(playerAttackChecks[2]);

                for (int i = 0; i < playerColliderList.Count; ++i)
                {
                    playerColliderList[i].enabled = true;
                    playerAttackCheckList[i].isEnable = true;
                }

                //P_Value.curAnimName = comboName03;
                break;
            case 4:
                //양발 다
                //Debug.Log("[attack test]플레이어 공격 콜라이더 활성화 : 양발 다4");
                playerColliderList.Add(attackColliders[1]);
                playerAttackCheckList.Add(playerAttackChecks[1]);
                //playerColliderList.Add(attackColliders[2]);
                //playerAttackCheckList.Add(playerAttackChecks[2]);

                for (int i = 0; i < playerColliderList.Count; ++i)
                {
                    playerColliderList[i].enabled = true;
                    playerAttackCheckList[i].isEnable = true;
                }

                //P_Value.curAnimName = comboName04;
                break;
            case 5:
                //검
                //Debug.Log("[attack test]플레이어 공격 콜라이더 활성화 : 검5");
                playerColliderList.Add(attackColliders[0]);
                playerAttackCheckList.Add(playerAttackChecks[0]);

                for (int i = 0; i < playerColliderList.Count; ++i)
                {
                    playerColliderList[i].enabled = true;
                    playerAttackCheckList[i].isEnable = true;
                }

                //P_Value.curAnimName = comboName05;
                break;
            default:
                //P_Value.curAnimName = "";
                break;
        }
        // 이펙트
        P_Controller.playAttackEffect();
    }

    public void AttackColliderOff()
    {
        //Debug.Log("[attack test] AttackColliderOff()");
        if (playerAttackCheckList.Count != 0)
        {
            //Debug.Log("[attack test]플레이어 공격 콜라이더 비활성화");
            for (int i = 0; i < playerColliderList.Count; ++i)
            {
                playerColliderList[i].enabled = false;
                playerAttackCheckList[i].isEnable = false;
            }
            playerColliderList.Clear();
            playerAttackCheckList.Clear();
        }
        P_States.hadAttack = false; //* 공격 여부 비활성화
    }

    private void AnimAttack()
    {
        //* 공격 애니메이션 재생
        //P_Com.animator.Play(P_Value.curAnimName);
        P_Com.animator.SetTrigger("onAttackCombo");
    }

    IEnumerator Attacking() //클릭해서 들어오면
    {
        //Debug.Log("[attack test]플레이어 공격 코루틴 입장");
        P_Com.animator.SetInteger("comboCount", 0);
        P_States.hadAttack = false; //* 공격 여부 비활성화
        P_States.isStartComboAttack = true;

        while (true)
        {
            P_Value.isCombo = false;    //* 이전 공격 여부 초기화(비활성화)
            P_Com.animator.SetInteger("comboCount", P_Value.index);

            // 콜라이더 인데스 맞춰서 활성화
            AttackIndexColliderSet();
            

            //* 공격 시 앞으로 찔끔찔끔 가도록
            Vector3 dir;
            //앞이 막혀있지 않고
            if (P_PhysicsCheck.forwardHit == null && P_States.canGoForwardInAttack)
            {
                //적이 있다면 //* 전진
                if (P_Value.nowEnemy != null)
                {
                    Monster nowEnemy_Monster = P_Value.nowEnemy.GetComponent<Monster>();

                    if (nowEnemy_Monster.monsterData.isBottomlessMonster)
                    {
                        int curW_index = nowEnemy_Monster.GetIndex_NearestLegs(this.transform);

                        Vector3 monster_ = new Vector3(nowEnemy_Monster.monsterData.bottomlessMonsterLegs[curW_index].position.x,
                                                            0, nowEnemy_Monster.monsterData.bottomlessMonsterLegs[curW_index].position.z);
                        dir = (monster_ - this.transform.position).normalized;
                    }
                    else
                    {
                        dir = (P_Value.nowEnemy.transform.position - this.transform.position).normalized;
                    }

                    Vector3 pos = transform.position + dir * 4f;
                    transform.position = Vector3.Lerp(transform.position, pos, 5 * Time.deltaTime);
                }
                //적이 없다면 //* 전진
                else if (P_Value.nowEnemy == null)
                {
                    dir = this.gameObject.transform.forward.normalized;
                    Vector3 pos = transform.position + dir * 2f;
                    transform.position = Vector3.Lerp(transform.position, pos, 5 * Time.deltaTime);
                }
            }
            //앞에 막혀있거나 앞으로 가지 못한다면 //* 그대로
            else if (P_PhysicsCheck.forwardHit != null || !P_States.canGoForwardInAttack)
            {
                //dir = this.gameObject.transform.forward.normalized;
            }

            // 공격 애니메이션 
            AnimAttack();

            //* 이펙트
            //P_Controller.playAttackEffect(P_Value.curAnimName);

            P_Movement.StopIdleMotion();
            P_Movement.StartIdleMotion(1);    //공격 대기 모션으로 

            //yield return new WaitUntil(() => P_Com.animator.GetCurrentAnimatorStateInfo(0).IsName(P_Value.curAnimName));
            //yield return new WaitUntil(() => P_Com.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f);
            yield return null;

            //플레이어 공격 콜라이더 비활성화
            AttackColliderOff();

            P_Controller.AnimState(PlayerState.ComboAttack, P_Value.index);

            int curIndex = P_Value.index;
            P_Value.time = 0;

            while (P_Value.time <= comboClickTime)//P_Com.animator.GetCurrentAnimatorStateInfo(0).normalizedTime)  //* 콤보 클릭 시간 전까지
            {
                P_Value.time += Time.deltaTime; //* 시간 누적
                //* 애니메이션 70퍼센트 진행까지 대기
                //yield return new WaitUntil(() => P_Com.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f);
                //yield return null;
                P_States.hadAttack = false; //* 공격 여부 비활성화

                if (Input.GetMouseButton(0) && curIndex == P_Value.index)   //* 마우스 입력 받음
                {
                    P_Value.isCombo = false;    //* 이전 공격 여부 비활성화
                    if (P_Value.index >= 5) //* 5타 이상이면
                    {
                        P_Value.index = 1;  //* 인덱스 초기화
                        P_Value.time = 0;   //* 시간 초기화
                        //yield return new WaitUntil(() => P_Com.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f);
                        //yield return null;
                        break;
                    }
                    else
                    {
                        P_Value.index = P_Value.index + 1;    //* 인덱스 추가
                        P_Value.time = 0;   //* 시간 초기화
                        P_Value.isCombo = true; //* 이전 공격 여부 활성화
                    }
                    P_States.hasAttackSameMonster = false;
                    P_States.notSameMonster = false;
                    break;  // ...1
                }
                yield return null;
            }   // ...1 (while (P_Value.time <= comboClickTime))
            if (P_Value.isCombo == false || !P_States.isStartComboAttack)   //* 5타 이상이었다면(이후 공격 안한다면)
            {
                //* 원래대로
                P_Controller.AnimState(PlayerState.FinishComboAttack, P_Value.index);
                //P_Com.animator.SetInteger("comboCount", P_Value.index);
                //P_Com.animator.SetBool("p_Locomotion", true);
                break;  // ...2
            }

        }   // ...2 (while (true))

        P_States.isStartComboAttack = false;    //* 공격 끝
        P_InputHandle.isAttack = false;
        P_Value.index = 1;  //* 인덱스 초기화
        P_Value.time = 0;   //* 시간 초기화
    }

}
