using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    public MonsterData monsterData;
    public MonsterPattern monsterPattern;

    public AudioClip[] monsterSoundClips;
    public PlayerController playerController;
    private Transform playerTrans;

    [SerializeField] private MonsterUI_Info m_hPBar;

    public enum monsterSound
    {
        Hit,
        GetDamage,
        Death
    }

    private void Awake()
    {


    }
    private void Start()
    {
        Init();
    }
    private void Update()
    {
        //임시 테스트 코드---===================//
        if (Input.GetKeyDown(KeyCode.P))
        {
            GetDamage(3);

        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            Death();
        }
        //---====================================//
    }
    //*------------------------------------------------------------------------------------------//
    //* 초기화 //
    private void Init()
    {
        playerController = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
        playerTrans = GameManager.Instance.gameData.GetPlayerTransform();
        monsterData.HP = monsterData.MaxHP;
        m_hPBar = null;
        //GetHPBar();
    }

    private void Reset()
    {

    }

    //*------------------------------------------------------------------------------------------//
    //* 몬스터 //
    public virtual void OnHit(float Damage = 0, Action action = null)
    {
        if (!playerController._currentState.isGettingHit)
        {
            //몬스터가 플레이어를 때렸을 때 처리.
            playerController.OnHitPlayerEffect = action;
            playerController.GetHit(this);
        }
    }

    public virtual void GetDamage(double Damage)//플레이어에게 공격 당함.
    {
        if (monsterData.HP > 0)
        {
            if (!monsterPattern.noAttack || monsterPattern.GetCurMonsterState() != MonsterPattern.MonsterState.Death)
            {
                if (HPBar_CheckNull() == false)
                    GetHPBar();
                monsterData.HP -= Damage;
                m_hPBar.UpdateHP();

                //플레이어의 반대 방향으로 넉백
                if (monsterData.HP <= 0)
                {
                    //죽음
                    Death();
                }
                else
                {
                    //아직 살아있음.
                    monsterPattern.Monster_Motion(MonsterPattern.MonsterMotion.GetHit_KnockBack);
                }
            }
        }

    }

    public virtual void Death()
    {
        //죽다.
        monsterData.HP = 0;
        monsterPattern.Monster_Motion(MonsterPattern.MonsterMotion.Death);

        //퀘스트 진행도 ++
        GameManager.Instance.questManager.currentQuestValue_++;
        Debug.Log(GameManager.Instance.questManager.currentQuestValue_);
    }
    //*------------------------------------------------------------------------------------------//
    //* 사운드 //
    private void SoundPlay(monsterSound m_sound)
    {
        SoundManager.Instance.Play_MonsterSound(monsterSoundClips[(int)m_sound]);
    }
    //*------------------------------------------------------------------------------------------//
    //* HP바 //
    public void GetHPBar()
    {
        m_hPBar = GameManager.Instance.hPBarManager.Get_HPBar();
        m_hPBar.Reset(monsterData.MaxHP, this);
    }

    public void SetActive_HPBar()
    {
        if (m_hPBar.gameObject.activeSelf == true)
            m_hPBar.gameObject.SetActive(false);
        else
            m_hPBar.gameObject.SetActive(true);
    }

    public void RetrunHPBar()
    {
        if (m_hPBar != null)
        {
            GameManager.Instance.hPBarManager.Add_HPBarPool(m_hPBar);
            m_hPBar = null;
        }

    }

    public bool HPBar_CheckNull()
    {
        if (m_hPBar != null)
            return true;
        return false;
    }
}
