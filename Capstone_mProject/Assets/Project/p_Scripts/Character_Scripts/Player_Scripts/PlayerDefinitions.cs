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
}

[Serializable]
public class PlayerInput
{
    public float verticalMovement;   //상하
    public float horizontalMovement; //좌우
    public float mouseY;             //마우스 상하
    public float mouseX;             //마우스 좌우
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
    public float sprintSpeed = 9f;

    [Range(1f, 30f), Tooltip("달리는 속도")]
    public float runningSpeed = 6f;

    [Range(1f, 30f), Tooltip("걷는 속도")]
    public float walkingSpeed = 2f;

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

    [Space]
    public bool isPerformingAction; //액션을 수행 중인지 여부

    [Space]
    public bool isForwardBlocked;   //앞에 장애물이 있는지 여부
    public bool isGround;           //Player가 지면에 닿아있는 상태인지.
    public bool isOnSteepSlop;      //가파른 경사 있음!

}

[Serializable]
public class CurrentValue
{
    public float moveAmount;        // 움직임. (0 움직이지않음, 1 움직임)
    public Vector3 moveDirection;   //이동 방향
    public Vector3 groundNormal;    //지면의 방향 벡터
    public Vector3 groundCross;     //지면의 외적 (캐릭터 이동벡터 회전축)
    public Vector3 playerVelocity;  //이동을 위한 플레이어 속도

    [Space]
    public float groundDistance;    //플레이어와 땅의 거리
    public float groundSlopeAngle;  //현재 바닥의 경사각
    public float forwardSlopeAngle; //캐릭터가 바라보는 방향의 경사각
    public float slopeAccel;        // 경사로 인한 가속/감속 비율

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

    [Header("Value")]
    public float left_right_LookSpeed = 500; //왼 오 돌리는 스피드
    public float up_down_LookSpeed = 500;    //위아래로 돌리는 스피드
    public float minPivot = -35;              //위아래 고정 시키기 위한 Pivot -35로 아래 고정
    public float maxPivot = 35;               //35로 위 고정

    [Header("Camera Debug")]
    //카메라가 캐릭터를 쫒아가는 데 속력. zero로 초기화
    public Vector3 cameraFllowVelocity = Vector3.zero;
    public float left_right_LookAngle;
    public float up_down_LookAngle;
}