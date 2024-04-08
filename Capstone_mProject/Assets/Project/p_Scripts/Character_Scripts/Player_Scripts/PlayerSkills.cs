using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerSkills : MonoBehaviour
{
    public PlayerController _controller;// = new PlayerController();
    private PlayerController P_Controller => _controller;
    private PlayerComponents P_Com => P_Controller._playerComponents;
    private CurrentState P_States => P_Controller._currentState;
    private CurrentValue P_Value => P_Controller._currentValue;
    private KeyState P_KState => P_Controller._keyState;
    private PlayerArrows P_Arrows => P_Controller._playerArrows;
    private SkillInfo P_SkillInfo => P_Controller._skillInfo;
    private PlayerAttackCheck playerAttackCheck;

    private GameObject arrow;// => P_Controller.arrow;

    private SkillButton skill_E;// => P_Controller.P_Movement.skill_E;
    private string R_Start_Name = "Bow_Attack_Charging";
    private string R_Name = "Bow_Attack_launch_02";
    private string R_StrongName = "ChargingArrowLaunch";
    private SkillButton skill_R;// => P_Controller.P_Movement.skill_R;
    private SkillButton skill_Q;// => P_Controller.P_Movement.skill_Q;

    private Dictionary<string, SOSkill> skillMap;
    private List<SOSkill> selectSkill;
    private int selectSize = 3;

    public ScrollRect skillScrollWindow;

    void Awake()
    {
        skillMap = new Dictionary<string, SOSkill>();
        skillMap.Clear();
        selectSkill = new List<SOSkill>();
        selectSkill.Clear();
        arrow = P_Controller.arrow;
        //playerAttackCheck = arrow.GetComponent<PlayerAttackCheck>();
    }
    void Start()
    {
        SkillMapAdd("Bowmode", P_SkillInfo.bowmode);    // 기본 제공?
        SkillMapAdd("Heal", P_SkillInfo.heal);
        SkillMapAdd("Ultimate", P_SkillInfo.ultimate);
        Invoke("Setting",1f);
    }
    void Setting(){
        skillScrollWindow = P_Controller.P_Movement.skillScrollWindow;
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
        SkillPreset();
    }

    //* skill
    public void SkillMapAdd(string name, SOSkill skill){
        if(skillMap.ContainsKey(name)){ // 이미 등록된 스킬 이름?
            Debug.Log("이미 등록된 스킬 이름!");
            return;
        } 
        else
        {
            skillMap.Add(name, skill);  // 스킬 등록
            if (selectSkill.Count < selectSize)  // 플레이어가 고른 스킬 갯수 3개 미만?
            {
                selectSkill.Add(skill); // 자동 추가
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
        P_Value.HP += P_Value.MaxHP * 0.5f;

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

    public void skillMotion(char a)
    {
        switch (a)
        {
            case 'R':   //* weapon change
                if (skill_R.imgCool.fillAmount == 0)
                {
                    Effect effect = GameManager.Instance.objectPooling.ShowEffect("weaponChange");
                    effect.gameObject.transform.position = this.gameObject.transform.position + Vector3.up;
                    if (P_States.isBowMode) //* 활 모드 -> 칼 모드
                    {
                        P_States.isBowMode = false;
                        P_Controller.bow.SetActive(false);
                        P_Controller.sword.SetActive(true);
                    }
                    else if (!P_States.isBowMode) //* 칼 모드 -> 활 모드
                    {
                        P_States.isBowMode = true;
                        P_Controller.bow.SetActive(true);
                        P_Controller.shootPoint.gameObject.SetActive(false);
                        P_Controller.sword.SetActive(false);
                    }
                    skill_R.OnClicked();
                }
                break;

            case 'E':   //* heal
                if (skill_E.imgCool.fillAmount == 0)
                {
                    P_States.isSkill = true;
                    StartCoroutine(PlayerHeal_co());
                }
                skill_E.OnClicked();
                break;

            case 'Q':
                if (skill_Q.imgCool.fillAmount == 0)
                {
                    P_States.isSkill = true;
                    Debug.Log("스킬Q");
                }
                skill_Q.OnClicked();
                break;

            default:
                break;
        }
    }

    bool isOn = false;
    public void SkillPreset(){
        //todo: K 누르면 스킬 프리셋 설정할 수 있는 창 뜨면서 선택한 스킬이 스킬아이콘에 등록
        //todo  창 뜨면 마우스 움직일 수 있도록(플레이어 멈추기?)
        if (P_KState.KDown)
        {
            P_KState.KDown = false;
            if (!isOn) isOn = true; // 창 켜짐
            else isOn = false;
            skillScrollWindow.gameObject.SetActive(isOn);
            P_Controller.PlayerUI_SetActive(!isOn);
            UIManager.gameIsPaused = isOn;
            P_States.isStop = isOn;
            P_Com.animator.SetBool("p_Locomotion", true);
            P_Com.animator.Rebind();
            Cursor.visible = isOn;     //마우스 커서
            if (!isOn) Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정
            else Cursor.lockState = CursorLockMode.None;
        }

    }
}
