using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackCheck : MonoBehaviour
{
    public bool isEnable = false;
    [SerializeField] private Monster monster;

    public PlayerController _controller;// = new PlayerController();
    private PlayerController P_Controller => _controller;
    private CurrentValue P_Value => P_Controller._currentValue;
    private CurrentState P_States => P_Controller._currentState;

    void Start()
    {
        Transform currentTransform = transform;
        while (currentTransform.parent != null)
        {
            currentTransform = currentTransform.parent;
        }
        _controller = currentTransform.GetComponent<PlayerController>();
    }

    private void isBouncingToFalse()
    {
        P_States.isBouncing = false;
        P_Value.maxHitScale = 1.2f;
        P_Value.minHitScale = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEnable)
        {
            if (other.gameObject.tag == "Monster")
            {
                monster = other.GetComponentInParent<Monster>();

                if (monster.monsterPattern.GetCurMonsterState() != MonsterPattern.MonsterState.Death)
                {
                    //Debug.Log($"hit monster ,  curState  {monster.monsterPattern.GetCurMonsterState()}");
                    if (monster != null && !P_States.hadAttack)
                    {
                        P_States.hadAttack = true;
                        //TODO: 나중에 연산식 사용.
                        monster.GetDamage(700);

                        P_Value.nowEnemy = monster.gameObject;  //* 몬스터 객체 저장
                        P_Value.curHitTime = Time.time; //* 현재 시간 저장

                        P_Controller.CheckHitTime();
                        P_Value.hits = P_Value.hits + 1;    //* 히트 수 증가

                        P_States.isBouncing = true;     //* 히트 UI 출력효과
                        Invoke("isBouncingToFalse", 0.3f);  //* 히트 UI 출력효과 초기화

                        Debug.Log("hits : " + P_Value.hits);
                    }
                    else if (monster != null && P_States.hadAttack)
                    {
                        //이미 한번 때린 상태
                    }
                    else
                        Debug.LogError("몬스터 : null");
                }
            }
            else
            {

            }
        }

    }
}
