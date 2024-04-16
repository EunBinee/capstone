using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    private PlayerController _controller;// = new PlayerController();
    private PlayerController P_Controller => _controller;
    private PlayerComponents P_Com => P_Controller._playerComponents;
    private PlayerInput P_Input => P_Controller._input;
    private CurrentState P_States => P_Controller._currentState;
    private KeyState P_KeyState => P_Controller._keyState;
    private CurrentValue P_Value => P_Controller._currentValue;
    private CheckOption P_COption => P_Controller._checkOption;
    private PlayerFollowCamera P_Camera => P_Controller._playerFollowCamera;
    private PlayerSkills P_Skills => P_Controller.P_Skills;
    private SkillInfo P_SkillInfo => P_Controller._skillInfo;
    private PlayerPhysicsCheck P_PhysicsCheck;// => P_Controller.P_PhysicsCheck;
    private PlayerInputHandle P_InputHandle;

    Coroutine idleMotion_co;

    public SkillButton skill_T; // weapon change
    public SkillButton skill_E;
    public SkillButton skill_R;
    public SkillButton skill_F;
    public SkillButton skill_Q; // Ultimate

    public float comboClickTime = 0.5f;
    [Header("플레이어 공격 콜라이더 : 인덱스 0번 칼, 1번 L발, 2번 R발")]
    public Collider[] attackColliders;
    private List<PlayerAttackCheck> playerAttackChecks;
    public List<PlayerAttackCheck> playerArrowList; //* 현재 사용중인 화살

    float ElecTime = 0;
    bool showElec = false;

    public Vector3 camForward;

    public ScrollRect skillScrollWindow;

    void Start()
    {
        _controller = GetComponent<PlayerController>();
        playerAttackChecks = new List<PlayerAttackCheck>();
        P_PhysicsCheck = GetComponent<PlayerPhysicsCheck>();
        P_InputHandle = GetComponent<PlayerInputHandle>();
        SetUIVariable();
        for (int i = 0; i < attackColliders.Length; i++)
        {
            PlayerAttackCheck attackCheck = attackColliders[i].gameObject.GetComponent<PlayerAttackCheck>();
            playerAttackChecks.Add(attackCheck);
        }
        playerArrowList = new List<PlayerAttackCheck>();
        P_Value.index = 1;
        P_States.hadAttack = false;
        P_States.canGoForwardInAttack = true; // 플레이어 앞으로 가기 제어 true 움직이기 , false 안움직임
    }

    public void SetUIVariable()
    {
        //* 필수 UI 가지고 오기
        if (CanvasManager.instance.playerUI == null)
        {
            CanvasManager.instance.playerUI = CanvasManager.instance.GetCanvasUI(CanvasManager.instance.dialogueUIName);
            if (CanvasManager.instance.playerUI == null)
                return;
        }
        PlayerUI_info playerUI_info = CanvasManager.instance.playerUI.GetComponent<PlayerUI_info>();

         skill_T = playerUI_info.skill_T;
        skill_T.gameObject.SetActive(true);
        //_controller.originTpos = skill_T.gameObject.transform.position;
        _controller.originTpos = new Vector3(1000, -1000, 0);

        skill_E = playerUI_info.skill_E;
        skill_E.gameObject.SetActive(true);
        _controller.originEpos = skill_E.gameObject.transform.position;

        skill_R = playerUI_info.skill_R;
        skill_R.gameObject.SetActive(true);
        _controller.originRpos = skill_R.gameObject.transform.position;

        skill_F = playerUI_info.skill_F;
        skill_F.gameObject.SetActive(true);
        _controller.originFpos = skill_F.gameObject.transform.position;

        skill_Q = playerUI_info.skill_Q;
        skill_Q.gameObject.SetActive(false);
        _controller.originQpos = skill_Q.gameObject.transform.position;

        skillScrollWindow = playerUI_info.skillScrollWindow;
        skillScrollWindow.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!UIManager.gameIsPaused)
        {
            AnimationParameters();
            Inputs();
        }
    }
    void FixedUpdate()
    {
        if (P_States.isStartComboAttack && P_Com.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f
            && !P_States.isGettingHit
            && (Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton(1) || P_Input.jumpMovement == 1))
        {
            P_Value.index = 1;
            P_Value.time = 0;
            P_Value.isCombo = false;
            P_States.isStartComboAttack = false;
            P_Com.animator.SetInteger("comboCount", P_Value.index);
            P_Com.animator.SetBool("p_Locomotion", true);
            P_Com.animator.Rebind();
        }
        P_PhysicsCheck.CheckedGround();
        if (!P_States.isPerformingAction) //액션 수행중이 아닐 때만..
        {
            //캐릭터의 실제 이동을 수행하는 함수
            AllPlayerLocomotion();
        }
        if (P_States.isStop)
        {
            if (P_States.isAim)    //* 조준 모드라면
            {
                P_Com.animator.SetTrigger("shoot");
                P_Skills.arrowSkillOff();
            }
        }
    }

    private void Inputs()
    {
        if (!HandleJump()
                || !P_Com.animator.GetCurrentAnimatorStateInfo(0).IsName("KnockDown"))   //* 넉백 애니메이션 시 or
        {
            //HandleSprint();
            HandleWalkOrRun();

            P_InputHandle.MouseMoveInput();

            P_InputHandle.MouseClickInput();

            P_InputHandle.Key2Movement();

            //* skills input
            P_InputHandle.SkillKeyInput();

            //* [미카 디버프 단축키]==================================================
            //if (P_KeyState.ODown)
            //{
            //    P_Value.HP = 10;
            //}
            //if (P_KeyState.PDown)
            //{
            //    //Debug.Log("Electric on");
            //    P_States.isElectricShock = true;    // 감전
            //}


            if (P_States.isSkill == false && P_States.startAim == false && P_States.isStartComboAttack == false
                && (P_Controller.curPlayerState == PlayerState.Idle || P_Controller.curPlayerState == PlayerState.FinishComboAttack))
            {
                StartIdleMotion(0);  //일반 대기 모션으로 
            }
            //Clamp01 >> 0에서 1의 값을 돌려줍니다. value 인수가 0 이하이면 0, 이상이면 1입니다
            P_Value.moveAmount = Mathf.Clamp01(Mathf.Abs(P_Input.verticalMovement) + Mathf.Abs(P_Input.horizontalMovement) + P_Input.jumpMovement);
            if (P_Input.horizontalMovement == 0 && P_Input.verticalMovement == 0 && P_Input.jumpMovement == 0)
            {
                P_States.isNotMoving = true;
                P_Controller.AnimState(PlayerState.Idle);
            }
            else { P_States.isNotMoving = false; }
        }
    }

    public void Attacking_co()
    {
        StartCoroutine(Attacking());
    }

    private void HandleSprint()
    {
        if (Input.GetKey(KeyCode.LeftControl) && P_Value.moveAmount > 0)
        {
            //moveAmount > 0 하는 이유
            //제자리에서 멈춰서 자꾸 뛴다.
            P_States.isSprinting = true;
            //P_States.isWalking = false;
            P_States.isRunning = false;
            //P_States.isJumping = false;
        }
        else
        {
            P_States.isSprinting = false;
        }
    }

    private void HandleWalkOrRun()
    {
        //shift => 걷기
        //뛰기중이면, 걷기 생략
        if (P_States.isSprinting)
            return;

        if (P_Value.moveAmount > 0)
        {
            //P_States.isWalking = false;
            P_States.isRunning = true;
        }
        else
        {
            //P_States.isWalking = false;
            P_States.isRunning = false;
            P_Com.rigidbody.velocity = Vector3.zero;
        }
    }

    private bool HandleJump()
    {
        if (Input.GetKey(KeyCode.Space) && !P_States.isJumping && !P_States.isElectricShock)
        {
            //Debug.Log(P_Value.hitDistance);
            P_Input.jumpMovement = 1;
            return true;
        }
        return false;
    }
    private void PlayerJump()
    {
        if (P_Input.jumpMovement == 1 && !P_States.isJumping)
        {
            P_States.isJumping = true;
            P_Value.gravity = P_COption.gravity;
            P_Com.rigidbody.AddForce(Vector3.up * P_COption.jumpPower, ForceMode.Impulse);
            P_Com.animator.Play("jump 0");
            P_Com.animator.SetBool("isJump_Up", true);
        }
        if (P_States.isJumping && P_States.isGround)
        {
            P_Com.animator.SetBool("isJump_Up", false);
            P_States.isJumping = false;
            P_Input.jumpMovement = 0;
            P_Value.gravity = 0;
        }
    }

    private bool returnDodgeAnim()
    {
        if (P_Com.animator.GetCurrentAnimatorStateInfo(1).IsName("Front")
        || P_Com.animator.GetCurrentAnimatorStateInfo(1).IsName("Back")
        || P_Com.animator.GetCurrentAnimatorStateInfo(1).IsName("Left")
        || P_Com.animator.GetCurrentAnimatorStateInfo(1).IsName("Right"))
        {
            return true;
        }
        else return false;
    }
    private void HandleDodge()
    {
        P_States.currentDodgeKeyPress = (Input.GetKey(KeyCode.LeftShift) || Input.GetMouseButton(1));
        if (P_States.previousDodgeKeyPress && P_States.currentDodgeKeyPress && P_States.isDodgeing
            && returnDodgeAnim())
        {
            //Debug.Log("이전 프레임에도 누름!");
            return;
        }
        else if (!returnDodgeAnim()
        && P_States.currentDodgeKeyPress && !P_States.isDodgeing && P_Value.moveAmount > 0 && !P_States.isStartComboAttack)
        {
            P_States.isDodgeing = true;
        }
        P_States.previousDodgeKeyPress = P_States.currentDodgeKeyPress;
    }
    private void dodgeOut()
    {
        P_States.isDodgeing = false;
        // 대쉬 종료 후 Rigidbody 속도를 다시 원래 속도로 변경
        P_Com.rigidbody.velocity = Vector3.zero;
    }

    private void AllPlayerLocomotion()
    {
        //캐릭터의 실제 이동을 수행하는 함수.
        PlayerRotation(); //플레이어의 방향 전환을 수행하는 함수
        PlayerMovements(); //플레이어의 움직임을 수행하는 함수.
        PlayerJump();
        HandleDodge();
    }
    private void PlayerRotation()
    {
        if (P_States.isGettingHit)
        {
            Monster curmonster = P_Controller.Get_CurHitEnemy();
            Vector3 rotationDirection;
            rotationDirection = curmonster.transform.position - this.transform.position;
            rotationDirection.y = 0;
            rotationDirection.Normalize();
            Quaternion tr = Quaternion.LookRotation(rotationDirection);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, P_COption.rotSpeed * Time.deltaTime);
            transform.rotation = targetRotation;
        }
        else if (P_States.isStop || P_States.isJumping || P_States.doNotRotate)
        {
            if (P_Com.animator.GetCurrentAnimatorStateInfo(0).IsName("locomotion"))
            {
                P_Value.gravity = 0;
            }
            return;
        }
        else if (P_States.isAim && !P_States.isCamOnAim) // 조준모드 들어갔을 때 한번만 실행하도록
        {
            //if (!P_States.isCamOnAim)   //* 카메라가 바라보는 방향으로 플레이어 회전
            {
                P_States.isCamOnAim = true;
                Vector3 rotationDirection = camForward;
                rotationDirection.y = 0;
                rotationDirection.Normalize();
                Quaternion tr = Quaternion.LookRotation(rotationDirection);
                transform.rotation = tr;
            }
        }
        else if (P_States.isStrafing) //* 주목할때만 쓰임
        {
            Vector3 rotationDirection = P_Value.moveDirection;
            if (rotationDirection != Vector3.zero)
            {
                //!@ 여기에 약점과 플레이어의 거리체크후,거리가 가까우면 가까운약점 쪽으로 로테이션 
                //멀어지면 다시 카메라로 로테이션
                if (GameManager.instance.cameraController.curTargetMonster != null)
                {
                    Monster targetMonster = GameManager.instance.cameraController.curTargetMonster;
                    if (targetMonster.monsterData.isBottomlessMonster)
                    {
                        int curW_index = targetMonster.GetIndex_NearestLegs(this.transform);
                        float distance = Vector3.Distance(targetMonster.monsterData.bottomlessMonsterLegs[curW_index].transform.position, this.transform.position);

                        if (distance < 2f)
                        {
                            //가까워지면 Player 몸을 약점 쪽으로 돌려주기
                            Vector3 weaknessPos = targetMonster.monsterData.bottomlessMonsterLegs[curW_index].transform.position;
                            Vector3 targetPos = new Vector3(weaknessPos.x, targetMonster.gameObject.transform.position.y, weaknessPos.z);

                            rotationDirection = targetPos - this.transform.position;
                            rotationDirection.y = 0;
                        }
                        else
                        {
                            rotationDirection = P_Camera.cameraObj.transform.forward;
                            rotationDirection.y = 0;
                        }
                    }
                    else
                    {
                        rotationDirection = P_Camera.cameraObj.transform.forward;
                        rotationDirection.y = 0;
                    }
                    rotationDirection.Normalize();
                    Quaternion tr = Quaternion.LookRotation(rotationDirection);
                    Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, P_COption.rotSpeed * Time.deltaTime);
                    transform.rotation = targetRotation;
                }
                else
                {
                    rotationDirection = P_Camera.cameraObj.transform.forward;
                    rotationDirection.y = 0;

                    rotationDirection.Normalize();
                    Quaternion tr = Quaternion.LookRotation(rotationDirection);
                    Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, P_COption.rotSpeed * Time.deltaTime);
                    transform.rotation = targetRotation;
                    transform.rotation = tr;
                }
            }
        }
        else// if (!P_States.isAim)
        {
            //걷기와 뛰기는 동일하게

            Vector3 targetDirect = Vector3.zero;

            if (P_Value.nowEnemy != null && (P_States.isShortArrow || (P_States.isStartComboAttack && P_Value.isCombo)))   //* 최근에 공격한 적(몬서터)이 있다면
            {
                Monster nowEnemy_Monster = P_Value.nowEnemy.GetComponent<Monster>();
                Vector3 toMonsterDir = Vector3.zero;
                if (nowEnemy_Monster.monsterData.isBottomlessMonster)
                {
                    float distance = 10000;
                    int curW_index = 0;
                    for (int i = 0; i < nowEnemy_Monster.monsterData.bottomlessMonsterLegs.Count; ++i)
                    {
                        float m_distance = Vector3.Distance(nowEnemy_Monster.monsterData.bottomlessMonsterLegs[i].position, this.transform.position);
                        if (m_distance < distance)
                        {
                            distance = m_distance;
                            curW_index = i;
                        }
                    }
                    Vector3 _monster = new Vector3(nowEnemy_Monster.monsterData.bottomlessMonsterLegs[curW_index].position.x, 0, nowEnemy_Monster.monsterData.bottomlessMonsterLegs[curW_index].position.z);
                    toMonsterDir = (_monster - this.transform.position).normalized;
                }
                else
                {
                    toMonsterDir = (P_Value.nowEnemy.transform.position - this.transform.position).normalized;
                }
                targetDirect = toMonsterDir * P_Input.verticalMovement;
                targetDirect = targetDirect + (P_Value.nowEnemy.transform.right.normalized - this.transform.right.normalized).normalized * P_Input.horizontalMovement;

            }
            else    //* 최근에 공격한 적이 없다면
            {
                targetDirect = P_Camera.cameraObj.transform.forward * P_Input.verticalMovement;
                targetDirect = targetDirect + P_Camera.cameraObj.transform.right * P_Input.horizontalMovement;
            }
            targetDirect.Normalize(); //대각선 이동이 더 빨라지는 것을 방지하기 위해서
            targetDirect.y = 0;
            if (targetDirect == Vector3.zero)
            {
                //vector3.zero는 0,0,0 이다.
                //방향 전환이 없기에 캐릭터의 방향은 고냥 원래 방향.
                targetDirect = transform.forward;
            }
            Quaternion turnRot = Quaternion.LookRotation(targetDirect);
            Quaternion targetRot = Quaternion.Slerp(transform.rotation, turnRot, P_COption.rotSpeed * Time.deltaTime);
            transform.rotation = targetRot;
        }
    }

    private void PlayerMovements()
    {
        //플레이어의 움직임을 수행하는 함수.

        if (P_States.doNotRotate || (P_States.isStartComboAttack
                && (!P_Com.animator.GetCurrentAnimatorStateInfo(0).IsName("locomotion")
                && P_Com.animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8f))
                || P_Com.animator.GetCurrentAnimatorStateInfo(0).IsName("KnockDown")   //* 넉백 애니메이션 시 or
                || P_Com.animator.GetCurrentAnimatorStateInfo(0).IsName("StandUp")     //* 넉백 후 일어나는 애니메이션 시 or
                || (P_States.isClickDown && P_States.isShortArrow))
        {
            P_Com.rigidbody.velocity = Vector3.zero;    //* 꼼짝마
            P_Com.animator.SetBool("p_Locomotion", true);
            return;
        }
        if (P_States.isStop)
        {
            P_Com.rigidbody.velocity = Vector3.zero;
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            return;
        }

        //**마우스로 화면을 돌리기때문에 카메라 방향으로 캐릭터가 앞으로 전진한다.
        P_Value.moveDirection = P_Camera.cameraObj.transform.forward * P_Input.verticalMovement;
        P_Value.moveDirection = P_Value.moveDirection + P_Camera.cameraObj.transform.right * P_Input.horizontalMovement;

        P_Value.moveDirection.Normalize(); //정규화시켜준다.

        Vector3 p_velocity;

        if (P_States.isElectricShock)   //*감전
        {
            P_Value.finalSpeed = P_COption.slowlySpeed;
            P_States.isJumping = false; P_Input.jumpMovement = 0;
            P_States.isDodgeing = false;
            if (P_States.isBowMode && P_States.startAim)
            {
                P_Skills.arrowSkillOff();
            }
            StartCoroutine(electricity_Damage());
            ElecTime += Time.deltaTime;
            if (ElecTime >= P_COption.electricShock_Time) //* 5초 후
            {
                P_States.isElectricShock = false;
                ElecTime = 0f;
                //Debug.Log("Electric off");
            }
            P_Value.moveDirection = P_Value.moveDirection * P_Value.finalSpeed;

            p_velocity = Vector3.ProjectOnPlane(P_Value.moveDirection, P_Value.groundNormal);
            p_velocity = p_velocity + Vector3.up * (P_Value.gravity);
            P_Com.rigidbody.velocity = p_velocity;
        }
        else if (P_States.isAim)    //조준
        {
            P_Value.finalSpeed = P_COption.slowlySpeed;
            P_States.isJumping = false; P_Input.jumpMovement = 0;
            P_States.isDodgeing = false;
            P_Value.moveDirection = P_Value.moveDirection * P_Value.finalSpeed;

            p_velocity = Vector3.ProjectOnPlane(P_Value.moveDirection, P_Value.groundNormal);
            p_velocity = p_velocity + Vector3.up * (P_Value.gravity);
            P_Com.rigidbody.velocity = p_velocity;
        }
        else if (P_States.isJumping)
        {
            //Time.timeScale = 0.1f;
            p_velocity = P_Com.rigidbody.velocity + Vector3.up * (P_Value.gravity) * Time.fixedDeltaTime;
            P_Com.rigidbody.velocity = p_velocity;
        }
        else if (P_States.isDodgeing)
        {
            if (P_States.isStrafing)    //* 주목중이라면 
            {
                if (P_Input.verticalMovement > 0)//* front
                {
                    P_Com.animator.Play("Front", 1);
                }
                else if (P_Input.verticalMovement < 0)//* back
                {
                    P_Com.animator.Play("Back", 1);
                }
                else if (P_Input.horizontalMovement > 0)//* right
                {
                    P_Com.animator.Play("Right", 1);
                }
                else if (P_Input.horizontalMovement < 0)//* left
                {
                    P_Com.animator.Play("Left", 1);
                }
            }
            else    //* 주목중이 아니면
            {
                P_Com.animator.Play("Front", 1);
            }
            P_Value.moveDirection.y = 0;
            P_Com.rigidbody.velocity += P_Value.moveDirection * P_COption.dodgingSpeed;

            Invoke("dodgeOut", 0.2f);    //닷지 유지 시간 = 0.2초

        }
        else if (P_States.isWalking || P_States.isSprinting || P_States.isRunning)
        {
            //Time.timeScale = 1f;
            P_Value.moveDirection.y = 0;

            if (P_States.isSprinting)    //전력질주
                P_Value.finalSpeed = P_COption.sprintSpeed;
            else if (P_States.isWalking) //걸을때
                P_Value.finalSpeed = P_COption.walkingSpeed;
            else if (P_States.isRunning) //뛸때
                P_Value.finalSpeed = P_COption.runningSpeed;
            P_Value.moveDirection = P_Value.moveDirection * P_Value.finalSpeed;

            p_velocity = Vector3.ProjectOnPlane(P_Value.moveDirection, P_Value.groundNormal);
            p_velocity = p_velocity + Vector3.up * (P_Value.gravity);
            P_Com.rigidbody.velocity = p_velocity;
            P_Controller.AnimState(PlayerState.Move);
        }

    }

    IEnumerator electricity_Damage()
    {   //todo: 파직 파직 파직(느리게) 나오면서 "감전" UI 같이 출력
        float num = 0;
        while (num < 6 && !showElec)
        {
            num++;
            showElec = true;
            float x = UnityEngine.Random.Range(-0.1f, 0.1f);
            float y = UnityEngine.Random.Range(-0.5f, 0.5f);
            float z = UnityEngine.Random.Range(-0.1f, 0.1f);
            Vector3 randomPos = new Vector3(x, y, z);   //* 랜덤 위치 저장

            Effect effect = GameManager.Instance.objectPooling.ShowEffect("Player_electric", this.transform);
            StartCoroutine(followEffect(effect, randomPos));
            yield return new WaitForSeconds(1f);
            showElec = false;
        }
    }
    IEnumerator followEffect(Effect effect, Vector3 randomPos)
    {
        bool endElec = false;
        effect.finishAction = () => { endElec = true; };    //* 이펙트 끝나면
        while (!endElec)    //* 종료
        {
            effect.transform.position = P_Com.playerTargetPos.position + randomPos;
            yield return null;
        }
    }

    //애니메이터 블랜더 트리의 파라미터 변경
    private void AnimationParameters()
    {
        //캐릭터의 애니메이션 변경을 수행하는 함수
        //isStrafing에 쓰인다. >주목기능; 현재 카메라가 바라보고 있는 방향을 주목하면서 이동
        float snappedVertical = 0.0f;
        float snappedHorizontal = 0.0f;
        #region Horizontal 
        if (P_Input.horizontalMovement > 0 && P_Input.horizontalMovement <= 0.5f)
        {
            //0보다 큰데 0.5보다 같거나 작은 경우
            snappedHorizontal = 0.5f;
        }
        else if (P_Input.horizontalMovement > 0.5f)
        {
            //0.5보다 큰경우
            snappedHorizontal = 1;
        }
        else if (P_Input.horizontalMovement < 0 && P_Input.horizontalMovement >= -0.5f)
        {
            //0보다 작은데 -0.5보다 같거나 큰 경우
            snappedHorizontal = -0.5f;
        }
        else if (P_Input.horizontalMovement < -0.5f)
        {
            //-0.5보다 작은 경우
            snappedHorizontal = -1;
        }
        else
        {
            //아무것도 누르지 않은 경우
            snappedHorizontal = 0;
        }
        #endregion
        #region Vertical
        if (P_Input.verticalMovement > 0 && P_Input.verticalMovement <= 0.5f)
        {
            //0보다 큰데 0.5보다 같거나 작은 경우
            snappedVertical = 0.5f;
        }
        else if (P_Input.verticalMovement > 0.5f)
        {
            //0.5보다 큰경우
            snappedVertical = 1;
        }
        else if (P_Input.verticalMovement < 0 && P_Input.verticalMovement >= -0.5f)
        {
            //0보다 작은데 -0.5보다 같거나 큰 경우
            snappedVertical = -0.5f;
        }
        else if (P_Input.verticalMovement < -0.5f)
        {
            //-0.5보다 작은 경우
            snappedVertical = -1;
        }
        else
        {
            //아무것도 누르지 않은 경우
            snappedVertical = 0;
        }
        #endregion
        if ((P_States.isStartComboAttack || !P_States.isGround /*|| P_States.isDodgeing*/ || P_States.isShortArrow)
                && !P_Com.animator.GetCurrentAnimatorStateInfo(0).IsName("locomotion"))
        {
            P_Com.animator.SetFloat("Vertical", 0, 0f, Time.deltaTime);   //상
            P_Com.animator.SetFloat("Horizontal", 0, 0f, Time.deltaTime); //하
            return;
        }
        if (P_States.isSprinting)
        {
            //전력질주
            P_States.isStrafing = false; //뛸때는 주목 해제
            P_Com.animator.SetFloat("Vertical", 2, 0f, Time.deltaTime);   //상
            P_Com.animator.SetFloat("Horizontal", 0, 0f, Time.deltaTime);
        }
        else //전력질주 아닐 경우
        {
            if (P_States.isStrafing)
            {
                if (P_States.isRunning)
                {
                    //뛰기일 경우
                    P_Com.animator.SetFloat("Vertical", snappedVertical, 0f, Time.deltaTime);   //상
                    P_Com.animator.SetFloat("Horizontal", snappedHorizontal, 0f, Time.deltaTime);               //하
                }
            }
            else
            {
                if (P_States.isAim || P_States.isWalking)
                {
                    P_Com.animator.SetFloat("Vertical", P_Value.moveAmount / 3, 0.2f, Time.deltaTime);
                    P_Com.animator.SetFloat("Horizontal", 0, 0.2f, Time.deltaTime);
                }
                if (P_States.isRunning)
                {
                    //뛰기의 경우
                    //Debug.Log("뛰기");
                    P_Com.animator.SetFloat("Vertical", P_Value.moveAmount, 0.05f, Time.deltaTime);   //상
                    P_Com.animator.SetFloat("Horizontal", 0, 0.05f, Time.deltaTime);                   //하
                }
                if (P_Value.moveAmount == 0)
                {
                    // Debug.Log("멈춤");
                    //아무것도 안누른 경우. >idle
                    P_Com.animator.SetFloat("Vertical", 0, 0.2f, Time.deltaTime);   //상
                    P_Com.animator.SetFloat("Horizontal", 0, 0.2f, Time.deltaTime); //하
                }
            }
        }
    }

    public void StartIdleMotion(float value)
    {
        idleMotion_co = StartCoroutine(IdleMotion(value));
    }
    public void StopIdleMotion()
    {
        if (idleMotion_co != null)
        {
            StopCoroutine(idleMotion_co);
        }
    }

    IEnumerator IdleMotion(float value) // value = 1 쩍벌
    {
        if (value == 0) //가만히 두고 있다
        {
            float dTime = 0;
            while (dTime < 5f)
            {
                dTime += Time.deltaTime;
                yield return null;
            }
            //yield return new WaitForSeconds(5f);    //5초 뒤에 실행
            while (P_Com.animator.GetFloat("Idle") > 0.0f && !P_States.isStartComboAttack && !P_States.startAim) //0.0f보다 값이 크고 공격을 하지 않았다면
            {
                P_Com.animator.SetFloat("Idle", value, 0.2f, Time.deltaTime);
                yield return null;
            }
        }
        else
        {
            P_Com.animator.SetFloat("Idle", value);
        }
        yield return null;
    }

    IEnumerator Attacking() //클릭해서 들어오면
    {
        //Debug.Log("[attack test]플레이어 공격 코루틴 입장");
        P_Com.animator.SetInteger("comboCount", 0);

        string comboName01 = "Attack_Combo_1";
        string comboName02 = "Attack_Combo_2";
        string comboName03 = "Attack_Combo_3";
        string comboName04 = "Attack_Combo_4";
        string comboName05 = "Attack_Combo_5";

        List<Collider> playerColliderList = new List<Collider>();
        List<PlayerAttackCheck> playerAttackCheckList = new List<PlayerAttackCheck>();

        while (true)
        {
            P_Value.isCombo = false;    //* 이전 공격 여부 초기화(비활성화)
                                        //P_Controller.ChangePlayerState(PlayerState.ComboAttack);
                                        //AnimState(PlayerState.ComboAttack, index);
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

                    P_Value.curAnimName = comboName01;
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

                    P_Value.curAnimName = comboName02;
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

                    P_Value.curAnimName = comboName03;
                    break;
                case 4:
                    //양발 다
                    //Debug.Log("[attack test]플레이어 공격 콜라이더 활성화 : 양발 다4");
                    playerColliderList.Add(attackColliders[1]);
                    playerAttackCheckList.Add(playerAttackChecks[1]);
                    playerColliderList.Add(attackColliders[2]);
                    playerAttackCheckList.Add(playerAttackChecks[2]);

                    for (int i = 0; i < playerColliderList.Count; ++i)
                    {
                        playerColliderList[i].enabled = true;
                        playerAttackCheckList[i].isEnable = true;
                    }

                    P_Value.curAnimName = comboName04;
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

                    P_Value.curAnimName = comboName05;
                    break;
                default:
                    P_Value.curAnimName = "";
                    break;
            }

            //* 공격 시 앞으로 찔끔찔끔 가도록
            Vector3 dir;
            //앞이 막혀있지 않고 적이 있다면 //* 전진
            if (P_Value.nowEnemy != null && P_PhysicsCheck.forwardHit == null && P_States.canGoForwardInAttack)
            {
                Monster nowEnemy_Monster = P_Value.nowEnemy.GetComponent<Monster>();

                if (nowEnemy_Monster.monsterData.isBottomlessMonster)
                {
                    int curW_index = nowEnemy_Monster.GetIndex_NearestLegs(this.transform);

                    Vector3 monster_ = new Vector3(nowEnemy_Monster.monsterData.bottomlessMonsterLegs[curW_index].position.x, 0, nowEnemy_Monster.monsterData.bottomlessMonsterLegs[curW_index].position.z);
                    dir = (monster_ - this.transform.position).normalized;
                }
                else
                {
                    dir = (P_Value.nowEnemy.transform.position - this.transform.position).normalized;
                }

                Vector3 pos = transform.position + dir * 4f;
                transform.position = Vector3.Lerp(transform.position, pos, 5 * Time.deltaTime);
            }
            //앞이 막혀있지 않고 적이 없다면 //* 전진
            else if (P_Value.nowEnemy == null && P_PhysicsCheck.forwardHit == null && P_States.canGoForwardInAttack)
            {
                dir = this.gameObject.transform.forward.normalized;
                Vector3 pos = transform.position + dir * 2f;
                transform.position = Vector3.Lerp(transform.position, pos, 5 * Time.deltaTime);
            }
            //앞에 막혀있거나 앞으로 가지 못한다면 //* 그대로
            else if (P_PhysicsCheck.forwardHit != null || !P_States.canGoForwardInAttack)
            {
                //dir = this.gameObject.transform.forward.normalized;
            }

            if (!P_States.isGettingHit)
            {
                //* 이펙트
                P_Controller.playAttackEffect(P_Value.curAnimName);
            }

            //* 공격 애니메이션 재생
            P_Com.animator.Play(P_Value.curAnimName);
            StopIdleMotion();
            StartIdleMotion(1);    //공격 대기 모션으로 

            yield return new WaitUntil(() => P_Com.animator.GetCurrentAnimatorStateInfo(0).IsName(P_Value.curAnimName));
            yield return new WaitUntil(() => P_Com.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f);

            //플레이어 공격 콜라이더 비활성화
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

            //P_Controller.ChangePlayerState(PlayerState.FinishComboAttack);
            P_Controller.AnimState(PlayerState.FinishComboAttack, P_Value.index);

            int curIndex = P_Value.index;
            P_Value.time = 0;

            while (P_Value.time <= comboClickTime)  //* 콤보 클릭 시간 전까지
            {
                P_Value.time += Time.deltaTime; //* 시간 누적
                                                //* 애니메이션 70퍼센트 진행까지 대기
                yield return new WaitUntil(() => P_Com.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f);
                //P_States.isStartComboAttack = false;

                if (Input.GetMouseButton(0) && curIndex == P_Value.index/**/)   //* 마우스 입력 받음
                {
                    //P_States.isStartComboAttack = true; //* 공격 시작
                    if (P_Value.index >= 5) //* 5타 이상이면
                    {
                        yield return new WaitUntil(() => P_Com.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.7f);
                        P_Value.index = 1;  //* 인덱스 초기화
                        P_Value.time = 0;   //* 시간 초기화
                                            //P_States.isStartComboAttack = false;    //* 공격 끝
                    }
                    else
                    {
                        P_Value.index = P_Value.index + 1;    //* 인덱스 추가
                                                              //P_Value.isCombo = true; //* 이전 공격 여부 활성화
                                                              //P_States.hadAttack = false; //* 공격 여부 비활성화
                    }
                    P_Value.isCombo = false;    //* 이전 공격 여부 비활성화
                    P_States.hadAttack = false; //* 공격 여부 비활성화
                    P_States.hasAttackSameMonster = false;
                    P_States.notSameMonster = false;
                    break;  // ...1
                }
            }   // ...1 (while (P_Value.time <= comboClickTime))
            if (P_Value.isCombo == false)   //* 5타 이상이었다면(이후 공격 안한다면)
            {
                //* 원래대로
                P_Com.animator.SetInteger("comboCount", P_Value.index);
                P_Com.animator.SetBool("p_Locomotion", true);
                break;  // ...2
            }

        }   // ...2 (while (true))

        P_States.isStartComboAttack = false;    //* 공격 끝
    }

    public void StopPlayer() //연출쪽에서 Player멈추도록.
    {
        P_States.isStop = true;

        P_Value.index = 1;
        P_Value.time = 0;
        P_Value.isCombo = false;
        P_States.isStartComboAttack = false;
        P_Com.animator.SetInteger("comboCount", P_Value.index);
        P_Com.animator.SetBool("p_Locomotion", true);
        //P_Com.animator.Play("locomotion");
        P_Com.animator.Rebind();
    }

    public void PlayPlayer()
    {
        P_States.isStop = false;
    }


    public void PlayerElectricShock(bool electrocution = true)
    {
        P_States.isElectricShock = electrocution;
    }
}
