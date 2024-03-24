using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;     //UI 클릭시 터치 이벤트 발생 방지.

public class PlayerInputHandle : MonoBehaviour
{
    public PlayerController _controller;// = new PlayerController();
    private PlayerController P_Controller => _controller;
    private PlayerComponents P_Com => P_Controller._playerComponents;
    private PlayerInput P_Input => P_Controller._input;
    private CurrentState P_States => P_Controller._currentState;
    private CurrentValue P_Value => P_Controller._currentValue;
    private PlayerFollowCamera P_Camera => P_Controller._playerFollowCamera;
    private PlayerSkills P_Skills => P_Controller.P_Skills;
    private PlayerMovement P_Movement => P_Controller.P_Movement;

    void Awake()
    {
        _controller = GetComponent<PlayerController>();
    }
    public void MouseMoveInput(){
        P_Input.mouseX = Input.GetAxis("Mouse X");  //마우스 좌우
        P_Input.mouseY = Input.GetAxis("Mouse Y");  //마우스 상하
    }
    public void WASDInput(){
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

    public void MouseClickInput(){
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

        else if (Input.GetMouseButtonUp(0) && P_States.isClickDown)
        {
            if (P_Value.aimClickDown <= 0.25f && !P_States.isShortArrow)
            {
                // 짧게 클릭 로직 실행
                //Debug.Log("[player test] Short click action");
                P_States.isShortArrow = true; // 짧게 클릭한 상태로 설정
                P_Com.animator.SetTrigger("isShortArrow");
                P_Skills.onArrow();
                // /Debug.Log("P_Skills.arrowSkillOn(); 이후");
                //P_States.isShortArrow = false;
            }
            P_States.isClickDown = false;
            P_Value.aimClickDown = 0;
            if (P_States.startAim)
            {
                P_States.startAim = false;
                P_Skills.arrowSkillOff();
            }
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

    public void SkillKeyInput(){
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
}
