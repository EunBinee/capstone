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
using Michsky.UI.Reach;

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
    public KeyState _keyState = new KeyState();
    public CurrentValue _currentValue = new CurrentValue();
    public PlayerFollowCamera _playerFollowCamera = new PlayerFollowCamera();
    public PlayerArrows _playerArrows = new PlayerArrows();
    public SkillInfo _skillInfo = new SkillInfo();
    private PlayerComponents P_Com => _playerComponents;
    private PlayerInput P_Input => _input;
    private CheckOption P_COption => _checkOption;
    private CurrentState P_States => _currentState;
    private KeyState P_KeyState => _keyState;
    private CurrentValue P_Value => _currentValue;
    private PlayerFollowCamera P_Camera => _playerFollowCamera;
    private PlayerArrows P_Arrows => _playerArrows;
    private SkillInfo P_SkillInfo => _skillInfo;
    public PlayerInputHandle P_InputHandle;
    public PlayerMovement P_Movement;
    public PlayerPhysicsCheck P_PhysicsCheck;
    public PlayerSkills P_Skills;

    public List<NavMeshSurface> navMeshSurface;

    public bool isGettingHit = false;
    public Action OnHitPlayerEffect = null;

    public PlayerState curPlayerState;

    private Monster curEnemy; //*현재 플레이어를 공격한 몬스터

    public TMP_Text hitNum;
    public GameObject hitUI;
    public GameObject hitUiGuide;
    public GameObject portrait;
    public GameObject chargingImg;
    public Slider HPgauge;
    float nowHitTime;
    public List<GameObject> hitMonsters;
    //public List<Collider> forwardHit;
    private float hitStop = 0f;

    public GameObject bow;
    public GameObject sword;
    public TMP_Text crosshairImage; // 조준점 이미지

    public GameObject arrow;
    public Transform shootPoint; // 화살이 발사될 위치를 나타내는 트랜스폼
    public Transform spine;     // 아바타 모델링

    public Vector3 originTpos;
    public Vector3 originEpos;
    public Vector3 originRpos;
    public Vector3 originFpos;
    public Vector3 originQpos;

    //private Vector3 screenCenter;
    public bool EnablePlayerUI = true;
    private bool isOn = false;
    public bool retIsOn()
    {
        return isOn;
    }
    public void setIsOn(bool val)
    {
        isOn = val;
    }
    void Awake()
    {
        P_Com.animator = GetComponent<Animator>();
        P_Com.rigidbody = GetComponent<Rigidbody>();
        P_InputHandle = GetComponent<PlayerInputHandle>();
        P_Movement = GetComponent<PlayerMovement>();
        P_Skills = GetComponent<PlayerSkills>();
        P_PhysicsCheck = GetComponent<PlayerPhysicsCheck>();
        InitPlayer();


        Cursor.visible = false;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정

        P_Value.HP = P_Value.MaxHP;

        bow.SetActive(false);
        sword.SetActive(true);

        //* 씬이동 처리
    }

    void Start()
    {
        //screenCenter = new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);
        if (GameManager.instance.gameData.player == null || GameManager.instance.gameData.player == this.gameObject)
            DontDestroyOnLoad(this.gameObject);
        else
        {
            Destroy(this.gameObject);
        }
        SetUIVariable();
        _playerArrows.Init();
        // InitComponent();
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
        hitNum = playerUI_info.hitNum;
        hitUI = playerUI_info.hitUI;
        hitUiGuide = playerUI_info.hitUiGuide;
        portrait = playerUI_info.portrait;
        chargingImg = playerUI_info.chargingImg;
        HPgauge = playerUI_info.HPgauge;
        crosshairImage = playerUI_info.crosshairImage;
    }

    void Update()
    {
        hitNum.text = P_Value.hits.ToString();

        //* 정지 상태
        if (UIManager.gameIsPaused == true && EnablePlayerUI)
        {
            PlayerUI_SetActive(false);
        }
        else if (UIManager.gameIsPaused == false && !EnablePlayerUI)
        {
            if (!GameManager.instance.curCutScene_ing)
                PlayerUI_SetActive(true);
        }

        CheckHitTime();
        CheckAnim();
        CheckHP();
    }

    //* 플레이어 UI 활성화 비활성화
    public void PlayerUI_SetActive(bool activeSelf)
    {
        if (!activeSelf)
        {
            Debug.Log("플레이어 UI 비활성화");
            P_Movement.skill_T.gameObject.transform.position = new Vector3(1000, -1000, 0);
            P_Movement.skill_E.gameObject.transform.position = new Vector3(1000, -1000, 0);
            P_Movement.skill_R.gameObject.transform.position = new Vector3(1000, -1000, 0);
            P_Movement.skill_F.gameObject.transform.position = new Vector3(1000, -1000, 0);
            P_Movement.skill_Q.gameObject.transform.position = new Vector3(1000, -1000, 0);
            //Debug.Log("HPgauge = false");
            if (P_States.isBowMode && P_States.startAim)
                P_Skills.arrowSkillOff();
            HPgauge.gameObject.SetActive(false);
            hitUI.SetActive(false);
            hitUiGuide.SetActive(false);
            portrait.SetActive(false);
            hitNum.gameObject.SetActive(false);
            chargingImg.SetActive(false);
            EnablePlayerUI = false;
        }
        else if (activeSelf)
        {
            Debug.Log("플레이어 UI 활성화");
            HPgauge.gameObject.SetActive(true);
            hitUI.SetActive(true);
            hitUiGuide.SetActive(true);
            portrait.SetActive(true);
            hitNum.gameObject.SetActive(true);
            chargingImg.SetActive(true);
            P_Movement.skill_T.gameObject.transform.position = originTpos;
            P_Movement.skill_E.gameObject.transform.position = originEpos;
            P_Movement.skill_R.gameObject.transform.position = originRpos;
            P_Movement.skill_F.gameObject.transform.position = originFpos;
            P_Movement.skill_Q.gameObject.transform.position = originQpos;
            EnablePlayerUI = true;


        }
    }

    public void LateUpdate()
    {
        if (P_States.isAim)
            Operation_boneRotation();   // 모델링 변환


    }
    Vector3 ChestOffset = new Vector3(0, 180, 0);

    Vector3 ChestDir = new Vector3();

    Quaternion lastRotation; // 마지막 회전값을 저장할 변수
    //float rotationSpeed = 2.0f; // 회전 속도를 조절하는 변수
    void Operation_boneRotation()
    {
        //Transform camTrans = Camera.main.transform;
        RaycastHit hit;
        // 화면 중앙에서 레이를 생성합니다.
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            //Debug.DrawRay(ray.origin, ray.direction * 20, Color.yellow, 5f);

            //Debug.Log(hit.point);
        }

        // 레이의 방향으로 대상을 회전시킵니다.
        // 레이의 방향은 ray.direction에 저장되어 있습니다.
        //RotateTowardsRayDirection(ray.direction);

        //Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        //Debug.DrawRay(P_Camera.cameraObj.transform.position, P_Camera.cameraObj.transform.forward * 10f, Color.red);
        //Debug.DrawRay();
        ChestDir = this.transform.position + ray.direction * 30;
        //카메라가 보고있는 방향 '벡터=목적지-출발지'
        //ChestDir = P_Camera.cameraObj.transform.position + P_Camera.cameraObj.transform.forward * 50f;
        //ChestDir = hit.point - this.transform.position;

        spine.LookAt(ChestDir); //상체를 카메라 보는방향으로 보기
        spine.rotation = spine.rotation * Quaternion.Euler(ChestOffset); // 상체가 꺽여 잇어 상체로테이션을 보정하기 
        lastRotation = spine.rotation;
    }

    public bool returnIsAim()
    {
        return P_States.isAim;
    }
    public bool returnIsBowMode()
    {
        return P_States.isBowMode;
    }

    private void InitPlayer()
    {
        if (P_Com.playerTargetPos == null)
            P_Com.playerTargetPos = GameManager.Instance.gameData.playerTargetPos;
        P_PhysicsCheck.InitCapsuleCollider();

        NavMeshSurface_ReBuild();

        spine = P_Com.animator.GetBoneTransform(HumanBodyBones.UpperChest); // 값 가져오기 

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

    public void PlayerStop(bool isStop)
    {
        P_States.isStop = isStop;
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
                AnimState(PlayerState.Idle);
                break;
            case PlayerState.GetHit_KnockBack:
                if (!isGettingHit)
                {
                    isGettingHit = true;
                    if (P_States.isBowMode && P_States.startAim)
                        P_Skills.arrowSkillOff();
                    StartCoroutine(GetHit_KnockBack_co(knockbackDistance));
                }
                break;

        }
    }

    public void CheckAnim()
    {
        if (P_Com.animator.GetCurrentAnimatorStateInfo(0).IsName("Get_Damage")
            && P_Com.animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f)
        {
            P_Com.animator.Rebind();
        }
    }

    public void CheckHP()
    {
        if (HPgauge != null)
        {
            ProgressBar progressBar = HPgauge.GetComponent<ProgressBar>();
            //progressBar.UpdateUI();
            //progressBar.SetBarDirection();
            //HPgauge.value = P_Value.HP / P_Value.MaxHP;
            progressBar.currentValue = P_Value.HP; /// P_Value.MaxHP;
            progressBar.UpdateUI();
            //Debug.Log(progressBar.currentValue);
        }
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
        fallDownDistance = Mathf.Clamp(fallDownDistance, 5f, fallDownDistance);

        StartCoroutine(PlayerGetHit(enemy, damage, fallDownDistance));
    }

    IEnumerator PlayerGetHit(Monster enemy, float damage, float knockbackDistance = 1.5f)
    {
        if (!P_States.isGettingHit)
            P_States.isGettingHit = true;
        //임시로 시간지나면 isGettingHit false로 만들어줌
        //나중에 연출 변경 바람.

        GameManager.Instance.cameraController.cameraShake.ShakeCamera(0.2f, 2, 1);

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
            if (P_States.isBowMode && P_States.startAim)    //* 조준 모드면 피격 시 조준 해제
            {
                P_Com.animator.SetTrigger("shoot");
                P_Skills.arrowSkillOff();
            }

            //* 데미지가 크면 넘어지고 데미지가 작으면 안넘어짐.

            AnimState(PlayerState.GetHit_KnockBack, 0, knockbackDistance);
        }

        //HP같은 플레이어 정보와 연출은 코루틴에서 변경하면 깔끔할것같음
        yield return new WaitForSeconds(hitStop);
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
        Debug.Log("플레이어 사망");
        UIManager.Instance.PadeInBlack(1);

        UIManager.instance.PlayerDie();
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

        if (P_States.isBackwardBlocked)     //* 뒤가 막혀있다면
        {
            knockbackDistance = 0;
        }
        knockback_Dir = knockback_Dir.normalized;
        Vector3 KnockBackPos = transform.position + knockback_Dir * knockbackDistance; // 넉백 시 이동할 위치
        KnockBackPos.y = 0;

        if (knockbackDistance > 1.5f)
        {
            hitStop = 1.4f;
            P_Com.animator.SetTrigger("isKnockback");
        }
        else
        {
            hitStop = 0.367f;
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
    public Action TriggerMonsterCheck = null; //* 몬스터 쪽에서 플레이어 트리거 쓸 일있을때 사용할거임

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
                DialogueManager.instance.dialogueInfo.StartInteraction(interObject);
                if (!DialogueManager.instance.DoQuest)
                    interObject.SetActive(false);
                //StopToFalse(true);
            }
        }
        if (other.gameObject.tag == "LoadScene" && !DialogueManager.instance.DoQuest)
        {
            LoadSceneObj_info loadSceneObj_info = other.gameObject.GetComponent<LoadSceneObj_info>();

            if (loadSceneObj_info != null)
            {
                loadSceneObj_info.PreLoadSceneSetting();
                LoadingSceneController.LoadScene(loadSceneObj_info.sceneName);
            }
        }

        TriggerMonsterCheck?.Invoke();
    }


}
