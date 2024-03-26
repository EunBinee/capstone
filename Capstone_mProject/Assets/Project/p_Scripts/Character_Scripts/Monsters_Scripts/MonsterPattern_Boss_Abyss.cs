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

public class MonsterPattern_Boss_Abyss : MonsterPattern_Boss
{
    //! 보스 몬스터 나락.

    [Header("스킬 02 잔해물 범위")]
    public int rangeXZ = 50;
    public List<Wreckage> wreckages;
    public Transform prefabPos;

    GameObject wreckage_obj; //실제 게임에서 사용될 잔해물 오브젝트
    public GameObject redImage;
    public GameObject bossText;
    public Vector3 centerPoint;

    List<Vector3> randomPos_skill02;
    [Space]
    [Header("스킬 03")]
    public Transform bossNeck;
    bool findPlayer = false;
    public float skillRadius = 10;
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

    bool isJump = false;
    bool isDodge = false;

    bool startSkill = false;
    bool ing_skill01 = false;
    bool ing_skill02 = false;

    bool ing_skill03 = false;
    bool ing_skill04 = false;

    //보스 페이즈 체력 기준
    //코루틴
    Coroutine skill02_MoveMonster_Co = null;
    Coroutine skill02_Co = null;
    Coroutine changePhase02_Co = null;


    public List<NavMeshSurface> navMeshSurface;

    public override void Init()
    {
        m_monster = GetComponent<Monster>();
        m_animator = GetComponent<Animator>();

        //rigid = GetComponent<Rigidbody>();
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
        switch (monsterMotion)
        {
            case BossMonsterMotion.Skill01:
                //*내려찍기
                ing_skill01 = true;
                Skill01();
                break;
            case BossMonsterMotion.Skill02:
                //* 폭탄
                ing_skill02 = true;
                Skill02();
                break;
            case BossMonsterMotion.Skill03:
                if (curBossPhase != BossMonsterPhase.Phase1)
                {
                    //* 총
                    ing_skill03 = true;
                    Skill03();
                }
                break;
            case BossMonsterMotion.Skill04:
                if (curBossPhase != BossMonsterPhase.Phase1)
                {
                    //* 전기
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
        StartCoroutine(JumpUp());
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

            StartCoroutine(JumpDown(newRandomPos));
            yield return new WaitUntil(() => isJump == false);
            GameManager.Instance.cameraController.AttentionMonster();
            GameManager.Instance.cameraController.banAttention = true;
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

            //*s나중에 주석 풀기 !
            GameManager.instance.PadeIn_Alpha(redImage, false, 0);
            // GameManager.instance.PadeIn_Alpha(redImage, true, 0, false);
            bossText.SetActive(false);
            CheckPlayerPos = false;
            Base_Phase_HP(false);
            yield return new WaitForSeconds(1f);
            ChangeMonsterState(MonsterState.Tracing);
            changePhase02_Co = null;
            GameManager.Instance.cameraController.UndoAttention();
        }
    }

    IEnumerator CheckPlayer_Production()
    {
        while (CheckPlayerPos)
        {
            CheckPlayerInMonster_skill03(skillRadius);
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
            if (curBossPhase != curBossP)
                break;

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
                //다시 뽑기
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
    //* 스킬 01 내려찍기
    #region 스킬 01

    private void Skill01()
    {
        StartCoroutine(BossAbyss_Skill01());
    }

    IEnumerator BossAbyss_Skill01()
    {
        //* 이펙트
        Vector3 originPos = transform.position;
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("HeartOfBattle_01");
        effect.transform.position = originPos;

        yield return new WaitForSeconds(1f);
        //* 점프 --------------------------------------------------------------------//
        isJump = true;
        StartCoroutine(JumpUp());
        yield return new WaitUntil(() => isJump == false);
        //*-------------------------------------------------------------------------------//
        if (curMonsterState != MonsterState.Death)
        {
            //* 플레이어를 쫓아 다니는 이펙트 
            float duration = 5f;
            StartCoroutine(FollowPlayer_Effect_InSkill01(duration));
            yield return new WaitForSeconds(duration);
            Vector3 curPlayerPos = playerTrans.position;

            isJump = true;
            StartCoroutine(JumpDown(curPlayerPos));
            yield return new WaitUntil(() => isJump == false);
        }
        //------------------------------------------------------------------------------------//


        EndSkill(BossMonsterMotion.Skill01);
    }

    IEnumerator JumpUp()
    {
        //*네비메쉬 끄기
        noAttack = true;

        NavMesh_Enable(false);
        Vector3 originPos = transform.position;
        float speed = 30;
        float time = 0;


        SetBossAttackAnimation(BossMonsterAttackAnimation.Skill01, 0);
        yield return new WaitForSeconds(1f);

        if (curMonsterState != MonsterState.Death)
        {
            //------------------------------------------------------------------------------------//
            //*점프전 주목 풀기
            GameManager.instance.cameraController.AttentionBan(true);
            //-----------------------------------------------------------------------------------//

            // 점프전 잠깐 밑으로 내려감.
            while (time < 0.1)
            {
                time += Time.deltaTime;
                transform.Translate(-Vector3.up * speed * Time.deltaTime);
                yield return null;
            }
            //? 연기이펙트-----------------------------------------------------------------------//

            Effect effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_02");
            Vector3 effectPos = originPos;
            effectPos.y += 2.5f;
            effect.transform.position = effectPos;
            //-------------------------------------------------------------------------------------//
            GameManager.Instance.cameraController.cameraShake.ShakeCamera(1f, 2, 2);
            //*점프
            time = 0;
            Vector3 targetPos = transform.position + (Vector3.up * 60);
            while (time < 5f)
            {
                time += Time.deltaTime;
                speed = Mathf.Lerp(90, 60, Time.time);
                transform.Translate(Vector3.up * speed * Time.deltaTime);

                if (transform.position.y >= targetPos.y)
                    break;
                yield return null;
            }
        }
        else
        {
            SetBossAttackAnimation(BossMonsterAttackAnimation.Skill01, 2);
        }


        isJump = false;
    }

    IEnumerator JumpDown(Vector3 curPlayerPos, bool getDamage = true)
    {
        Debug.Log($"curPlayerPos   {curPlayerPos}");
        float speed;
        float time = 0;
        transform.position = new Vector3(curPlayerPos.x, transform.position.y, curPlayerPos.z);

        speed = 50f;
        SetBossAttackAnimation(BossMonsterAttackAnimation.Skill01, 1);

        while (time < 5f)
        {
            time += Time.deltaTime;
            speed = Mathf.Lerp(50, 90, Time.time);
            transform.Translate(-Vector3.up * speed * Time.deltaTime);
            if (transform.position.y <= curPlayerPos.y)
                break;
            yield return null;
        }
        Debug.Log($"curMonster 2  {transform.position}");

        transform.position = new Vector3(curPlayerPos.x, curPlayerPos.y, curPlayerPos.z);

        //! 사운드
        m_monster.SoundPlay(Monster.monsterSound.Alarm, false);
        if (getDamage)
            CheckPlayerDamage(6.5f, transform.position, 20, true);

        //? 연기이펙트-----------------------------------------------------------------------//
        GameManager.Instance.cameraController.cameraShake.ShakeCamera(1f, 3, 3);
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_03");
        Vector3 effectPos = transform.position;
        effectPos.y -= 1.5f;
        effect.transform.position = effectPos;

        //* 점프 후 주목 가능
        GameManager.instance.cameraController.AttentionBan(false);
        //- 떨어지고 나서 주목 On
        //GameManager.instance.cameraController.AttentionMonster();

        isJump = false;
        if (curMonsterState != MonsterState.Death)
            noAttack = false;
        NavMesh_Enable(true);

        SetMove_AI(false);
        SetAnimation(MonsterAnimation.Idle);

    }

    IEnumerator FollowPlayer_Effect_InSkill01(float duration)
    {
        //* 스킬01 내려찍기 중, 플레이어를 쫒아다니는 이펙트 
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("PulseGrenade_01");
        EffectController effectController = effect.gameObject.GetComponent<EffectController>();
        effectController.ChangeSize();

        Vector3 GroundPos = GetGroundPos(playerTrans);
        effect.transform.position = GroundPos; // playerTrans.position;
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            GroundPos = GetGroundPos(playerTrans);
            effect.transform.position = GroundPos; // playerTrans.position;
            yield return null;
        }

        yield return new WaitForSeconds(4f);
        effect.StopEffect();
    }

    #endregion

    // *---------------------------------------------------------------------------------------------------------//
    //* 스킬 02  폭탄 떨구기
    #region 스킬 02

    private void Skill02()
    {
        skill02_Co = StartCoroutine(BossAbyss_Skill02());
    }

    IEnumerator BossAbyss_Skill02()
    {
        yield return new WaitForSeconds(4f);
        //* 몬스터 뒤로 이동하는 코루틴
        if (skill02_MoveMonster_Co != null)
        {
            StopCoroutine(skill02_MoveMonster_Co);
        }

        skill02_MoveMonster_Co = StartCoroutine(MoveMonster_Skill02());

        yield return new WaitForSeconds(2f);
        float time = 0;
        bool getRandomPos = false;
        Vector3 newRandomPos = Vector3.zero;
        Vector3 curMonsterPoint;

        float getrandomTime = 0;
        while (time < 15)
        {
            time += Time.deltaTime;
            float randTime = UnityEngine.Random.Range(0.5f, 3f);
            yield return new WaitForSeconds(randTime);
            time += randTime;

            while (!getRandomPos)
            {
                getrandomTime += Time.deltaTime;
                getRandomPos = true;
                curMonsterPoint = GetGroundPos(playerTrans);
                newRandomPos = GetRandomPos(3f, curMonsterPoint);
                foreach (Vector3 randomPos in randomPos_skill02)
                {
                    if (Vector3.Distance(newRandomPos, randomPos) <= 4f)
                    {
                        getRandomPos = false;
                        break;
                    }
                }

                if (getRandomPos)
                {
                    randomPos_skill02.Add(newRandomPos);
                }

                if (getrandomTime > 3f)
                {
                    //1.5초 동안 못찾으면 걍 break;
                    getrandomTime = 0;
                    break;
                }
                yield return null;
            }

            if (getRandomPos)
                StartCoroutine(SetBomb(newRandomPos));
            getRandomPos = false;
            yield return null;
        }

        yield return new WaitForSeconds(5f);

        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_03");
        Vector3 effectPos = transform.position;
        effectPos.y -= 1.5f;
        effect.transform.position = effectPos;

        effect = GameManager.Instance.objectPooling.ShowEffect("MC01_Red_02");
        EffectController effectController = effect.GetComponent<EffectController>();
        effectController.ChangeSize();
        effect.transform.position = transform.position;


        GameManager.Instance.cameraController.cameraShake.ShakeCamera(1f, 3, 3);

        yield return new WaitForSeconds(0.7f);

        //* 보스 주변에 공격--------------------------------------------
        //* 보스의 마지막 공격

        if (skill02_MoveMonster_Co != null)
        {
            StopCoroutine(skill02_MoveMonster_Co);
            skill02_MoveMonster_Co = null;

            SetMove_AI(false);
            SetAnimation(MonsterAnimation.Idle);
        }
        curMonsterPoint = GetGroundPos(transform);
        List<Vector3> roundPos = GetRoundPos(curMonsterPoint);
        useExplosionSound = false;
        foreach (Vector3 pos in roundPos)
        {
            randomPos_skill02.Add(pos);
            StartCoroutine(SetBomb(pos, true, true));
        }
        //*-------------------------------------------------------------
        //만약 플레이어가 현재 몬스터 아래에 있으면.. 공격할때 앞으로 이동하는 거 멈추기
        float radius = 7.5f;
        while (randomPos_skill02.Count != 0)
        {
            // 몬스터 아래에 있는지 확인
            Collider[] playerColliders = Physics.OverlapSphere(this.transform.position, radius, playerlayerMask);

            if (0 < playerColliders.Length)
            {
                // player 공격시 앞으로 이동 금지
                // 플레이어
                if (playerController._currentState.canGoForwardInAttack)
                    playerController._currentState.canGoForwardInAttack = false;
            }
            else
            {
                //여기 없으면 이동 풀기
                if (!playerController._currentState.canGoForwardInAttack)
                    playerController._currentState.canGoForwardInAttack = true;
            }

            yield return null;
        }

        if (!playerController._currentState.canGoForwardInAttack)
            playerController._currentState.canGoForwardInAttack = true;
        //*-------------------------------------------------------------

        if (curBossPhase != BossMonsterPhase.Phase1)
        {
            //* 잔해물 떨어지기
            yield return new WaitForSeconds(3f);
            StartCoroutine(SetWreckage());
            yield return new WaitForSeconds(1f);
            EndSkill(BossMonsterMotion.Skill02);
        }
        else
        {
            yield return new WaitForSeconds(1f);

            EndSkill(BossMonsterMotion.Skill02);
        }
        skill02_Co = null;

    }

    //* 보스 뒤로 이동
    IEnumerator MoveMonster_Skill02()
    {
        float time = 0;

        SetMove_AI(false);
        SetAnimation(MonsterAnimation.Idle);

        float defaultSpeed = navMeshAgent.speed;
        float defaultAcceleration = navMeshAgent.acceleration;
        NavMeshHit hit;

        randomPos_skill02.Clear();

        Vector3 targetDir = (playerTrans.position - transform.position).normalized;
        Vector3 monsterNewPos = transform.position + (-targetDir * 20);

        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_03");
        Vector3 effectPos = transform.position;
        effectPos.y -= 1.5f;
        effect.transform.position = effectPos;

        effect = GameManager.Instance.objectPooling.ShowEffect("MC01_Red_02");
        EffectController effectController = effect.GetComponent<EffectController>();
        effectController.ChangeSize();
        effect.transform.position = transform.position;


        GameManager.Instance.cameraController.cameraShake.ShakeCamera(1f, 3, 3);
        yield return new WaitForSeconds(0.7f);

        if (NavMesh.SamplePosition(monsterNewPos, out hit, 20f, NavMesh.AllAreas))
        {
            if (monsterNewPos != hit.position)
            {
                monsterNewPos = hit.position;
            }
            SetMove_AI(true);
            navMeshAgent.SetDestination(monsterNewPos);
            SetAnimation(MonsterAnimation.Move);

            while (true)
            {
                if (Vector3.Distance(monsterNewPos, transform.position) <= 2f)
                {
                    SetMove_AI(false);

                    //TODO: 몬스터 방향 플레이어 쪽으로 돌리기
                    time = 0;
                    while (time < 2)
                    {
                        time += Time.deltaTime;
                        Vector3 direction = playerTrans.position - transform.position;
                        Quaternion targetAngle = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetAngle, Time.deltaTime * 0.8f);

                        if (Quaternion.Angle(transform.rotation, targetAngle) < 1.5f)
                            break;

                        yield return null;
                    }
                    time = 0;
                    break;
                }
                yield return null;
            }

        }
        else
        {
            Debug.Log("보스가 못가는 곳입니다..");
        }

        SetAnimation(MonsterAnimation.Idle);
        skill02_MoveMonster_Co = null;

    }

    bool useExplosionSound = false;
    IEnumerator SetBomb(Vector3 randomPos, bool usePhase01 = false, bool soundCancle = false)
    {
        float time = 0;
        Effect effect;
        if (curBossPhase == BossMonsterPhase.Phase1 || usePhase01)
        {
            effect = GameManager.Instance.objectPooling.ShowEffect("RocketBarrage");
            effect.transform.position = randomPos;
            effect.finishAction = () =>
            {
                randomPos_skill02.Remove(randomPos);
            };

            yield return new WaitForSeconds(2f);
            //! 사운드
            if (!soundCancle)
                m_monster.SoundPlay(Monster.monsterSound.Hit_Close, false);
            else if (soundCancle && !useExplosionSound)
            {
                useExplosionSound = true;
                m_monster.SoundPlay(Monster.monsterSound.Hit_Close, false);
            }
        }
        else
        {
            effect = GameManager.Instance.objectPooling.ShowEffect("PulseGrenade_02");
            effect.transform.position = randomPos;
            yield return new WaitForSeconds(0.5f);
            effect = GameManager.Instance.objectPooling.ShowEffect("MeteorStrike");
            effect.transform.position = randomPos;
            effect.finishAction = () =>
            {
                randomPos_skill02.Remove(randomPos);
            };

            yield return new WaitForSeconds(1f);
            //! 사운드
            if (!soundCancle)
                m_monster.SoundPlay(Monster.monsterSound.Hit_Long2, false);
            else if (soundCancle && !useExplosionSound)
            {
                useExplosionSound = true;
                m_monster.SoundPlay(Monster.monsterSound.Hit_Long2, false);
            }
        }




        while (time < 5)
        {
            time += Time.deltaTime;
            if (curMonsterState == MonsterState.Death)
            {
                effect.StopEffect();
                break;
            }

            //TODO: 범위 지정
            bool playerTrue = CheckPlayerDamage(2f, randomPos, 20, true);

            if (playerTrue)
                break;

            yield return null;
        }
    }

    // 잔해물
    IEnumerator SetWreckage()
    {
        Vector3 wreckageRandomPos = Vector3.zero;

        if (wreckage_obj == null)
        {
            GameObject wreckagesPrefab = Resources.Load<GameObject>("GameObjPrefabs/" + "Wreckages");

            if (wreckagesPrefab == null)
            {
#if UNITY_EDITOR
                Debug.LogError("Projectile 프리펩 없음. 오류.");
#endif
            }
            else
            {
                //prefabPos에 생성
                wreckage_obj = UnityEngine.Object.Instantiate(wreckagesPrefab);
                wreckage_obj.gameObject.transform.SetParent(GameManager.instance.gameObject.transform);

                wreckages = new List<Wreckage>();

                Transform[] childTransforms = wreckage_obj.GetComponentsInChildren<Transform>(true);

                foreach (Transform childTransform in childTransforms)
                {
                    Wreckage wreckageComponent = childTransform.GetComponent<Wreckage>();
                    if (wreckageComponent != null)
                        wreckages.Add(wreckageComponent);
                }
            }
        }
        else
        {
            wreckage_obj.SetActive(true);
            wreckage_obj.gameObject.transform.SetParent(GameManager.instance.gameObject.transform);
        }

        //*잔해물 배치(플레이어와 떨어진 곳으로 지정)
        //randomPos 지정.

        List<Vector3> randomPosList = new List<Vector3>();
        NavMeshHit hit;
        Vector3 PosY = GetGroundPos(transform);
        Vector3 curMonsterPoint = centerPoint; //new Vector3(centerPoint.x, PosY.y, centerPoint.z);
        Debug.Log($"centerPoint {centerPoint}");
        for (int i = 0; i < wreckages.Count; i++)
        {
            Vector3 randomPos = Vector3.zero;

            bool stop = false;
            float time = 0;
            while (true)
            {
                time += Time.deltaTime;
                if (time < 2)
                {
                    randomPos = GetRandomPos(rangeXZ, curMonsterPoint);
                    if (Vector3.Distance(playerTrans.position, randomPos) >= 3f &&
                        Vector3.Distance(transform.position, randomPos) >= 10f)
                    {
                        // - 플레이어랑은 3 정도의 거리를 유지하고, 
                        // - 보스와는 20정도의 거리를 유지하고
                        if (NavMesh.SamplePosition(randomPos, out hit, 35f, NavMesh.AllAreas))
                        {
                            if (hit.position != randomPos)
                                randomPos = hit.position;
                            // - 네비에이전트가 움직일 수 있는 곳인지 체크
                            if (randomPosList.Contains(randomPos) == false)
                            {
                                //randomPos에도 없으면.. 생성
                                int j = 0;
                                bool canBreak = true;
                                while (j < randomPosList.Count)
                                {
                                    if (Vector3.Distance(randomPosList[j], randomPos) < 8f)
                                    {
                                        canBreak = false;
                                        break;
                                    }
                                    ++j;
                                }
                                if (canBreak)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    stop = true;
                    break;
                }
            }
            if (!stop)
            {
                randomPosList.Add(randomPos);
            }
            else
                break;
        }
        yield return new WaitForSeconds(1f);

        //떨어뜨리기 전 경고 이펙트 && 떨어뜨리기
        for (int i = 0; i < randomPosList.Count; ++i)
        {
            wreckages[i].m_monster = this.m_monster;
            wreckages[i].gameObject.SetActive(true);
            wreckages[i].StartDropWreckage(randomPosList[i]);
        }
        //떨어뜨리기   

        //전부 떨어지게 하면 네비 서페이스 (?)e 다시 Bake해주기
        bool navReBuild = false;
        while (true)
        {
            navReBuild = true;
            for (int i = 0; i < randomPosList.Count; ++i)
            {
                if (!wreckages[i].finishDrop)
                {
                    navReBuild = false;
                    break;
                }
            }
            if (navReBuild)
                break;
            yield return null;
        }

        //playerController.NavMeshSurface_ReBuild();
        // 사라지게 하기
        yield return null;
        if (curMonsterState != MonsterState.Death)
            Skill03();

        //잔해물 위치 다시 돌려두기
        //비활성화해서 나중에 재활용..
        // wreckage_obj.gameObject.transform.SetParent(GameManager.Instance.transform);
        yield return null;
    }

    #endregion

    // *---------------------------------------------------------------------------------------------------------//
    //* 스킬 03  총쏘기
    #region 스킬 03

    public void Skill03()
    {
        StartCoroutine(BossAbyss_Skill03());
    }

    float skillTime = 0;
    bool canFire = false;

    IEnumerator BossAbyss_Skill03()
    {
        canFire = CheckPlayerInMonster_skill03(skillRadius);

        yield return new WaitUntil(() => canFire == true);
        //* 총구들과 Neck 부분의 원래 Rotation 얻기
        List<Quaternion> muzzleL_OriginQ = new List<Quaternion>();
        List<Quaternion> muzzleR_OriginQ = new List<Quaternion>();
        for (int i = 0; i < muzzlesL.Length; i++)
            muzzleL_OriginQ.Add(GetWorldRotation(muzzlesL[i]));
        for (int i = 0; i < muzzlesR.Length; i++)
            muzzleR_OriginQ.Add(GetWorldRotation(muzzlesR[i]));
        //------------------------------------------------
        skillTime = 0;
        float fireTime = 0;
        Vector3 targetPos = playerTargetPos.position;
        //애니메이터 끄기
        SetAnimation(MonsterAnimation.Idle);
        yield return new WaitForSeconds(1f);

        m_animator.enabled = false;
        //--------------------------------------------------//
        Quaternion childWorldRotation = GetWorldRotation(bossNeck);
        Quaternion originChildWorldRot = childWorldRotation;
        bossNeck.rotation = childWorldRotation;
        // 가져온 월드 회전값을 출력해보기
        Vector3 rayPos = new Vector3(bossNeck.position.x, playerTargetPos.position.y, bossNeck.position.z);


        for (int i = 0; i < muzzlesL.Length; i++)
        {
            muzzlesL[i].rotation = Quaternion.Euler(muzzlesL[i].eulerAngles.x + 20, muzzlesL[i].eulerAngles.y, muzzlesL[i].eulerAngles.z);
        }
        for (int i = 0; i < muzzlesR.Length; i++)
        {
            muzzlesR[i].rotation = Quaternion.Euler(muzzlesR[i].eulerAngles.x + 20, muzzlesR[i].eulerAngles.y, muzzlesR[i].eulerAngles.z);
        }


        bool isStop = false;
        while (!isStop)
        {
            CheckPlayerInMonster_skill03(skillRadius);

            skillTime += Time.deltaTime;
            fireTime += Time.deltaTime;
            if (fireTime >= 0.1f)
            {
                fireTime = 0;
                //GameManager.Instance.cameraController.cameraShake.ShakeCamera(0.2f, 0.75f, 0.75f);
                //L
                StartCoroutine(Fire(playerTargetPos.position, muzzlePos[0]));
                //R
                StartCoroutine(Fire(playerTargetPos.position, muzzlePos[1]));
            }
            else
            {
                childWorldRotation = GetWorldRotation(bossNeck);
                Vector3 currentRotation = childWorldRotation.eulerAngles;

                bossNeck.rotation = Quaternion.Euler(currentRotation.x, currentRotation.y + (1f * Time.timeScale), currentRotation.z);

                CheckCheckPlayer_Front(rayPos);

                if (skillTime >= 10)
                {
                    //계속 체크
                    if (Mathf.Abs(bossNeck.rotation.eulerAngles.y - originChildWorldRot.eulerAngles.y) < 2)
                    {
                        bossNeck.rotation = originChildWorldRot;
                        isStop = true;
                        break;
                    }
                }
            }

            yield return null;
        }

        //--------------------------------------------------//
        //* 총구들원래 Rotation 로 바꿔주
        for (int i = 0; i < muzzlesL.Length; i++)
            muzzlesL[i].rotation = muzzleL_OriginQ[i];
        for (int i = 0; i < muzzlesR.Length; i++)
            muzzlesR[i].rotation = muzzleR_OriginQ[i];

        m_animator.enabled = true;
        SetAnimation(MonsterAnimation.Idle);

        yield return new WaitForSeconds(1f);
        EndSkill(BossMonsterMotion.Skill03);


        //* 잔해물 치우기
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < wreckages.Count; ++i)
        {
            wreckages[i].DisappearWreckage();
        }
        bool wreckageActive = false;

        while (!wreckageActive)
        {
            wreckageActive = true;
            for (int i = 0; i < wreckages.Count; ++i)
            {
                if (wreckages[i].gameObject.activeSelf)
                {
                    wreckageActive = false;
                    break;
                }
            }
            yield return null;
        }
        wreckage_obj.SetActive(false);
        //NavMeshSurface_ReBuild();

        yield return null;
    }

    IEnumerator Fire(Vector3 targetPos, Transform muzzlePos, bool findPlayer = false)
    {

        //* 총알 //
        GameObject bulletObj = GameManager.Instance.objectPooling.GetProjectilePrefab("BossMonsterAbyss_Bullet_02", prefabPos);
        Rigidbody bulletRigid = bulletObj.GetComponent<Rigidbody>();

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.Reset(m_monster, "BossMonsterAbyss_Bullet_02", muzzlePos);
        // 플레이어에게  총알이 맞았을 경우
        bullet.OnHitPlayerEffect = () =>
        {
            //*플레이어가 총에 맞았을 경우, 이펙트
            Effect effect = GameManager.Instance.objectPooling.ShowEffect("Basic_Impact_01");

            effect.gameObject.transform.position = targetPos;
            Vector3 curDirection = targetPos - bulletObj.transform.position;
            effect.gameObject.transform.position += curDirection * 0.35f;
        };
        //총알 발사.

        if (findPlayer)
        {
            Vector3 curDirection = GetDirection(targetPos, muzzlePos.position);

            Quaternion targetAngle = Quaternion.LookRotation(curDirection);
            bulletObj.transform.rotation = targetAngle;

            bullet.SetInfo(curDirection.normalized, "FX_Shoot_10_hit");
            bulletRigid.velocity = curDirection.normalized * 100f;
        }
        else
        {
            Quaternion targetAngle = Quaternion.LookRotation(muzzlePos.forward);
            bulletObj.transform.rotation = targetAngle;

            bullet.SetInfo(muzzlePos.forward, "FX_Shoot_10_hit"); //* Bullet.cs에 방향 벡터 보냄
            bulletRigid.velocity = (muzzlePos.forward) * 100f;
        }

        //총쏠때 이펙트
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("FX_Shoot_10_muzzle");
        EffectController effectController = effect.GetComponent<EffectController>();
        effectController.ChangeSize();
        effect.gameObject.transform.position = muzzlePos.position + (muzzlePos.forward * 0.2f);
        effect.gameObject.transform.rotation = muzzlePos.rotation;

        //! 사운드
        m_monster.SoundPlay(Monster.monsterSound.Hit_Long, false);

        yield return null;
    }

    private float CheckCheckPlayer_Front(Vector3 rayPos)
    {
        // 바로 앞에 플레이어가 있는지 확인
        float range = 100f;
        float angle = 0;
        Debug.DrawRay(rayPos, (-bossNeck.up) * range, Color.blue);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(rayPos, (-bossNeck.up), range, playerlayerMask);

        if (hits.Length != 0 && !findPlayer)
        {
            findPlayer = true;

            for (int i = 0; i < muzzlesL.Length; i++)
            {
                Vector3 direction = (playerTrans.position - muzzlesL[i].position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

                // targetRotation = Quaternion.Euler(targetRotation.eulerAngles.x, muzzlesL[i].eulerAngles.y, muzzlesL[i].eulerAngles.z);
                targetRotation = Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y, muzzlesL[i].eulerAngles.z);
                muzzlesL[i].rotation = targetRotation;
            }
            for (int i = 0; i < muzzlesR.Length; i++)
            {
                Vector3 direction = (playerTrans.position - muzzlesR[i].position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                // targetRotation = Quaternion.Euler(targetRotation.eulerAngles.x, muzzlesR[i].eulerAngles.y, muzzlesR[i].eulerAngles.z);
                targetRotation = Quaternion.Euler(targetRotation.eulerAngles.x, targetRotation.eulerAngles.y, muzzlesR[i].eulerAngles.z);
                muzzlesR[i].rotation = targetRotation;
            }

            StartCoroutine(Fire(playerTargetPos.position, muzzlePos[0], true));
            //R
            StartCoroutine(Fire(playerTargetPos.position, muzzlePos[1], true));
        }

        else if (hits.Length == 0 && findPlayer)
        {
            findPlayer = false;
        }

        return angle;
    }

    Effect Shield_Effect_skill03 = null;

    public bool CheckPlayerInMonster_skill03(float radius)
    {
        //* 스킬 3번 일때 플레이어가 몬스터의 아래에 있을 경우.
        //* false 플레이어가 아래에 있다. true 플레이어가 밖에 있다.
        float distance = Vector3.Distance(this.transform.position, playerTrans.position);
        {
            Collider[] playerColliders = Physics.OverlapSphere(this.transform.position, radius - 1, playerlayerMask);

            if (0 < playerColliders.Length)
            {
                //넉백!! 튕겨내기 후, 이펙트 생성.\
                StartCoroutine(BouncePlayer());
                return false;
            }
            else
            {
                //* 스킬 3번일때 플레이어가 몬스터 근처에 왔을경우.
                //* 장벽 치기
                playerColliders = Physics.OverlapSphere(this.transform.position, radius + 5, playerlayerMask);

                if (0 < playerColliders.Length)
                {
                    //이펙트생성으로 들어오는 것 막기
                    if (Shield_Effect_skill03 == null)
                    {
                        //null이면 이펙트가 없는것.
                        Shield_Effect_skill03 = GameManager.Instance.objectPooling.ShowEffect("BossMonsterShield_01", prefabPos);
                        Shield_Effect_skill03.gameObject.transform.position = transform.position;
                        //지속 시간 바꾸기
                        Shield_Effect_skill03.finishAction = () =>
                        {
                            Shield_Effect_skill03 = null;
                        };
                    }
                }
                else
                {
                    if (Shield_Effect_skill03 != null)
                    {
                        //이펙트가 있다면 없애기.
                        Shield_Effect_skill03.StopEffect();
                    }
                }

                return true;
            }
        }
    }

    IEnumerator BouncePlayer()
    {
        //* 스킬 3번 시작전 플레이어가 아래에 있으면 튕겨내는 함수.   

        //이펙트 주고
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("BossMonsterShield_StartEffect", prefabPos);
        effect.gameObject.transform.position = transform.position;

        yield return new WaitForSeconds(2.5f);
        bool checkPlayer = CheckPlayerInMonster_skill03(skillRadius);
        if (!checkPlayer)
        {
            //넉백
            m_monster.OnHit_FallDown(3, 60);

            yield return new WaitForSeconds(1f);
            //그리고 다시 체크
            canFire = CheckPlayerInMonster_skill03(skillRadius);
        }
        else
            canFire = checkPlayer;
    }
    #endregion

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
            Debug.Log("끝ㅌ");
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
                    Debug.Log($"count {count} ,   damage {damage}");
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
        //Abyss 보스 몬스터의 경우 => 스킬 2번 4번만 멈추면 될 듯
        //*스킬 2
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
        }
        //* 스킬 4번
        if (ing_skill04)
            StopSkill04();

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

    private void OnDrawGizmos()
    {
        //몬스터 감지 범위 Draw
        //크기는  monsterData.overlapRadius

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangeXZ);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, skillRadius);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, skillRadius + 5);
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
    //* 보스 마지막 약점 연출
    public override void DirectTheBossLastWeakness()
    {
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
        GameManager.instance.cameraController.CinemachineSetting(false);
        EnableBossWeaknessEffect(false);
        curRemainWeaknessesNum = m_monster.monsterData.lastWeaknessList.Count;

    }

}
