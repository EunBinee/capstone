using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss_Abyss_Skill01 : MonoBehaviour
{
    //! 스킬 01 점프 공격

    MonsterPattern_Boss_Abyss monsterPattern_Abyss;
    PlayerController playerController;
    Transform playerTrans;

    public void Init(MonsterPattern_Boss_Abyss _monsterPattern_Boss_Abyss)
    {

        monsterPattern_Abyss = _monsterPattern_Boss_Abyss;
        playerController = GameManager.instance.gameData.GetPlayerController();
        playerTrans = GameManager.instance.gameData.GetPlayerTransform();
    }


    public void Skill01()
    {
        StartCoroutine(BossAbyss_Skill01());
    }

    IEnumerator BossAbyss_Skill01()
    {
        //점프전 이펙트
        Vector3 originPos = transform.position;
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("HeartOfBattle_01");
        effect.transform.position = originPos;

        yield return new WaitForSeconds(1f);
        //* 점프 up--------------------------------------------------------------------//
        monsterPattern_Abyss.isJump = true;
        BossAbyss_JumpUp();
        yield return new WaitUntil(() => monsterPattern_Abyss.isJump == false);
        //*-------------------------------------------------------------------------------//
        MonsterPattern.MonsterState monsterState = monsterPattern_Abyss.GetCurMonsterState();
        if (monsterState != MonsterPattern.MonsterState.Death)
        {
            //* 플레이어를 쫓아 다니는 이펙트 
            float duration = 3f;
            StartCoroutine(FollowPlayer_Effect_InSkill01(duration));
            yield return new WaitForSeconds(duration);
            Vector3 curPlayerPos = playerTrans.position;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(curPlayerPos, out hit, 20f, NavMesh.AllAreas))
            {
                if (hit.position != curPlayerPos)
                    curPlayerPos = hit.position;
            }
            else
            {
                Debug.Log("보스가 못가는 곳입니다..");
                curPlayerPos = originPos;
            }
            yield return new WaitForSeconds(0.5f);
            monsterPattern_Abyss.isJump = true;
            BossAbyss_JumpDown(curPlayerPos);
            StartCoroutine(JumpDown(curPlayerPos));
            yield return new WaitUntil(() => monsterPattern_Abyss.isJump == false);
        }
        //------------------------------------------------------------------------------------//
        monsterPattern_Abyss.EndSkill(MonsterPattern_Boss.BossMonsterMotion.Skill01);
    }

    public void BossAbyss_JumpUp()
    {
        StartCoroutine(JumpUp());
    }

    public void BossAbyss_JumpDown(Vector3 landPoint, bool getDamage = true)
    {
        StartCoroutine(JumpDown(landPoint, getDamage));
    }

    bool originNoAttack = false;

    IEnumerator JumpUp()
    {
        //*네비메쉬 끄기
        if (monsterPattern_Abyss.noAttack)
        {
            originNoAttack = true;
        }
        else
        {
            monsterPattern_Abyss.noAttack = true;
            originNoAttack = false;
        }

        monsterPattern_Abyss.NavMesh_Enable(false);
        Vector3 originPos = transform.position;
        float speed = 30;
        float time = 0;

        monsterPattern_Abyss.SetBossAttackAnimation(MonsterPattern_Boss.BossMonsterAttackAnimation.Skill01, 0);

        yield return new WaitForSeconds(0.8f);

        MonsterPattern.MonsterState monsterState = monsterPattern_Abyss.GetCurMonsterState();
        if (monsterState != MonsterPattern.MonsterState.Death)
        {
            //------------------------------------------------------------------------------------//
            //*점프전 주목 풀기
            GameManager.instance.cameraController.AttentionBan(true);
            //-----------------------------------------------------------------------------------//

            // 점프전 잠깐 밑으로 내려감.
            while (time < 0.05)
            {
                time += Time.deltaTime;
                transform.Translate(-Vector3.up * speed * Time.deltaTime);
                yield return null;
            }

            //* 연기이펙트-----------------------------------------------------------------------//
            Effect effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_02");
            Vector3 effectPos = originPos;
            effectPos.y += 2.5f;
            effect.transform.position = effectPos;
            //-------------------------------------------------------------------------------------//
            GameManager.Instance.cameraController.cameraShake.ShakeCamera(1f, 2, 2);

            //*점프 Up
            time = 0;
            Vector3 targetPos = transform.position + (Vector3.up * 60);

            while (time < 5f)
            {
                time += Time.deltaTime;
                speed = Mathf.Lerp(90, 60, Time.time);
                transform.Translate(Vector3.up * speed * Time.deltaTime);

                if (transform.position.y >= targetPos.y)
                {
                    Debug.Log("Break~~~");
                    break;
                }
                yield return null;
            }

            //TODO:  그림자 끄기

        }
        else
        {
            monsterPattern_Abyss.SetBossAttackAnimation(MonsterPattern_Boss.BossMonsterAttackAnimation.Skill01, 2);
        }


        monsterPattern_Abyss.isJump = false;
    }

    IEnumerator JumpDown(Vector3 curTargetPos, bool getDamage = true)
    {
        float speed;
        float time = 0;
        transform.position = new Vector3(curTargetPos.x, transform.position.y, curTargetPos.z);

        Vector3 monsterPos = new Vector3(curTargetPos.x, 0, curTargetPos.z);
        Vector3 playerPos = new Vector3(playerTrans.position.x, 0, playerTrans.position.z);

        // 몬스터가 플레이어를 향하도록 하는 방향 벡터
        Vector3 direction = (playerPos - monsterPos).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation;

        speed = 50f;
        monsterPattern_Abyss.SetBossAttackAnimation(MonsterPattern_Boss.BossMonsterAttackAnimation.Skill01, 1);

        while (time < 5f)
        {
            time += Time.deltaTime;
            speed = Mathf.Lerp(50, 90, Time.time);
            transform.Translate(-Vector3.up * speed * Time.deltaTime);
            if (transform.position.y <= curTargetPos.y)
                break;
            yield return null;
        }

        transform.position = new Vector3(curTargetPos.x, curTargetPos.y, curTargetPos.z);

        //* 사운드
        //monsterPattern_Abyss.m_monster.SoundPlay(Monster.monsterSound.Alarm, false);
        monsterPattern_Abyss.m_monster.SoundPlay("Boss_Skill01", false);
        //* 데미지 체크
        if (getDamage)
            monsterPattern_Abyss.CheckPlayerDamage(6.5f, transform.position, 20, true);

        //* 연기이펙트-----------------------------------------------------------------------//
        GameManager.Instance.cameraController.cameraShake.ShakeCamera(1f, 3, 3);
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Smoke_Effect_03");
        Vector3 effectPos = transform.position;
        effectPos.y -= 1.5f;
        effect.transform.position = effectPos;

        //* 점프 후 주목 가능
        GameManager.instance.cameraController.AttentionBan(false);

        //* 점프 끝!
        monsterPattern_Abyss.isJump = false;
        MonsterPattern.MonsterState monsterState = monsterPattern_Abyss.GetCurMonsterState();
        if (monsterState != MonsterPattern.MonsterState.Death && !originNoAttack)
            monsterPattern_Abyss.noAttack = false; //* 이제 공격 가능

        monsterPattern_Abyss.NavMesh_Enable(true);

        monsterPattern_Abyss.SetMove_AI(false);
        monsterPattern_Abyss.SetAnimation(MonsterPattern.MonsterAnimation.Idle);
    }

    //* 스킬01 내려찍기 중, 플레이어를 쫒아다니는 이펙트 
    IEnumerator FollowPlayer_Effect_InSkill01(float duration)
    {
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("PulseGrenade_01");
        EffectController effectController = effect.gameObject.GetComponent<EffectController>();
        effectController.ChangeSize();
        //* 0.5=> 1.4로 scale;
        Vector3 GroundPos = monsterPattern_Abyss.GetGroundPos(playerTrans);
        effect.transform.position = GroundPos;
        float time = 0;

        while (time < duration - 1)
        {
            time += Time.deltaTime;
            GroundPos = monsterPattern_Abyss.GetGroundPos(playerTrans);
            effect.transform.position = GroundPos;
            yield return null;
        }
        time = 0;
        duration = 1;
        Vector3 startScale = new Vector3(0.2f, 0.2f, 0.2f);
        Vector3 endScale = new Vector3(1.4f, 1.4f, 1.4f);


        while (time < duration)
        {
            GroundPos = monsterPattern_Abyss.GetGroundPos(playerTrans);
            effect.transform.position = GroundPos;

            effect.transform.localScale = Vector3.Lerp(startScale, endScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(1f);

        effect.finishAction = () =>
            {
                effect.transform.localScale = startScale;
            };
        effect.StopEffect();
    }


}
