using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss_Abyss_Skill03 : MonoBehaviour
{
    //! 스킬 03  총쏘기
    MonsterPattern_Boss_Abyss monsterPattern_Abyss;
    PlayerController playerController;
    Transform playerTrans;

    [Header("스킬 02 잔해물 범위")]
    public int rangeXZ = 50;
    public List<Wreckage> wreckages;


    public GameObject wreckage_obj; //실제 게임에서 사용될 잔해물 오브젝트

    bool findPlayer = false;
    public float skillRadius = 10;

    Transform bossNeck;

    Transform[] muzzlesL;
    Transform[] muzzlesR;
    Transform[] muzzlePos;

    float skillTime = 0;
    bool canFire = false;

    List<Quaternion> muzzleL_OriginQ = null;
    List<Quaternion> muzzleR_OriginQ = null;
    Quaternion originChildWorldRot = Quaternion.identity;

    Coroutine skill03_co = null;

    public void Init(MonsterPattern_Boss_Abyss _monsterPattern_Boss_Abyss)
    {
        monsterPattern_Abyss = _monsterPattern_Boss_Abyss;
        playerController = GameManager.instance.gameData.GetPlayerController();
        playerTrans = GameManager.instance.gameData.GetPlayerTransform();
        bossNeck = monsterPattern_Abyss.bossNeck;
        muzzlesL = monsterPattern_Abyss.muzzlesL;
        muzzlesR = monsterPattern_Abyss.muzzlesR;
        muzzlePos = monsterPattern_Abyss.muzzlePos;
    }

    public void Skill03()
    {
        skill03_co = StartCoroutine(BossAbyss_Skill03());
    }


    IEnumerator BossAbyss_Skill03()
    {
        canFire = CheckPlayerInMonster_skill03(skillRadius);

        yield return new WaitUntil(() => canFire == true);

        //* 총구들과 Neck 부분의 원래 Rotation 얻기---------------------------------------------------//

        muzzleL_OriginQ = new List<Quaternion>();
        muzzleR_OriginQ = new List<Quaternion>();
        for (int i = 0; i < muzzlesL.Length; i++)
            muzzleL_OriginQ.Add(monsterPattern_Abyss.GetWorldRotation(muzzlesL[i]));
        for (int i = 0; i < muzzlesR.Length; i++)
            muzzleR_OriginQ.Add(monsterPattern_Abyss.GetWorldRotation(muzzlesR[i]));

        //--------------------------------------------------------------------------------------------------//

        skillTime = 0;
        float fireTime = 0;

        Vector3 targetPos = monsterPattern_Abyss.playerTargetPos.position;
        //애니메이터 끄기
        monsterPattern_Abyss.SetAnimation(MonsterPattern.MonsterAnimation.Idle);

        yield return new WaitForSeconds(1f);
        monsterPattern_Abyss.m_animator.enabled = false;

        //--------------------------------------------------//
        Quaternion childWorldRotation = monsterPattern_Abyss.GetWorldRotation(bossNeck);
        originChildWorldRot = childWorldRotation;
        bossNeck.rotation = childWorldRotation;
        // 가져온 월드 회전값을 출력해보기
        Vector3 rayPos = new Vector3(bossNeck.position.x, monsterPattern_Abyss.playerTargetPos.position.y, bossNeck.position.z);


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
                StartCoroutine(Fire(monsterPattern_Abyss.playerTargetPos.position, muzzlePos[0]));
                //R
                StartCoroutine(Fire(monsterPattern_Abyss.playerTargetPos.position, muzzlePos[1]));
            }
            else
            {
                childWorldRotation = monsterPattern_Abyss.GetWorldRotation(bossNeck);
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
        //* 총구들원래 Rotation 로 바꿔주기
        ResetSkill02Rotation();

        yield return new WaitForSeconds(1f);

        //* 잔해물 치우기
        ClearWreckage();
        yield return new WaitForSeconds(1f);
        monsterPattern_Abyss.EndSkill(MonsterPattern_Boss.BossMonsterMotion.Skill03);

        skill03_co = null;
        yield return null;
    }

    // * 총을 쏘면서 틀어진 Rotation을 reset시키는 함수
    public void ResetSkill02Rotation()
    {
        bossNeck.rotation = originChildWorldRot;
        originChildWorldRot = Quaternion.identity;

        for (int i = 0; i < muzzlesL.Length; i++)
        {
            muzzlesL[i].rotation = muzzleL_OriginQ[i];
        }
        for (int i = 0; i < muzzlesR.Length; i++)
        {
            muzzlesR[i].rotation = muzzleR_OriginQ[i];
        }
        muzzleL_OriginQ.Clear();
        muzzleR_OriginQ.Clear();

        monsterPattern_Abyss.m_animator.enabled = true;


        monsterPattern_Abyss.SetAnimation(MonsterPattern.MonsterAnimation.Idle);
    }

    public void SettingWreckage()
    {
        StartCoroutine(SetWreckage());
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

        //* 잔해물 배치(플레이어와 떨어진 곳으로 지정)
        //* randomPos 지정.
        List<Vector3> randomPosList = new List<Vector3>();
        NavMeshHit hit;
        Vector3 PosY = monsterPattern_Abyss.GetGroundPos(transform);
        Vector3 curMonsterPoint = monsterPattern_Abyss.centerPoint;

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
                    randomPos = monsterPattern_Abyss.GetRandomPos(rangeXZ, curMonsterPoint);
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
            wreckages[i].m_monster = monsterPattern_Abyss.m_monster;
            wreckages[i].gameObject.SetActive(true);
            wreckages[i].StartDropWreckage(randomPosList[i]);
        }

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

        MonsterPattern.MonsterState monsterState = monsterPattern_Abyss.GetCurMonsterState();
        if (monsterState != MonsterPattern.MonsterState.Death)
            monsterPattern_Abyss.Monster_Motion(MonsterPattern_Boss.BossMonsterMotion.Skill03);

        yield return null;
    }


    public void ClearWreckage()
    {
        for (int i = 0; i < wreckages.Count; ++i)
        {
            wreckages[i].DisappearWreckage();
        }
        bool wreckageActive = false;

        //* 잔해물 전부 비활성화 됐는지 확인. => 무한 루프 문제 있음. 나중에 이펙트 넣을 때 주의
        while (!wreckageActive)
        {
            wreckageActive = true;
            if (wreckages.Count > 0)
            {
                Debug.Log("hh");
                for (int i = 0; i < wreckages.Count; ++i)
                {
                    if (wreckages[i].gameObject.activeSelf)
                    {
                        wreckageActive = false;
                        break;
                    }
                }
            }
            else
                break;
        }

        wreckage_obj.SetActive(false);
    }

    IEnumerator Fire(Vector3 targetPos, Transform muzzlePos, bool findPlayer = false)
    {

        //* 총알 //
        GameObject bulletObj = GameManager.Instance.objectPooling.GetProjectilePrefab("BossMonsterAbyss_Bullet_02", monsterPattern_Abyss.prefabPos);
        Rigidbody bulletRigid = bulletObj.GetComponent<Rigidbody>();

        Bullet bullet = bulletObj.GetComponent<Bullet>();
        bullet.Reset(monsterPattern_Abyss.m_monster, "BossMonsterAbyss_Bullet_02", muzzlePos);
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
            Vector3 curDirection = monsterPattern_Abyss.GetDirection(targetPos, muzzlePos.position);

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

        //* 사운드
        // monsterPattern_Abyss.m_monster.SoundPlay(Monster.monsterSound.Hit_Long, false);
        monsterPattern_Abyss.m_monster.SoundPlay("Boss_Skill03", false);
        yield return null;
    }

    private float CheckCheckPlayer_Front(Vector3 rayPos)
    {
        // 바로 앞에 플레이어가 있는지 확인
        float range = 100f;
        float angle = 0;
        Debug.DrawRay(rayPos, (-bossNeck.up) * range, Color.blue);
        RaycastHit[] hits;
        hits = Physics.RaycastAll(rayPos, (-bossNeck.up), range, monsterPattern_Abyss.playerlayerMask);

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

            StartCoroutine(Fire(monsterPattern_Abyss.playerTargetPos.position, muzzlePos[0], true));
            //R
            StartCoroutine(Fire(monsterPattern_Abyss.playerTargetPos.position, muzzlePos[1], true));
        }

        else if (hits.Length == 0 && findPlayer)
        {
            findPlayer = false;
        }

        return angle;
    }

    Effect Shield_Effect_skill03 = null;


    //* 총 쏘기전 플레이어가 몬스터의 아래에 있는지 체크하는 함수.
    public bool CheckPlayerInMonster_skill03(float radius)
    {
        //* 스킬 3번 일때 플레이어가 몬스터의 아래에 있을 경우.
        //* false 플레이어가 아래에 있다. true 플레이어가 밖에 있다.
        float distance = Vector3.Distance(this.transform.position, playerTrans.position);
        {
            Collider[] playerColliders = Physics.OverlapSphere(this.transform.position, radius - 1, monsterPattern_Abyss.playerlayerMask);

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
                playerColliders = Physics.OverlapSphere(this.transform.position, radius + 5, monsterPattern_Abyss.playerlayerMask);

                if (0 < playerColliders.Length)
                {
                    //이펙트생성으로 들어오는 것 막기
                    if (Shield_Effect_skill03 == null)
                    {
                        //null이면 이펙트가 없는것.
                        Shield_Effect_skill03 = GameManager.Instance.objectPooling.ShowEffect("BossMonsterShield_01", monsterPattern_Abyss.prefabPos);
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
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("BossMonsterShield_StartEffect", monsterPattern_Abyss.prefabPos);
        effect.gameObject.transform.position = transform.position;

        yield return new WaitForSeconds(2.5f);
        bool checkPlayer = CheckPlayerInMonster_skill03(skillRadius);
        if (!checkPlayer)
        {
            //넉백
            monsterPattern_Abyss.m_monster.OnHit_FallDown(3, 60);

            yield return new WaitForSeconds(1f);
            //그리고 다시 체크
            canFire = CheckPlayerInMonster_skill03(skillRadius);
        }
        else
            canFire = checkPlayer;
    }

    //* 스킬 03번 정지--------------------------------------------------------------//
    public void Stop_MonsterSkill03()
    {
        if (skill03_co != null)
        {
            StopCoroutine(skill03_co);
            skill03_co = null;

            ResetSkill02Rotation();
            ClearWreckage();
        }
        monsterPattern_Abyss.EndSkill(MonsterPattern_Boss.BossMonsterMotion.Skill03);

    }

}
