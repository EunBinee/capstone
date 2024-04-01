using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;     //UI 클릭시 터치 이벤트 발생 방지.

public class PlayerInputHandle : MonoBehaviour
{
    public PlayerController _controller;// = new PlayerController();
    private PlayerController P_Controller => _controller;
    private PlayerComponents P_Com => P_Controller._playerComponents;
    private CheckOption P_COption => P_Controller._checkOption;
    private PlayerInput P_Input => P_Controller._input;
    private CurrentState P_States => P_Controller._currentState;
    private CurrentValue P_Value => P_Controller._currentValue;
    private PlayerFollowCamera P_Camera => P_Controller._playerFollowCamera;
    private PlayerSkills P_Skills => P_Controller.P_Skills;
    private PlayerMovement P_Movement => P_Controller.P_Movement;
    bool endArrow = false; //화살을 쏘고 난 후인지 아닌지
    void Awake()
    {
        _controller = GetComponent<PlayerController>();
        endArrow = false;
    }
    public void MouseMoveInput()
    {
        P_Input.mouseX = Input.GetAxis("Mouse X");  //마우스 좌우
        P_Input.mouseY = Input.GetAxis("Mouse Y");  //마우스 상하
    }
    public void WASDInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            P_Input.verticalMovement = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            P_Input.verticalMovement = -1;
        }
        else
        {
            P_Input.verticalMovement = 0;
        }
        if (Input.GetKey(KeyCode.D))
        {
            P_Input.horizontalMovement = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            P_Input.horizontalMovement = -1;
        }
        else
        {
            P_Input.horizontalMovement = 0;
        }
    }


    public void MouseClickInput()
    {
        if (Input.GetMouseButtonDown(0) && !P_States.isBowMode)    //* 누를 때 => 기본공격
        {   //* 마우스 클릭
            if (P_States.isGround && !P_States.isDodgeing && !P_States.isStop && !P_States.isElectricShock
                && !EventSystem.current.IsPointerOverGameObject())
            {
                if (!P_States.isStartComboAttack)
                {
                    P_States.isStartComboAttack = true;
                    P_Movement.Attacking_co();
                }
            }
        }

        //* 활 
        if (Input.GetMouseButtonDown(0) && P_States.isBowMode && !P_States.isElectricShock)
        {
            P_Value.aimClickDown = 0;
            P_States.isClickDown = true;
            // 짧게 클릭 로직을 바로 실행하지 않고, 상태만 설정합니다.
        }

        else if (Input.GetMouseButtonUp(0) && P_States.isClickDown&&!endArrow) //endArrow가 false이면 활 o, true이면 x
        {     
            if (P_Value.aimClickDown <= 0.25f && !P_States.isShortArrow)
            {
                // 짧게 클릭 로직 실행
                //Debug.Log("[player test] Short click action");
                P_States.isShortArrow = true; // 짧게 클릭한 상태로 설정
                P_Com.animator.SetTrigger("isShortArrow");

                //* 플레이어 회전(몬스터 방향으로)
                Vector3 targetDirect = Vector3.zero;
                if (P_Value.nowEnemy != null)   //* 최근에 공격한 적(몬스터)이 있다면
                {//! P_Value.nowEnemy가 금방 사라짐;;
                    Monster nowEnemy_Monster = P_Value.nowEnemy.GetComponent<Monster>();
                    Vector3 toMonsterDir = Vector3.zero;
                    toMonsterDir = (P_Value.nowEnemy.transform.position - this.transform.position).normalized;
                    targetDirect = toMonsterDir;// + (P_Value.nowEnemy.transform.right.normalized - this.transform.right.normalized).normalized;

                }
                targetDirect.Normalize(); //대각선 이동이 더 빨라지는 것을 방지하기 위해서
                targetDirect.y = 0;
                if (targetDirect == Vector3.zero)
                {
                    targetDirect = transform.forward;
                }
                Quaternion turnRot = Quaternion.LookRotation(targetDirect);
                transform.rotation = turnRot;

                P_Skills.onArrow();
                StartCoroutine(DelayAfterAction());
            }
            P_States.isClickDown = false;
            P_Value.aimClickDown = 0;
            if (P_States.startAim)
            {
                P_States.startAim = false;
                P_Skills.arrowSkillOff();
            }
            P_Movement.StopIdleMotion();
            P_Movement.StartIdleMotion(1);    //공격 대기 모션으로 
        }

        else if (Input.GetMouseButton(0) && P_States.isBowMode && !P_States.isElectricShock)
        {
            // 길게 누르고 있는 중
            P_Value.aimClickDown += Time.deltaTime;

            if (P_Value.aimClickDown > 0.25f && !P_States.startAim && !P_States.isShortArrow)
            {
                // 길게 클릭 로직 실행
                //Debug.Log("[player test] Long click action - Entering aim mode");
                if (!P_States.isAim)
                {
                    P_Movement.camForward = P_Camera.cameraObj.transform.forward;
                    P_States.startAim = true;
                    P_Skills.onArrow();
                }
            }
        }
    }

    public void SkillKeyInput()
    {
        if (Input.GetKeyDown(KeyCode.R))  //* Bow Mode & Sword Mode
        {
            if (P_States.startAim)   // 조준 중일때 전환 키 누르면
            {
                P_Skills.arrowSkillOff();    // 조준 헤제
            }
            P_Skills.skillMotion('R');
        }
        if (Input.GetKeyUp(KeyCode.E))  //*Heal
        {
            P_Skills.skillMotion('E');
        }
        /*if (Input.GetKeyDown(KeyCode.Q))
        {
            if (P_States.isSkill)
            {
                return;
            }
            skillMotion('Q');
        }*/
    }

    IEnumerator DelayAfterAction()
    {
        endArrow = true; //화살 쏘고 나서 true
        yield return new WaitForSeconds(0.5f); // 딜레이
        endArrow = false; //다시 화살 쏠 수 있게 false로 해줘야함. 
    }

}
