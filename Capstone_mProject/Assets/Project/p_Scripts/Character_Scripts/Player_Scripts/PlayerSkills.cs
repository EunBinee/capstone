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
    private PlayerArrows P_Arrows => P_Controller._playerArrows;
    private SkillInfo P_SkillInfo => P_Controller._skillInfo;
    private PlayerAttackCheck playerAttackCheck;
    private PlayerInputHandle P_InputHandle;

    private GameObject arrow;// => P_Controller.arrow;

    private SkillButton skill_T;
    private string R_Start_Name = "Bow_Attack_Charging";
    private string R_Name = "Bow_Attack_launch_02";
    private string R_StrongName = "ChargingArrowLaunch";

    [SerializeField]
    public Dictionary<string, SOSkill> skillMap;
    //public List<SOSkill> selectSkill;
    private int selectSize = 3;

    public GameObject skillScrollWindow;
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
        skillMap = new Dictionary<string, SOSkill>();
        skillMap.Clear();
        P_SkillInfo.selectSkill = new List<SOSkill>();
        P_SkillInfo.selectSkill.Clear();
        arrow = P_Controller.arrow;
        //skillRangeIndicator = UnityEngine.Object.Instantiate(skillRangeIndicator);
        skillRangeIndicator = Resources.Load<GameObject>("TargetMarker");
        skillRangeIndicator.SetActive(false);
        //playerAttackCheck = arrow.GetComponent<PlayerAttackCheck>();
        P_Movement = GetComponent<PlayerMovement>();
    }
    void Start()
    {
        P_InputHandle = GetComponent<PlayerInputHandle>();
        Invoke("Setting", 0.1f);
    }
    void Setting()
    {
        skillScrollWindow = P_Controller.P_Movement.skillScrollWindow;
        skill_T = P_Controller.P_Movement.skill_T;
        SkillMapAdd("Bowmode", P_SkillInfo.bowmode);    // 기본지급 스킬
        P_SkillInfo.haveBowmode = true;
        SkillMapAdd("Heal", P_SkillInfo.heal);
        P_SkillInfo.haveHeal = true;
        SkillMapAdd("Ultimate", P_SkillInfo.ultimate);  // 기본지급 스킬
        P_SkillInfo.haveUltimate = true;
        SkillMapAdd("Sample1", P_SkillInfo.sample1);
        P_SkillInfo.haveSample1 = true;
        SkillMapAdd("Sample2", P_SkillInfo.sample2);
        P_SkillInfo.haveSample2 = true;
        SkillMapAdd("Restraint", P_SkillInfo.restraint);
        P_SkillInfo.haveRestraint = true;
    }

    void FixedUpdate()
    {
        if (P_Value.aimClickDown > 1.8f)
        {
            if (P_States.isStrongArrow == false)
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
    public List<string> getskillMap()
    {
        callName.Clear();
        foreach (KeyValuePair<string, SOSkill> i in skillMap)
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

    //* skill
    public void SkillMapAdd(string name, SOSkill skill)
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
            if (P_SkillInfo.selectSkill.Count < selectSize)  // 플레이어가 고른 스킬 갯수 3개 미만?
            {
                if (name == "Bowmode" || name == "Ultimate")   // 무기변경스킬이나 궁 스킬 이라면 무시
                { }
                else
                    P_SkillInfo.selectSkill.Add(skill); // 자동 추가
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
            if (P_States.isStrongArrow)
            {
                Effect effect = GameManager.Instance.objectPooling.ShowEffect(R_StrongName);
                effect.gameObject.transform.position = this.gameObject.transform.position + Vector3.up;
                effect.transform.rotation = Quaternion.LookRotation(playerAttackCheck.transform.forward);
            }
            else
            {
                Effect effect = GameManager.Instance.objectPooling.ShowEffect(R_Name);
                effect.gameObject.transform.position = this.gameObject.transform.position + Vector3.up;
                effect.transform.rotation = Quaternion.LookRotation(playerAttackCheck.transform.forward);
            }
        }

        offArrow();
    }

    void PoolingArrow()
    {
        // 화살을 발사할 위치에 화살을 생성하고 방향을 설정
        arrow = P_Arrows.GetArrowPrefab();
        if (arrow == null) Debug.LogError("arrow null!");
        arrow.SetActive(true);

        playerAttackCheck = arrow.GetComponent<PlayerAttackCheck>();
    }
    public void onArrow()
    {
        if (P_States.isBowMode)
        {
            if (!P_States.isAim || P_States.isShortArrow)
            {
                P_Com.animator.SetBool("isAim", true);  //* 애니메이션
                P_States.isAim = true;
                P_Controller.shootPoint.gameObject.SetActive(true);
                if (!P_States.isShortArrow)
                {
                    //* 조준 on
                    Effect Effect = GameManager.Instance.objectPooling.ShowEffect(R_Start_Name);
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
                //* 단타 
                PoolingArrow(); //* 화살 풀링
                if (P_States.isShortArrow)
                {
                    //Debug.Log("[arrow test] onArrow() / if (P_States.isShortArrow)");
                    arrowSkillOff();
                }
            }
        }
    }
    public void offArrow()
    {
        if (P_States.isBowMode)
        {
            if (P_States.isAim || P_States.isShortArrow)
            {
                if (!P_States.isShortArrow)
                {
                    if (P_States.beenAttention) // 조준 전 주목 하고 있었다면
                    {
                        //주목 풀기
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
                    Debug.Log("switch true");
                    isPressed = true; P_States.isSkill = true;
                    skillRangeIndicator = UnityEngine.Object.Instantiate(skillRangeIndicator);
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
                    Debug.Log("switch false");
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

        yield return new WaitForSeconds(1.6f);
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
                if (skill_T.imgCool.fillAmount == 0)
                {
                    Effect effect = GameManager.Instance.objectPooling.ShowEffect("weaponChange");
                    effect.gameObject.transform.position = this.gameObject.transform.position + Vector3.up;
                    if (P_States.isBowMode) //* 활 모드 -> 칼 모드
                    {
                        P_States.isBowMode = false;
                        P_Controller.bow.SetActive(false);
                        P_Controller.sword.SetActive(true);
                        P_Com.animator.SetFloat("isBow", 0);
                    }
                    else if (!P_States.isBowMode) //* 칼 모드 -> 활 모드
                    {
                        P_States.isBowMode = true;
                        P_Controller.bow.SetActive(true);
                        P_Controller.shootPoint.gameObject.SetActive(false);
                        P_Controller.sword.SetActive(false);
                        P_Com.animator.SetFloat("isBow", 1);
                    }
                }
                skill_T.OnClicked();
                break;

            case "Heal":
                P_States.isSkill = true;
                P_InputHandle.skillBtnOnclick(whatKey);
                StartCoroutine(PlayerHeal_co());
                break;

            case "Ultimate":
                P_States.isSkill = true;
                P_InputHandle.skillBtnOnclick(whatKey);
                Debug.Log("스킬Q");
                break;

            case "Restraint":
                StartCoroutine(Skill_Restraint(whatKey));
                Debug.Log("속박스킬");
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
        skillScrollWindow.gameObject.SetActive(true);
        P_Controller.PlayerUI_SetActive(false);
        UIManager.gameIsPaused = true;
        P_States.isStop = true;

        Cursor.visible = true;     //마우스 커서
        Cursor.lockState = CursorLockMode.None;

        ModalWindowManager modalWindowManager = P_Movement.skillScrollWindow.GetComponent<ModalWindowManager>();
        modalWindowManager.mwAnimator.enabled = true;
        modalWindowManager.mwAnimator.SetFloat("AnimSpeed", modalWindowManager.animationSpeed);
        modalWindowManager.mwAnimator.Play("In");
    }

    private void CloseSkillWindowInternal()
    {
        P_Controller.setIsOn(false);
        P_InputHandle.skillIconApply();
        skillScrollWindow.gameObject.SetActive(false);
        P_Controller.PlayerUI_SetActive(true);
        UIManager.gameIsPaused = false;
        P_States.isStop = false;

        Cursor.visible = false;     //마우스 커서
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정
    }

    public void OpenSkillWindow()
    {
        SkillWindow(false,true);
    }

    public void CloseSkillWindow()
    {
        SkillWindow(true,true);
        Debug.Log("CloseSkillWindow()");
    }
}
