using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_AttackCheck : MonoBehaviour
{
    PlayerController playerController;
    public enum PlayerWeapons
    {
        None,
        Sword,
        Leg_L,
        Leg_R,
        Arrow
    }
    public void Init()
    {
        playerController = GetComponent<PlayerController>();
    }

    //* 검 
    public bool playerHitMonster(Monster monster, Vector3 collisionPoint, Quaternion otherQuaternion, PlayerWeapons playerWeapon, bool HitWeakness = false)
    {
        if (!monster.monsterPattern.noAttack)
        {
            double damageValue;

            if (playerWeapon == PlayerWeapons.Arrow)//isArrow)
            {
                if (playerController._currentState.isStrongArrow) //* 예스 차징
                {
                    if (HitWeakness && monster.monsterData.useWeakness)
                    {
                        damageValue = monster.monsterData.MaxHP * monster.monsterData.weaknessDamageRate;
                    }
                    else
                        damageValue = 550;
                    playerController._currentState.isStrongArrow = false;
                }
                else                        //* 노 차징
                {
                    damageValue = 400;
                }
            }
            else                            //* 검
            {
                damageValue = 350;
            }



            if (playerController._currentValue.hits % 5 != 0)
            {
                GameManager.instance.damageCalculator.damageExpression = "A+B";
                GameManager.instance.damageCalculator.CalculateAndPrint();
                damageValue = GameManager.instance.damageCalculator.result;
            }
            else if (playerController._currentValue.hits % 5 == 0 && playerController._currentValue.hits != 0)
            {
                GameManager.instance.damageCalculator.damageExpression = "A+C";
                GameManager.instance.damageCalculator.CalculateAndPrint();
                damageValue = GameManager.instance.damageCalculator.result;
            }

            monster.GetDamage(damageValue, collisionPoint, otherQuaternion, HitWeakness);

            if (!playerController._currentState.isBowMode)
            {
                playerController.playAttackEffect("Attack_Combo_Hit"); //* 히트 이펙트 출력
            }

            playerController._currentValue.nowEnemy = monster.gameObject;  //* 몬스터 객체 저장
            playerController._currentValue.curHitTime = Time.time; //* 현재 시간 저장

            playerController.CheckHitTime();
            playerController._currentValue.hits = playerController._currentValue.hits + 1;    //* 히트 수 증가
            playerController._currentState.hadAttack = true;
            playerController._currentState.notSameMonster = false;

            playerController._currentState.isBouncing = true;     //* 히트 UI 출력효과

            Invoke("isBouncingToFalse", 0.3f);  //* 히트 UI 출력효과 초기화

            return true;
        }
        else
            return false;
    }

    private void isBouncingToFalse()
    {
        playerController._currentState.isBouncing = false;
        playerController._currentValue.maxHitScale = 1.2f;
        playerController._currentValue.minHitScale = 1f;
    }

    //* 플레이어가 몬스터의 방패를 때렸을때.
    public void playerHitShield(Monster monster, Vector3 collisionPoint, Quaternion otherQuaternion)
    {
        int damageValue;

        GameManager.instance.damageCalculator.damageExpression = "A+B";
        GameManager.instance.damageCalculator.CalculateAndPrint();
        damageValue = 0;

        monster.GetDamage(damageValue, collisionPoint, otherQuaternion);
        playerController.playAttackEffect("Attack_Combo_Hit"); //* 히트 이펙트 출력
    }
}
