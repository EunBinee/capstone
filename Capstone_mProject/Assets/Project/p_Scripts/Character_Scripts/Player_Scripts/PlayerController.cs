using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
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
    [SerializeField] public PlayerComponents _playerComponents = new PlayerComponents();
    public PlayerInput _input = new PlayerInput();
    [SerializeField] public CheckOption _checkOption = new CheckOption();
    public CurrentState _currentState = new CurrentState();
    [SerializeField] public CurrentValue _currentValue = new CurrentValue();
    [SerializeField] public PlayerFollowCamera _playerFollowCamera = new PlayerFollowCamera();
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

    public NavMeshSurface navMeshSurface;

    private bool isGettingHit = false;

    public Action OnHitPlayerEffect = null;

    public PlayerState curPlayerState;


    private Monster curEnemy; //*현재 플레이어를 공격한 몬스터

    public TMP_Text hitNum;
    public Slider HPgauge;
    float nowHitTime;

    void Awake()
    {
        P_Com.animator = GetComponent<Animator>();
        P_Com.rigidbody = GetComponent<Rigidbody>();
        InitPlayer();

        Cursor.visible = false;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정

        P_Value.HP = P_Value.MaxHP;
    }
    // Update is called once per frame
    void Update()
    {
        hitNum.text = P_Value.hits.ToString();
    }
    void FixedUpdate()
    {
        if (!UIManager.gameIsPaused)
        {
            HPgauge.gameObject.SetActive(true);
            _fixedDeltaTime = Time.fixedDeltaTime;
            Update_Physics();
            //전방 지면 체크
            CheckedForward();
            CheckedGround();
            CheckHitTime();
            CheckAnim();
            CheckHP();
        }
        else
        {
            Debug.Log("HPgauge = false");
            HPgauge.gameObject.SetActive(false);
        }
    }

    private void InitPlayer()
    {
        if (P_Com.playerTargetPos == null)
            P_Com.playerTargetPos = GameManager.Instance.gameData.playerTargetPos;
        InitCapsuleCollider();

        navMeshSurface.BuildNavMesh();
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
        Debug.Log(GameManager.Instance.dialogueManager.isDialogue);
        if (GameManager.Instance.dialogueManager.isDialogue)
        {
            P_States.isStop = true;
            Debug.Log("트루");
        }
        else
        {
            P_States.isStop = false;
            Debug.Log("폴스");
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

    public void ChangePlayerState(PlayerState playerState)
    {
        curPlayerState = playerState;

    }

    public void AnimState(PlayerState playerState, int index = 0)
    {
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
                    StartCoroutine(GetHit_KnockBack_co());
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

    // UI 버튼에 의해 호출됩니다.
    // 인자로 넘어온 skill 정보에 따라 애니메이션을 플레이하고
    // damage 정보 만큼 피해를 입힙니다.
    public void ActivateSkill(SOSkill skill)
    {
        P_Com.animator.Play(skill.animationName);
        //print(string.Format("적에게 스킬 {0} 로 {1} 의 피해를 주었습니다.", skill.name, skill.damage));
    }

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

    void CheckedForward()
    {
        //캐릭터가 이동하는 방향으로 막힘 길이 있는가?
        // 함수 파라미터 : Capsule의 시작점, Capsule의 끝점,
        // Capsule의 크기(x, z 중 가장 큰 값이 크기가 됨), Ray의 방향,
        // RaycastHit 결과, Capsule의 회전값, CapsuleCast를 진행할 거리
        bool cast = Physics.CapsuleCast(CapsuleBottomCenterPoint, CapsuleTopCenterPoint,
        _castRadius, P_Value.moveDirection + Vector3.down * 0.25f,
        out var hit, P_COption.forwardCheckDistance, -1, QueryTriggerInteraction.Ignore);
        // QueryTriggerInteraction.Ignore 란? 트리거콜라이더의 충돌은 무시한다는 뜻
        P_Value.hitDistance = hit.distance;
        P_States.isForwardBlocked = false;
        if (cast)
        {
            float forwardObstacleAngle = Vector3.Angle(hit.normal, Vector3.up);
            P_States.isForwardBlocked = forwardObstacleAngle >= P_COption.maxSlopAngle;
            //if (P_States.isForwardBlocked)
            //Debug.Log("앞에 장애물있음!" + forwardObstacleAngle + "도");
            //Debug.Log("P_Value.hitDistance : " + P_Value.hitDistance);
        }
    }

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


    public void GetHit(Monster enemy)
    {

        StartCoroutine(PlayerGetHit(enemy, 2f));

    }


    IEnumerator PlayerGetHit(Monster enemy, float Damage)
    {
        P_States.isGettingHit = true;
        //임시로 시간지나면 isGettingHit false로 만들어줌
        //나중에 연출 변경 바람.

        GameManager.Instance.cameraShake.ShakeCamera(0.2f, 2, 1);

        curEnemy = enemy;

        P_Value.HP -= Damage;
        //플레이어의 반대 방향으로 넉백

        if (P_Value.HP <= 0)
        {
            //죽음
            Death();
        }
        else
        {
            //아직 살아있음.
            //P_Com.animator.SetTrigger("isGetDamage");
            P_Com.animator.Play("Get_Damage", 0);

            AnimState(PlayerState.GetHit_KnockBack);
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
    }

    IEnumerator GetHit_KnockBack_co() //넉백만을 수행
    {
        PlayerState preState = curPlayerState;
        //ChangePlayerState(PlayerState.GetHit);

        Vector3 knockback_Dir = transform.position - curEnemy.transform.position;

        OnHitPlayerEffect?.Invoke();
        if (OnHitPlayerEffect == null)
        {

            //transform.position = Vector3.Lerp(transform.position, KnockBackPos, 5 * Time.deltaTime);


            //null일시 기본 이펙트.
            playerGetHitEffect();
        }

        knockback_Dir = knockback_Dir.normalized;
        Vector3 KnockBackPos = transform.position + knockback_Dir * 1.5f; // 넉백 시 이동할 위치
        KnockBackPos.y = 0;

        transform.position = Vector3.Lerp(transform.position, KnockBackPos, 5 * Time.deltaTime);

        yield return null;

        ChangePlayerState(preState);

        isGettingHit = false;
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

    //-----------------------------------------------------------------//
    //카메라 움직임

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Npc") //플레이어가 들어가면 대화창 활성화
        {
            //Debug.Log("엔피시 대화 에리어");
            GameObject interObject = other.gameObject;

            if (interObject != null)
            {
                //오브젝트가 비어있지 않을 때..
                //GameManager.GetInstance().StartInteraction(interObject);


                P_Com.animator.Rebind();
                GameManager.GetInstance().dialogueInfo.StartInteraction(interObject);
                //StopToFalse(true);
            }
        }
    }
    //*-------------------------------------------------------------------//

}
