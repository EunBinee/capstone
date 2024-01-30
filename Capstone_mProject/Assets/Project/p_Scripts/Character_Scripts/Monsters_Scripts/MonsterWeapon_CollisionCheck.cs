using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterWeapon_CollisionCheck : MonoBehaviour
{
    public bool onEnable = false;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private Monster monster;

    public bool yetAttack = true;  //true : 아직 콜라이더에 플레이어가 닿지않았다. flase : 콜라이더에 플레이어가 닿았다.

    void Start()
    {
        playerController = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
        monster = transform.GetComponentInParent<Monster>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (onEnable && yetAttack)
        {
            if (other.CompareTag("Player") && monster.monsterPattern.canAttack)
            {
                yetAttack = false;
                monster.OnHit(5);
            }
        }
    }
}
