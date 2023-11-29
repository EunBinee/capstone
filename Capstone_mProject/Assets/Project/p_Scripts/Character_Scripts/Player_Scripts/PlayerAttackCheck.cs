using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackCheck : MonoBehaviour
{
    public bool isEnable = false;
    [SerializeField] private Monster monster;

    public PlayerController _playerController;// = new PlayerController();
    public PlayerController P_Controller => _playerController;
    private CurrentValue P_Value => _playerController._currentValue;
    private CurrentState P_States => _playerController._currentState;

    private GameObject player;
    private bool isArrow;

    void Start()
    {
        player = GameManager.Instance.gameData.player;
        // Transform currentTransform = transform;
        // while (currentTransform.parent != null)
        // {
        //     currentTransform = currentTransform.parent;
        // }
        _playerController = player.GetComponent<PlayerController>();
        //currentTransform.GetComponent<PlayerController>();
    }
    void FixedUpdate()
    {
        if (gameObject.tag == "Arrow")
        {
            isArrow = true;
            transform.position = P_Controller.shootPoint.position;
            transform.rotation = player.transform.rotation;
            if (!P_Controller.returnIsAim())
            {
                // 키네매틱 끄기
                GetComponent<Rigidbody>().isKinematic = false;
                //Vector3 dir = GameManager.Instance.gameData.player.transform.forward;
                Vector3 dir = Camera.main.transform.forward;
                //transform.position += dir * 0.1f;
                GetComponent<Rigidbody>().velocity += dir * 30f;
            }
        }
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

    private void OnCollisionEnter(Collision other)
    {
        if (isArrow)
        {
            this.gameObject.SetActive(false);
        }
    }
}
