using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
//using UnityEditor.AnimatedValues;
using System.Security.Claims;
using System;
using Unity.VisualScripting;
using Michsky.UI.Reach;


public class PlayerSkills : MonoBehaviour
{
    public PlayerController _controller;// = new PlayerController();
    private PlayerController P_Controller => _controller;
    private PlayerMovement P_Movement;
    private PlayerComponents P_Com => P_Controller._playerComponents;
    private CurrentState P_States => P_Controller._currentState;
    private CurrentValue P_Value => P_Controller._currentValue;
    private KeyState P_KState => P_Controller._keyState;
    private PlayerProjectile P_Projectile => P_Controller._playerProjectile;
    private SkillInfo P_SkillInfo => P_Controller._skillInfo;
    private PlayerAttackCheck playerAttackCheck;
    private PlayerInputHandle P_InputHandle;

    [SerializeField] private GameObject arrow;// => P_Controller.arrow;
    [SerializeField] private GameObject bullet;
    [HideInInspector] public Vector3 bulletDir;

    //private SkillButton skill_Q;
    private string Bow_Start_Name = "Bow_Attack_Charging";
    private string Bow_Name = "Bow_Attack_launch_02";
    private string Bow_StrongName = "ChargingArrowLaunch";
    private string Gun_ShootMuzzle = "PistolShoot";

    [SerializeField]
    public Dictionary<string, PlayerSkillName> skillMap;

    //public GameObject skillTreeWindow;
    // 스킬 맵 업데이트 시 발동할 이벤트
    public event Action OnSkillMapUpdated;
    //Effect effect; //스킬 이펙트
    Dictionary<MonsterPattern, Effect> monsterEffects = new Dictionary<MonsterPattern, Effect>(); // 몬스터와 이펙트의 매핑을 위한 딕셔너리

    //*스킬 속박 
    private float skillDuration = 5f; // 스킬의 지속 시간
    private bool isPressed = false;
    public GameObject skillRangeIndicator; // 원 범위를 나타낼 오브젝트
    public float cylinderRadius = 5f; // 스킬 속박범위 원기둥 반지름
    public float cylinderHeight = 5f; // 스킬 속박범위 원기둥 높이


    void Awake()
    {
        skillMap = new Dictionary<string, PlayerSkillName>();
        skillMap.Clear();
        P_SkillInfo.selectSkill = new List<PlayerSkillName>();
        P_SkillInfo.selectSkill.Clear();
        arrow = P_Controller.arrow;
        bullet = P_Controller.bullet;
        //skillRangeIndicator = UnityEngine.Object.Instantiate(skillRangeIndicator);
        //skillRangeIndicator = GameManager.Instance.objectPooling.GetProjectilePrefab("TargetMarker");
        //skillRangeIndicator = Resources.Load<GameObject>("TargetMarker");

        skillRangeIndicator.SetActive(false);
        //playerAttackCheck = arrow.GetComponent<PlayerAttackCheck>();
        P_Movement = GetComponent<PlayerMovement>();
    }
    void Start()
    {
        P_InputHandle = GetComponent<PlayerInputHandle>();

        //Invoke("Setting", 0.1f);
    }
    public void Setting()
    {
        //skillTreeWindow = P_Movement.skillTree;
        //skill_Q = P_Movement.skill_Q;
        SkillMapAdd("Bowmode", P_SkillInfo.bowmode);    // 기본지급 스킬
        P_SkillInfo.haveBowmode = true;
        SkillMapAdd("Heal", P_SkillInfo.heal);
        P_SkillInfo.haveHeal = true;
        SkillMapAdd("Restraint", P_SkillInfo.restraint);
        P_SkillInfo.haveRestraint = true;

        SkillMapAdd("Ultimate", P_SkillInfo.ultimate);  // 기본지급 스킬
        P_SkillInfo.haveUltimate = true;
        SkillMapAdd("Sample1", P_SkillInfo.sample1);
        P_SkillInfo.haveSample1 = true;
        SkillMapAdd("Sample2", P_SkillInfo.sample2);
        P_SkillInfo.haveSample2 = true;
        //skillTreeWindow = P_Controller.playerSkillTree.gameObject;

        //P_Controller.P_InputHandle.Setting();
    }

    void FixedUpdate()
    {
        if (P_Value.aimClickDown > 1.8f)
        {
            if (P_States.isStrongArrow == false && P_States.isBowMode)
                playerAttackCheck.StrongArrowEffect_co();
            P_States.isStrongArrow = true;
        }
        else
        {
            P_States.isStrongArrow = false;
        }
        SkillWindow(P_Controller.retIsOn());
    }

    private void OnTriggerStay(Collider other)
    {
        SkillSettingPC obj = other.GetComponent<SkillSettingPC>();
        if (obj != null)
        {
            SkillWindow(P_Controller.retIsOn());
        }
    }

    List<string> callName = new List<string>();
    public List<string> getskillMapToName()
    {
        callName.Clear();
        foreach (KeyValuePair<string, PlayerSkillName> i in skillMap)
        {
            if (i.Key == "Bowmode" || i.Key == "Ultimate")   // 무기변경스킬이나 궁 스킬 이라면 무시
            { }
            else
            {
                callName.Add(i.Key);
            }
        }
        return callName;
    }
    List<PlayerSkillName> callSkill = new List<PlayerSkillName>();
    public List<PlayerSkillName> getskillMapToSkill()
    {
        callSkill.Clear();
        foreach (KeyValuePair<string, PlayerSkillName> i in skillMap)
        {
            if (i.Key == "Bowmode" || i.Key == "Ultimate")   // 무기변경스킬이나 궁 스킬 이라면 무시
            { }
            else
            {
                callSkill.Add(i.Value);
            }
        }
        return callSkill;
    }

    //* skill
    public void SkillMapAdd(string name, PlayerSkillName skill)
    {
        if (skillMap.ContainsKey(name))
        {
            Debug.Log("이미 등록된 스킬 이름!");
            return;
        }
        else
        {
            skillMap.Add(name, skill);  // 스킬 등록
            // 스킬 맵이 업데이트되면 이벤트 발동
            OnSkillMapUpdated?.Invoke();
            if (P_SkillInfo.selectSkill.Count < 3)  // 플레이어가 고른 스킬 갯수 3개 미만?
            {
                if (name == "Bowmode" || name == "Ultimate")   // 무기변경스킬이나 궁 스킬 이라면 무시
                { }
                else
                {
                    //Debug.Log("[skill test] map add");
                    P_SkillInfo.selectSkill.Add(skill); // 자동 추가
                }
            }
        }
    }

    // 인자로 넘어온 skill 정보에 따라 애니메이션을 플레이
    public void ActivateSkill(SOSkill skill)
    {
        if (skill.animationName != "Skill_Heal")
        {
            P_Com.animator.Play(skill.animationName);
            P_States.isSkill = false;
        }
    }

    void PoolingArrow()
    {
        // 화살을 발사할 위치에 화살을 생성하고 방향을 설정
        arrow = P_Projectile.GetArrowPrefab();
        if (arrow == null) Debug.LogError("arrow null!");
        arrow.SetActive(true);

        playerAttackCheck = arrow.GetComponent<PlayerAttackCheck>();
    }
    void PoolingBullet()
    {
        bullet = P_Projectile.GetBulletPrefab();
        if (bullet == null) Debug.LogError("bullet null!");
        bullet.SetActive(true);

        playerAttackCheck = bullet.GetComponent<PlayerAttackCheck>();
    }

    public void GetBulletDir(Vector3 dir)
    {
        bulletDir = dir;
    }

    public void onArrow()
    {
        if (P_States.isBowMode)
        {
            if (!P_States.isAim || P_States.isShortArrow)
            {
                P_Com.animator.SetBool("isAim", true);  //* 애니메이션
                //P_Com.animator.SetLayerWeight(1, 1f);
                P_States.isAim = true;
                P_Controller.shootPoint.gameObject.SetActive(true);
                if (!P_States.isShortArrow)
                {
                    //* 조준 on effect
                    Effect Effect = GameManager.Instance.objectPooling.ShowEffect(Bow_Start_Name);
                    Effect.gameObject.transform.position = this.gameObject.transform.position + Vector3.up;
                    Effect.transform.rotation = Quaternion.LookRotation(this.transform.forward);

                    if (GameManager.instance.cameraController.isBeingAttention) // 주목 하고 있으면
                    {
                        //주목 풀기
                        GameManager.instance.cameraController.UndoAttention();
                        P_States.beenAttention = true;
                    }
                    GameManager.instance.cameraController.SetAimCamera();   //* 카메라 셋팅
                    P_Controller.crosshairImage.gameObject.SetActive(true);  //* 조준점
                }
                PoolingArrow(); //* 화살 풀링
                //* 단타 
                if (P_States.isShortArrow)
                {
                    arrowSkillOff();
                }
            }
        }
    }

    /// <summary>
    ///* 총 모드 on
    /// 카메라 aimCam
    /// </summary>
    public void onBulletCam()
    {
        P_Com.animator.SetBool("isAim", true);  //* 애니메이션
        P_Com.animator.SetLayerWeight(1, 1f);
        P_States.isAim = true;
        P_Controller.shootPoint.gameObject.SetActive(true);

        if (GameManager.instance.cameraController.isBeingAttention) // 주목 하고 있으면
        {
            //주목 풀기
            GameManager.instance.cameraController.UndoAttention();
            P_States.beenAttention = true;
        }
        GameManager.instance.cameraController.SetAimCamera();   //* 카메라 셋팅
        P_Controller.crosshairImage.gameObject.SetActive(true);  //* 조준점
    }
    public void onBullet()
    {
        if (P_States.isShoot)
        {
            PoolingBullet(); //* 총알 풀링
            bulletOff();
        }
    }

    public void bulletOff()
    {
        if (!P_States.onZoomIn) P_Com.animator.SetBool("onClickGun", false);
        P_States.isClickDown = false;
        P_Value.aimClickDown = 0;
        if (P_States.isGunMode)
        {
            Effect effect = GameManager.Instance.objectPooling.ShowEffect(Gun_ShootMuzzle);
            effect.gameObject.transform.position = this.gameObject.transform.position + Vector3.up;
            effect.transform.rotation = Quaternion.LookRotation(playerAttackCheck.transform.forward);
        }
        P_States.isShoot = false;
    }

/// <summary>
///* 닷지, 점프 할 때 카메라 복구 했다가 다시 켜주는 함수
/// </summary>
    public void bulletOnOff(bool value)
    {
        if (!P_States.isGunMode) return;

        if (value == true)  // 시작할때 카메라 원래대로
        {
            if (P_States.onShootAim) bulletOff();   // 시작할 때 조준중이면 조준 강종
            arrowSkillOff();
        }
        else if (value == false)    // 끝날때 카메라 다시 조준
        {
            onBulletCam();
        }
    }

    public void arrowSkillOff()
    {
        //* 발사 
        //P_States.isOnAim = false;
        P_States.startAim = false;
        P_States.isCamOnAim = false;
        P_States.isClickDown = false;
        P_Value.aimClickDown = 0;

        if (P_States.isShortArrow)
        {

        }
        else
        {
            if (P_States.isBowMode && P_States.isStrongArrow)
            {
                Effect effect = GameManager.Instance.objectPooling.ShowEffect(Bow_StrongName);
                effect.gameObject.transform.position = this.gameObject.transform.position + Vector3.up;
                effect.transform.rotation = Quaternion.LookRotation(playerAttackCheck.transform.forward);   // 화살 방향으로
            }
            else if (P_States.isBowMode)
            {
                Effect effect = GameManager.Instance.objectPooling.ShowEffect(Bow_Name);
                effect.gameObject.transform.position = this.gameObject.transform.position + Vector3.up;
                effect.transform.rotation = Quaternion.LookRotation(playerAttackCheck.transform.forward);   // 화살 방향으로
            }
        }

        offArrow();
    }

    public void offArrow()
    {
        if (P_States.isBowMode || P_States.isGunMode)
        {
            if (P_States.isAim || P_States.isShortArrow)
            {
                if (!P_States.isShortArrow)
                {
                    if (P_States.beenAttention) // 조준 전 주목 하고 있었다면
                    {
                        //주목 풀기
                        if (!GameManager.instance.cameraController.banAttention)
                            GameManager.instance.cameraController.AttentionMonster();
                        P_States.beenAttention = false;
                    }
                    //arrow.SetActive(true);
                    GameManager.instance.cameraController.OffAimCamera();   //* 카메라 끄기
                }
                P_Com.animator.SetBool("isAim", false);
                P_Com.animator.SetTrigger("shoot");
                P_Controller.crosshairImage.gameObject.SetActive(false);
                P_Controller.shootPoint.gameObject.SetActive(false);
                P_States.isAim = false;
                P_Com.animator.SetLayerWeight(1, 0f);
            }
        }
    }

    IEnumerator PlayerHeal_co()
    {
        //Debug.Log("Player Heal");
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Player_Heal");
        P_Value.HP = Mathf.Clamp(P_Value.HP + P_Value.MaxHP * 0.5f, P_Value.HP + P_Value.MaxHP * 0.5f, P_Value.MaxHP);

        bool stopHeal = false;

        effect.finishAction = () =>
        {
            stopHeal = true;
        };

        while (!stopHeal)
        {
            //1. 플레이어 위치 계속
            effect.gameObject.transform.position = this.gameObject.transform.position + Vector3.up;

            yield return null;
        }
    }
    IEnumerator isPressedCode()
    {
        //Effect effect = GameManager.Instance.objectPooling.ShowEffect("Time cast");

        while (isPressed)
        {
            //Debug.Log("if (isPressed)");
            Vector3 mousePosition = Input.mousePosition;
            mousePosition.z = 12f; // 원이 카메라에서 멀리 표시되도록 z 좌표 조정

            Camera playerCamera1 = GameManager.Instance.cameraController.playerCamera.GetComponentInChildren<Camera>();
            Vector3 targetPosition = playerCamera1.ScreenToWorldPoint(mousePosition);
            targetPosition.y = 0.1f;

            skillRangeIndicator.transform.position = targetPosition;
            //Debug.Log($"skillRangeIndicator {skillRangeIndicator.transform.position}");
            //Debug.Log($"skillRangeIndicator {skillRangeIndicator.transform.position}");

            // 플레이어의 위치를 기준으로 skillRangeIndicator의 위치를 향하는 벡터
            Vector3 direction = skillRangeIndicator.transform.position - this.transform.position;
            // 해당 방향 벡터를 이용하여 플레이어의 회전값 계산
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            // 플레이어 회전 
            this.transform.rotation = targetRotation;

            yield return null;
        }


    }

    IEnumerator Skill_Restraint(char whatKey)
    {
        yield return new WaitUntil(() => (P_KState.EDown || P_KState.RDown || P_KState.FDown));
        if (!isPressed)
        {
            switch (whatKey)
            {
                case 'E':
                case 'R':
                case 'F':
                    isPressed = true; P_States.isSkill = true;
                    //skillRangeIndicator = UnityEngine.Object.Instantiate(skillRangeIndicator);
                    skillRangeIndicator = GameManager.Instance.objectPooling.GetProjectilePrefab("TargetMarker");

                    skillRangeIndicator.SetActive(true);
                    break;
                default: break;
            }
        }

        //if (isPressed)
        {
            StartCoroutine(isPressedCode());
            //Vector3 mousePosition = Input.mousePosition;
            //mousePosition.z = 12f; // 원이 카메라에서 멀리 표시되도록 z 좌표 조정
            //Camera playerCamera1 = GameManager.Instance.cameraController.playerCamera.GetComponentInChildren<Camera>();
            //Vector3 targetPosition = playerCamera1.ScreenToWorldPoint(mousePosition);
            //targetPosition.y = 0.2f;
            //skillRangeIndicator.transform.position = targetPosition;
        }

        yield return new WaitUntil(() => (!P_KState.EDown && !P_KState.RDown && !P_KState.FDown));
        if (isPressed)
        {
            switch (whatKey)
            {
                case 'E':
                case 'R':
                case 'F':
                    isPressed = false; P_States.isSkill = false;
                    P_InputHandle.skillBtnOnclick(whatKey);
                    skillRangeIndicator.SetActive(false);
                    StopCoroutine(isPressedCode());
                    StartCoroutine(Skill_RestraintCo());
                    break;
                default: break;
            }
        }
    }

    IEnumerator Skill_RestraintCo()
    {
        Collider[] colliders = Physics.OverlapCapsule(skillRangeIndicator.transform.position - Vector3.up * cylinderHeight / 2f,
                                                        skillRangeIndicator.transform.position + Vector3.up * cylinderHeight / 2f,
                                                        cylinderRadius); //범위 원기둥으로 만듦

        //스킬 이펙트
        Effect skillEffect = GameManager.Instance.objectPooling.ShowEffect("Temporary explosion");
        skillEffect.transform.position = skillRangeIndicator.transform.position;

        //Effect skillEffectCast = GameManager.Instance.objectPooling.ShowEffect("Time cast");
        Vector3 playerPos = this.transform.position;
        playerPos.y = 0.1f;
        //skillEffectCast.transform.position = playerPos;
        //skillEffectCast.transform.rotation = this.transform.rotation;

        P_States.isStop = true;

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Monster"))
            {
                MonsterPattern monsterPattern = collider.GetComponent<MonsterPattern>();

                if (monsterPattern != null)
                {
                    monsterPattern.isRestraint = true;

                    yield return new WaitForSeconds(0.3f);
                    Effect effect = GameManager.Instance.objectPooling.ShowEffect("Magic shield loop yellow");
                    effect.transform.position = monsterPattern.transform.position + Vector3.up;
                    float smallestMonsterSize, largestMonsterSize;

                    // GetMonsterSizeRange 함수를 호출하여 몬스터의 크기 범위를 가져옵니다.
                    GetMonsterSizeRange(colliders, out smallestMonsterSize, out largestMonsterSize);
                    AdjustEffectSize(effect, smallestMonsterSize, largestMonsterSize);
                    if (monsterEffects.ContainsKey(monsterPattern))
                    {
                        //Debug.Log("이미 있는 키값");
                    }
                    else monsterEffects.Add(monsterPattern, effect); // 몬스터와 이펙트 매핑 추가
                    //Debug.Log("속박");
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        skillEffect.StopEffect();
        //skillEffectCast.StopEffect();
        P_States.isStop = false;

        // 일정 시간 후에 스킬 비활성화
        yield return new WaitForSeconds(skillDuration);
        // 스킬 비활성화
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Monster"))
            {
                MonsterPattern monsterPattern = collider.GetComponent<MonsterPattern>();
                if (monsterPattern != null)
                {
                    monsterPattern.isRestraint = false;
                    if (monsterEffects.ContainsKey(monsterPattern))
                    {
                        //Debug.Log("effect off");
                        Effect monsterEffect = monsterEffects[monsterPattern];
                        monsterEffect.StopEffect(); //이펙트 멈춤
                        monsterEffects.Remove(monsterPattern); // 이펙트를 딕셔너리에서 제거 
                    }
                    //Debug.Log("속박풀림");
                }
            }
        }
    }
    // 몬스터의 크기 범위를 계산하는 함수
    void GetMonsterSizeRange(Collider[] colliders, out float smallestMonsterSize, out float largestMonsterSize)
    {
        smallestMonsterSize = Mathf.Infinity;
        largestMonsterSize = 0f;

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Monster"))
            {
                // 각 몬스터의 크기를 계산합니다.
                Vector3 monsterScale = collider.transform.localScale;
                float monsterSize = Mathf.Max(Mathf.Max(monsterScale.x, monsterScale.y), monsterScale.z);

                // 가장 작은 크기와 가장 큰 크기를 업데이트합니다.
                smallestMonsterSize = Mathf.Min(smallestMonsterSize, monsterSize);
                largestMonsterSize = Mathf.Max(largestMonsterSize, monsterSize);
            }
        }
    }

    /// 몬스터 크기에 따라 스킬 이펙트의 크기를 조절하는 함수
    void AdjustEffectSize(Effect skillEffect, float smallestMonsterSize, float largestMonsterSize)
    {
        // 이펙트 크기를 조절하는 비율을 계산합니다.
        float effectSizeMultiplier = Mathf.Lerp(0.3f, 1.1f, (skillEffect.transform.localScale.magnitude - smallestMonsterSize) / (largestMonsterSize - smallestMonsterSize));
        // 이펙트 크기를 조절합니다.
        skillEffect.transform.localScale *= effectSizeMultiplier;
    }


    private void OnDrawGizmosSelected()
    {
        // 스킬 속박 범위 그리기
        //Gizmos.color = Color.blue;
        //Gizmos.DrawWireSphere(skillRangeIndicator.transform.position - Vector3.up * cylinderHeight / 2f, cylinderRadius);
        //Gizmos.DrawWireSphere(skillRangeIndicator.transform.position + Vector3.up * cylinderHeight / 2f, cylinderRadius);
        //Gizmos.DrawLine(skillRangeIndicator.transform.position - Vector3.up * cylinderHeight / 2f + Vector3.left * cylinderRadius, skillRangeIndicator.transform.position + Vector3.up * cylinderHeight / 2f + Vector3.left * cylinderRadius);
        //Gizmos.DrawLine(skillRangeIndicator.transform.position - Vector3.up * cylinderHeight / 2f + Vector3.right * cylinderRadius, skillRangeIndicator.transform.position + Vector3.up * cylinderHeight / 2f + Vector3.right * cylinderRadius);
    }

    public void skillMotion(string skillName, char whatKey = 'a')
    {
        switch (skillName)
        {
            case "ChangeWeapon":   //* weapon change
                Effect effect = GameManager.Instance.objectPooling.ShowEffect("weaponChange");
                effect.gameObject.transform.position = this.gameObject.transform.position + Vector3.up;
                if (P_States.isGunMode) //* 총 모드 -> 칼 모드 
                {
                    offArrow();
                    P_States.isGunMode = false;
                    P_Controller.bow.SetActive(false);
                    P_Controller.sword.SetActive(true);
                    P_Com.animator.SetFloat("isBowmode", 0);
                }
                else if (!P_States.isGunMode) //* 칼 모드 -> 총 모드 
                {
                    P_States.isGunMode = true;
                    P_States.isBowMode = false;
                    P_Controller.bow.SetActive(true);
                    P_Controller.shootPoint.gameObject.SetActive(false);
                    P_Controller.sword.SetActive(false);
                    P_Com.animator.SetFloat("isBowmode", 1);
                    onBulletCam();
                }
                P_Movement.skill_V.OnClicked();
                break;
            case "ChangeWeapon_Bow":   //* weapon change
                Effect effectB = GameManager.Instance.objectPooling.ShowEffect("weaponChange");
                effectB.gameObject.transform.position = this.gameObject.transform.position + Vector3.up;
                if (P_States.isBowMode) //* 활 모드 -> 칼 모드 
                {
                    P_States.isBowMode = false;
                    P_Controller.bow.SetActive(false);
                    P_Controller.sword.SetActive(true);
                    P_Com.animator.SetFloat("isBowmode", 0);
                }
                else if (!P_States.isBowMode) //* 칼 모드 -> 활 모드
                {
                    offArrow();
                    P_States.isBowMode = true;
                    P_States.isGunMode = false;
                    P_Controller.bow.SetActive(true);
                    P_Controller.shootPoint.gameObject.SetActive(false);
                    P_Controller.sword.SetActive(false);
                    P_Com.animator.SetFloat("isBowmode", 1);
                }
                P_Movement.skill_V.OnClicked();
                break;

            case "Heal":
                P_States.isSkill = true;
                P_InputHandle.skillBtnOnclick(whatKey);
                StartCoroutine(PlayerHeal_co());
                break;

            case "Ultimate":
                P_States.isSkill = true;
                P_InputHandle.skillBtnOnclick(whatKey);
#if UNITY_EDITOR
                Debug.Log("스킬T");
#endif
                break;

            case "Restraint":
                StartCoroutine(Skill_Restraint(whatKey));
#if UNITY_EDITOR
                Debug.Log("속박스킬");
#endif
                break;

            default:
                break;
        }
    }

    public void SkillWindow(bool isOn, bool isBtn = false)
    {
        if (P_KState.PDown || (isOn && Input.GetKeyUp(KeyCode.Escape)) || isBtn)
        {
            P_KState.PDown = false;
            if (!isOn)  // 켜져있지 않다면 -> 켜기
            {
                OpenSkillWindowInternal();
            }
            else  //켜져있다면 -> 끄기
            {
                CloseSkillWindowInternal();
            }
            P_Com.animator.SetBool("p_Locomotion", true);
            P_Com.animator.Rebind();
        }
    }

    private void OpenSkillWindowInternal()
    {
        P_Controller.setIsOn(true);
        //skillTreeWindow.gameObject.SetActive(true);
        P_Movement.skillTree.gameObject.SetActive(true);
        P_Controller.PlayerUI_SetActive(false);
        UIManager.gameIsPaused = true;
        P_States.isStop = true;

        Cursor.visible = true;     //마우스 커서
        Cursor.lockState = CursorLockMode.None;

        ModalWindowManager modalWindowManager = P_Movement.skillTree.GetComponent<ModalWindowManager>();
        modalWindowManager.mwAnimator.enabled = true;
        modalWindowManager.mwAnimator.SetFloat("AnimSpeed", modalWindowManager.animationSpeed);
        modalWindowManager.mwAnimator.Play("In");
    }

    private void CloseSkillWindowInternal()
    {
        P_Controller.setIsOn(false);
        P_InputHandle.skillIconApply();
        //skillTreeWindow.gameObject.SetActive(false);
        P_Movement.skillTree.gameObject.SetActive(false);
        P_Controller.PlayerUI_SetActive(true);
        UIManager.gameIsPaused = false;
        P_States.isStop = false;

        Cursor.visible = false;     //마우스 커서
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정
    }

    public void OpenSkillWindow()
    {
        SkillWindow(false, true);
    }

    public void CloseSkillWindow()
    {
        SkillWindow(true, true);
        Debug.Log("CloseSkillWindow()");
    }
}
