using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossWeakness : MonoBehaviour
{
    public Monster m_monster;
    public GameObject bossWeaknessEffect;
    //! 보스 약점
    public bool destroy_BossWeakness = false; //* false 아직 공격안당한 보스 약점. true 이미 공격당한 보스 약점
    Vector3 normalHitPoint = Vector3.zero;

    public string effectName01;
    public string effectName02;

    public void SetMonster(Monster _monster)
    {
        m_monster = _monster;
        effectName01 = "explosion_360_v1_M";
        effectName02 = "explosion_360_v2_M";
    }

    public void WeaknessGetDamage(Vector3 _normalHitPoint, Vector3 hitPoint)
    {
        //* 공격 당했을 때 연출
        destroy_BossWeakness = true;
        GameManager.Instance.cameraController.cameraShake.ShakeCamera(0.8f, 2f, 2f);
        StartCoroutine(GetDamageEffect(_normalHitPoint, hitPoint));
        m_monster.bossMonsterPattern.ReduceRemainWeaknessesNum(this);
    }

    IEnumerator GetDamageEffect(Vector3 _normalHitPoint, Vector3 hitPoint)
    {
        ShowExplosionEffect(effectName01, _normalHitPoint, hitPoint);
        yield return new WaitForSeconds(0.5f);
        ShowExplosionEffect(effectName02, _normalHitPoint, hitPoint);

        this.gameObject.SetActive(false);
    }

    void ShowExplosionEffect(string effectName, Vector3 _normalHitPoint, Vector3 hitPoint)
    {
        // 이펙트        
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, _normalHitPoint);
        Vector3 pos = hitPoint;

        Effect effect = GameManager.Instance.objectPooling.ShowEffect(effectName);
        effect.gameObject.transform.position = pos;
        effect.gameObject.transform.rotation = rot;
    }

}
