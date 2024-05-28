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
//using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine.UIElements;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class MonsterPattern_Boss_Abyss : MonsterPattern_Boss
{
    //! 보스 몬스터 나락.
    public Boss_Abyss_Skill01 boss_Abyss_Skill01;
    public Boss_Abyss_Skill02 boss_Abyss_Skill02;
    public Boss_Abyss_Skill03 boss_Abyss_Skill03;
    public Boss_Abyss_Skill04 boss_Abyss_Skill04;

    public Vector3 bossForward; // 스킬 4번에 쓰임.

    [Header("연출에 쓰이는 Obj들")]
    public GameObject redImage;
    public GameObject bossText;

    public Transform prefabPos;
    public Vector3 centerPoint;

    public Transform bossNeck;

    [Header("몬스터 총알이 나가는 위치")]
    public Transform[] muzzlesL;
    public Transform[] muzzlesR;
    public Transform[] muzzlePos;

    public bool isJump = false;
    bool isDodge = false;

    bool startSkill = false;
    bool ing_skill01 = false;
    bool ing_skill02 = false;

    bool ing_skill03 = false;
    bool ing_skill04 = false;

    //* 사운드
    public bool useExplosionSound = false; // 폭발음 중복을 막기 위한..

    Coroutine changePhase02_Co = null;

    public List<NavMeshSurface> navMeshSurface;

    bool StartFirstScene = false;


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
        boss_Abyss_Skill04 = GetComponent<Boss_Abyss_Skill04>();
        boss_Abyss_Skill04.Init(this);

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

        ChangeMonsterState(MonsterState.Stop);
        originPosition = transform.position;
        bossForward = transform.forward;

        overlapRadius = m_monster.monsterData.overlapRadius; //플레이어 감지 범위.
        roaming_RangeX = m_monster.monsterData.roaming_RangeX; //로밍 범위 x;
        roaming_RangeZ = m_monster.monsterData.roaming_RangeZ; //로밍 범위 y;
        CheckRoam_Range();

        playerHide = true;

        curBossPhase = BossMonsterPhase.Phase2;

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



        centerPoint = GetGroundPos(transform);

        CheckBossHP();
        noAttack = false;

        //*----------------------------------------------------------------------//
        //* 몬스터 약점
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

        // 스타트 컷씬 
        //DirectFirstAppearance_TimeLine();
        //DirectTheBossLastWeakness();
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
        if (StartFirstScene == false)
        {
            StartFirstScene = true;
            DirectFirstAppearance_TimeLine();
        }

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
                        boss_Abyss_Skill04.Skill04();
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
                    //* 보스 약점 전부 끄기
                    if (m_monster.monsterData.useWeakness)
                    {
                        for (int i = 0; i < m_monster.monsterData.weaknessList.Count; ++i)
                        {
                            m_monster.monsterData.weaknessList[i].gameObject.SetActive(false);
                        }
                    }
                    break;
                case BossMonsterPhase.Phase2:
                    // 2페이지 시작 연출
                    //! 연출후, Tracing으로 변환
                    if (changePhase02_Co == null)
                    {
                        changePhase02_Co = StartCoroutine(Phase02_Production());
                        //* 보스 약점 전부 키기
                        if (m_monster.monsterData.useWeakness)
                        {
                            for (int i = 0; i < m_monster.monsterData.weaknessList.Count; ++i)
                            {
                                m_monster.monsterData.weaknessList[i].gameObject.SetActive(true);
                            }
                        }
                    }
                    break;
                case BossMonsterPhase.Phase3:
                    //나락은 3페이즈 없음.
                    break;
                case BossMonsterPhase.Death:
                    break;
            }
        }
        Debug.Log($"CurBossPhase_후 {curBossPhase}");
    }

    //* 보스 연출---------------------------------------------------------------------------------------------------------------------//
    #region 보스 페이즈 넘어가는 연출 (+ 시네머신 컷씬)
    //* 페이즈 02
    bool CheckPlayerPos = false;
    IEnumerator Phase02_Production()
    {
        float time = 0;
        noAttack = true;
        yield return new WaitUntil(() => startSkill == false);

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
            m_monster.SoundPlay("Boss_ChangePhase", true);

            yield return new WaitForSeconds(10f);

            //! 사운드 멈춤
            m_monster.SoundPlayStop("Boss_ChangePhase");
            GameManager.Instance.cameraController.cameraShake.ShakeCamera(1f, 3f, 3f);
            //* 연기 이펙트
            effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_04");
            effectPos = transform.position;
            effectPos.y -= 2.5f;
            effect.transform.position = effectPos;

            yield return new WaitForSeconds(2f);
            GameManager.instance.PadeIn_Alpha(redImage, false, 0);
            bossText.SetActive(false);
            CheckPlayerPos = false;
            //* 타임 라인
            DirectTheBossWeakness();
        }
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
    #endregion
    // *---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 로밍(시네머신 등장 신.)
    public override void Roam_Monster()
    {
        if (!isRoaming && !GameManager.instance.isLoading)
        {
            isRoaming = true;
            //TODO: 나중에 범위안에 들어오면, 등장씬 나오도록 수정
            //* 일단은 바로 공격하도록
            //ChangeBossPhase(BossMonsterPhase.Phase2);
            //Monster_Motion(BossMonsterMotion.Skill04);
            Monster_Motion(BossMonsterMotion.Skill01);
            //* 테스트 후 아래 주석 풀기
            //ChangeBossPhase(BossMonsterPhase.Phase1);
            //ChangeMonsterState(MonsterState.Tracing);
        }
    }
    // *---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> Tracing (=> 발견 즉시 바로 스킬 시작)
    Coroutine startMonsterSkill_co = null;
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
                    //* 추적이 시작되면 몬스터 스킬 시작
                    if (startMonsterSkill_co != null)
                    {
                        StopCoroutine(startMonsterSkill_co);
                    }
                    startMonsterSkill_co = StartCoroutine(StartMonsterSkill());
                    break;
                default:
                    break;
            }
        }
    }

    //* 몬스터 스킬 시작
    IEnumerator StartMonsterSkill()
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
                break;
            }
            if (forcedReturnHome)
            {
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
                        breakTime = 1;
                    }
                    else
                        pickAgain = true;
                    break;
                case 1:
                    if (!ing_skill02)
                    {
                        //* 페이즈 2 이상이면, 스킬 2번에서 바로 스킬 3번으로 연계
                        Monster_Motion(BossMonsterMotion.Skill02);
                        breakTime = 2;
                    }
                    else
                        pickAgain = true;
                    break;
                case 2:
                    if (!ing_skill04)
                    {
                        //* 스킬 4번으로 연계
                        Monster_Motion(BossMonsterMotion.Skill04);
                        breakTime = 2;
                    }
                    break;
                default:
                    break;
            }
            if (!pickAgain)
            {
                startSkill = true;
                Debug.Log($"breakTime {breakTime}");
                //* 스킬이 끝날 때까지 기다림.
                yield return new WaitUntil(() => startSkill == false);
                Debug.Log("startSkill false");
                //* 쉬는 시간 (플레이어 공격 시간)

                if (curBossPhase != curBossP) //* 페이즈 넘어가면 break;
                {
                    break;
                }
                else
                {
                    yield return new WaitForSeconds(breakTime);
                    Debug.Log("break 끝");
                }

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
                Base_Phase_HP();
                break;
            case BossMonsterMotion.Skill02:
                if (curBossPhase == BossMonsterPhase.Phase1)
                {
                    //스킬 2는 페이즈 2,3 일때 바로 스킬 3으로 들어감.
                    startSkill = false;
                }
                ing_skill02 = false;
                Base_Phase_HP();
                break;
            case BossMonsterMotion.Skill03:
                startSkill = false;
                ing_skill03 = false;
                Base_Phase_HP();
                break;
            case BossMonsterMotion.Skill04:
                startSkill = false;
                ing_skill04 = false;
                Base_Phase_HP();
                break;
            default:
                break;
        }
    }

    //*피격----------------------------------------------------------------------------------------------------------//
    #region 피격
    private void GetHit()
    {
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

    //*죽음----------------------------------------------------------------------------------------------------------//
    #region 죽음
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

    //* 몬스터 몸에서 폭발
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


            Effect effect = GameManager.Instance.objectPooling.ShowEffect("explosion_360_v1_M");
            effect.transform.position = randomPos;
            //! 사운드


            if (!useOneSound)
            {
                // m_monster.SoundPlay(Monster.monsterSound.Death, false);
                m_monster.SoundPlay("Boss_Death", false);

            }
            else if (useOneSound && !useExplosionSound)
            {
                useExplosionSound = true;
                //m_monster.SoundPlay(Monster.monsterSound.Death, false);
                m_monster.SoundPlay("Boss_Death", false);
            }
        }
    }
    #endregion

    //*보스 몬스터 정지----------------------------------------------------------------------------------------------------------//
    #region 보스 몬스터 정지
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
            boss_Abyss_Skill02.Stop_MonsterSkill02();
        }
        //* 스킬 3번
        if (ing_skill03)
        {
            boss_Abyss_Skill03.Stop_MonsterSkill03();
        }
        //* 스킬 4번
        if (ing_skill04)
            boss_Abyss_Skill04.Stop_MonsterSkill04();
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

    #endregion
    //*----------------------------------------------------------------------------------------------------------//
    //* HP보고 Phase 나누기
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

    //* target 포스 주변의 원형의 좌표 리스트를 반환하는 함수
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
    #region 시네머신 컷씬 관련 함수 (타임라인)
    //* 보스 첫등장 씬 

    //* 타임라인에서 사용되는 이펙트 
    bool playerWalk = false;

    public void DirectFirstAppearance_TimeLine()
    {
        SetMove_AI(false);
        SetAnimation(MonsterAnimation.Idle);

        noAttack = true;
        GameManager.instance.CutSceneSetting(true);
        GameManager.instance.cameraController.CinemachineSetting(true);

        //*--------------------------------------------------------------------//
        //* 스킵 버튼
        Button_Controller.instance.skipBtn.onClick.RemoveAllListeners();
        Button_Controller.instance.skipBtn.onClick.AddListener(() =>
        {
            PlayableDirector director = CurSceneManager.instance.GetTimeLine("Abyss_FirstStart_TimeLine");
            director.Stop();

            if (playerWalk)
            {
                playerWalk = false;
            }
            playerController._currentState.doNotRotate = false;
            EndDirectFirstAppearance();
            ShowBosHPBar();
            GameManager.instance.cameraController.CameraRecovery();

        });
        Button_Controller.instance.SetActiveBtn(Button_Controller.Btns.SkipBtn, true);

        //---------------------------------------------------------------------//

        //* 모든 것 멈추기
        CurSceneManager.instance.PlayTimeline("Abyss_FirstStart_TimeLine");


    }
    public void WalkPlayer()
    {
        playerWalk = true;
        StartCoroutine(WalkPlayer_co());
    }
    public void StopWalkPlayer()
    {
        playerWalk = false;
    }

    IEnumerator WalkPlayer_co()
    {
        Debug.Log("플레이어 움직임");
        playerController._currentState.doNotRotate = true;  // 플레이어 움직임 막음
        float duration = 7f;
        float initialMoveSpeed = 3;
        float elapsedTime = 0;

        while (playerWalk)
        {
            playerController._playerComponents.animator.SetFloat("Vertical", 0.5f, 0.05f, Time.deltaTime);   //상
            playerController._playerComponents.animator.SetFloat("Horizontal", 0, 0.05f, Time.deltaTime);  //하

            float moveSpeed = Mathf.Lerp(initialMoveSpeed, 0f, elapsedTime / duration);
            playerTrans.Translate(playerTrans.forward * moveSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;

            if (elapsedTime >= duration)
            { break; }

            yield return null;
        }
        playerController._playerComponents.animator.SetFloat("Vertical", 0, 0f, Time.deltaTime);   //상
        playerController._playerComponents.animator.SetFloat("Horizontal", 0, 0f, Time.deltaTime);  //하

        Debug.Log("플레이어 멈춤");
    }

    public void FirstAppearance_TimeLineEffect()
    {
        StartCoroutine(FirstAppearance_TimeLineEffect_co());
    }
    IEnumerator FirstAppearance_TimeLineEffect_co()
    {
        playerController._currentState.doNotRotate = false; // 플레이어 움직임 풀음
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
    }

    public void EndDirectFirstAppearance()
    {
        noAttack = false;
        GameManager.instance.CutSceneSetting(false);
        GameManager.instance.cameraController.CinemachineSetting(false);

        ChangeMonsterState(MonsterState.Roaming);

        Button_Controller.instance.SetActiveBtn(Button_Controller.Btns.SkipBtn, false);
    }

    public void ShowBosHPBar()
    {
        if (m_monster.HPBar_CheckNull() == false)
        {
            if (!m_monster.resetHP)
                m_monster.ResetHP();
            m_monster.GetHPBar();
        }
    }

    //* 보스 일반 약점------------------------------------------------------------------------//

    public override void DirectTheBossWeakness()
    {
        //*--------------------------------------------------------------------//
        //* 스킵 버튼
        Button_Controller.instance.skipBtn.onClick.RemoveAllListeners();
        Button_Controller.instance.skipBtn.onClick.AddListener(() =>
        {
            PlayableDirector director = CurSceneManager.instance.GetTimeLine("Abyss_Weakness_TimeLine");
            director.Stop();

            if (boss_Abyss_Skill03.Shield_Effect_skill03 != null)
            {
                boss_Abyss_Skill03.Shield_Effect_skill03.StopEffect();
                boss_Abyss_Skill03.Shield_Effect_skill03 = null;
            }

            EndDirectTheBossWeakness();
            GameManager.instance.cameraController.CameraRecovery();
        });

        Button_Controller.instance.SetActiveBtn(Button_Controller.Btns.SkipBtn, true);
        //---------------------------------------------------------------------//

        SetMove_AI(false);
        SetAnimation(MonsterAnimation.Idle);

        ChangeMonsterState(MonsterState.Stop);
        noAttack = true;
        GameManager.instance.CutSceneSetting(true);
        GameManager.instance.cameraController.CinemachineSetting(true);
        //* 모든 것 멈추기
        CurSceneManager.instance.PlayTimeline("Abyss_Weakness_TimeLine");
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
        noAttack = false;
        GameManager.instance.CutSceneSetting(false);
        GameManager.instance.cameraController.CinemachineSetting(false);
        EnableBossWeaknessEffect(false);

        Button_Controller.instance.SetActiveBtn(Button_Controller.Btns.SkipBtn, false);
    }

    //*-------------------------------------------------------------------------------------//
    //* 보스 마지막 약점 연출
    Effect auraEffect = null;
    //Coroutine monsterLastWeakness_co = null;
    public override void DirectTheBossLastWeakness()
    {
        SetMove_AI(false);
        SetAnimation(MonsterAnimation.Idle);

        //*--------------------------------------------------------------------//
        //* 스킵 버튼
        Button_Controller.instance.skipBtn.onClick.RemoveAllListeners();
        Button_Controller.instance.skipBtn.onClick.AddListener(() =>
        {
            //Debug.Log("1");
            PlayableDirector director = CurSceneManager.instance.GetTimeLine("Abyss_LastWeakness_TimeLine");
            director.Stop();

            EndDirectorMonsterLastWeakness();
            GameManager.instance.cameraController.CameraRecovery();
        });

        Button_Controller.instance.SetActiveBtn(Button_Controller.Btns.SkipBtn, true);
        //---------------------------------------------------------------------//

        noAttack = true;
        GameManager.instance.CutSceneSetting(true);
        GameManager.instance.cameraController.CinemachineSetting(true);
        //* 모든 것 멈추기
        CurSceneManager.instance.PlayTimeline("Abyss_LastWeakness_TimeLine");
    }

    //* 타임라인에서 사용되는 이펙트 

    public void MonsterLastWeakness_TimeLineEffect()
    {
        Coroutine monsterLastWeakness_co = StartCoroutine(MonsterLastWeakness_TimeLineEffect_co());
        Button_Controller.instance.skipBtn.onClick.AddListener(() =>
        {
            //Debug.Log("2");
            StopCoroutine(monsterLastWeakness_co);
            monsterLastWeakness_co = null;
        });
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
        auraEffect = GameManager.Instance.objectPooling.ShowEffect("BossMonster_aura");
        originPos.y += 1.1f;
        auraEffect.transform.position = originPos;

        auraEffect.finishAction = () =>
        {
            auraEffect = null;
        };

        Button_Controller.instance.skipBtn.onClick.AddListener(() =>
        {
            //Debug.Log("3");
            if (auraEffect != null)
            {
                //Debug.Log("4");
                auraEffect.gameObject.SetActive(false);
                Destroy(auraEffect);
                auraEffect = null;
            }
        });


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
        noAttack = false;
        GameManager.instance.CutSceneSetting(false);
        GameManager.instance.cameraController.CinemachineSetting(false);
        EnableBossWeaknessEffect(false);
        curRemainWeaknessesNum = m_monster.monsterData.lastWeaknessList.Count;

        Button_Controller.instance.SetActiveBtn(Button_Controller.Btns.SkipBtn, false);
    }

    # endregion


}