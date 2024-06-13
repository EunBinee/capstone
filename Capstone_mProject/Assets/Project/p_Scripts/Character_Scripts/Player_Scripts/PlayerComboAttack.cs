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
        P_Value.index = 0;
    }

    public void inAttackClick()
    {

        //Debug.Log("[attack test] inAttackClick()");
        P_Com.animator.SetTrigger("onAttackCombo");

        P_Value.index = (P_Value.index + 1) % 5;
        //Debug.Log($"[attack test] P_Value.index {P_Value.index}");
        P_Com.animator.SetInteger("comboCount", P_Value.index);
        
        AttackIndexColliderSet();
        // 이펙트
        //P_Controller.playAttackEffect(P_Value.index);
        

        //AttackColliderOff();
    }
    public void FirstAttackEffect()
    {//Debug.Log("[attack test] 1111111111111111");
        P_States.isStartAnim = true;
        P_Controller.playAttackEffect(0);
        //AttackColliderOff();
    }
    public void SecondAttackEffect()
    {//Debug.Log("[attack test] 2222222222222222");
        P_States.isStartAnim = true;
        P_Controller.playAttackEffect(1);
        //AttackColliderOff();
    }
    public void ThirdAttackEffect()
    {//Debug.Log("[attack test] 3333333333333333");
        P_States.isStartAnim = true;
        P_Controller.playAttackEffect(2);
        //AttackColliderOff();
    }
    public void FourthAttackEffect()
    {//Debug.Log("[attack test] 4444444444444444");
        P_States.isStartAnim = true;
        P_Controller.playAttackEffect(3);
        //AttackColliderOff();
    }
    public void FifthAttackEffect()
    {//Debug.Log("[attack test] 5555555555555555");
        P_States.isStartAnim = true;
        P_Controller.playAttackEffect(4);
        //AttackColliderOff();
    }

    private void AttackIndexColliderSet()
    {
        //AttackColliderOff();
        Debug.Log("[attack test] AttackIndexColliderSet()");
        switch (P_Value.index)
        {
            case 0:
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
            case 1:
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
            case 2:
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
            case 3:
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
            case 4:
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
    }

    public void AttackColliderOff()
    {
        Debug.Log("[attack test] AttackColliderOff()");
        P_States.isStartComboAttack = false;
        P_States.isStartAnim = false;
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
            P_States.hadAttack = false; //* 공격 여부 비활성화
            P_States.isStartComboAttack = false;
        }
    }
}
