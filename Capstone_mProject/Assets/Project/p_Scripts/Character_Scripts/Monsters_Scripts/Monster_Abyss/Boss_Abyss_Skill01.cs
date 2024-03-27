using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Abyss_Skill01 : MonoBehaviour
{
    MonsterPattern_Boss_Abyss monsterPattern_Boss_Abyss;

    public void Init(MonsterPattern_Boss_Abyss _monsterPattern_Boss_Abyss)
    {

        monsterPattern_Boss_Abyss = _monsterPattern_Boss_Abyss;

    }
    /*
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
            NavMeshHit hit;

            if (NavMesh.SamplePosition(curPlayerPos, out hit, 20f, NavMesh.AllAreas))
            {
                if (hit.position != curPlayerPos)
                    curPlayerPos = hit.position;
            }

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
    */
}
