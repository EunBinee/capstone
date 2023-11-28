using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.Animations;
using UnityEditor.Rendering;
using System.Threading;

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

    public
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

        playerController.NavMeshSurface_ReBuild();
    }

    public virtual void UpdateRotation()
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
                //내려찍기      //* 네비 메쉬 꺼야함.
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
                break;
            case BossMonsterMotion.Death:
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
                GameManager.instance.bossBattle = false;
                GameManager.instance.cameraController.Check_Z();
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
            Monster_Motion(BossMonsterMotion.Skill01);
            //스킬 2
            //Monster_Motion(BossMonsterMotion.Skill02);
            //StartCoroutine(SetWreckage());
            //스킬 3
            //Monster_Motion(BossMonsterMotion.Skill03);
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
        //점프
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
            CheckPlayerDamage(8f, transform.position, 20);
        //? 연기이펙트-----------------------------------------------------------------------//
        GameManager.Instance.cameraShake.ShakeCamera(1f, 3, 3);
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_03");
        Vector3 effectPos = transform.position;
        effectPos.y -= 1.5f;
        effect.transform.position = effectPos;

        isJump = false;
        NavMesh_Enable(true);

        if (curBossPhase != BossMonsterPhase.Phase1)
        {
            //* 잔해물 떨어지기

            yield return new WaitForSeconds(2f);
            StartCoroutine(SetWreckage());
        }

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

        while (time < 15)
        {
            time += Time.deltaTime;
            float randTime = UnityEngine.Random.Range(0.5f, 1.5f);
            yield return new WaitForSeconds(randTime);
            time += randTime;
            while (!getRandomPos)
            {
                getRandomPos = true;
                newRandomPos = GetRandomPos(3f, playerTrans.position);
                foreach (Vector3 randomPos in randomPos_skill02)
                {
                    if (Vector3.Distance(newRandomPos, randomPos) <= 3.5f)
                    {
                        getRandomPos = false;
                        break;
                    }
                }

                if (getRandomPos)
                {
                    randomPos_skill02.Add(newRandomPos);
                }
            }
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
            CheckPlayerDamage(4f, randomPos, 20);
            yield return null;
        }
    }



    #endregion

    // *---------------------------------------------------------------------------------------------------------//
    //* 스킬 03  총쏘기

    public void Skill03()
    {
        StartCoroutine(BossAbyss_Skill03());
    }

    IEnumerator BossAbyss_Skill03()
    {
        //애니메이터 끄기
        SetAnimation(MonsterAnimation.Idle);
        yield return new WaitForSeconds(0.5f);

        m_animator.enabled = false;
        //--------------------------------------------------//

        Quaternion curRotation = bossNeck.rotation;
        Vector3 curRotation_Euler = bossNeck.eulerAngles;
        Debug.Log("X: " + curRotation_Euler.x + ", Y: " + curRotation_Euler.y + ", Z: " + curRotation_Euler.z);



        //--------------------------------------------------//
        //m_animator.enabled = true;
        //SetAnimation(MonsterAnimation.Idle);
        yield return null;
    }

    // *---------------------------------------------------------------------------------------------------------//
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

        //잔해물 위치 다시 돌려두기
        //비활성화해서 나중에 재활용..
        // wreckage_obj.gameObject.transform.SetParent(GameManager.Instance.transform);
        yield return null;
    }
    private void OnDrawGizmos()
    {
        //몬스터 감지 범위 Draw
        //크기는  monsterData.overlapRadius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ground_Center.position, rangeXZ);

    }




}
