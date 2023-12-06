using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterWeapon_CollisionCheck : MonoBehaviour
{
    public bool onEnable = false;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private Monster monster;

    void Start()
    {
        playerController = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
        monster = transform.GetComponentInParent<Monster>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (onEnable)
        {
            if (other.CompareTag("Player"))
            {
                monster.OnHit(5);
            }
        }
    }

}
