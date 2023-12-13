using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum PlayerState
{
    Idle,
    Move,
    Jump,
    Dodge,
    ComboAttack,
    FinishComboAttack,
    GetHit_KnockBack,
    GetHit,
    Death
}

public class PlayerController : MonoBehaviour
{
    public PlayerComponents _playerComponents = new PlayerComponents();
    public PlayerInput _input = new PlayerInput();
    public CheckOption _checkOption = new CheckOption();
    public CurrentState _currentState = new CurrentState();
    public CurrentValue _currentValue = new CurrentValue();
    public PlayerFollowCamera _playerFollowCamera = new PlayerFollowCamera();
    //public PlayerSkills _playerSkills = new PlayerSkills();
    private PlayerComponents P_Com => _playerComponents;
    private PlayerInput P_Input => _input;
    private CheckOption P_COption => _checkOption;
    private CurrentState P_States => _currentState;
    private CurrentValue P_Value => _currentValue;
    private PlayerFollowCamera P_Camera => _playerFollowCamera;
    //private PlayerSkills P_Skills => _playerSkills;
    private CameraController P_CamController;
    public PlayerMovement P_Movement;

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

    public List<NavMeshSurface> navMeshSurface;

    private bool isGettingHit = false;
    public Action OnHitPlayerEffect = null;

    public PlayerState curPlayerState;

    private Monster curEnemy; //*현재 플레이어를 공격한 몬스터

    public TMP_Text hitNum;
    public GameObject hitUI;
    public Slider HPgauge;
    float nowHitTime;

    //private Vector3 originCamPos;
    //private Quaternion originCamQua;
    public Camera mainCam;
    public Camera AimmingCam;
    public CameraController AimmingCamCon;

    public GameObject bow;
    public GameObject sword;
    public TMP_Text crosshairImage; // 조준점 이미지

    GameObject arrow;
    public Transform shootPoint; // 화살이 발사될 위치를 나타내는 트랜스폼

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);

        P_Com.animator = GetComponent<Animator>();
        P_Com.rigidbody = GetComponent<Rigidbody>();
        P_CamController = P_Camera.cameraObj.GetComponent<CameraController>();
        AimmingCamCon = AimmingCam.GetComponent<CameraController>();
        P_Movement = GetComponent<PlayerMovement>();
        InitPlayer();


        Cursor.visible = false;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정

        P_Value.HP = P_Value.MaxHP;

        bow.SetActive(false);
        sword.SetActive(true);
        P_Movement.skill_V.gameObject.SetActive(true);
        //AimOnCameraReturn();
        //P_Camera.cameraObj = mainCam;
        AimmingCam.enabled = false;
        P_Camera.cameraObj.enabled = true;
        //playerFollowCamera.enabled = true;
        //onAimCamera.enabled = false;
        //mainCam.enabled = true;

        //* 씬이동 처리

    }
    void Start()
    {
        InitComponent();
    }
    // Update is called once per frame
    void Update()
    {
        hitNum.text = P_Value.hits.ToString();
    }
    void FixedUpdate()
    {
        if (UIManager.gameIsPaused == true)
        {
            //Debug.Log("HPgauge = false");
            HPgauge.gameObject.SetActive(false);
            hitUI.SetActive(false);
            hitNum.gameObject.SetActive(false);
            //P_Movement.skill_E.gameObject.SetActive(false);
            P_Movement.skill_V.gameObject.SetActive(false);
        }
        else if (UIManager.gameIsPaused == false)
        {
            HPgauge.gameObject.SetActive(true);
            hitUI.SetActive(true);
            hitNum.gameObject.SetActive(true);
            //P_Movement.skill_E.gameObject.SetActive(true);
            P_Movement.skill_V.gameObject.SetActive(true);
            _fixedDeltaTime = Time.fixedDeltaTime;
            Update_Physics();
            //전방 지면 체크
            //Debug.Log("전방 지면 체크");
            CheckedForward();
            CheckedGround();
            CheckHitTime();
            CheckAnim();
            CheckHP();
        }
    }

    public bool returnIsAim()
    {
        return P_States.isAim;
    }

    private void InitPlayer()
    {
        if (P_Com.playerTargetPos == null)
            P_Com.playerTargetPos = GameManager.Instance.gameData.playerTargetPos;
        InitCapsuleCollider();

        NavMeshSurface_ReBuild();

        //_playerSkills.Init();
    }
    private void InitComponent()
    {
        P_Camera.playerCamera = GameManager.instance.gameData.playerCamera;
        P_Camera.playerCameraPivot = GameManager.instance.gameData.playerCameraPivot;
        P_Camera.cameraObj = GameManager.instance.gameData.cameraObj;
    }

    public void NavMeshSurface_ReBuild()
    {


        if (navMeshSurface != null)
        {
            for (int i = 0; i < navMeshSurface.Count; ++i)
                navMeshSurface[i].BuildNavMesh();
        }
    }

    void InitCapsuleCollider()
    {
        P_Com.capsuleCollider = GetComponent<CapsuleCollider>();
        _castRadius = P_Com.capsuleCollider.radius * 0.9f;
        _castRadiusDiff = P_Com.capsuleCollider.radius - _castRadius + 0.05f;
        //그냥 캡슐 콜라이더 radius와 castRadius의 차이
    }

    public void StopToFalse()
    {
        if (GameManager.Instance.dialogueManager.isDialogue)
        {

            P_States.isStop = true;
        }
        else
        {
            P_States.isStop = false;
        }
        //stop = GameManager.Instance.dialogueManager.isDialogue;
        //P_States.isStop = stop;
        //Debug.Log(P_States.isStop);
    }

    public void CheckHitTime()
    {
        float deltaHitTime = Time.time - P_Value.curHitTime;
        if (deltaHitTime > 5.0f) //5초 지나면
        {
            //Debug.Log("hits 초기화");
            P_Value.hits = 0;   //히트수 초기화
            P_Value.nowEnemy = null;
        }

        hitNum.rectTransform.localScale = Vector3.one *
            (P_States.isBouncing ? (P_Value.minHitScale++ * 0.05f)
                : (nowHitTime == P_Value.curHitTime ? 1f
                    : P_Value.maxHitScale-- * 0.1f));
        nowHitTime = P_Value.curHitTime;
    }

    public void AnimState(PlayerState playerState, int index = 0, float knockbackDistance = 1.5f)
    {
        curPlayerState = playerState;
        switch (playerState)
        {
            case PlayerState.Idle:
                break;
            case PlayerState.Move:
                break;
            case PlayerState.ComboAttack:
                P_Com.animator.SetInteger("comboCount", index);
                P_Com.animator.SetBool("p_Locomotion", false);
                break;
            case PlayerState.FinishComboAttack:
                P_Com.animator.SetInteger("comboCount", index);
                P_Com.animator.SetBool("p_Locomotion", true);
                break;
            case PlayerState.GetHit_KnockBack:
                if (!isGettingHit)
                {
                    isGettingHit = true;
                    StartCoroutine(GetHit_KnockBack_co(knockbackDistance));
                }
                break;

        }
    }

    public void CheckAnim()
    {
        if (P_Com.animator.GetCurrentAnimatorStateInfo(0).IsName("Get_Damage")
            && P_Com.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            P_Com.animator.Rebind();
        }
    }

    public void CheckHP()
    {
        if (HPgauge != null)
            HPgauge.value = P_Value.HP / P_Value.MaxHP;
    }

    //* skill
    // UI 버튼에 의해 호출됩니다.
    // 인자로 넘어온 skill 정보에 따라 애니메이션을 플레이하고
    // damage 정보 만큼 피해를 입힙니다.
    public void ActivateSkill(SOSkill skill)
    {
        if (skill.isTwice && !P_States.isAim)
        {
            P_States.isAim = true;
            P_Com.animator.SetBool("isAim", true);  //* 애니메이션
            AimOnCamera();  //* 카메라
            bow.SetActive(true);    //* 무기 교체
            sword.SetActive(false);
            crosshairImage.gameObject.SetActive(true);  //* 조준점
            PoolingArrow(); //* 화살 풀링
        }
        else if (skill.isTwice && P_States.isAim)
        {
            P_Com.animator.SetBool("isAim", false);
            P_Com.animator.SetTrigger("shoot");
            P_States.isAim = false;
            skill.isFirsttime = true;
            AimOnCameraReturn();
            bow.SetActive(false);
            sword.SetActive(true);
            crosshairImage.gameObject.SetActive(false);
            P_States.isSkill = false;
        }
        else if (!skill.isTwice)
        {
            P_Com.animator.Play(skill.animationName);
            P_States.isSkill = false;
        }
    }

    void PoolingArrow()
    {
        // 화살을 발사할 위치에 화살을 생성하고 방향을 설정
        //arrow = P_Skills.GetArrowFromPool();
        if (arrow == null) Debug.LogError("arrow null!");
        //arrow.SetActive(true);
    }

    //* camera controll
    public void AimOnCamera()
    {
        //todo: 조준 스킬 시 카메라 이동(시네머신이든 그냥 이동이든)
        //Debug.Log("AimOnCamera()");
        P_CamController.left_right_LookAngle = 0;
        P_CamController.up_down_LookAngle = 0;

        //* 카메라 전환
        AimmingCam.enabled = true;
        P_Camera.cameraObj.enabled = false;
        P_Camera.cameraObj = AimmingCam;

        //* 카메라 주목 기능 밴 처리 맟 주목 해제
        if (AimmingCamCon.isBeingAttention)
        {
            AimmingCamCon.isBeingAttention = false;
        }
        AimmingCamCon.banAttention = true;

        //* 카메라 회전 높낮이 제한 설정
        AimmingCamCon.minPivot = -45;
        AimmingCamCon.maxPivot = 45;
    }

    public void AimOnCameraReturn()
    {
        //todo: 카메라 원래대로
        //Debug.Log("CameraReturn()");
        AimmingCamCon.left_right_LookAngle = 0;
        AimmingCamCon.up_down_LookAngle = 0;

        AimmingCamCon.minPivot = 0;
        AimmingCamCon.maxPivot = 0;
        AimmingCamCon.banAttention = false;
        if (GameManager.instance.monsterUnderAttackList.Count > 0)
        {
            MonsterData isBoss = GameManager.instance.monsterUnderAttackList[0].monsterData;
            if (isBoss.monsterType == MonsterData.MonsterType.BossMonster)
            {
                GameManager.instance.cameraController.AttentionMonster();
            }
        }

        P_Camera.cameraObj = mainCam;
        AimmingCam.enabled = false;
        P_Camera.cameraObj.enabled = true;
    }

    //* 물리(중력)
    private void Update_Physics()
    {
        if (P_States.isGround && !P_States.isJumping)
        {
            //지면에 잘 붙어있을 경우
            P_Value.gravity = 0f;
        }
        else if (!P_States.isGround && !P_States.isJumping)
        {
            P_Value.gravity += _fixedDeltaTime * P_COption.gravity;
        }
        else if (!P_States.isGround && P_States.isJumping)
        {
            //Debug.Log("Here");
            P_Value.gravity = P_COption.gravity * P_COption.jumpGravity;
        }

    }

    //* 전방체크
    public void CheckedForward()
    {
        //Debug.Log("CheckedForward()");
        //캐릭터가 이동하는 방향으로 막힘 길이 있는가?
        // 함수 파라미터 : Capsule의 시작점, Capsule의 끝점,
        // Capsule의 크기(x, z 중 가장 큰 값이 크기가 됨), Ray의 방향,
        // RaycastHit 결과, Capsule의 회전값, CapsuleCast를 진행할 거리
        /*bool cast = Physics.SphereCast(transform.position, 5f, transform.forward,
        out var hit, Mathf.Infinity, 0);*/

        bool cast = Physics.CapsuleCast(CapsuleBottomCenterPoint, CapsuleTopCenterPoint,
        _castRadius, P_Value.moveDirection + Vector3.down * 0.25f,
        out var hit, P_COption.forwardCheckDistance, -1, QueryTriggerInteraction.Ignore);

        //Debug.Log("cast : " + cast);
        // QueryTriggerInteraction.Ignore 란? 트리거콜라이더의 충돌은 무시한다는 뜻
        P_Value.hitDistance = hit.distance;
        P_States.isForwardBlocked = false;
        if (cast)
        {
            P_States.isForwardBlocked = true;
            //Debug.Log("if (cast)");
            float forwardObstacleAngle = Vector3.Angle(hit.normal, Vector3.up);
            P_States.isForwardBlocked = forwardObstacleAngle >= P_COption.maxSlopAngle;
            //if (P_States.isForwardBlocked)
            //Debug.Log("앞에 장애물있음!" + forwardObstacleAngle + "도");
            //Debug.Log("P_Value.hitDistance : " + P_Value.hitDistance);
        }
    }

    //*바닥 체크
    public void CheckedGround()
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

    //* 데미지 받는 코루틴 실행
    public void GetHit(Monster enemy, float damage)
    {
        //* 데미지에 따른 넉백 Distance 계산
        float distance = Calculate_KnockBackDistance(damage);

        StartCoroutine(PlayerGetHit(enemy, damage, distance));
    }

    //* 무조건 넘어지는 GetHit
    public void GetHit_FallDown(Monster enemy, float damage, float fallDownDistance = 5f)
    {
        if (fallDownDistance < 5f)
            fallDownDistance = 5f;

        StartCoroutine(PlayerGetHit(enemy, damage, fallDownDistance));
    }

    IEnumerator PlayerGetHit(Monster enemy, float damage, float knockbackDistance = 1.5f)
    {
        P_States.isGettingHit = true;
        //임시로 시간지나면 isGettingHit false로 만들어줌
        //나중에 연출 변경 바람.

        GameManager.Instance.cameraShake.ShakeCamera(0.2f, 2, 1);

        curEnemy = enemy;

        P_Value.HP -= damage;
        //플레이어의 반대 방향으로 넉백

        if (P_Value.HP <= 0)
        {
            //죽음
            Death();
        }
        else
        {
            //아직 살아있음.
            if (P_States.isAim)    //* 조준 모드면 피격 시 조준 해제
            {
                P_Com.animator.SetTrigger("shoot");
                P_Movement.skillMotion('E');
            }

            //* 데미지가 크면 넘어지고 데미지가 작으면 안넘어짐.

            AnimState(PlayerState.GetHit_KnockBack, 0, knockbackDistance);
        }

        //HP같은 플레이어 정보와 연출은 코루틴에서 변경하면 깔끔할것같음
        yield return new WaitForSeconds(1f);
        P_States.isGettingHit = false;

    }

    public Monster Get_CurHitEnemy()
    {
        return curEnemy;
    }

    public void Death()
    {
        //죽다.
        P_Value.HP = 0;
        AnimState(PlayerState.Death);
        Time.timeScale = 0f;
        Debug.Log("플레이어 사망");
    }

    IEnumerator GetHit_KnockBack_co(float knockbackDistance = 1.5f) //넉백만을 수행
    {
        //* ~ 1.5f : 안넘어지고 가벼운 피격 모션. 
        //*   1.6f ~ : 넘어지면서 뒹구는 큰 피격 모션.
        Vector3 knockback_Dir = transform.position - curEnemy.transform.position;

        OnHitPlayerEffect?.Invoke();
        if (OnHitPlayerEffect == null)
        {
            //transform.position = Vector3.Lerp(transform.position, KnockBackPos, 5 * Time.deltaTime);
            //null일시 기본 이펙트.
            playerGetHitEffect();
        }

        knockback_Dir = knockback_Dir.normalized;
        Vector3 KnockBackPos = transform.position + knockback_Dir * knockbackDistance; // 넉백 시 이동할 위치
        KnockBackPos.y = 0;

        if (knockbackDistance > 1.5f)
        {
            P_Com.animator.SetTrigger("isKnockback");
        }
        else
        {
            P_Com.animator.Play("Get_Damage", 0);
        }
        P_Value.hits = 0;   //* 피격 시 히트 초기화
        transform.position = Vector3.Lerp(transform.position, KnockBackPos, 5 * Time.deltaTime);

        isGettingHit = false;
        yield return null;
    }

    private void playerGetHitEffect()
    {
        //피격시 기본 이펙트
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Basic_Impact_01");

        effect.gameObject.transform.position = P_Com.playerTargetPos.position;
        Vector3 curDirection = P_Com.playerTargetPos.position - curEnemy.transform.position;
        effect.gameObject.transform.position += curDirection * 0.35f;
    }

    public void playAttackEffect(string name)
    {
        //* 이펙트
        Effect effect = GameManager.Instance.objectPooling.ShowEffect(name);
        effect.gameObject.transform.position = this.gameObject.transform.position + Vector3.up;
        //* 이펙트 회전
        Quaternion effectRotation = this.gameObject.transform.rotation;
        effectRotation.x = 0;
        effectRotation.z = 0;
        effect.gameObject.transform.rotation = effectRotation;
    }

    //*-------------------------------------------------------------------//
    //* 데미지에 따른 넉백 Distance 계산

    private float Calculate_KnockBackDistance(float playerDamage)
    {
        float distance = 1.5f;
        if (playerDamage > GameManager.instance.gameData.bigDamage) //임시 수치. 나중에 기획자가 변경할 수 있도록 수정.
        {
            distance = 10f;
        }
        else if (playerDamage > GameManager.instance.gameData.midDamage)
        {
            distance = 1.5f;
        }
        else
        {
            distance = 1.5f;
        }
        return distance;
    }

    //*-------------------------------------------------------------------//
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Npc") //플레이어가 들어가면 대화창 활성화
        {
            //Debug.Log("엔피시 대화 에리어");
            GameObject interObject = other.gameObject;

            if (interObject != null)
            {
                //오브젝트가 비어있지 않을 때..

                P_Com.animator.Rebind();
                GameManager.GetInstance().dialogueInfo.StartInteraction(interObject);
                if (!GameManager.Instance.dialogueManager.DoQuest)
                    interObject.SetActive(false);
                //StopToFalse(true);
            }
        }
        if (other.gameObject.tag == "LoadScene") //플레이어가 들어가면 대화창 활성화
        {
            //Debug.Log("엔피시 대화 에리어");
            GameObject interObject = other.gameObject;

            if (interObject != null)
            {
                UIManager.Instance.GoBossField(true);
            }
        }
    }


}
