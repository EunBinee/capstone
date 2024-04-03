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
    private KeyState P_KState => P_Controller._keyState;
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
    void Update()
    {
        //WASDInput();
        //KeyInput();
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                switch (keyCode)
                {
                    case KeyCode.Q: P_KState.QDown = true; break;   //궁
                    case KeyCode.W: P_KState.WDown = true; break;   //앞
                    case KeyCode.E: P_KState.EDown = true; break;   //힐
                    case KeyCode.R: P_KState.RDown = true; break;   //조준
                    case KeyCode.T: P_KState.TDown = true; break;
                    case KeyCode.Y: P_KState.YDown = true; break;
                    case KeyCode.U: P_KState.UDown = true; break;
                    case KeyCode.I: P_KState.IDown = true; break;
                    case KeyCode.O: P_KState.ODown = true; break;
                    case KeyCode.P: P_KState.PDown = true; break;
                    case KeyCode.A: P_KState.ADown = true; break;   //좌
                    case KeyCode.S: P_KState.SDown = true; break;   //뒤
                    case KeyCode.D: P_KState.DDown = true; break;   //우
                    case KeyCode.F: P_KState.FDown = true; break;
                    case KeyCode.G: P_KState.GDown = true; break;
                    case KeyCode.H: P_KState.HDown = true; break;
                    case KeyCode.J: P_KState.JDown = true; break;
                    case KeyCode.K: P_KState.KDown = true; break;
                    case KeyCode.L: P_KState.LDown = true; break;
                    case KeyCode.Z: P_KState.ZDown = true; break;
                    case KeyCode.X: P_KState.XDown = true; break;
                    case KeyCode.C: P_KState.CDown = true; break;
                    case KeyCode.V: P_KState.VDown = true; break;
                    case KeyCode.B: P_KState.BDown = true; break;
                    case KeyCode.N: P_KState.NDown = true; break;
                    case KeyCode.M: P_KState.MDown = true; break;
                    case KeyCode.CapsLock: P_States.isWalking = true; break;
                    default: break;
                }
            }

            if (Input.GetKeyUp(keyCode))
            {
                switch (keyCode)
                {
                    case KeyCode.Q: P_KState.QDown = false; break;
                    case KeyCode.W: P_KState.WDown = false; break;
                    case KeyCode.E: P_KState.EDown = false; break;
                    case KeyCode.R: P_KState.RDown = false; break;
                    case KeyCode.T: P_KState.TDown = false; break;
                    case KeyCode.Y: P_KState.YDown = false; break;
                    case KeyCode.U: P_KState.UDown = false; break;
                    case KeyCode.I: P_KState.IDown = false; break;
                    case KeyCode.O: P_KState.ODown = false; break;
                    case KeyCode.P: P_KState.PDown = false; break;
                    case KeyCode.A: P_KState.ADown = false; break;
                    case KeyCode.S: P_KState.SDown = false; break;
                    case KeyCode.D: P_KState.DDown = false; break;
                    case KeyCode.F: P_KState.FDown = false; break;
                    case KeyCode.G: P_KState.GDown = false; break;
                    case KeyCode.H: P_KState.HDown = false; break;
                    case KeyCode.J: P_KState.JDown = false; break;
                    case KeyCode.K: P_KState.KDown = false; break;
                    case KeyCode.L: P_KState.LDown = false; break;
                    case KeyCode.Z: P_KState.ZDown = false; break;
                    case KeyCode.X: P_KState.XDown = false; break;
                    case KeyCode.C: P_KState.CDown = false; break;
                    case KeyCode.V: P_KState.VDown = false; break;
                    case KeyCode.B: P_KState.BDown = false; break;
                    case KeyCode.N: P_KState.NDown = false; break;
                    case KeyCode.M: P_KState.MDown = false; break;
                    case KeyCode.CapsLock: P_States.isWalking = false; break;
                    default: break;
                }
            }
        }
    }
    public void MouseMoveInput()
    {
        P_Input.mouseX = Input.GetAxis("Mouse X");  //마우스 좌우
        P_Input.mouseY = Input.GetAxis("Mouse Y");  //마우스 상하
    }

    public void Key2Movement()
    {
        if (P_KState.WDown)
        {
            P_Input.verticalMovement = 1;
        }
        else if (P_KState.SDown)
        {
            P_Input.verticalMovement = -1;
        }
        else
        {
            P_Input.verticalMovement = 0;
        }
        if (P_KState.DDown)
        {
            P_Input.horizontalMovement = 1;
        }
        else if (P_KState.ADown)
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

        else if (Input.GetMouseButtonUp(0) && P_States.isClickDown && !endArrow) //endArrow가 false이면 활 o, true이면 x
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
        if (P_KState.RDown)  //* Bow Mode & Sword Mode
        {
            P_KState.RDown = false;
            if (P_States.startAim)   // 조준 중일때 전환 키 누르면
            {
                P_Skills.arrowSkillOff();    // 조준 헤제
            }
            P_Skills.skillMotion('R');
        }
        if (P_KState.EDown)  //*Heal
        {
            P_KState.EDown = false;
            P_Skills.skillMotion('E');
        }
        /*if (P_KState.QDown)
        {
            P_KState.QDown = false;
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
