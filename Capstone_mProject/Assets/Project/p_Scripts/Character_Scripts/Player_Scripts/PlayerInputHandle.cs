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
    private SkillInfo P_SkillInfo => P_Controller._skillInfo;
    private PlayerMovement P_Movement => P_Controller.P_Movement;
    public bool endArrow = false; //화살을 쏘고 난 후인지 아닌지
    bool setCam = false;

    private SkillButton skill_Q;
    private SkillButton skill_E;
    private SkillButton skill_R;
    private SkillButton skill_V;

    void Awake()
    {
        _controller = GetComponent<PlayerController>();
        endArrow = false;
    }
    void Start()
    {
        Invoke("Setting", 0.15f);
    }
    void Setting()
    {
        skill_Q = P_Movement.skill_Q;
        skill_E = P_Movement.skill_E;
        skill_R = P_Movement.skill_R;
        //skill_F = P_Movement.skill_F;
        skill_V = P_Movement.skill_V;

        //skillIconApply();
    }
    public void skillIconApply()
    {
        //todo: 선택스킬이 null이면 자동으로 리스트 앞에서부터 넣기
        skill_Q.imgIcon.sprite = P_SkillInfo.selectSkill[0].iconImg.sprite;
        skill_E.imgIcon.sprite = P_SkillInfo.selectSkill[1].iconImg.sprite;
        skill_R.imgIcon.sprite = P_SkillInfo.selectSkill[2].iconImg.sprite;
    }
    void Update()
    {
        // foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        // {
        //     if (Input.GetKeyDown(keyCode))
        //     {
        //         switch (keyCode)
        //         {
        //             case KeyCode.Q: P_KState.QDown = true; break;   //궁
        //             case KeyCode.W: P_KState.WDown = true; break;   //앞
        //             case KeyCode.E: P_KState.EDown = true; break;   //스킬1
        //             case KeyCode.R: P_KState.RDown = true; break;   //스킬2
        //             case KeyCode.T: P_KState.TDown = true; break;   //조준
        //             case KeyCode.Y: P_KState.YDown = true; break;
        //             case KeyCode.U: P_KState.UDown = true; break;
        //             case KeyCode.I: P_KState.IDown = true; break;
        //             case KeyCode.O: P_KState.ODown = true; break;
        //             case KeyCode.P: P_KState.PDown = true; break;
        //             case KeyCode.A: P_KState.ADown = true; break;   //좌
        //             case KeyCode.S: P_KState.SDown = true; break;   //뒤
        //             case KeyCode.D: P_KState.DDown = true; break;   //우
        //             case KeyCode.F: P_KState.FDown = true; break;   //스킬3
        //             case KeyCode.G: P_KState.GDown = true; break;
        //             case KeyCode.H: P_KState.HDown = true; break;
        //             case KeyCode.J: P_KState.JDown = true; break;
        //             case KeyCode.K: P_KState.KDown = true; break;
        //             case KeyCode.L: P_KState.LDown = true; break;
        //             case KeyCode.Z: P_KState.ZDown = true; break;
        //             case KeyCode.X: P_KState.XDown = true; break;
        //             case KeyCode.C: P_KState.CDown = true; break;
        //             case KeyCode.V: P_KState.VDown = true; break;
        //             case KeyCode.B: P_KState.BDown = true; break;
        //             case KeyCode.N: P_KState.NDown = true; break;
        //             case KeyCode.M: P_KState.MDown = true; break;
        //             case KeyCode.CapsLock: P_States.isWalking = true; break;    //걷기 on
        //             default: break;
        //         }
        //     }

        //     if (Input.GetKeyUp(keyCode))
        //     {
        //         switch (keyCode)
        //         {
        //             case KeyCode.Q: P_KState.QDown = false; break;  //궁
        //             case KeyCode.W: P_KState.WDown = false; break;  //앞
        //             case KeyCode.E: P_KState.EDown = false; break;  //스킬1
        //             case KeyCode.R: P_KState.RDown = false; break;  //스킬2
        //             case KeyCode.T: P_KState.TDown = false; break;  //조준
        //             case KeyCode.Y: P_KState.YDown = false; break;
        //             case KeyCode.U: P_KState.UDown = false; break;
        //             case KeyCode.I: P_KState.IDown = false; break;
        //             case KeyCode.O: P_KState.ODown = false; break;
        //             case KeyCode.P: P_KState.PDown = false; break;
        //             case KeyCode.A: P_KState.ADown = false; break;  //좌
        //             case KeyCode.S: P_KState.SDown = false; break;  //뒤
        //             case KeyCode.D: P_KState.DDown = false; break;  //우
        //             case KeyCode.F: P_KState.FDown = false; break;  //스킬3
        //             case KeyCode.G: P_KState.GDown = false; break;
        //             case KeyCode.H: P_KState.HDown = false; break;
        //             case KeyCode.J: P_KState.JDown = false; break;
        //             case KeyCode.K: P_KState.KDown = false; break;
        //             case KeyCode.L: P_KState.LDown = false; break;
        //             case KeyCode.Z: P_KState.ZDown = false; break;
        //             case KeyCode.X: P_KState.XDown = false; break;
        //             case KeyCode.C: P_KState.CDown = false; break;
        //             case KeyCode.V: P_KState.VDown = false; break;
        //             case KeyCode.B: P_KState.BDown = false; break;
        //             case KeyCode.N: P_KState.NDown = false; break;
        //             case KeyCode.M: P_KState.MDown = false; break;
        //             case KeyCode.CapsLock: P_States.isWalking = false; break;   //걷기 off
        //             default: break;
        //         }
        //     }
        // }

        if (Input.GetKeyDown(KeyCode.Q)) P_KState.QDown = true;   // 궁
        if (Input.GetKeyDown(KeyCode.E)) P_KState.EDown = true;   // 스킬1
        if (Input.GetKeyDown(KeyCode.R)) P_KState.RDown = true;   // 스킬2
        if (Input.GetKeyDown(KeyCode.W)) P_KState.WDown = true;   // 앞
        if (Input.GetKeyDown(KeyCode.A)) P_KState.ADown = true;   // 좌
        if (Input.GetKeyDown(KeyCode.S)) P_KState.SDown = true;   // 뒤
        if (Input.GetKeyDown(KeyCode.D)) P_KState.DDown = true;   // 우
        if (Input.GetKeyDown(KeyCode.V)) P_KState.VDown = true;   // 우
        if (Input.GetKeyDown(KeyCode.CapsLock)) P_States.isWalking = true; // 걷기 on

        if (Input.GetKeyUp(KeyCode.Q)) P_KState.QDown = false;
        if (Input.GetKeyUp(KeyCode.E)) P_KState.EDown = false;
        if (Input.GetKeyUp(KeyCode.R)) P_KState.RDown = false;
        if (Input.GetKeyUp(KeyCode.W)) P_KState.WDown = false;
        if (Input.GetKeyUp(KeyCode.A)) P_KState.ADown = false;   // 좌
        if (Input.GetKeyUp(KeyCode.S)) P_KState.SDown = false;   // 뒤
        if (Input.GetKeyUp(KeyCode.D)) P_KState.DDown = false;   // 우
        if (Input.GetKeyUp(KeyCode.V)) P_KState.VDown = false;
        if (Input.GetKeyUp(KeyCode.CapsLock)) P_States.isWalking = false; // 걷기 off

    }

    public void KeyRebind()
    {
        //Debug.Log("KeyRebind()");
        P_States.isWalking = false;
        P_States.isRunning = false;
        //P_States.isNotMoving = true;
        P_Com.animator.Rebind();

        {
            P_KState.QDown = false;
            P_KState.WDown = false;
            P_KState.EDown = false;
            P_KState.RDown = false;
            P_KState.TDown = false;
            P_KState.YDown = false;
            P_KState.UDown = false;
            P_KState.IDown = false;
            P_KState.ODown = false;
            P_KState.PDown = false;
            P_KState.ADown = false;
            P_KState.SDown = false;
            P_KState.DDown = false;
            P_KState.FDown = false;
            P_KState.GDown = false;
            P_KState.HDown = false;
            P_KState.JDown = false;
            P_KState.KDown = false;
            P_KState.LDown = false;
            P_KState.ZDown = false;
            P_KState.XDown = false;
            P_KState.CDown = false;
            P_KState.VDown = false;
            P_KState.BDown = false;
            P_KState.NDown = false;
            P_KState.MDown = false;
        }
    }

    public void CameraMouseMoveInput()
    {
        P_Input.camMouseX = Input.GetAxis("DHorizontal");  //우측 조이스틱 좌우
        P_Input.camMouseY = Input.GetAxis("DVertical");  //우측 조이스틱 상하
    }

    public void MouseMoveInput()
    {
        P_Input.mouseX = Input.GetAxis("Mouse X");  //마우스 좌우
        P_Input.mouseY = Input.GetAxis("Mouse Y");  //마우스 상하
    }

    public float Key2Movement()
    {
        if (P_KState.WDown || Input.GetAxis("JVertical") > 0)
        {
            P_Input.verticalMovement = 1;
        }
        else if (P_KState.SDown || Input.GetAxis("JVertical") < 0)
        {
            P_Input.verticalMovement = -1;
        }
        else
        {
            P_Input.verticalMovement = 0;
        }
        if (P_KState.DDown || Input.GetAxis("JHorizontal") > 0)
        {
            P_Input.horizontalMovement = 1;
        }
        else if (P_KState.ADown || Input.GetAxis("JHorizontal") < 0)
        {
            P_Input.horizontalMovement = -1;
        }
        else
        {
            P_Input.horizontalMovement = 0;
        }
        return P_Input.verticalMovement + P_Input.horizontalMovement;
    }

    /// <summary>
    /// Joystick1Button0 = A
    /// Joystick1Button1 = B
    /// joystick1button2 = X
    /// joystick1button3 = Y
    /// joystick1button4 = Right Bumper
    /// joystick1button5 = Left Bumper
    /// </summary>
    public void MouseClickInput()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Joystick1Button1)) && !(P_States.isBowMode || P_States.isGunMode))    //* 누를 때 => 기본공격
        {   //* 마우스 클릭
            if (P_States.isGround
                //&& !P_States.isGettingHit 
                && !P_States.isDodgeing
                && !P_States.isStop && !P_States.isElectricShock)
            //&& !EventSystem.current.IsPointerOverGameObject())
            {
                P_States.isClickAttack = true;
            }
        }

        //* 원거리 
        if ((Input.GetMouseButtonDown(0) || Input.GetAxis("Fire") > 0) && (P_States.isBowMode || P_States.isGunMode) && !P_States.isElectricShock && !P_States.onShootAim)
        {
            //Debug.Log("[player test] Input.GetMouseButtonDown(0) 다운");
            P_Value.aimClickDown = 0;
            P_States.isClickDown = true;
            // 짧게 클릭 로직을 바로 실행하지 않고, 상태만 설정합니다.
            P_States.onShootAim = true;
            P_Com.animator.SetBool("onLeftClick", true);
        }
        else if (Input.GetMouseButtonUp(0) || Input.GetAxis("Fire") <= 0) //endArrow가 false이면 활 o, true이면 x
        {
            //* 활모드일때 클릭업
            if (P_States.isClickDown && P_States.isBowMode && P_Value.aimClickDown <= 0.25f && !P_States.isShortArrow && !endArrow)
            {
                // 짧게 클릭 로직 실행
                //Debug.Log("[player test] Short click action");
                P_States.isShortArrow = true; // 짧게 클릭한 상태로 설정
                P_Com.animator.SetTrigger("isShortArrow");

                //* 플레이어 회전(몬스터 방향으로)
                Vector3 targetDirect = Vector3.zero;
                if (P_Value.nowEnemy != null)   //* 최근에 공격한 적(몬스터)이 있다면
                {//! P_Value.nowEnemy가 금방 사라짐
                    Monster nowEnemy_Monster = P_Value.nowEnemy.GetComponent<Monster>();
                    targetDirect = (P_Value.nowEnemy.transform.position - this.transform.position).normalized;
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
            // P_Movement.StopIdleMotion();
            // P_Movement.StartIdleMotion(1);    //공격 대기 모션으로 
        }

        //* 총모드일 때 꾹 누르고 있으면
        if ((Input.GetMouseButton(0) || Input.GetAxis("Fire") > 0) && P_States.isGunMode && P_States.onZoomIn && !P_States.isElectricShock)
        {
            //Debug.Log("[player test] Input.GetMouseButton(0) 꾹");
            // 길게 누르고 있는 중
            P_Com.animator.SetBool("onLeftClick", true);
            if (!endArrow)
            //* 총모드일때 발사
            {
                //P_Com.animator.SetTrigger("shoot");
                P_States.isShoot = true;
                P_Skills.onShoot();
                StartCoroutine(DelayZoomInGun());
            }
        }
        //* 활모드일 때 꾹 누르고 있으면
        else if ((Input.GetMouseButton(0) || Input.GetAxis("Fire") > 0) && P_States.isBowMode && !P_States.isElectricShock)
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
        else if (P_States.isGunMode)
        {
            //Debug.Log("[player test] Input.GetMouseButton(0) else");
            P_States.onShootAim = false;
            P_Com.animator.SetBool("onLeftClick", false);
        }

        //* 총모드일 때 우클릭(좌범퍼) 누르고 있으면 -> 변수 설정(속도 감소) + 줌인
        if ((Input.GetMouseButton(1) || Input.GetAxis("Aim") > 0) && P_States.isGunMode && !P_States.onZoomIn)
        {
            P_States.onZoomIn = true;
            P_Com.animator.SetBool("onClickGun", true);
            if (!setCam)
            {
                setCam = true;
                P_Skills.ZoomOnOff(true);
            }
        }
        else if (Input.GetMouseButtonUp(1) //|| Input.GetAxis("Aim") <= 0) 
            && P_States.isGunMode && P_States.onZoomIn)
        {
            P_States.onZoomIn = false;
            P_Com.animator.SetBool("onClickGun", false);

            if (setCam)
            {
                setCam = false;
                P_Skills.ZoomOnOff(false);
            }
        }
    }

    public void SkillKeyInput()
    {
        if (P_KState.VDown ||   //* Bow Mode & Sword Mode  
                Input.GetKey(KeyCode.Joystick1Button0)) // A
        {
            P_KState.VDown = false;
            if (skill_V.imgCool.fillAmount == 0)
            {
                if (P_States.startAim)   // 조준 중일때 전환 키 누르면
                {
                    P_Skills.arrowSkillOff();    // 조준 헤제
                }
                P_Skills.skillMotion("ChangeWeapon", 'V');
            }
        }
        if (P_KState.QDown && !P_States.isSkill)    // no use
        {
            if (skill_Q.imgCool.fillAmount == 0)
            {
                skill_Q.skill = P_SkillInfo.selectSkill[0].skillData;
                skill_Q.imgIcon.sprite = P_SkillInfo.selectSkill[0].skillData.icon;
                P_Skills.skillMotion(mapValueReturnKey(P_SkillInfo.selectSkill[0]), 'Q');
            }
        }
        if ((P_KState.EDown || Input.GetKey(KeyCode.Joystick1Button3)) && !P_States.isSkill)    // heal
        {
            if (skill_E.imgCool.fillAmount == 0)
            {
                skill_E.skill = P_SkillInfo.selectSkill[1].skillData;
                skill_E.imgIcon.sprite = P_SkillInfo.selectSkill[1].skillData.icon;
                P_Skills.skillMotion(mapValueReturnKey(P_SkillInfo.selectSkill[1]), 'E');
            }
        }
        if (P_KState.RDown && !P_States.isSkill) 
        {
            if (skill_R.imgCool.fillAmount == 0)
            {
                skill_R.skill = P_SkillInfo.selectSkill[2].skillData;
                skill_R.imgIcon.sprite = P_SkillInfo.selectSkill[2].skillData.icon;
                P_Skills.skillMotion(mapValueReturnKey(P_SkillInfo.selectSkill[2]), 'R');
            }
        }
        //if (P_KState.TDown && !P_States.isSkill)
        //{
        //    P_KState.TDown = false;
        //    if (skill_T.imgCool.fillAmount == 0)
        //        P_Skills.skillMotion("Ultimate", 'T');
        //}
    }

    public void skillBtnOnclick(char key)
    {
        switch (key)
        {
            case 'Q':
                P_KState.QDown = false;
                skill_Q.OnClicked();
                break;
            case 'E':
                P_KState.EDown = false;
                skill_E.OnClicked();
                break;
            case 'R':
                P_KState.RDown = false;
                skill_R.OnClicked();
                break;
            //case 'T':
            //    P_KState.TDown = false;
            //    skill_T.OnClicked();
            //    break;
            default: break;
        }
        P_States.isSkill = false;
    }

    public string mapValueReturnKey(PlayerSkillName skill)
    {
        foreach (KeyValuePair<string, PlayerSkillName> item in P_Skills.skillMap)
        {
            if (item.Value == skill)
            {
                return item.Key;
            }
        }
        return null;
    }

    IEnumerator DelayAfterAction()
    {
        endArrow = true; //화살 쏘고 나서 true
        yield return new WaitForSeconds(0.5f); // 딜레이
        endArrow = false; //다시 화살 쏠 수 있게 false로 해줘야함. 
    }

    IEnumerator DelayZoomOutGun()
    {
        endArrow = true; //화살 쏘고 나서 true
        yield return new WaitForSeconds(0.5f); // 딜레이
        endArrow = false; //다시 화살 쏠 수 있게 false로 해줘야함. 
    }
    IEnumerator DelayZoomInGun()
    {
        endArrow = true; //화살 쏘고 나서 true
        yield return new WaitForSeconds(0.1f); // 딜레이
        endArrow = false; //다시 화살 쏠 수 있게 false로 해줘야함. 
    }

}
