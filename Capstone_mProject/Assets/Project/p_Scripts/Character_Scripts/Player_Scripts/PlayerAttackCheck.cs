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

    void Start()
    {
        Transform currentTransform = transform;
        while (currentTransform.parent != null)
        {
            currentTransform = currentTransform.parent;
        }
        _controller = currentTransform.GetComponent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEnable)
        {
            if (other.gameObject.tag == "Monster")
            {
                Debug.Log("hit monster");
                monster = other.GetComponentInParent<Monster>();
                if (monster != null)
                {
                    monster.GetDamage(15);
                    P_Value.curHitTime = Time.time; //현재 시간 저장
                    P_Controller.CheckHitTime();
                    P_Value.hits = P_Value.hits + 1;
                    Debug.Log("hits : " + P_Value.hits);
                }
                else
                    Debug.LogError("몬스터 : null");

            }
            else
            {

            }
        }

    }
}
