using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterWeapon_CollisionCheck : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Monster monster;

    void Start()
    {
        playerController = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
        monster = transform.GetComponentInParent<Monster>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!playerController._currentState.isGettingHit)
            {
                monster.OnHit();
            }
        }
    }

}
