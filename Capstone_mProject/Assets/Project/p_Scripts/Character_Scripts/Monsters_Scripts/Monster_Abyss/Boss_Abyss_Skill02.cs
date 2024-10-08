using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss_Abyss_Skill02 : MonoBehaviour
{
    //! 스킬 02  폭탄 떨구기
    MonsterPattern_Boss_Abyss monsterPattern_Abyss;
    PlayerController playerController;
    Transform playerTrans;

    List<Vector3> randomPos_skill02; //스킬 02번 폭탄의 랜덤 좌표

    //* 스킬 2번 코루틴
    Coroutine skill02_MonsterMovement_Co = null;
    Coroutine skill02_Co = null;
    //Coroutine changePhase02_Co = null;

    public void Init(MonsterPattern_Boss_Abyss _monsterPattern_Boss_Abyss)
    {
        monsterPattern_Abyss = _monsterPattern_Boss_Abyss;
        playerController = GameManager.instance.gameData.GetPlayerController();
        playerTrans = GameManager.instance.gameData.GetPlayerTransform();

        randomPos_skill02 = new List<Vector3>();
    }

    public void Skill02()
    {
        skill02_Co = StartCoroutine(BossAbyss_Skill02());
    }

    IEnumerator BossAbyss_Skill02()
    {
        //* 몬스터 뒤로 이동하는 코루틴-------------------------------------------//
        if (skill02_MonsterMovement_Co != null)
        {
            StopCoroutine(skill02_MonsterMovement_Co);
        }
        skill02_MonsterMovement_Co = StartCoroutine(MonsterMovement_Skill02());
        //*---------------------------------------------------------------------------//

        yield return new WaitForSeconds(2f);

        float time = 0;
        bool getRandomPos = false;
        Vector3 newRandomPos = Vector3.zero;
        Vector3 curMonsterPoint;

        float getRandomTime = 0;
        randomPos_skill02.Clear();
        //* 15초동안 공격
        while (time < 15)
        {
            time += Time.deltaTime;
            float randTime = UnityEngine.Random.Range(0.5f, 1.5f);
            yield return new WaitForSeconds(randTime);
            time += randTime;

            //* 겹치지 않는 (특정 거리) 공격 폭탄 랜덤 위치 얻는 While문 
            while (!getRandomPos)
            {
                getRandomTime += Time.deltaTime;
                getRandomPos = true;
                curMonsterPoint = monsterPattern_Abyss.GetGroundPos(playerTrans);
                newRandomPos = monsterPattern_Abyss.GetRandomPos(3f, curMonsterPoint);

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

                if (getRandomTime > 3f)
                {
                    //1.5초 동안 못찾으면 걍 break;
                    getRandomTime = 0;
                    break;
                }

                yield return null;
            }

            if (getRandomPos)
                StartCoroutine(SetBomb(newRandomPos));

            getRandomPos = false;
            yield return null;
        }

        yield return new WaitForSeconds(4f);

        //* 연기 이펙트
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

        //* 보스 주변에 마지막 공격--------------------------------------------//
        if (skill02_MonsterMovement_Co != null)
        {
            StopCoroutine(skill02_MonsterMovement_Co);
            skill02_MonsterMovement_Co = null;

            monsterPattern_Abyss.SetMove_AI(false);
            monsterPattern_Abyss.SetAnimation(MonsterPattern.MonsterAnimation.Idle);
        }

        curMonsterPoint = monsterPattern_Abyss.GetGroundPos(transform);
        List<Vector3> roundPos = monsterPattern_Abyss.GetRoundPos(curMonsterPoint);

        monsterPattern_Abyss.useExplosionSound = false;

        foreach (Vector3 pos in roundPos)
        {
            randomPos_skill02.Add(pos);
            StartCoroutine(SetBomb(pos, true, true));
        }
        //*-------------------------------------------------------------//
        //* Abyss는 밑이 뚫려있는 몬스터
        //* 만약 보스 스킬 02 의 마지막 공격중, 플레이어가 몬스터의 아래에 들어가서 공격 중이라면?
        //* 플레이어가 공격하면서 몬스터쪽으로 나아가는 거 막기 
        float radius = 7.5f;
        while (randomPos_skill02.Count != 0)
        {
            // 몬스터 아래에 있는지 확인
            Collider[] playerColliders = Physics.OverlapSphere(this.transform.position, radius, monsterPattern_Abyss.playerlayerMask);

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

        //*-------------------------------------------------------------//
        //TODO: 아래 코드 스킬 3번으로 옮기기    
        MonsterPattern_Boss.BossMonsterPhase monsterPhase = monsterPattern_Abyss.GetBossMonsterPhase();

        if (monsterPhase != MonsterPattern_Boss.BossMonsterPhase.Phase1)
        {
             //이펙트 추가하기.
            effect = GameManager.Instance.objectPooling.ShowEffect("SpiritBomb");

            Vector3 originPos = this.transform.position;
            originPos.y += 0.5f;
            effect.transform.position = originPos;

            //* 잔해물 떨어지기
            yield return new WaitForSeconds(1.5f);
            monsterPattern_Abyss.boss_Abyss_Skill03.SettingWreckage();
            yield return new WaitForSeconds(1f);
            monsterPattern_Abyss.EndSkill(MonsterPattern_Boss.BossMonsterMotion.Skill02);
        }
        else
        {
            yield return new WaitForSeconds(1f);

            monsterPattern_Abyss.EndSkill(MonsterPattern_Boss.BossMonsterMotion.Skill02);
        }

        skill02_Co = null;
    }

    //* 보스 플레이어 뒤로 이동
    IEnumerator MonsterMovement_Skill02()
    {
        float time = 0;

        monsterPattern_Abyss.SetMove_AI(false);
        monsterPattern_Abyss.SetAnimation(MonsterPattern.MonsterAnimation.Idle);

        float defaultSpeed = monsterPattern_Abyss.navMeshAgent.speed; // 몬스터 속도
        float defaultAcceleration = monsterPattern_Abyss.navMeshAgent.acceleration; // 몬스터 가속도

        NavMeshHit hit;

        //randomPos_skill02.Clear();

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
            monsterPattern_Abyss.SetMove_AI(true);
            monsterPattern_Abyss.navMeshAgent.SetDestination(monsterNewPos);
            monsterPattern_Abyss.SetAnimation(MonsterPattern.MonsterAnimation.Move);

            while (true)
            {
                if (Vector3.Distance(monsterNewPos, transform.position) <= 2f)
                {
                    monsterPattern_Abyss.SetMove_AI(false);

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

        monsterPattern_Abyss.SetAnimation(MonsterPattern.MonsterAnimation.Idle);

        skill02_MonsterMovement_Co = null;
    }

    //* 폭발
    IEnumerator SetBomb(Vector3 randomPos, bool usePhase01 = false, bool soundCancle = false)
    {
        float time = 0;
        Effect effect;
        MonsterPattern_Boss.BossMonsterPhase bossMonsterPhase = monsterPattern_Abyss.GetBossMonsterPhase();

        //* 페이즈 1의 폭발 (페이즈 1, 2 마다 이펙트 다름)
        if (bossMonsterPhase == MonsterPattern_Boss.BossMonsterPhase.Phase1 || usePhase01)
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
            {
                //monsterPattern_Abyss.m_monster.SoundPlay(Monster.monsterSound.Hit_Close, false);
                monsterPattern_Abyss.m_monster.SoundPlay("Boss_Skill02_phase01", false);
            }

            else if (soundCancle && !monsterPattern_Abyss.useExplosionSound)
            {
                monsterPattern_Abyss.useExplosionSound = true;
                //monsterPattern_Abyss.m_monster.SoundPlay(Monster.monsterSound.Hit_Close, false);
                monsterPattern_Abyss.m_monster.SoundPlay("Boss_Skill02_phase01", false);
            }
        }
        else
        {
            //* 페이즈 2의 폭발
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
            {
                // monsterPattern_Abyss.m_monster.SoundPlay(Monster.monsterSound.Hit_Long2, false);
                monsterPattern_Abyss.m_monster.SoundPlay("Boss_Skill02_phase02", false);
            }
            else if (soundCancle && !monsterPattern_Abyss.useExplosionSound)
            {
                monsterPattern_Abyss.useExplosionSound = true;
                //monsterPattern_Abyss.m_monster.SoundPlay(Monster.monsterSound.Hit_Long2, false);
                monsterPattern_Abyss.m_monster.SoundPlay("Boss_Skill02_phase02", false);
            }
        }

        while (time < 5)
        {
            time += Time.deltaTime;
            MonsterPattern.MonsterState monsterState = monsterPattern_Abyss.GetCurMonsterState();
            if (monsterState == MonsterPattern.MonsterState.Death)
            {
                effect.StopEffect();
                break;
            }

            //TODO: 범위 지정
            bool playerAttack = monsterPattern_Abyss.CheckPlayerDamage(2f, randomPos, 20, true);

            if (playerAttack)
                break;

            yield return null;
        }
    }

    //* 스킬 02번 정지--------------------------------------------------------------//
    public void Stop_MonsterSkill02()
    {
        if (skill02_Co != null)
        {
            StopCoroutine(skill02_Co);
            skill02_Co = null;

            if (!playerController._currentState.canGoForwardInAttack)
                playerController._currentState.canGoForwardInAttack = true;
        }
        if (skill02_MonsterMovement_Co != null)
        {
            monsterPattern_Abyss.StopCoroutine(skill02_MonsterMovement_Co);
            skill02_MonsterMovement_Co = null;
            monsterPattern_Abyss.SetMove_AI(false);
            monsterPattern_Abyss.SetAnimation(MonsterPattern.MonsterAnimation.Idle);
        }

        monsterPattern_Abyss.EndSkill(MonsterPattern_Boss.BossMonsterMotion.Skill02);
    }
}
