using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Player캐릭터 정의의 모든 것

[Serializable]
public class PlayerComponents
{
    [Header("Player")]
    public Animator animator;
    public Rigidbody rigidbody;
    public CapsuleCollider capsuleCollider;
    public Transform playerTargetPos;
}

[Serializable]
public class PlayerInput
{
    public float verticalMovement;   //상하
    public float horizontalMovement; //좌우
    public float mouseY;             //마우스 상하
    public float mouseX;             //마우스 좌우
    public float jumpMovement;       //점프

}

[Serializable]
public class CheckOption
{

    [Tooltip("지면으로 체크할 레이어 설정")]
    public LayerMask groundLayerMask = -1;

    [Range(0.01f, 0.05f), Tooltip("전방 장애물 감지 거리")]
    public float forwardCheckDistance = 0.1f;

    [Range(0.1f, 10.0f), Tooltip("지면 감지 거리")]
    public float groundCheckDistance = 2.0f;
    [Range(0.0f, 0.1f), Tooltip("지면 감지 거리")]
    public float groundCheckThreshold = 0.01f;

    [Range(1f, 70f), Tooltip("등반이 가능한 경사각")]
    public float maxSlopAngle = 50f;

    [Range(1f, 20f), Tooltip("회전속도")]
    public float rotSpeed = 20f;

    [Range(1f, 30f), Tooltip("전력으로 달리는 속도")]
    public float sprintSpeed = 15f;

    [Range(1f, 30f), Tooltip("달리는 속도")]
    public float runningSpeed = 10f;

    [Range(1f, 30f), Tooltip("걷는 속도")]
    public float walkingSpeed = 2f;

    [Range(1f, 30f), Tooltip("점프할때 속도")]
    public float jumpPower = 1f;
    [Range(1f, 30f), Tooltip("점프할때 추가 중력")]
    public float jumpGravity = 1f;

    [Range(1f, 30f), Tooltip("닷지 속도")]
    public float dodgingSpeed = 20f;

    [Range(-9.81f, 0f), Tooltip("경사로 이동속도 변화율(가속/감속)")]
    public float slopeAccel = 1f;

    [Range(-9.81f, 0f), Tooltip("중력")]
    public float gravity = -9.81f;
}

[Serializable]
public class CurrentState
{
    public bool isNotMoving;
    public bool isWalking;  //걷기
    public bool isRunning;  //뛰기
    public bool isSprinting; //전력 질주
    public bool isStrafing; //주목, 현재 카메라가 바라보고 있는 방향을 주목하면서 이동
    public bool isJumping;  //점프
    public bool isDodgeing;  //닷지
    public bool previousDodgeKeyPress;   //이전 프레임에서 대시 키 여부
    public bool currentDodgeKeyPress;    //현재 프레임에서 대시 키 여부
    public bool isStartComboAttack;
    public bool isSkill;
    public bool hadAttack;

    [Space]
    public bool isPerformingAction; //액션을 수행 중인지 여부

    [Space]
    public bool isForwardBlocked;   //앞에 장애물이 있는지 여부
    public bool isGround;           //Player가 지면에 닿아있는 상태인지.
    public bool isOnSteepSlop;      //가파른 경사 있음!

    [Space]
    public bool isGettingHit; //몬스터에게 맞았을 경우.
}

[Serializable]
public class CurrentValue
{
    public float moveAmount;        // 움직임. (0 움직이지않음, 1 움직임)
    public Vector3 moveDirection;   //이동 방향
    public Vector3 groundNormal;    //지면의 방향 벡터
    public Vector3 groundCross;     //지면의 외적 (캐릭터 이동벡터 회전축)
    public Vector3 playerVelocity;  //이동을 위한 플레이어 속도
    public int comboCount;          // 현재 콤보 카운트
    public double HP = 100;               //플레이어 체력
    public int index;
    public float time;
    public bool isCombo;
    public string curAnimName = "";
    public int hits = 0;
    public float curHitTime = 0;

    [Space]
    public float groundDistance;    //플레이어와 땅의 거리
    public float hitDistance;       //플레이어 충돌체크 결과 거리
    public float groundSlopeAngle;  //현재 바닥의 경사각
    public float forwardSlopeAngle; //캐릭터가 바라보는 방향의 경사각
    public float slopeAccel;        // 경사로 인한 가속/감속 비율
    public float comboResetTime = 1f;   // 콤보가 리셋되기까지의 시간 (초)
    public float lastClickTime = 0f;    // 마지막 클릭 시간


    [Space]
    public float gravity = 0f; // 직접 제어하는 중력값
}

[Serializable]
public class PlayerFollowCamera
{
    [Header("Object")]
    public GameObject playerCamera;      //카메라 오브젝트
    public GameObject playerCameraPivot; //카메라 피봇
    public Camera cameraObj;             //카메라.
}