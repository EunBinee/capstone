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
    public MonsterPattern_Boss bossMonsterPattern;
    public AudioClip[] monsterSoundClips;
    public PlayerController playerController;
    private Transform playerTrans;

    [SerializeField] private HPBarUI_Info m_hPBar;


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
            //monsterPattern.StopMonster();
        }

        //---====================================//
    }
    //*------------------------------------------------------------------------------------------//
    //* 초기화 //
    private void Init()
    {
        playerController = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
        playerTrans = GameManager.Instance.gameData.GetPlayerTransform();
        //GetHPBar();
    }

    private void Reset()
    {

    }

    public void ResetHP()
    {
        monsterData.HP = monsterData.MaxHP;
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

    public virtual void GetDamage(double damage, Vector3 attackPos, Quaternion atteckRot)//플레이어에게 공격 당함.
    {
        if (monsterData.HP > 0)
        {
            if (!monsterPattern.noAttack || monsterPattern.GetCurMonsterState() != MonsterPattern.MonsterState.Death)
            {
                //* 데미지 UI 처리---------------------------------------------------------------//
                Get_DamageUI(damage);

                //* HP 와 HPBar처리---------------------------------------------------------------//
                if (HPBar_CheckNull() == false)
                    GetHPBar();

                monsterData.HP -= damage;
                m_hPBar.UpdateHP();
                //--------------------------------------------------------------------------------//

                //플레이어의 반대 방향으로 넉백
                if (monsterData.HP <= 0)
                {
                    //죽음
                    Death();
                }
                else
                {
                    //아직 살아있음.
                    if (monsterData.monsterType == MonsterData.MonsterType.BossMonster)
                    {
                        monsterPattern.SetGetDemageMonster(attackPos, atteckRot);
                        bossMonsterPattern.Monster_Motion(MonsterPattern_Boss.BossMonsterMotion.GetHit);
                    }
                    else
                        monsterPattern.Monster_Motion(MonsterPattern.MonsterMotion.GetHit_KnockBack);
                }
            }
        }

    }

    public virtual void Death()
    {
        //죽다.
        monsterData.HP = 0;
        if (monsterData.monsterType == MonsterData.MonsterType.BossMonster)
        {

        }
        else
            monsterPattern.Monster_Motion(MonsterPattern.MonsterMotion.Death);

        //퀘스트 진행도 ++

        GameManager.Instance.questManager.currentQuestValue_++;
        Debug.Log(GameManager.Instance.questManager.currentQuestValue_);
    }

    public MonsterPattern.MonsterState GetMonsterState()
    {
        return monsterPattern.GetCurMonsterState();
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
        if (monsterData.monsterType == MonsterData.MonsterType.BossMonster)
        {
            m_hPBar = GameManager.instance.hPBarManager.Get_BossHPBar();
            m_hPBar.Reset(monsterData.MaxHP, this, true);
        }
        else
        {
            m_hPBar = GameManager.Instance.hPBarManager.Get_HPBar();
            m_hPBar.Reset(monsterData.MaxHP, this);
        }
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
        if (monsterData.monsterType == MonsterData.MonsterType.BossMonster)
        {
            if (m_hPBar != null)
            {
                GameManager.Instance.hPBarManager.Return_BossHPBar();
                m_hPBar = null;
            }
        }
        else
        {
            if (m_hPBar != null)
            {
                GameManager.Instance.hPBarManager.Add_HPBarPool(m_hPBar);
                m_hPBar = null;
            }
        }
    }


    public bool HPBar_CheckNull()
    {
        if (m_hPBar != null)
            return true;
        return false;
    }
    //*------------------------------------------------------------------------------------------//
    //* 데미지 UI //
    public void Get_DamageUI(double damage)
    {
        float randomRange = 0;

        if (monsterData.monsterType == MonsterData.MonsterType.BossMonster)
        {
            //보스전일때는 좀더 크게
            randomRange = 1.5f;
        }
        else
            randomRange = 0.5f;
        Debug.Log("damage UI");
        DamageUI_Info damageUI = GameManager.Instance.damageManager.Get_DamageUI();

        float x = UnityEngine.Random.Range(-randomRange, randomRange);
        float y = UnityEngine.Random.Range(-randomRange, randomRange);
        float z = UnityEngine.Random.Range(-randomRange, randomRange);
        Vector3 randomPos = new Vector3(x, y, z);
        randomPos = monsterData.effectTrans.position + randomPos;

        damageUI.Reset(this, randomPos, damage);
    }
}
