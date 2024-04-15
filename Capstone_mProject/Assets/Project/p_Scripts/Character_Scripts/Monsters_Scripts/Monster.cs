using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using UnityEngine.UI;
using Unity.PlasticSCM.Editor.WebApi;

public class Monster : MonoBehaviour
{
    public MonsterData monsterData;
    public MonsterPattern monsterPattern;
    public MonsterPattern_Boss bossMonsterPattern;
    public AudioClip[] monsterSoundClips;
    public PlayerController playerController;
    private Transform playerTrans;

    [SerializeField] private HPBarUI_Info m_hPBar;
    public bool resetHP = false;
    double normalHP = 0f;
    double weaknessHP = 0f;
    public enum monsterSound
    {
        Hit_Close,
        Hit_Long,
        Alarm,
        Death,
        Phase,
        Hit_Long2
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
        if (Input.GetKeyDown(KeyCode.U))
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
        //GetHPBar();
        if (!resetHP)
            ResetHP();
    }

    private void Reset()
    {

    }

    public void ResetHP()
    {
        //* 처음 시작햇을때 HP 
        resetHP = true;
        monsterData.HP = monsterData.MaxHP;

        if (monsterData.useWeakness)
        {
            int monsterWeaknessNum = 0;
            if (monsterData.useWeakness)
            {
                for (int i = 0; i < monsterData.weaknessList.Count; i++)
                {
                    monsterWeaknessNum++;
                }

                if (monsterData.haveLastWeakness)
                {
                    for (int i = 0; i < monsterData.lastWeaknessList.Count; i++)
                    {
                        monsterWeaknessNum++;
                    }
                }
            }

            weaknessHP = monsterData.MaxHP * monsterData.weaknessDamageRate * monsterWeaknessNum;
            normalHP = monsterData.MaxHP - weaknessHP;
        }
    }

    //*------------------------------------------------------------------------------------------//
    //* 몬스터가 플레이어를 때렸을 때 //
    public virtual void OnHit(float damage = 0, Action action = null)
    {
        if (!playerController._currentState.isGettingHit)
        {
            if (!playerController._currentState.isGettingHit)
                playerController._currentState.isGettingHit = true;
            //몬스터가 플레이어를 때렸을 때 처리.
            playerController.OnHitPlayerEffect = action;
            playerController.GetHit(this, damage);
        }
    }

    public virtual void OnHit_FallDown(float damage = 0, float distance = 10f, Action action = null)
    {
        if (!playerController._currentState.isGettingHit)
        {
            if (!playerController._currentState.isGettingHit)
                playerController._currentState.isGettingHit = true;
            //몬스터가 플레이어를 때렸을 때 처리.
            playerController.OnHitPlayerEffect = action;
            playerController.GetHit_FallDown(this, damage, distance);
        }
    }
    //*------------------------------------------------------------------------------------------//
    //*몬스터가 플레이어에게 공격 당함.

    public virtual void GetDamage(double damage, Vector3 attackPos, Quaternion atteckRot, bool HitWeakness = false)
    {
        if (monsterData.HP > 0)
        {
            if (!monsterPattern.noAttack || monsterPattern.GetCurMonsterState() != MonsterPattern.MonsterState.Death)
            {
                //*----------------------------------------------------------------------------//
                if (monsterData.useWeakness)
                {
                    //* 만약 약점이 있다면, 약점 HP와 그냥 HP를 구분하기
                    if (!HitWeakness)
                    {
                        if (normalHP > 0)
                        {
                            if (normalHP - damage <= 0)
                            {
                                damage = normalHP;
                                normalHP = 0;
                            }
                            else
                            {
                                normalHP -= damage;
                            }
                        }
                        else
                        {
                            damage = 0;
                        }
                    }
                    else if (HitWeakness)
                    {
                        if (weaknessHP != 0)
                        {
                            if (weaknessHP - damage < 0)
                            {
                                damage = weaknessHP;
                                weaknessHP = 0;
                            }
                            else
                            {
                                weaknessHP -= damage;
                            }
                        }
                        else
                        {
                            damage = 0;
                        }
                    }
                }
                //* 데미지 UI 처리---------------------------------------------------------------//

                Get_DamageUI(damage);

                //* HP 와 HPBar처리---------------------------------------------------------------//
                if (HPBar_CheckNull() == false)
                    GetHPBar();

                monsterData.HP -= damage;
                m_hPBar.UpdateHP();
                //*-------------------------------------------------------------------------------//
                //플레이어의 반대 방향으로 넉백
                if (monsterData.HP <= 0)
                {
                    //죽음
                    Death();
                }
                else
                {
                    //아직 살아있음.
                    if (monsterData.monsterType == MonsterData.MonsterType.BossMonster)//* 만약 보스라면?
                    {
                        //* HP에 따른! 페이즈 수정.!
                        //bossMonsterPattern.Base_Phase_HP();
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
        resetHP = false;
        if (playerController.hitMonsters.Count > 0)
            playerController.hitMonsters.Remove(this.gameObject);

        if (monsterData.monsterType == MonsterData.MonsterType.BossMonster)
        {
            bossMonsterPattern.Monster_Motion(MonsterPattern_Boss.BossMonsterMotion.Death);
        }
        else
            monsterPattern.Monster_Motion(MonsterPattern.MonsterMotion.Death);

        //퀘스트 진행도 ++
        if (DialogueManager.instance.DoQuest)//GameManager.Instance.questManager != null
        {

            DialogueManager.Instance.questManager.currentQuestValue_++;
            Debug.Log(DialogueManager.Instance.questManager.currentQuestValue_);

        }
        else if (!DialogueManager.instance.DoQuest)
        {
        }
    }

    public MonsterPattern.MonsterState GetMonsterState()
    {
        return monsterPattern.GetCurMonsterState();
    }
    //*------------------------------------------------------------------------------------------//
    //* 사운드 //
    public void SoundPlay(monsterSound m_sound, bool useLoop = false)
    {
        SoundManager.Instance.Play_MonsterSound(monsterSoundClips[(int)m_sound], useLoop);
    }
    public void SoundPlayStop(monsterSound m_sound)
    {
        SoundManager.Instance.Stop_MonsterSound(monsterSoundClips[(int)m_sound]);
    }
    //*------------------------------------------------------------------------------------------//
    //* HP바 //
    public void GetHPBar()
    {
        if (monsterData.monsterType == MonsterData.MonsterType.BossMonster)
        {
            m_hPBar = UIManager.Instance.hPBarManager.Get_BossHPBar();
            m_hPBar.Reset(monsterData.MaxHP, this, true);
        }
        else
        {
            m_hPBar = UIManager.Instance.hPBarManager.Get_HPBar();
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
                UIManager.Instance.hPBarManager.Return_BossHPBar();
                m_hPBar = null;
            }
        }
        else
        {
            if (m_hPBar != null)
            {
                UIManager.Instance.hPBarManager.Add_HPBarPool(m_hPBar);
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
        //float randomRange = 0;
        float randomRangeMin = 0;
        float randdomRangeMax = 0;

        if (monsterData.monsterType == MonsterData.MonsterType.BossMonster)
        {
            //보스전일때는 좀더 크게
            //randomRange = 1.5f;
            randomRangeMin = 0.0f;
            randdomRangeMax = 1.2f;
        }
        else
        {
            //randomRange = 0.5f;
            randomRangeMin = -0.5f;
            randdomRangeMax = 0.5f;
        }

        DamageUI_Info damageUI = UIManager.Instance.damageManager.Get_DamageUI();

        float x = UnityEngine.Random.Range(randomRangeMin, randdomRangeMax);
        float y = UnityEngine.Random.Range(randomRangeMin, randdomRangeMax);
        float z = UnityEngine.Random.Range(randomRangeMin, randdomRangeMax);
        Vector3 randomPos = new Vector3(x, y, z);
        randomPos = GameManager.Instance.gameData.playerHeadPos.position + randomPos;//monsterData.effectTrans.position + randomPos;

        damageUI.Reset(this, randomPos, damage);
    }
    //*---------------------------------------------------------------------------------------//
    public int GetIndex_NearestLegs(Transform target)
    {
        //! 몬스터의 아래가 뚫려있을 경우, 플레이어와 가장 가까운 다리의 Index를 알려줌
        if (monsterData.isBottomlessMonster)
        {
            float distance = 10000;
            int curW_index = 0;
            for (int i = 0; i < monsterData.bottomlessMonsterLegs.Count; ++i)
            {
                float m_distance = Vector3.Distance(monsterData.bottomlessMonsterLegs[i].position, target.gameObject.transform.position);
                if (m_distance < distance)
                {
                    distance = m_distance;
                    curW_index = i;
                }
            }
            return curW_index;
        }

        return -1;

    }
}
