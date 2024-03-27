using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.Animations;
using UnityEditor.Rendering;
using System.Threading;
using UnityEditor;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine.UIElements;

public class MonsterPattern_Boss_Abyss : MonsterPattern_Boss
{
    //! 보스 몬스터 나락.
    public Boss_Abyss_Skill01 boss_Abyss_Skill01;
    public Boss_Abyss_Skill02 boss_Abyss_Skill02;
    public Boss_Abyss_Skill03 boss_Abyss_Skill03;

    public GameObject redImage;
    public GameObject bossText;
    public Transform prefabPos;
    public Vector3 centerPoint;


    List<Vector3> randomPos_skill02;
    [Space]
    [Header("스킬 03")]
    public Transform bossNeck;
    //bool findPlayer = false;
    //public float skillRadius = 10;
    [Header("스킬 03 총알이 나가는 위치")]
    public Transform[] muzzlesL;
    public Transform[] muzzlesR;
    public Transform[] muzzlePos;

    [Space]
    [Header("스킬 04")]
    public GameObject targetMarker_Prefabs;
    public List<GameObject> targetMarkerList = new List<GameObject>();
    public GameObject targetMarker_Pattern05_Prefabs;
    GameObject targetMarker_Pattern05_obj;
    public List<Skill_Indicator> targetMarker_Pattern05_List = new List<Skill_Indicator>();


    public bool isJump = false;
    bool isDodge = false;

    bool startSkill = false;
    bool ing_skill01 = false;
    bool ing_skill02 = false;

    bool ing_skill03 = false;
    bool ing_skill04 = false;

    //* 사운드
    public bool useExplosionSound = false; // 폭발음 중복을 막기 위한..

    //보스 페이즈 체력 기준
    //* 스킬 2번 코루틴
    Coroutine skill02_MoveMonster_Co = null;
    Coroutine skill02_Co = null;
    Coroutine changePhase02_Co = null;


    public List<NavMeshSurface> navMeshSurface;

    public override void Init()
    {
        m_monster = GetComponent<Monster>();
        m_animator = GetComponent<Animator>();

        boss_Abyss_Skill01 = GetComponent<Boss_Abyss_Skill01>();
        boss_Abyss_Skill01.Init(this);
        boss_Abyss_Skill02 = GetComponent<Boss_Abyss_Skill02>();
        boss_Abyss_Skill02.Init(this);
        boss_Abyss_Skill03 = GetComponent<Boss_Abyss_Skill03>();
        boss_Abyss_Skill03.Init(this);

        playerController = GameManager.Instance.gameData.GetPlayerController();
        playerMovement = GameManager.Instance.gameData.GetPlayerMovement();
        playerTrans = GameManager.Instance.gameData.GetPlayerTransform();
        playerTargetPos = GameManager.Instance.gameData.playerTargetPos;
        m_monster.monsterPattern = this;

        if (m_monster.monsterData.movingMonster)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.updateRotation = false;
        }

        playerlayerMask = 1 << playerLayerId; //플레이어 레이어

        ChangeMonsterState(MonsterState.Roaming);
        originPosition = transform.position;

        overlapRadius = m_monster.monsterData.overlapRadius; //플레이어 감지 범위.
        roaming_RangeX = m_monster.monsterData.roaming_RangeX; //로밍 범위 x;
        roaming_RangeZ = m_monster.monsterData.roaming_RangeZ; //로밍 범위 y;
        CheckRoam_Range();

        playerHide = true;

        curBossPhase = BossMonsterPhase.Phase2;
        randomPos_skill02 = new List<Vector3>();
        SetPlayerAttackList(true);
        if (GameManager.instance.cameraController != null)
        {
            GameManager.instance.bossBattle = true;
            GameManager.instance.cameraController.Check_Z();
            GameManager.instance.cameraController.ResetCameraZ();
        }
        else if (GameManager.instance.cameraController == null)
        {
            GameManager.instance.startActionCam += (cameraObj) =>
            {
                GameManager.instance.bossBattle = true;
                cameraObj.Check_Z();
                cameraObj.ResetCameraZ();
            };
        }

        if (m_monster.HPBar_CheckNull() == false)
        {
            if (!m_monster.resetHP)
                m_monster.ResetHP();
            m_monster.GetHPBar();
        }

        centerPoint = GetGroundPos(transform);

        CheckBossHP();
        noAttack = false;

        //*----------------------------------------------------------------------//
        if (m_monster.monsterData.useWeakness)
        {
            curRemainWeaknessesNum = m_monster.monsterData.weaknessList.Count;
        }

        for (int i = 0; i < m_monster.monsterData.weaknessList.Count; i++)
        {
            BossWeakness bossWeakness = m_monster.monsterData.weaknessList[i].GetComponent<BossWeakness>();

            if (bossWeakness.m_monster == null)
                bossWeakness.SetMonster(m_monster);
        }
        //*----------------------------------------------------------------------//

    }

    public override void UpdateRotation()
    {
        if (navMeshAgent.desiredVelocity.sqrMagnitude >= 0.1f * 0.1f)
        {
            //적 ai의 이동방향
            Vector3 direction = navMeshAgent.desiredVelocity;
            //회전 각도 산출 후, 선형 보간 함수로 부드럽게 회전
            Quaternion targetAngle = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Time.deltaTime * 0.3f);
        }
    }

    public override void useUpdate()
    {
        //* -----------------------------------------------------------------------------//
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!ing_skill01)
                StopMonster();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            StartMonster();
        }
        //* -----------------------------------------------------------------------------//
        if (isDodge)
        {
            Vector3 velocity = navMeshAgent.velocity;
            int x = 0;
            int z = 0;
            Debug.Log($"velocity.x  {velocity.x}, velocity.z  {velocity.z}");
            if (velocity.x == 0)
                x = 0;
            else
                x = (velocity.x < 0) ? -1 : 1;
            if (velocity.z == 0)
                z = 0;
            else
                z = (velocity.z < 0) ? -1 : 1;

            m_animator.SetFloat("Vertical", x, 0f, Time.deltaTime);   //상
            m_animator.SetFloat("Horizontal", z, 0f, Time.deltaTime); //하
        }

        BossWeaknessUpdate();
    }

    public override void Monster_Pattern()
    {
        if (curMonsterState != MonsterState.Death)
        {
            switch (curMonsterState)
            {
                case MonsterState.Roaming:
                    Roam_Monster();
                    break;
                case MonsterState.Discovery:
                    break;
                case MonsterState.Tracing:
                    Tracing_Movement();
                    break;
                case MonsterState.Attack:
                case MonsterState.GetHit:
                    break;
                case MonsterState.GoingBack:
                    break;
                case MonsterState.Death:
                    break;
                case MonsterState.Stop:
                    break;
                default:
                    break;
            }
        }
    }

    public override void Monster_Motion(BossMonsterMotion monsterMotion)
    {
        if (!forcedReturnHome)
        {
            switch (monsterMotion)
            {
                case BossMonsterMotion.Skill01:
                    //*내려찍기
                    Debug.Log("다음 스킬 : 01");
                    ing_skill01 = true;
                    boss_Abyss_Skill01.Skill01();
                    break;
                case BossMonsterMotion.Skill02:
                    //* 폭탄
                    Debug.Log("다음 스킬 : 02");
                    ing_skill02 = true;
                    boss_Abyss_Skill02.Skill02();
                    break;
                case BossMonsterMotion.Skill03:
                    if (curBossPhase != BossMonsterPhase.Phase1)
                    {
                        //* 총
                        Debug.Log("다음 스킬 : 03");
                        ing_skill03 = true;
                        boss_Abyss_Skill03.Skill03();
                    }
                    break;
                case BossMonsterMotion.Skill04:
                    if (curBossPhase != BossMonsterPhase.Phase1)
                    {
                        //* 전기
                        Debug.Log("다음 스킬 : 04");
                        ing_skill04 = true;
                        Skill04();
                    }
                    break;
                case BossMonsterMotion.GetHit:
                    //getHit
                    GetHit();

                    break;
                case BossMonsterMotion.Death:
                    noAttack = true;
                    ChangeMonsterState(MonsterState.Death);

                    StartCoroutine(DeathBossMonster());

                    m_monster.RetrunHPBar();
                    break;
                default:
                    break;
            }
        }

    }

    public override void SetAnimation(MonsterAnimation m_anim)
    {

        switch (m_anim)
        {
            case MonsterAnimation.Idle:

                m_animator.SetBool("m_Idle", true);
                m_animator.SetBool("m_Walk", false);
                m_animator.SetBool("m_Dodge", false);
                break;
            case MonsterAnimation.Move:
                m_animator.SetBool("m_Idle", false);
                m_animator.SetBool("m_Walk", true);
                m_animator.SetBool("m_Dodge", false);
                break;
            case MonsterAnimation.Move_Dodge:
                isDodge = true;
                m_animator.SetBool("m_Idle", false);
                m_animator.SetBool("m_Walk", false);
                m_animator.SetBool("m_Dodge", true);
                break;
            case MonsterAnimation.GetHit:
                break;
            case MonsterAnimation.Death:

                break;
            default:
                break;
        }

        if (m_anim != MonsterAnimation.Move_Dodge && isDodge)
        {
            isDodge = false;
        }
    }

    public override void SetBossAttackAnimation(BossMonsterAttackAnimation bossMonsterAttackAnimation, int animIndex = 0)
    {
        switch (bossMonsterAttackAnimation)
        {
            case BossMonsterAttackAnimation.ResetAttackAnim:
                break;
            case BossMonsterAttackAnimation.Skill01:
                switch (animIndex)
                {
                    case 0:
                        //점프
                        m_animator.SetTrigger("isJumping");
                        break;
                    case 1:
                        //착지
                        m_animator.SetTrigger("isLanding");
                        break;
                    case 2:
                        //0번 점프 상태에서 바로 캔슬
                        m_animator.SetTrigger("stopJumping");
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }
    //*----------------------------------------------------------------------------------------------------------//
    public override void ChangeBossPhase(BossMonsterPhase bossMonsterPhase, bool production = true)
    {
        Debug.Log($"CurBossPhase_전 {curBossPhase}");

        curBossPhase = bossMonsterPhase;
        if (production)
        {
            switch (curBossPhase)
            {
                case BossMonsterPhase.Phase1:
                    break;
                case BossMonsterPhase.Phase2:
                    // 2페이지 시작 연출
                    //! 연출후, Tracing으로 변환
                    if (changePhase02_Co == null)
                        changePhase02_Co = StartCoroutine(Phase02_Production());
                    break;
                case BossMonsterPhase.Phase3:
                    // 3페이지 시작 연출
                    if (changePhase02_Co == null)
                        changePhase02_Co = StartCoroutine(Phase02_Production());
                    break;
                case BossMonsterPhase.Death:
                    break;
            }
        }
        Debug.Log($"CurBossPhase_후 {curBossPhase}");
    }

    //* 보스 연출
    //* 페이즈 02
    bool CheckPlayerPos = false;
    IEnumerator Phase02_Production()
    {
        float time = 0;
        yield return new WaitUntil(() => startSkill == false);

        yield return new WaitForSeconds(0.5f);
        //* 몬스터 지정된 장소로 점프
        isJump = true;
        boss_Abyss_Skill01.BossAbyss_JumpUp();
        yield return new WaitUntil(() => isJump == false);

        if (curMonsterState != MonsterState.Death)
        {
            //*랜덤 포스로 이동(플레이어와 20정도 떨어진 곳으로)
            //- 랜덤 Pos
            NavMeshHit hit;
            Vector3 newRandomPos = Vector3.zero;
            Vector3 playerPos = GetGroundPos(playerTrans);
            bool getRandomPos = false;

            while (!getRandomPos)
            {
                time += Time.deltaTime;
                getRandomPos = true;

                newRandomPos = GetRandomPos(35f, playerPos);

                if (NavMesh.SamplePosition(newRandomPos, out hit, 20f, NavMesh.AllAreas))
                {
                    if (hit.position != newRandomPos)
                        newRandomPos = hit.position;

                    float distance = Vector3.Distance(newRandomPos, playerPos);
                    if (distance <= 20f || distance >= 40f) //10보다 작거나 20보다 크면  pos 다시 받아오기'
                        getRandomPos = false;
                }
                else
                {
                    getRandomPos = false;
                }

                if (time > 3f)
                {
                    //3초 동안 못찾으면 걍 break;
                    time = 0;
                    break;
                }

                yield return null;
            }
            isJump = true;
            boss_Abyss_Skill01.BossAbyss_JumpDown(newRandomPos);
            yield return new WaitUntil(() => isJump == false);
            //* 강제 주목 -------------------------------------------------------//
            GameManager.Instance.cameraController.AttentionMonster();
            GameManager.Instance.cameraController.banAttention = true;
            //---------------------------------------------------------------------//
            //* 연출 중, 플레이어 못다가오도록 이펙트
            CheckPlayerPos = true;
            StartCoroutine(CheckPlayer_Production());

            //* 빨간색 화면 PadeIn
            if (redImage == null)
            {
                //TODO: 현재는 그냥 인스펙터에서 redImage를 가져오지만 여기처럼 나중에 resource폴더에서 가져올 수 있도록.
                Debug.Log("보스 redImage 넣어주세여 null입니다.00");
            }
            redImage.SetActive(true);
            GameManager.instance.PadeIn_Alpha(redImage, true, 90);
            bossText.SetActive(true);
            //* 카메라 흔들림        
            GameManager.Instance.cameraController.cameraShake.ShakeCamera(8f, 1.5f, 1.5f);

            //* 연기 이펙트
            Effect effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_03");
            Vector3 effectPos = transform.position;
            effectPos.y -= 1.5f;
            effect.transform.position = effectPos;

            //* 검은 오오라 => 12초
            Vector3 originPos = transform.position;
            effect = GameManager.Instance.objectPooling.ShowEffect("BossMonster_aura");
            effect.transform.position = originPos;
            //- 대화 시스템 ON

            //! 사운드
            m_monster.SoundPlay(Monster.monsterSound.Phase, true);

            yield return new WaitForSeconds(10f);

            //! 사운드 멈춤
            m_monster.SoundPlayStop(Monster.monsterSound.Phase);

            GameManager.Instance.cameraController.cameraShake.ShakeCamera(1f, 3f, 3f);
            //* 연기 이펙트
            effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_04");
            effectPos = transform.position;
            effectPos.y -= 2.5f;
            effect.transform.position = effectPos;

            yield return new WaitForSeconds(2f);
            GameManager.instance.PadeIn_Alpha(redImage, false, 0);
            bossText.SetActive(false);
            //* 타임 라인
            DirectTheBossWeakness();
        }
    }

    public void End_Phase02_Production()
    {
        //* 나중에 주석 풀기 !

        CheckPlayerPos = false;
        Base_Phase_HP(false);
        ChangeMonsterState(MonsterState.Tracing);
        changePhase02_Co = null;
        GameManager.Instance.cameraController.UndoAttention();
    }

    IEnumerator CheckPlayer_Production()
    {
        while (CheckPlayerPos)
        {
            //* 페이즈 넘어갈때, 플레이어가 몬스터 밑에 있는 지 체크
            boss_Abyss_Skill03.CheckPlayerInMonster_skill03(boss_Abyss_Skill03.skillRadius);
            yield return null;
        }
    }

    // *---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 로밍(시네머신 등장 신.)
    public override void Roam_Monster()
    {
        if (!isRoaming && !GameManager.instance.isLoading)
        {
            isRoaming = true;
            //TODO: 나중에 범위안에 들어오면, 등장씬 나오도록 수정
            //* 일단은 바로 공격하도록

            ChangeBossPhase(BossMonsterPhase.Phase2);
            //isRoaming = false;
            //Skill04();
            //* 테스트 후 아래 주석 풀기
            //ChangeBossPhase(BossMonsterPhase.Phase1);
            // ChangeMonsterState(MonsterState.Tracing);
        }
    }
    // *---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> Tracing
    public override void Tracing_Movement()
    {
        //*페이즈 마다 실행되도록.
        if (!isTracing)
        {
            //! 연출 씬 있을때는 멈추기 (나중에 구현)
            isTracing = true;
            switch (curBossPhase)
            {
                case BossMonsterPhase.Phase1:
                case BossMonsterPhase.Phase2:
                case BossMonsterPhase.Phase3:
                    //! 일단은 페이즈 1이랑 2,3랑 실행시키는 건 똑같으니깐 페이즈가 달라도 이 코루틴 사용.
                    StartCoroutine(Phase01_Abyss_Tracing());
                    break;
                default:
                    break;
            }
        }
    }

    //! 일단은 페이즈 1이랑 2랑 실행시키는 건 똑같으니깐 페이즈가 달라도 일단은? 이 코루틴 사용.
    IEnumerator Phase01_Abyss_Tracing()
    {
        int skill = 0;
        bool pickAgain = false;
        float breakTime = 0; //* 스킬 있은 후, 쉬는 간
        BossMonsterPhase curBossP = curBossPhase;

        List<int> skill_List = new List<int>(); //중복된 스킬을 막기 위한 리스트

        while (true)
        {
            if (curMonsterState == MonsterState.Death)
                break;
            Base_Phase_HP();
            if (curBossPhase != curBossP) //* 페이즈 넘어가면 break;
            {
                Debug.Log("rrr");
                break;
            }
            if (forcedReturnHome)
            {
                Debug.Log("Eee");
                break;
            }

            if (curBossPhase == BossMonsterPhase.Phase1)
                skill = UnityEngine.Random.Range(0, 2);
            else if (curBossPhase != BossMonsterPhase.Phase1)
                skill = UnityEngine.Random.Range(0, 3);
            //* 중복 체크-----------------------//
            if (skill_List.Count <= 0)
            {
                skill_List.Add(skill);
            }
            else if (skill_List.Count == 1)
            {
                if (skill_List[0] != skill)
                {
                    skill_List.Clear();
                }

                skill_List.Add(skill);
            }
            else
            {
                if (skill_List[0] != skill)
                {
                    skill_List.Clear();
                    skill_List.Add(skill);
                }
                else
                {
                    int curSkill = skill_List[0];
                    while (true)
                    {
                        skill = UnityEngine.Random.Range(0, 2);
                        if (curSkill != skill)
                        {
                            skill_List.Clear();
                            skill_List.Add(skill);
                            break;
                        }
                    }
                }
            }

            //----------------------------------//
            pickAgain = false;
            //스킬 시작
            switch (skill)
            {
                case 0:
                    if (!ing_skill01) //실행중이 아닐 때만...
                    {
                        Monster_Motion(BossMonsterMotion.Skill01);
                        breakTime = 2;
                    }
                    else
                        pickAgain = true;
                    break;
                case 1:
                    if (!ing_skill02)
                    {
                        //* 페이즈 2 이상이면, 스킬 2번에서 바로 스킬 3번으로 연계
                        Monster_Motion(BossMonsterMotion.Skill02);
                        breakTime = 4;
                    }
                    else
                        pickAgain = true;
                    break;
                case 2:
                    if (!ing_skill04)
                    {
                        //* 스킬 4번으로 연계
                        Monster_Motion(BossMonsterMotion.Skill04);
                        breakTime = 4;
                    }
                    break;
                default:
                    break;
            }
            if (!pickAgain)
            {
                startSkill = true;

                //* 스킬이 끝날 때까지 기다림.
                yield return new WaitUntil(() => startSkill == false);
                //* 쉬는 시간 (플레이어 공격 시간)
                yield return new WaitForSeconds(breakTime);
            }
            else
            {
                yield return null;
            }
        }

        //* 페이즈가 바꼈을때 빠져나옴.
        if (curMonsterState != MonsterState.Death)
        {
            ChangeMonsterState(MonsterState.Stop);
            isTracing = false;
        }
    }

    //* 스킬 끝났을 때 무조건 실행해주는 함수. (변수 수정할때 사용.)
    public void EndSkill(BossMonsterMotion skill)
    {
        //monsterMotion 스킬 끝
        switch (skill)
        {
            case BossMonsterMotion.Skill01:
                startSkill = false;
                ing_skill01 = false;
                break;
            case BossMonsterMotion.Skill02:
                if (curBossPhase == BossMonsterPhase.Phase1)
                {
                    //스킬 2는 페이즈 2,3 일때 바로 스킬 3으로 들어감.
                    startSkill = false;
                }
                ing_skill02 = false;
                break;
            case BossMonsterMotion.Skill03:
                startSkill = false;
                ing_skill03 = false;
                break;
            case BossMonsterMotion.Skill04:
                startSkill = false;
                ing_skill04 = false;
                break;
            default:
                break;
        }
    }

    //*----------------------------------------------------------------------------------------------------------//


    // *---------------------------------------------------------------------------------------------------------//

    // *---------------------------------------------------------------------------------------------------------//


    // *---------------------------------------------------------------------------------------------------------//
    //* 스킬 04  전기
    #region 스킬 04

    int createTargetMarker = 0;
    float angle = 0;
    bool skillOver = false;
    List<Skill_Indicator> curTargetMarker;

    Coroutine skill04_Co = null;
    Coroutine skill04_Pattern04_Co = null;

    public void Skill04()
    {
        curTargetMarker = new List<Skill_Indicator>();
        skill04_Co = StartCoroutine(BossAbyss_Skill04());
    }

    public void SettingSkill04Pattern(int patternNum)
    {
        //* 패턴세팅
        switch (patternNum)
        {
            case 1:
                createTargetMarker = 8;
                angle = 360 / createTargetMarker;
                break;
            case 2:
                createTargetMarker = 9;
                angle = 360 / createTargetMarker;
                break;
            case 3:
                createTargetMarker = 10;
                angle = 360 / createTargetMarker;

                break;
            case 4:
                createTargetMarker = 19;
                angle = 20;
                break;
            case 5:
                break;
            default:
                break;
        }
    }

    int curRandomSkillPattern_num = 0;

    IEnumerator BossAbyss_Skill04()
    {
        //* 5개의 패턴중 하나 랜덤으로 고름

        SetMove_AI(false);
        SetAnimation(MonsterAnimation.Idle);


        //*------------------------------------------------------------------//
        GameManager.Instance.cameraController.cameraShake.ShakeCamera(1f, 3, 3);
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_03");
        Vector3 effectPos = transform.position;
        effectPos.y -= 1.5f;
        effect.transform.position = effectPos;

        yield return new WaitForSeconds(4f);
        //*-----------------------------------------------------------------------------------//
        int count = 0;
        int curIndex = -1;
        while (count < 4)
        {
            while (true)
            {
                curRandomSkillPattern_num = UnityEngine.Random.Range(1, 6);

                if (curIndex != curRandomSkillPattern_num)
                {
                    //중복 패턴 없도록
                    curIndex = curRandomSkillPattern_num;
                    break;
                }
                yield return null;
            }

            SettingSkill04Pattern(curRandomSkillPattern_num);
            if (curRandomSkillPattern_num < 4)
            {
                //*1~3 번 패턴
                CreateTargetMarker();
            }
            else if (curRandomSkillPattern_num == 4)
            {
                //* 4번 패턴
                CreateTargetMarker_Pattern04();
            }
            else if (curRandomSkillPattern_num == 5)
            {
                //* 5번 패턴
                CreateTargetMarker_5();
            }
            yield return new WaitUntil(() => skillOver == true && curTargetMarker.Count == 0);
            count++;
            isTrigger_si = false;
        }

        //! 스킬 끝
        EndSkill(BossMonsterMotion.Skill04);
        skill04_Co = null;

        yield return null;
    }
    //! 스킬 4의 패턴 1~3
    public void CreateTargetMarker()
    {
        float mAngle = UnityEngine.Random.Range(0, 70);
        skillOver = false;
        for (int i = 0; i < createTargetMarker; i++)
        {
            if (i > 0)
            {
                mAngle += angle;
            }

            StartCoroutine(SkillActivation(mAngle, i, true));
        }
    }
    //! 스킬 4의 패턴 4
    public void CreateTargetMarker_Pattern04()
    {
        skill04_Pattern04_Co = StartCoroutine(CreateTargetMarker_4_co());
    }

    IEnumerator CreateTargetMarker_4_co()
    {

        float mAngle = 180 + GameManager.instance.GetAngleSeparation(transform.position, transform.forward * 20, playerController.gameObject.transform.position);
        Debug.Log($"angle {mAngle}");

        bool isleftRight = PlayerLocationCheck_LeftRight();

        if (isleftRight == false) //* 왼쪽일 경우,
        {
            mAngle *= -1;
        }

        //float mAngle = 0f;
        float time = 0;
        skillOver = false;

        for (int i = 0; i < createTargetMarker; i++)
        {
            if (i > 0)
            {
                mAngle += angle;
            }
            StartCoroutine(SkillActivation(mAngle, i, false));

            while (true)
            {
                if (i >= (createTargetMarker - 1))
                    break;
                time += Time.deltaTime;

                if (time > 0.8f)
                {
                    time = 0;
                    break;
                }
                yield return null;
            }
        }
        skill04_Pattern04_Co = null;
        yield return null;
    }



    IEnumerator SkillActivation(float mAngle, int index = 0, bool simultaneous = true)
    {
        //스킬 targetMarkerList
        //* 타임 세팅---------------------------//
        float waitTime = 2;//빨간색 경고후 기다리는 시간
        float electricity_DurationTime = 5;//빨간색 경고후, 번개 친 후 지속 시간
        float endSkillTime = 2 + electricity_DurationTime; //스킬이 끝나는 시간
                                                           //---------------------------------------//
        GameObject skillIndicator_obj;
        float posY = GetGroundPos(transform).y;
        //* 오브젝트 풀링 ---------------------------------------------------------------------------------//
        if (targetMarkerList.Count == 0)
        {
            skillIndicator_obj = Instantiate(targetMarker_Prefabs, transform.position, Quaternion.identity);
            skillIndicator_obj.transform.SetParent(transform);
        }
        else
        {
            skillIndicator_obj = targetMarkerList[0];
            targetMarkerList.RemoveAt(0);
            skillIndicator_obj.SetActive(true);
        }

        //*------------------------------------------------------------------------------------------------//
        //* 세팅-------------------------------------------------------------------------------------------//
        skillIndicator_obj.transform.position = new Vector3(transform.position.x, posY + 0.05f, transform.position.z);
        Quaternion originRotate = skillIndicator_obj.transform.rotation;
        Skill_Indicator skill_Indicator = skillIndicator_obj.GetComponent<Skill_Indicator>();
        skill_Indicator.Init();
        curTargetMarker.Add(skill_Indicator);

        skill_Indicator.SetBounds();
        skill_Indicator.SetAngle(mAngle);

        Quaternion rotation = Quaternion.Euler(0f, mAngle, 0f);
        skillIndicator_obj.transform.rotation = skillIndicator_obj.transform.rotation * rotation;

        //*------------------------------------------------------------------------------------------------//
        yield return new WaitForSeconds(waitTime); //* 8초 뒤 체크

        //* 번개, 파지직 번개
        StartCoroutine(ElectricityProduction(skill_Indicator, electricity_DurationTime, mAngle, simultaneous));

        yield return new WaitForSeconds(endSkillTime); //* 7초후 종료
                                                       //* 스킬끝났음.----------------------------------------------//
                                                       //전기 공격 끄기

        if (!skillOver)
            skillOver = true;

        for (int i = 0; i < skill_Indicator.electricity_Effects.Count; ++i)
        {
            skill_Indicator.electricity_Effects[i].StopEffect();
        }

        yield return new WaitUntil(() => skill_Indicator.electricity_Effects.Count == 0);

        skill_Indicator.CheckTrigger(false);

        skill_Indicator.gameObject.transform.rotation = originRotate;

        curTargetMarker.Remove(skill_Indicator);
        //*------------------------------------------------------------//
        if (targetMarkerList.Count > 8)
        {
            Destroy(skillIndicator_obj);
        }
        else
        {
            targetMarkerList.Add(skillIndicator_obj);
            skillIndicator_obj.SetActive(false);
        }

        if (index == (createTargetMarker - 1))
        {
            stopSkillPattern = true;
        }

    }
    //*--------------------------------------------------------//
    //* ### 체크문양 패턴
    public void CreateTargetMarker_5()
    {
        skillOver = false;
        if (targetMarker_Pattern05_obj == null)
        {
            targetMarker_Pattern05_obj = Instantiate(targetMarker_Pattern05_Prefabs, GameManager.instance.transform.position, Quaternion.identity);
            targetMarker_Pattern05_obj.transform.SetParent(GameManager.instance.transform);

            Transform targetMarker_trans = targetMarker_Pattern05_obj.GetComponent<Transform>();
            foreach (Transform child in targetMarker_trans)
            {
                Skill_Indicator skill_Indicator = child.GetComponent<Skill_Indicator>();
                if (skill_Indicator != null)
                {
                    if (skill_Indicator.gameObject.activeSelf == true)
                        targetMarker_Pattern05_List.Add(skill_Indicator);
                }
            }
        }
        StartCoroutine(SkillActivation_Pattern05(-1));
    }

    IEnumerator SkillActivation_Pattern05(float mAngle)
    {
        //스킬 targetMarkerList
        //* 타임 세팅---------------------------//
        float waitTime = 2;//빨간색 경고후 기다리는 시간
        float electricity_DurationTime = 5;//빨간색 경고후, 번개 친 후 지속 시간
        float endSkillTime = 2 + electricity_DurationTime; //스킬이 끝나는 시간
                                                           //---------------------------------------//
        GameObject skillIndicator_obj;
        float posY = GetGroundPos(transform).y;
        //* 오브젝트 풀링 ---------------------------------------------------------------------------------//
        skillIndicator_obj = targetMarker_Pattern05_obj;
        //* 세팅-------------------------------------------------------------------------------------------//
        skillIndicator_obj.transform.position = new Vector3(transform.position.x, posY + 0.05f, transform.position.z);
        for (int i = 0; i < targetMarker_Pattern05_List.Count; ++i)
        {
            curTargetMarker.Add(targetMarker_Pattern05_List[i]);
        }
        //*------------------------------------------------------------------------------------------------//
        yield return new WaitForSeconds(waitTime); //* 8초 뒤 체크

        //* 번개, 파지직 번개
        for (int i = 0; i < targetMarker_Pattern05_List.Count; ++i)
        {
            StartCoroutine(ElectricityProduction(targetMarker_Pattern05_List[i], electricity_DurationTime, mAngle));
        }

        yield return new WaitForSeconds(endSkillTime); //* 7초후 종료
                                                       //* 스킬끝났음.----------------------------------------------//
                                                       //전기 공격 끄
        if (!skillOver)
            skillOver = true;



        for (int i = 0; i < targetMarker_Pattern05_List.Count; ++i)
        {
            for (int j = 0; j < targetMarker_Pattern05_List[i].electricity_Effects.Count; ++j)
            {
                targetMarker_Pattern05_List[i].electricity_Effects[j].StopEffect();
            }
        }
        for (int i = 0; i < targetMarker_Pattern05_List.Count; ++i)
        {
            yield return new WaitUntil(() => targetMarker_Pattern05_List[i].electricity_Effects.Count == 0);
        }

        for (int i = 0; i < targetMarker_Pattern05_List.Count; ++i)
        {
            targetMarker_Pattern05_List[i].CheckTrigger(false);
            curTargetMarker.Remove(targetMarker_Pattern05_List[i]);
        }

        skillIndicator_obj.SetActive(false);

        stopSkillPattern = true;
    }

    bool isTrigger_si = false; // 동시 체크 트리거
    IEnumerator ElectricityProduction(Skill_Indicator skill_Indicator, float duration, float angle, bool simultaneous = true)
    {
        float durationTime = duration / 3;
        int count = 0;
        float time = 0;
        Vector3 randomPos = Vector3.zero;
        bool isTrigger = false;
        bool getBounds = false;
        while (time < 2f)
        {
            time += Time.deltaTime;
            if (angle == -1)
                getBounds = true;
            randomPos = skill_Indicator.GetRandomPos(getBounds);
            Effect effect = GameManager.Instance.objectPooling.ShowEffect("LightningStrike2_red", skill_Indicator.gameObject.transform);

            effect.transform.position = randomPos;

            if (!isTrigger_si)
            {
                if (time > 1f && !isTrigger)
                {
                    if (simultaneous)
                        SimultaneousCheck();
                    else
                    {
                        isTrigger = true;
                        skill_Indicator.CheckTrigger(true); // 트리거 ON
                        if (CheckPlayerInMarker_co == null)
                        {
                            CheckPlayerInMarker_co = StartCoroutine(CheckPlayerInMarker());
                        }
                    }
                }
            }
            if (angle != -1)
            {
                Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
                effect.transform.localPosition = rotation * effect.transform.localPosition;
            }
            yield return null;
        }

        time = 0;
        while (count < 3)
        {
            while (true)
            {
                if (time < durationTime)
                {
                    time += Time.deltaTime;
                    if (angle == -1)
                        getBounds = true;
                    randomPos = skill_Indicator.GetRandomPos(getBounds);

                    Effect effect = GetDamage_electricity(randomPos, skill_Indicator.gameObject.transform, angle);
                    effect.finishAction = () =>
                    {
                        skill_Indicator.electricity_Effects.Remove(effect);
                    };
                    skill_Indicator.electricity_Effects.Add(effect);
                    yield return null;
                }
                else
                {
                    count++;
                    time = 0;
                    break;
                }
            }
        }

        yield return null;
    }

    private void SimultaneousCheck()
    {
        isTrigger_si = true;
        for (int i = 0; i < curTargetMarker.Count; i++)
        {
            curTargetMarker[i].CheckTrigger(true); // 트리거 ON
        }
        if (CheckPlayerInMarker_co == null)
        {
            CheckPlayerInMarker_co = StartCoroutine(CheckPlayerInMarker());
        }
    }

    Coroutine CheckPlayerInMarker_co = null;
    bool stopSkillPattern = false;
    IEnumerator CheckPlayerInMarker()
    {
        LayerMask layerMask = LayerMask.GetMask("Monster");
        int count = 0;
        stopSkillPattern = false;

        while (!stopSkillPattern)
        {
            if (!playerController._currentState.isElectricShock)
            {
                count = 0;
                // 플레이어의 위치와 방향에서 아래로 레이케스트를 쏴서 오브젝트를 탐지
                Collider[] overlappedColliders = Physics.OverlapSphere(playerController.gameObject.transform.position, 0.3f, layerMask);
                foreach (Collider hit in overlappedColliders)
                {
                    Skill_Indicator skill_Indicator = hit.gameObject.gameObject.GetComponent<Skill_Indicator>();
                    if (skill_Indicator != null)
                    {
                        if (skill_Indicator.checkTrigger)
                        {
                            count++;
                        }
                    }
                }

                if (count > 0)
                {
                    //* 만약 플레이어가 indicator에 플레이어가 포함 되어있다면?
                    playerMovement.PlayerElectricShock(true);
                    float damage = 20 * count;
                    m_monster.OnHit(damage);
                }
            }

            yield return null;
        }

        CheckPlayerInMarker_co = null;
    }

    public void StopSkill04()
    {
        if (skill04_Co != null)
            StopCoroutine(skill04_Co);
        if (skill04_Pattern04_Co != null)
            StopCoroutine(skill04_Pattern04_Co);
        if (stopSkillPattern == true)
            stopSkillPattern = false;
        EndSkill(BossMonsterMotion.Skill04);
    }



    #endregion

    //*----------------------------------------------------------------------------------------------------------//
    //* 피격 이펙트
    #region 피격
    private void GetHit()
    {
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("explosion_360_v1_S");
        effect.gameObject.transform.position = curHitPos;
        effect.gameObject.transform.rotation = curHitQuaternion;

        effect = GameManager.Instance.objectPooling.ShowEffect("FX_Shoot_04_hit");
        effect.gameObject.transform.position = curHitPos;
        effect.gameObject.transform.rotation = curHitQuaternion;

        StartCoroutine(electricity_Damage(2f, curHitPos));
    }

    IEnumerator electricity_Damage(float duration, Vector3 curHitPos, float range = 1, float randomMin = 0, float randomMax = 0.5f)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;

            float x = UnityEngine.Random.Range(-range, range);
            float y = UnityEngine.Random.Range(-range, range);
            float z = UnityEngine.Random.Range(-range, range);
            Vector3 randomPos = new Vector3(x, y, z);
            randomPos = curHitPos + randomPos;
            GetDamage_electricity(randomPos);

            float randomTime = UnityEngine.Random.Range(randomMin, randomMax);
            yield return new WaitForSeconds(randomTime);
            time += randomTime;
        }
    }
    #endregion

    //*----------------------------------------------------------------------------------------------------------//
    //* 죽음 구현
    IEnumerator DeathBossMonster()
    {
        useExplosionSound = false;
        //*모든 스킬 멈추기
        StopAtackCoroutine();
        //- 정지
        if (!navMeshAgent.enabled)
            NavMesh_Enable(true);
        SetMove_AI(false);
        SetAnimation(MonsterAnimation.Idle);

        //- 다리 몸통에서 전기와 폭탄 나오도록.
        for (int i = 0; i < m_monster.monsterData.weaknessList.Count; ++i)
        {
            StartCoroutine(Death_Production(m_monster.monsterData.weaknessList[i].position, 1));
        }
        Vector3 neckPos = new Vector3(bossNeck.position.x - 2, bossNeck.position.y, bossNeck.position.z);
        StartCoroutine(Death_Production(neckPos, 3));


        //- 사라질때는 전기만.
        yield return new WaitForSeconds(8f);
        useExplosionSound = true;
        for (int i = 0; i < m_monster.monsterData.weaknessList.Count; ++i)
        {
            StartCoroutine(Explode_Damage(0.1f, m_monster.monsterData.weaknessList[i].position, 1, 0, 0.1f, true));
        }
        StartCoroutine(Explode_Damage(0.1f, neckPos, 3, 0, 0.1f, true));
        StartCoroutine(Explode_Damage(0.1f, neckPos, 3, 0, 0.1f, true));
        yield return new WaitForSeconds(0.3f);

        GameManager.instance.cameraController.BossCameraReset(0.4f);
        UIManager.Instance.PadeInBlack(1f);
        useExplosionSound = false;
        this.gameObject.SetActive(false);
    }

    IEnumerator Death_Production(Vector3 point, float range = 1)
    {
        //2초동안은 1초만큼 뜨문뜨문 생기도록
        //3초 동안은 0.2~0.5만큼 빠르게 생기도록
        float duration = 2;
        StartCoroutine(electricity_Damage(duration, point, range, 0.3f, 0.7f));
        StartCoroutine(Explode_Damage(duration, point, range, 3f, 5f));

        yield return new WaitForSeconds(duration);

        duration = 5;
        StartCoroutine(electricity_Damage(duration, point, range, 0.1f, 0.3f));
        StartCoroutine(Explode_Damage(duration, point, range, 1f, 5f));
        GameManager.Instance.cameraController.cameraShake.ShakeCamera(2f, 3f, 3f);

        yield return null;
    }

    IEnumerator Explode_Damage(float duration, Vector3 curHitPos, float range = 1, float randomMin = 0, float randomMax = 0.5f, bool useOneSound = false)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;

            float randomTime = UnityEngine.Random.Range(randomMin, randomMax);
            yield return new WaitForSeconds(randomTime);

            time += randomTime;

            float x = UnityEngine.Random.Range(-range, range);
            float y = UnityEngine.Random.Range(-range, range);
            float z = UnityEngine.Random.Range(-range, range);
            Vector3 randomPos = new Vector3(x, y, z);
            randomPos = curHitPos + randomPos;

            GameManager.Instance.cameraController.cameraShake.ShakeCamera(1f, 2f, 2f);


            Effect effect = GameManager.Instance.objectPooling.ShowEffect("BossMonsterDeath");
            effect.transform.position = randomPos;
            //! 사운드


            if (!useOneSound)
                m_monster.SoundPlay(Monster.monsterSound.Death, false);
            else if (useOneSound && !useExplosionSound)
            {
                useExplosionSound = true;
                m_monster.SoundPlay(Monster.monsterSound.Death, false);
            }
        }
    }

    public override void StopAtackCoroutine()
    {
        forcedReturnHome = true;
        Debug.Log("보스 몬스터 stop");
        ChangeMonsterState(MonsterState.Stop);
        isTracing = false;
        //Abyss 보스 몬스터의 경우 => 스킬 2번 4번만 멈추면 될 듯
        //*스킬 2
        if (ing_skill02)
        {
            if (skill02_Co != null)
            {
                StopCoroutine(skill02_Co);
                skill02_Co = null;

                if (!playerController._currentState.canGoForwardInAttack)
                    playerController._currentState.canGoForwardInAttack = true;

                EndSkill(BossMonsterMotion.Skill02);
            }
            if (skill02_MoveMonster_Co != null)
            {
                StopCoroutine(skill02_MoveMonster_Co);
                skill02_MoveMonster_Co = null;
                SetMove_AI(false);
                SetAnimation(MonsterAnimation.Idle);
            }
        }
        //* 스킬 3번
        if (ing_skill03)
        {
            boss_Abyss_Skill03.Stop_MonsterSkill03();
        }
        //* 스킬 4번
        if (ing_skill04)
            StopSkill04();

    }
    public override void StopMonster()
    {
        StopAtackCoroutine();
    }
    public override void StartMonster()
    {
        Debug.Log("start");
        forcedReturnHome = false;

        ChangeMonsterState(MonsterState.Tracing);
    }
    //*----------------------------------------------------------------------------------------------------------//
    //* --

    public override void Base_Phase_HP(bool production = true)
    {
        //HP로 나누는 페이즈
        float curHP = (float)m_monster.monsterData.HP;

        switch (curBossPhase)
        {
            case BossMonsterPhase.Phase1:
                //70%, 20%모두 체크
                if (curHP == 0)
                {
                    ChangeBossPhase(BossMonsterPhase.Death, production);
                }
                else if (curHP < Phase2_BossHP)
                {
                    //*페이즈 2
                    ChangeBossPhase(BossMonsterPhase.Phase2, production);
                }
                break;
            case BossMonsterPhase.Phase2:
                //20%체크
                if (curHP == 0)
                {
                    ChangeBossPhase(BossMonsterPhase.Death, production);
                }
                break;
            default:
                break;
        }
    }

    //*----------------------------------------------------------------------------------------------------------//
    //* --

    // *---------------------------------------------------------------------------------------------------------//
    //* 공동 함수
    public Vector3 GetRandomPos(float range, Vector3 targetPos, float targetY = 0, bool useY = false)
    {
        if (!useY)
            targetY = targetPos.y;

        float rangeX = range;
        float rangeZ = range;
        //로밍시, 랜덤한 위치 생성
        float randomX = UnityEngine.Random.Range(targetPos.x + ((rangeX / 2) * -1), targetPos.x + (rangeX / 2));
        float randomZ = UnityEngine.Random.Range(targetPos.z + ((rangeZ / 2) * -1), targetPos.z + (rangeZ / 2));

        return new Vector3(randomX, targetY, randomZ);
    }

    public List<Vector3> GetRoundPos(Vector3 targetPos)
    {
        //* 타겟의 radius 길이의 12 ~ 10시 반 포지션을 가지고 옴.
        float radius = 10f; // 원의 반지름
        List<float> angleList = new List<float>();
        List<Vector3> posList = new List<Vector3>();
        int num = 0;
        for (int i = 0; i < 8; i++)
        {
            if (i == 0)
                num = 0;
            else
                num += 45;

            angleList.Add(num);
        }
        for (int i = 0; i < angleList.Count; i++)
        {
            float angle = angleList[i];

            // 삼각 함수를 사용하여 좌표를 계산합니다.
            float x = targetPos.x + radius * Mathf.Cos(Mathf.Deg2Rad * angle);
            float z = targetPos.z + radius * Mathf.Sin(Mathf.Deg2Rad * angle);

            Vector3 pos = new Vector3(x, targetPos.y, z);
            posList.Add(pos);

        }

        return posList;
    }



    //*---------------------------------------------------------------------------------//
    public void NavMeshSurface_ReBuild()
    {
        if (navMeshSurface != null)
        {
            for (int i = 0; i < navMeshSurface.Count; ++i)
                navMeshSurface[i].BuildNavMesh();
        }
    }
    //*-------------------------------------------------------------------------------------//
    //* 타임라인 => 보스 일반 약점
    public override void DirectTheBossWeakness()
    {
        GameManager.instance.CutSceneSetting(true);
        GameManager.instance.cameraController.CinemachineSetting(true);
        //* 모든 것 멈추기
        CurSceneManager.instance.PlayTimeline("Abyss_Weakness_TimLine");
    }

    public void ShowBossWeaknessEffect()
    {
        if (m_monster.monsterData.useWeakness)
        {
            for (int i = 0; i < m_monster.monsterData.weaknessList.Count; i++)
            {
                m_monster.monsterData.weaknessList[i].gameObject.SetActive(true);
                BossWeakness bossWeakness = m_monster.monsterData.weaknessList[i].GetComponent<BossWeakness>();
                if (!bossWeakness.destroy_BossWeakness)
                {
                    bossWeakness.bossWeaknessEffect.gameObject.SetActive(true);
                }
            }
        }
    }
    public void EndDirectTheBossWeakness()
    {
        GameManager.instance.CutSceneSetting(false);
        GameManager.instance.cameraController.CinemachineSetting(false);
        EnableBossWeaknessEffect(false);
        //
    }

    //*-------------------------------------------------------------------------------------//
    //* 보스 마지막 약점 연출
    public override void DirectTheBossLastWeakness()
    {
        GameManager.instance.CutSceneSetting(true);
        GameManager.instance.cameraController.CinemachineSetting(true);
        //* 모든 것 멈추기
        CurSceneManager.instance.PlayTimeline("Abyss_LastWeakness_TimeLine");
    }

    //* 타임라인에서 사용되는 이펙트 
    public void MonsterLastWeakness_TimeLineEffect()
    {
        StartCoroutine(MonsterLastWeakness_TimeLineEffect_co());
    }

    IEnumerator MonsterLastWeakness_TimeLineEffect_co()
    {
        //* 연기 이펙트
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_03");
        Vector3 effectPos = transform.position;
        effectPos.y -= 1.5f;
        effect.transform.position = effectPos;

        yield return new WaitForSeconds(0.5f);

        //* 연기 이펙트
        effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_04");
        effectPos = transform.position;
        effectPos.y -= 2.5f;
        effect.transform.position = effectPos;

        Vector3 originPos = transform.position;
        effect = GameManager.Instance.objectPooling.ShowEffect("BossMonster_aura");
        originPos.y += 1.1f;
        effect.transform.position = originPos;

        yield return new WaitForSeconds(8f);

        if (m_monster.monsterData.haveLastWeakness)
        {
            for (int i = 0; i < m_monster.monsterData.lastWeaknessList.Count; i++)
            {
                m_monster.monsterData.lastWeaknessList[i].gameObject.SetActive(true);
                BossWeakness bossWeakness = m_monster.monsterData.lastWeaknessList[i].GetComponent<BossWeakness>();
                if (!bossWeakness.destroy_BossWeakness)
                {
                    bossWeakness.bossWeaknessEffect.gameObject.SetActive(true);
                }
            }
        }
    }
    //* 보스 마지막 약점 연출
    public void EndDirectorMonsterLastWeakness()
    {
        GameManager.instance.CutSceneSetting(false);
        GameManager.instance.cameraController.CinemachineSetting(false);
        EnableBossWeaknessEffect(false);
        curRemainWeaknessesNum = m_monster.monsterData.lastWeaknessList.Count;
    }



}
