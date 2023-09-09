using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerComponents _playerComponents = new PlayerComponents();
    [SerializeField] private PlayerInput _input = new PlayerInput();
    [SerializeField] private CheckOption _checkOption = new CheckOption();
    [SerializeField] private CurrentState _currentState = new CurrentState();
    [SerializeField] private CurrentValue _currentValue = new CurrentValue();
    [SerializeField] private PlayerFollowCamera _playerFollowCamera = new PlayerFollowCamera();
    private PlayerComponents P_Com => _playerComponents;
    private PlayerInput P_Input => _input;
    private CheckOption P_COption => _checkOption;
    private CurrentState P_States => _currentState;
    private CurrentValue P_Value => _currentValue;
    private PlayerFollowCamera P_Camera => _playerFollowCamera;
    private float _castRadius; //레이캐스트 반지름
    private float _castRadiusDiff; //그냥 캡슐 콜라이더 radius와 castRadius의 차이
    private float _capsuleRadiusDiff;
    private float _fixedDeltaTime; //물리 업데이트 발생주기
    public float rayCastHeightOffset = 1;
    //캡슐 가운데 가장 위쪽
    private Vector3 CapsuleTopCenterPoint
    => new Vector3(transform.position.x, transform.position.y + P_Com.capsuleCollider.height - P_Com.capsuleCollider.radius, transform.position.z);
    //캡슐 가운데 가장 아래쪽
    private Vector3 CapsuleBottomCenterPoint
   => new Vector3(transform.position.x, transform.position.y + P_Com.capsuleCollider.radius, transform.position.z);
    void Awake()
    {
        P_Com.animator = GetComponent<Animator>();
        P_Com.rigidbody = GetComponent<Rigidbody>();
        InitPlayer();
    }
    // Update is called once per frame
    void Update()
    {
        //캐릭터 입력 받음
        Inputs();
        //캐릭터의 애니메이션 변경을 수행하는 함수
        AnimationParameters();
    }
    void FixedUpdate()
    {
        _fixedDeltaTime = Time.fixedDeltaTime;
        Update_Physics();
        //전방 지면 체크
        CheckedForward();
        CheckedGround();
        if (!P_States.isPerformingAction) //액션 수행중이 아닐 때만..
        {
            //캐릭터의 실제 이동을 수행하는 함수
            AllPlayerLocomotion();
        }
    }
    void LateUpdate()
    {
        CameraActions();
    }
    private void InitPlayer()
    {
        InitCapsuleCollider();
    }
    void InitCapsuleCollider()
    {
        P_Com.capsuleCollider = GetComponent<CapsuleCollider>();
        _castRadius = P_Com.capsuleCollider.radius * 0.9f;
        _castRadiusDiff = P_Com.capsuleCollider.radius - _castRadius + 0.05f;
        //그냥 캡슐 콜라이더 radius와 castRadius의 차이
    }

    //Input 함수
    private void Inputs()
    {
        HandleSprint();
        HandleWalkOrRun();
        HandleStrafe();
        P_Input.mouseY = Input.GetAxis("Mouse Y");  //마우스 상하
        P_Input.mouseX = Input.GetAxis("Mouse X");  //마우스 좌우
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
        //Clamp01 >> 0에서 1의 값을 돌려줍니다. value 인수가 0 이하이면 0, 이상이면 1입니다
        P_Value.moveAmount = Mathf.Clamp01(Mathf.Abs(P_Input.verticalMovement) + Mathf.Abs(P_Input.horizontalMovement));
        if (P_Input.horizontalMovement == 0 && P_Input.verticalMovement == 0)
            P_States.isNotMoving = true;
        else
            P_States.isNotMoving = false;
    }
    void HandleSprint()
    {
        if (Input.GetKey(KeyCode.CapsLock) && P_Value.moveAmount > 0)
        {
            //moveAmount > 0 하는 이유
            //제자리에서 멈춰서 자꾸 뛴다.
            P_States.isSprinting = true;
            P_States.isWalking = false;
            P_States.isRunning = false;
        }
        else
        {
            P_States.isSprinting = false;
        }
    }
    void HandleWalkOrRun()
    {
        //shift => 걷기
        //뛰기중이면, 걷기 생략
        if (P_States.isSprinting)
            return;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            P_States.isWalking = true;
            P_States.isRunning = false;
        }
        else
        {
            P_States.isWalking = false;
            P_States.isRunning = true;
        }
    }
    void HandleStrafe()
    {
        //오른쪽 마우스 클릭 => 주목하면서 걷기
        if (Input.GetMouseButton(1))
        {
            P_States.isStrafing = true;
        }
        else
        {
            P_States.isStrafing = false;
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
        if (P_States.isSprinting)
        {
            //전력질주
            P_States.isStrafing = false; //뛸때는 주목 해제
            P_Com.animator.SetFloat("Vertical", 2, 0.2f, Time.deltaTime);   //상
            P_Com.animator.SetFloat("Horizontal", 0, 0.2f, Time.deltaTime);
        }
        else //뛰기 아닐 경우
        {
            if (P_States.isStrafing)
            {
                //주목기능; 현재 카메라가 바라보고 있는 방향을 주목하면서 이동
                //걷기일 경우
                if (P_States.isWalking)
                {
                    //P_Com.animatorator.SetFloat("애니파라미터", value , damptime, Time.deltaTime);
                    //value는 내가 할당하고 싶은 값
                    //dampTime은 이전값에서 value에 도달하는데 걸리는데 소요될것이라 가정하는 "지연시간"
                    //Time.deltaTime: 직전의 실행과 현재 실행 사이의 시간 차가 Time.deltaTime만큼 나오므로 Time.deltaTime을 할당
                    P_Com.animator.SetFloat("Vertical", snappedVertical / 2, 0.2f, Time.deltaTime);   //상
                    P_Com.animator.SetFloat("Horizontal", snappedHorizontal / 2, 0.2f, Time.deltaTime);               //하
                }
                else
                {
                    //뛰기일 경우
                    P_Com.animator.SetFloat("Vertical", snappedVertical, 0.2f, Time.deltaTime);   //상
                    P_Com.animator.SetFloat("Horizontal", snappedHorizontal, 0.2f, Time.deltaTime);               //하
                }
            }
            else
            {
                //걷기 일 경우
                if (P_States.isWalking)
                {
                    // Debug.Log("걷기");
                    //P_Com.animatorator.SetFloat("애니파라미터", value , damptime, Time.deltaTime);
                    //value는 내가 할당하고 싶은 값
                    //dampTime은 이전값에서 value에 도달하는데 걸리는데 소요될것이라 가정하는 "지연시간"
                    //Time.deltaTime: 직전의 실행과 현재 실행 사이의 시간 차가 Time.deltaTime만큼 나오므로 Time.deltaTime을 할당
                    P_Com.animator.SetFloat("Vertical", P_Value.moveAmount / 2, 0.2f, Time.deltaTime);   //상
                    P_Com.animator.SetFloat("Horizontal", 0, 0.2f, Time.deltaTime);               //하
                    /*
                    Vertical에만 값을 넣어주는 이유
                    걷기 모션은 Front만 쓴다.
                    이유 : 애니메이션은 딱 하나 Front만 쓰기 때문
                     몸을 돌리는 건 코드에서 돌려준다.
                    그리고 주목 기능을 쓰게 되면, 몸이 한 방향을 주목하고 움직여야하기에
                    다른 애니메이션도 쓰이게 된다. 
                    그래서 snappedVertical과 snappedHorizontal을 통해서.. 모든 값을 준다. 그래야 여러 애니메이션을 쓸 수 있기 때문
                    */
                }
                else
                {
                    //뛰기의 경우
                    //Debug.Log("뛰기");
                    P_Com.animator.SetFloat("Vertical", P_Value.moveAmount, 0.2f, Time.deltaTime);   //상
                    P_Com.animator.SetFloat("Horizontal", 0, 0.2f, Time.deltaTime);          //하
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
    private void AllPlayerLocomotion()
    {
        //캐릭터의 실제 이동을 수행하는 함수.
        PlayerRotation(); //플레이어의 방향 전환을 수행하는 함수
        PlayerMovement(); //플레이어의 움직임을 수행하는 함수.
    }
    private void PlayerRotation()
    {
        //플레이어의 "방향 전환"을 수행하는 함수
        if (P_States.isStrafing)
        {
            Vector3 rotDirect = P_Value.moveDirection;
            rotDirect = P_Camera.cameraObj.transform.forward;
            rotDirect.y = 0;
            rotDirect.Normalize();
            Quaternion rotQ = Quaternion.LookRotation(rotDirect);
            Quaternion targetRot = Quaternion.Slerp(transform.rotation, rotQ, P_COption.rotSpeed * Time.deltaTime);
            transform.rotation = targetRot;
        }
        else
        {
            //걷기와 뛰기는 동일하게
            Vector3 targetDirect = Vector3.zero;
            targetDirect = P_Camera.cameraObj.transform.forward * P_Input.verticalMovement;
            targetDirect = targetDirect + P_Camera.cameraObj.transform.right * P_Input.horizontalMovement;
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
    private void PlayerMovement()
    {
        //플레이어의 움직임을 수행하는 함수.
        //**마우스로 화면을 돌리기때문에 카메라 방향으로 캐릭터가 앞으로 전진한다.
        P_Value.moveDirection = P_Camera.cameraObj.transform.forward * P_Input.verticalMovement;
        P_Value.moveDirection = P_Value.moveDirection + P_Camera.cameraObj.transform.right * P_Input.horizontalMovement;
        P_Value.moveDirection.Normalize(); //정규화시켜준다.
        P_Value.moveDirection.y = 0;
        if ((P_States.isSprinting || P_States.isRunning) || P_States.isWalking)
        {
            if (P_States.isSprinting)    //전력질주
                P_Value.moveDirection = P_Value.moveDirection * P_COption.sprintSpeed;
            else if (P_States.isRunning) //뛸때
                P_Value.moveDirection = P_Value.moveDirection * P_COption.runningSpeed;
            else if (P_States.isWalking) //걸을 때
                P_Value.moveDirection = P_Value.moveDirection * P_COption.walkingSpeed;
            Vector3 p_velocity = Vector3.ProjectOnPlane(P_Value.moveDirection, P_Value.groundNormal);
            p_velocity = p_velocity + Vector3.up * (P_Value.gravity);
            P_Com.rigidbody.velocity = p_velocity;
            return;
        }
    }
    private void Update_Physics()
    {
        if (P_States.isGround)
        {
            //지면에 잘 붙어있을 경우
            P_Value.gravity = 0f;
        }
        else
        {
            P_Value.gravity += _fixedDeltaTime * P_COption.gravity;
        }
    }
    void CheckedForward()
    {
        //캐릭터가 이동하는 방향으로 막힘 길이 있는가?
        bool cast = Physics.CapsuleCast(CapsuleBottomCenterPoint, CapsuleTopCenterPoint,
        _castRadius, P_Value.moveDirection + Vector3.down * 0.25f,
        out var hit, P_COption.forwardCheckDistance, -1, QueryTriggerInteraction.Ignore);
        // QueryTriggerInteraction.Ignore 란? 트리거콜라이더의 충돌은 무시한다는 뜻
        P_States.isForwardBlocked = false;
        if (cast)
        {
            float forwardObstacleAngle = Vector3.Angle(hit.normal, Vector3.up);
            P_States.isForwardBlocked = forwardObstacleAngle >= P_COption.maxSlopAngle;
            if (P_States.isForwardBlocked)
                Debug.Log("앞에 장애물있음!" + forwardObstacleAngle + "도");
        }
    }
    void CheckedGround()
    {
        //캐릭터와 지면사이의 높이
        P_Value.groundDistance = float.MaxValue; //float의 최대값을 넣어준다.
        P_Value.groundNormal = Vector3.up;      //현재 바닥의 노멀 값. 
        P_Value.groundSlopeAngle = 0f;          //바닥의 경사면.
        P_Value.forwardSlopeAngle = 0f;         // 플레이어가 이동하는 방향의 바닥의 경사면.
        bool cast = Physics.SphereCast(CapsuleBottomCenterPoint, _castRadius, Vector3.down,
        out var hit, P_COption.groundCheckDistance, P_COption.groundLayerMask, QueryTriggerInteraction.Ignore);
        if (cast)
        {
            //지면의 노멀값
            P_Value.groundNormal = hit.normal;
            //지면의 경사각(기울기)
            P_Value.groundSlopeAngle = Vector3.Angle(P_Value.groundNormal, Vector3.up);
            //캐릭터 앞의 경사각
            P_Value.forwardSlopeAngle = Vector3.Angle(P_Value.groundNormal, P_Value.moveDirection) - 90f;
            //가파른 경사 있는지 체크
            P_States.isOnSteepSlop = P_Value.groundSlopeAngle >= P_COption.maxSlopAngle;
            P_Value.groundDistance = Mathf.Max((hit.distance - _capsuleRadiusDiff - P_COption.groundCheckThreshold), -10f);
            P_States.isGround = (P_Value.groundDistance <= 0.03f) && !P_States.isOnSteepSlop;
        }
        P_Value.groundCross = Vector3.Cross(P_Value.groundNormal, Vector3.up);
        //경사면의 회전축벡터 => 플레이어가 경사면을 따라 움직일수있도록 월드 이동 벡터를 회전
    }
    //-----------------------------------------------------------------
    //카메라 움직임
    private void CameraActions()
    {
        CameraFollowPlayer(); //플레이어를 따라다니는 카메라
        CameraRotate();       //마우스 방향에 따른 카메라 방향
    }
    private void CameraFollowPlayer()
    {
        //플레이어를 따라다니는 카메라
        //ref는 call by reference를 하겠다는 것.
        Vector3 cameraPos = Vector3.SmoothDamp(P_Camera.playerCamera.transform.position, transform.position, ref P_Camera.cameraFllowVelocity, 0.1f);
        P_Camera.playerCamera.transform.position = cameraPos;
    }
    private void CameraRotate()
    {
        //마우스 방향에 따른 카메라 방향
        Vector3 cameraRot;
        Quaternion targetCameraRot;
        P_Camera.left_right_LookAngle += (P_Input.mouseX * P_Camera.left_right_LookSpeed) * Time.deltaTime;
        P_Camera.up_down_LookAngle -= (P_Input.mouseY * P_Camera.up_down_LookSpeed) * Time.deltaTime;
        P_Camera.up_down_LookAngle = Mathf.Clamp(P_Camera.up_down_LookAngle, P_Camera.minPivot, P_Camera.maxPivot); //위아래 고정
        cameraRot = Vector3.zero;
        cameraRot.y = P_Camera.left_right_LookAngle;
        //y에서 up_down_LookAngle을 안쓰고 left_right_LookAngle을 쓰는이유
        //*(중요)마우스가 위로 올라갈때, 유니티 좌표계에서는 좌표가 뒤바뀐다.
        // 마우스 X좌표가 위아래 좌표가 된다.
        //그래서 카메라rot y(위아래,세로)에는 마우스의 x축(가로)을 넣어주고
        // 카메라rot x축(좌우,가로)에는  마우스의 y축(세로)를 넣어준다
        targetCameraRot = Quaternion.Euler(cameraRot);
        P_Camera.playerCamera.transform.rotation = targetCameraRot;
        cameraRot = Vector3.zero;
        cameraRot.x = P_Camera.up_down_LookAngle;
        targetCameraRot = Quaternion.Euler(cameraRot);
        P_Camera.playerCameraPivot.transform.localRotation = targetCameraRot;
    }
}
