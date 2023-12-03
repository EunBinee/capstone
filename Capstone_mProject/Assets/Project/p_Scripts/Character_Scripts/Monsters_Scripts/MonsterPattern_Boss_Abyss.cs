using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.Animations;
using UnityEditor.Rendering;
using System.Threading;
using UnityEditor;

public class MonsterPattern_Boss_Abyss : MonsterPattern_Boss
{
    //! 보스 몬스터 나락.
    PlayerController playerController;
    [Header("스킬 02 잔해물 범위")]
    public Transform ground_Center;
    public int rangeXZ;
    public List<Wreckage> wreckages;
    public Transform prefabPos;
    GameObject wreckage_obj; //실제 게임에서 사용될 잔해물 오브젝트
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

    bool isJump = false;
    bool isDodge = false;

    public override void Init()
    {
        m_monster = GetComponent<Monster>();
        m_animator = GetComponent<Animator>();

        //rigid = GetComponent<Rigidbody>();
        playerController = GameManager.Instance.gameData.GetPlayerController();
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
        GameManager.instance.bossBattle = true;
        GameManager.instance.cameraController.Check_Z();
        GameManager.instance.cameraController.ResetCameraZ();

        playerController.NavMeshSurface_ReBuild();

        if (m_monster.HPBar_CheckNull() == false)
        {
            if (m_monster.monsterData.MaxHP != m_monster.monsterData.HP)
                m_monster.ResetHP();
            m_monster.GetHPBar();
        }

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
                    break;
                case MonsterState.Attack:
                case MonsterState.GetHit:
                    break;
                case MonsterState.GoingBack:
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
                //내려찍기
                StartCoroutine(BossAbyss_Skill01());
                break;
            case BossMonsterMotion.Skill02:
                Skill02();
                break;
            case BossMonsterMotion.Skill03:
                if (curBossPhase != BossMonsterPhase.Phase1)
                {
                    Skill03();
                }
                break;
            case BossMonsterMotion.Skill04:
                if (curBossPhase != BossMonsterPhase.Phase1)
                {

                }
                break;
            case BossMonsterMotion.GetHit:
                //getHit
                GetHit();

                break;
            case BossMonsterMotion.Death:
                m_monster.RetrunHPBar();

                GameManager.instance.bossBattle = false;
                GameManager.instance.cameraController.Check_Z();
                GameManager.instance.cameraController.ResetCameraZ();
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
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }
    //*----------------------------------------------------------------------------------------------------------//
    public override void ChangeBossPhase(BossMonsterPhase bossMonsterPhase)
    {
        curBossPhase = bossMonsterPhase;

        switch (curBossPhase)
        {
            case BossMonsterPhase.Phase1:
                // 등장 연출
                break;
            case BossMonsterPhase.Phase2:
                // 2페이지 시작 연출
                break;
            case BossMonsterPhase.Phase3:
                // 3페이지 시작 연출
                break;
        }
    }

    // *---------------------------------------------------------------------------------------------------------//
    // * 몬스터 상태 =>> 로밍(시네머신 등장 신.)
    public override void Roam_Monster()
    {
        if (!isRoaming)
        {
            isRoaming = true;
            //TODO: 나중에 범위안에 들어오면, 등장씬 나오도록 수정
            //* 일단은 바로 공격하도록

            isRoaming = false;
            isFinding = true;
            ChangeMonsterState(MonsterState.Tracing);
            //스킬 1
            // Monster_Motion(BossMonsterMotion.Skill01);
            //스킬 2
            //Monster_Motion(BossMonsterMotion.Skill02);
            //StartCoroutine(SetWreckage());
            //스킬 3
            Monster_Motion(BossMonsterMotion.Skill03);
        }
    }
    //*----------------------------------------------------------------------------------------------------------//
    //* 스킬 01 내려찍기
    #region 스킬 01
    IEnumerator BossAbyss_Skill01()
    {
        yield return new WaitForSeconds(2f);

        //*네비메쉬 끄기
        Vector3 originPos = transform.position;
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("HeartOfBattle_01");
        effect.transform.position = originPos;

        yield return new WaitForSeconds(1f);

        //* 점프 --------------------------------------------------------------------//
        isJump = true;
        StartCoroutine(JumpUp());
        yield return new WaitUntil(() => isJump == false);
        //*-------------------------------------------------------------------------------//
        //* 플레이어를 쫓아 다니는 이펙트 
        float duration = 5f;
        StartCoroutine(FollowPlayer_Effect_InSkill01(duration));
        yield return new WaitForSeconds(duration);
        Vector3 curPlayerPos = playerTrans.position;

        isJump = true;
        StartCoroutine(JumpDown(curPlayerPos));
        yield return new WaitUntil(() => isJump == false);
        //------------------------------------------------------------------------------------//
        yield return null;
    }

    IEnumerator JumpUp()
    {
        NavMesh_Enable(false);
        Vector3 originPos = transform.position;
        float speed = 30;
        float time = 0;

        SetBossAttackAnimation(BossMonsterAttackAnimation.Skill01, 0);
        yield return new WaitForSeconds(1f);

        // 점프전 잠깐 밑으로 내려감.
        while (time < 0.1)
        {
            time += Time.deltaTime;
            transform.Translate(-Vector3.up * speed * Time.deltaTime);
            yield return null;
        }
        //? 연기이펙트-----------------------------------------------------------------------//
        GameManager.Instance.cameraShake.ShakeCamera(1f, 3, 3);
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_02");
        Vector3 effectPos = originPos;
        effectPos.y += 2.5f;
        effect.transform.position = effectPos;
        //------------------------------------------------------------------------------------//
        //*점프전 주목 풀기
        if (GameManager.instance.cameraController.isBeingAttention)
        {
            //주목중.
            GameManager.instance.cameraController.UndoAttention();
            GameManager.instance.cameraController.banAttention = true;
        }
        else
        {
            GameManager.instance.cameraController.banAttention = true;
        }
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

        isJump = false;
    }

    IEnumerator JumpDown(Vector3 curPlayerPos, bool getDamage = true)
    {
        yield return new WaitForSeconds(3);
        float speed;
        float time = 0;
        transform.position = new Vector3(curPlayerPos.x, transform.position.y, curPlayerPos.z);

        speed = 50f;
        SetBossAttackAnimation(BossMonsterAttackAnimation.Skill01, 1);


        yield return new WaitForSeconds(1f);
        while (time < 5f)
        {
            time += Time.deltaTime;
            speed = Mathf.Lerp(50, 90, Time.time);
            transform.Translate(-Vector3.up * speed * Time.deltaTime);
            if (transform.position.y <= curPlayerPos.y)
                break;
            yield return null;
        }
        if (getDamage)
            CheckPlayerDamage(8f, transform.position, 20, true);
        //? 연기이펙트-----------------------------------------------------------------------//
        GameManager.Instance.cameraShake.ShakeCamera(1f, 3, 3);
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_03");
        Vector3 effectPos = transform.position;
        effectPos.y -= 1.5f;
        effect.transform.position = effectPos;

        //* 점프 후 주목 가능
        GameManager.instance.cameraController.banAttention = false;

        isJump = false;
        NavMesh_Enable(true);


    }
    IEnumerator FollowPlayer_Effect_InSkill01(float duration)
    {
        //* 스킬01 내려찍기 중, 플레이어를 쫒아다니는 이펙트 
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("PulseGrenade_01");
        EffectController effectController = effect.gameObject.GetComponent<EffectController>();
        effectController.ChangeSize();
        effect.transform.position = playerTrans.position;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;

            effect.transform.position = playerTrans.position;
            yield return null;
        }
        yield return new WaitForSeconds(2f);
        effect.StopEffect();
    }

    #endregion
    // *---------------------------------------------------------------------------------------------------------//
    //* 스킬 02  폭탄 떨구기
    #region 스킬 02

    private void Skill02()
    {
        StartCoroutine(BossAbyss_Skill02());

    }

    IEnumerator BossAbyss_Skill02()
    {
        yield return new WaitForSeconds(4f);
        //* 몬스터 뒤로 이동하는 코루틴
        StartCoroutine(MoveMonster_Skill02());

        yield return new WaitForSeconds(2f);
        float time = 0;
        bool getRandomPos = false;
        Vector3 newRandomPos = Vector3.zero;

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
                newRandomPos = GetRandomPos(3f, playerTrans.position);
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

                if (getrandomTime < 3f)
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

        yield return new WaitForSeconds(3f);

        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_03");
        Vector3 effectPos = transform.position;
        effectPos.y -= 1.5f;
        effect.transform.position = effectPos;

        effect = GameManager.Instance.objectPooling.ShowEffect("MC01_Red_02");
        EffectController effectController = effect.GetComponent<EffectController>();
        effectController.ChangeSize();
        effect.transform.position = transform.position;


        GameManager.Instance.cameraShake.ShakeCamera(1f, 3, 3);

        yield return new WaitForSeconds(0.7f);

        //* 보스 주변에 공격
        List<Vector3> roundPos = GetRoundPos(transform.position);
        foreach (Vector3 pos in roundPos)
        {
            StartCoroutine(SetBomb(pos, true));
        }


        if (curBossPhase != BossMonsterPhase.Phase1)
        {
            //* 잔해물 떨어지기

            yield return new WaitForSeconds(4f);
            StartCoroutine(SetWreckage());
        }

    }

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


        GameManager.Instance.cameraShake.ShakeCamera(1f, 3, 3);
        yield return new WaitForSeconds(0.7f);

        if (NavMesh.SamplePosition(monsterNewPos, out hit, 20f, NavMesh.AllAreas))
        {
            SetMove_AI(true);
            navMeshAgent.SetDestination(monsterNewPos);
            SetAnimation(MonsterAnimation.Move);

            while (true)
            {
                if (Vector3.Distance(monsterNewPos, transform.position) <= 0.5f)
                {
                    SetMove_AI(false);

                    //TODO: 몬스터 방향 플레이어 쪽으로 돌리기
                    time = 0;
                    while (time < 20)
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
            //!점프 갈겨
            //점프 끝나면 startAttack == true만들기
        }

        SetAnimation(MonsterAnimation.Idle);

    }

    IEnumerator SetBomb(Vector3 randomPos, bool usePhase01 = false)
    {
        float time = 0;
        if (curBossPhase == BossMonsterPhase.Phase1 || usePhase01)
        {
            Effect effect = GameManager.Instance.objectPooling.ShowEffect("RocketBarrage");
            effect.transform.position = randomPos;
            effect.finishAction = () =>
            {
                randomPos_skill02.Remove(randomPos);
            };
        }
        else
        {
            Effect effect = GameManager.Instance.objectPooling.ShowEffect("PulseGrenade_02");
            effect.transform.position = randomPos;

            effect = GameManager.Instance.objectPooling.ShowEffect("MeteorStrike");
            effect.transform.position = randomPos;
            effect.finishAction = () =>
            {
                randomPos_skill02.Remove(randomPos);
            };
        }

        yield return new WaitForSeconds(1.5f);

        while (time < 5)
        {
            time += Time.deltaTime;
            //TODO: 범위 지정
            bool playerTrue = CheckPlayerDamage(4f, randomPos, 20, true);

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
                wreckage_obj.gameObject.transform.SetParent(prefabPos);

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
            wreckage_obj.gameObject.transform.SetParent(prefabPos);
        }

        //*잔해물 배치(플레이어와 떨어진 곳으로 지정)
        //randomPos 지정.

        List<Vector3> randomPosList = new List<Vector3>();
        NavMeshHit hit;
        for (int i = 0; i < wreckages.Count; i++)
        {
            Vector3 randomPos = Vector3.zero;
            while (true)
            {
                randomPos = GetRandomPos(rangeXZ, ground_Center.position, originPosition.y, true);

                if (Vector3.Distance(playerTrans.position, randomPos) >= 3f &&
                    Vector3.Distance(transform.position, randomPos) >= 10f)
                {
                    // - 플레이어랑은 3 정도의 거리를 유지하고, 
                    // - 보스와는 20정도의 거리를 유지하고
                    if (NavMesh.SamplePosition(randomPos, out hit, 50f, NavMesh.AllAreas))
                    {
                        // - 네비에이전트가 움직일 수 있는 곳인지 체크
                        if (randomPosList.Contains(randomPos) == false)
                        {
                            //randomPos에도 없으면.. 생성
                            int j = 0;
                            bool canBreak = true;
                            while (j < randomPosList.Count)
                            {
                                if (Vector3.Distance(randomPosList[j], randomPos) < 15f)
                                {
                                    canBreak = false;
                                    break;
                                }
                                ++j;
                            }
                            if (canBreak)
                                break;
                        }
                    }
                }
            }
            randomPosList.Add(randomPos);
        }

        //떨어뜨리기 전 경고 이펙트 && 떨어뜨리기
        for (int i = 0; i < randomPosList.Count; ++i)
        {
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

        playerController.NavMeshSurface_ReBuild();
        // 사라지게 하기
        yield return null;
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
                //GameManager.Instance.cameraShake.ShakeCamera(0.2f, 0.75f, 0.75f);
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
            Debug.Log("Player 발견");

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
        float distance = Vector3.Distance(this.transform.position, playerTrans.position);
        {
            Collider[] playerColliders = Physics.OverlapSphere(this.transform.position, radius - 1, playerlayerMask);

            if (0 < playerColliders.Length)
            {
                //넉백!! 튕겨내기 후, 이펙트 생성.
                //TODO: 민지쪽에서 구현 완료하면 구현
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
            m_monster.OnHit_FallDown(3, 25);

            yield return new WaitForSeconds(1f);
            //그리고 다시 체크
            canFire = CheckPlayerInMonster_skill03(skillRadius);
        }
        else
            canFire = checkPlayer;
    }

    #endregion

    //*----------------------------------------------------------------------------------------------------------//
    //* 피격 이펙트
    private void GetHit()
    {

        Effect effect = GameManager.Instance.objectPooling.ShowEffect("FX_Shoot_04_hit");
        effect.gameObject.transform.position = curHitPos;
        effect.gameObject.transform.rotation = curHitQuaternion;
    }

    //*----------------------------------------------------------------------------------------------------------//
    //* --

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
        Gizmos.DrawWireSphere(ground_Center.position, rangeXZ);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, skillRadius);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, skillRadius + 5);
    }




}
