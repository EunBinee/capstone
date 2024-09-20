using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JetBrains.Annotations;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

//Player캐릭터 정의의 모든 것

[Serializable]
public class PlayerComponents
{
    [Header("Player")]
    public Animator animator;
    public Rigidbody rigidbody;
    public CapsuleCollider capsuleCollider;
    public Transform playerTargetPos;
    public Transform playerHeadPos;
    public Transform playerBackPos;
    public Material sickScreen;
    public List<SkinnedMeshRenderer> skinnedMeshRenderers;
    public Material hitMat;
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
    public float runningSpeed = 8f;

    [Range(1f, 30f), Tooltip("감전/조준 속도")]
    public float slowlySpeed = 2f;
    [Range(1f, 30f), Tooltip("걷는 속도")]
    public float walkingSpeed = 5f;

    [Range(1f, 30f), Tooltip("점프할때 속도")]
    public float jumpPower = 1f;
    [Range(1f, 30f), Tooltip("점프할때 추가 중력")]
    public float jumpGravity = 1f;

    [Range(1f, 30f), Tooltip("닷지 속도")]
    public float dodgingSpeed = 15f;

    [Range(-9.81f, 0f), Tooltip("경사로 이동속도 변화율(가속/감속)")]
    public float slopeAccel = 1f;

    [Range(-9.81f, 0f), Tooltip("중력")]
    public float gravity = -9.81f;

    [Range(1f, 10f), Tooltip("감전 시간")]
    public float electricShock_Time = 5;
}

[Serializable]
public class CurrentState
{
    [Header("Player Moving")]
    public bool isNotMoving;
    public bool isWalking;  //걷기
    public bool isRunning;  //뛰기
    public bool isSprinting; //전력 질주
    public bool isStrafing; //주목, 현재 카메라가 바라보고 있는 방향을 주목하면서 이동
    public bool isJumping;  //점프
    public bool isDodgeing;  //닷지
    public bool isSkill;
    public bool doNotRotate;
    public bool isDie;

    [Header("Timing Check")]
    public bool isPerformingAction; //액션을 수행 중인지 여부
    public bool isStop; //대화창 활성화될때 움직임 비활성화여부
    [Space]
    public bool previousDodgeKeyPress;   //이전 프레임에서 대시 키 여부
    public bool currentDodgeKeyPress;    //현재 프레임에서 대시 키 여부
    [Space]
    public bool isStartComboAttack;
    public bool colliderHit;
    public bool hadAttack = false;
    public bool hasAttackSameMonster = false;
    public bool notSameMonster = false;
    public bool canGoForwardInAttack;
    public bool isGettingHit; //몬스터에게 맞았을 경우.
    public bool isElectricShock;
    public bool isBouncing;

    [Header("Aimming")]
    public bool isBowMode = false;  //활 모드
    public bool isGunMode = false;  //총 모드
    public bool isClickDown;
    public bool isAim;  //조준스킬
    public bool startAim;
    public bool isShortArrow;   //단타?
    public bool isStrongArrow;

    public bool isShoot;    // 총 발사?
    public bool onShootAim;
    public bool onZoomIn;   // 총일때 우클릭

    public bool isCamOnAim;
    public bool beenAttention;

    [Header("Physics Check")]
    public bool isForwardBlocked;   //앞에 장애물이 있는지 여부
    public bool isBackwardBlocked;   //앞에 장애물이 있는지 여부
    public bool isGround;           //Player가 지면에 닿아있는 상태인지.
    public bool isOnSteepSlop;      //가파른 경사 있음!
}

[Serializable]
public class KeyState
{
    public bool QDown;
    public bool WDown;
    public bool EDown;
    public bool RDown;
    public bool TDown;
    public bool YDown;
    public bool UDown;
    public bool IDown;
    public bool ODown;
    public bool PDown;

    [Space]
    public bool ADown;
    public bool SDown;
    public bool DDown;
    public bool FDown;
    public bool GDown;
    public bool HDown;
    public bool JDown;
    public bool KDown;
    public bool LDown;

    [Space]
    public bool ZDown;
    public bool XDown;
    public bool CDown;
    public bool VDown;
    public bool BDown;
    public bool NDown;
    public bool MDown;
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
    public float MaxHP = 100;               //플레이어 체력
    public float HP;
    public int index;
    public float time;
    public bool isCombo;
    public string curAnimName = "";
    public int hits = 0;
    public float curHitTime = 0;
    public float maxHitScale = 1.2f;
    public float minHitScale = 1f;
    public GameObject nowEnemy;
    public float finalSpeed;
    public float aimClickDown;

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
public class SkillInfo
{
    [Header("bool")]
    public bool haveBowmode;    // 활 모드 스킬 얻음?
    public bool haveHeal;       // 힐 스킬 얻음?
    public bool haveUltimate;   // 궁 스킬 얻음?
    public bool haveSample1;
    public bool haveSample2;
    public bool haveRestraint; //속박스킬

    [Space]
    [Header("skill")]
    public PlayerSkillName bowmode;
    public PlayerSkillName heal;
    public PlayerSkillName ultimate;
    public PlayerSkillName sample1;
    public PlayerSkillName sample2;
    public PlayerSkillName restraint; //속박스킬

    public List<PlayerSkillName> selectSkill;
}

[Serializable]
public class PlayerFollowCamera
{
    [Header("Object")]
    public GameObject playerCamera;      //카메라 오브젝트
    public GameObject playerCameraPivot; //카메라 피봇
    public Camera cameraObj;             //카메라.
}