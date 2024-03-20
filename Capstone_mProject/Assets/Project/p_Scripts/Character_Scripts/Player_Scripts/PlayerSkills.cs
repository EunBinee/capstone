using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerSkills : MonoBehaviour
{
    public PlayerController _controller;// = new PlayerController();
    private PlayerController P_Controller => _controller;
    private PlayerComponents P_Com => P_Controller._playerComponents;
    private CurrentState P_States => P_Controller._currentState;
    private CurrentValue P_Value => P_Controller._currentValue;
    private PlayerArrows P_Arrows => P_Controller._playerArrows;
    private PlayerFollowCamera P_Camera => P_Controller._playerFollowCamera;
    public PlayerAttackCheck playerAttackCheck;

    private GameObject arrow;// => P_Controller.arrow;

    private SkillButton skill_E => P_Controller.P_Movement.skill_E; //* HEAL
    private string R_Start_Name = "Bow_Attack_Charging";
    private string R_Name = "Bow_Attack_launch_02";
    private SkillButton skill_Q => P_Controller.P_Movement.skill_Q;
    private SkillButton skill_R => P_Controller.P_Movement.skill_R; //* AIM

    void Awake()
    {
        arrow = P_Controller.arrow;
    }

    // Update is called once per frame
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
    }

    //* skill
    // UI 버튼에 의해 호출됩니다.
    // 인자로 넘어온 skill 정보에 따라 애니메이션을 플레이하고
    // damage 정보 만큼 피해를 입힙니다.
    public void ActivateSkill(SOSkill skill)
    {
        if (skill.animationName != "Skill_Heal")
        {
            P_Com.animator.Play(skill.animationName);
            P_States.isSkill = false;
        }
    }

    public void arrowSkillOn()
    {
        // //* 장전
        // //P_States.isOnAim = true;
        // P_Controller.shootPoint.gameObject.SetActive(true);
        // Effect effect = GameManager.Instance.objectPooling.ShowEffect(R_Start_Name);
        // effect.gameObject.transform.position = this.gameObject.transform.position + Vector3.up;
        // //* 이펙트 회전
        // effect.transform.rotation = Quaternion.LookRotation(this.transform.forward);

        onArrow();
    }
    public void arrowSkillOff()
    {
        //* 발사 
        //P_States.isOnAim = false;
        P_States.startAim = false;
        P_States.isCamOnAim = false;

        offArrow();
    }

    void PoolingArrow()
    {
        // 화살을 발사할 위치에 화살을 생성하고 방향을 설정
        arrow = P_Arrows.GetArrowFromPool();
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
                    P_States.isClickDown = false;
                    P_Value.aimClickDown = 0;
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

            case 'Q':
                if (skill_Q.imgCool.fillAmount == 0)
                {
                    P_States.isSkill = true;
                    Debug.Log("스킬Q");
                }
                skill_Q.OnClicked();
                break;

            case 'E':   //* heal
                if (skill_E.imgCool.fillAmount == 0)
                {
                    P_States.isSkill = true;
                    StartCoroutine(PlayerHeal_co());
                }
                skill_E.OnClicked();
                break;

            default:
                break;
        }
    }
}
