using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public PlayerController playerController;

    public Player_AttackCheck.PlayerWeapons playerWeapons;

    LayerMask monsterLayer;

    public Transform rayPoint;
    Vector3 rayDirect;

    public Transform startPoint;
    public Transform endPoint;

    List<Monster> curAttackMonster;

    public bool checkAttackCheck = false;

    public void Init()
    {
        playerController = GameManager.instance.gameData.GetPlayerController();
        monsterLayer = GameManager.instance.gameData.monsterLayer;
        curAttackMonster = new List<Monster>();
        SettingRayDirect();
    }

    void SettingRayDirect()
    {
        switch (playerWeapons)
        {
            case Player_AttackCheck.PlayerWeapons.Sword:
                rayDirect = rayPoint.forward;
                break;
            case Player_AttackCheck.PlayerWeapons.Leg_L:
                rayDirect = -rayPoint.up;
                break;
            case Player_AttackCheck.PlayerWeapons.Leg_R:
                rayDirect = -rayPoint.up;
                break;
        }
    }


    public void Update()
    {
        if (checkAttackCheck)
        {
            ComboBasicAttack_RayCheck();
        }
    }

    public void ResetAttackMonsterList()
    {
        curAttackMonster.Clear();
    }

    public void ComboBasicAttack_RayCheck()
    {

        rayDirect = (endPoint.position - startPoint.position).normalized;

        Ray weaponRay = new Ray(rayPoint.position, rayDirect);
        Debug.DrawRay(rayPoint.position, rayDirect * 3f, Color.red);
        RaycastHit hitInfo;
        if (Physics.Raycast(weaponRay, out hitInfo, 3f, monsterLayer))
        {
            // 충돌한 물체가 몬스터인지 확인
            if (hitInfo.collider.CompareTag("Monster"))
            {
                // 몬스터와 충돌한 경우
                Monster monster = hitInfo.collider.gameObject.GetComponent<Monster>();
                if (monster == null)
                {
                    //* 최상위 부모까지 타고 올라가면서 monster.cs 찾기
                    monster = FindMonsterInParent(hitInfo.collider.gameObject.transform);
                    if (monster == null)
                    {
                        Debug.LogError("몬스터 태그를 가진 오브젝트에 몬스터 스크립트가 없습니다.");
                        return;
                    }
                }

                if (!curAttackMonster.Contains(monster))
                {
                    if (monster.monsterPattern.GetCurMonsterState() != MonsterPattern.MonsterState.Death)
                    {
                        curAttackMonster.Add(monster);
                        Debug.Log($" monster.name  :  {monster.name}");
                        //playerController.hitMonsters.Add(hitInfo.collider.gameObject);

                        if (monster.monsterData.isShieldMonster && monster.monsterPattern.isShield)
                        {
                            monster.monsterPattern.isShield = false;

                            Quaternion hitInfoRot = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                            playerController.player_AttackCheck.playerHitShield(monster, hitInfo.point, hitInfoRot);
                        }
                        else
                        {
                            Quaternion hitInfoRot = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                            playerController.player_AttackCheck.playerHitMonster(monster, hitInfo.point, hitInfoRot, Player_AttackCheck.PlayerWeapons.None, false);
                        }
                        //사운드
                        SoundManager.Instance.Play_PlayerSound(SoundManager.PlayerSound.Hit, false);

                    }
                }
            }
        }
    }

    public Monster FindMonsterInParent(Transform childTransform)
    {
        Transform parentTransform = childTransform.parent;
        while (parentTransform != null)
        {
            Monster monster = parentTransform.GetComponent<Monster>();
            if (monster != null)
            {
                return monster;
            }
            parentTransform = parentTransform.parent;
        }
        return null; // 부모 오브젝트에 Monster.cs가 없는 경우
    }

}
