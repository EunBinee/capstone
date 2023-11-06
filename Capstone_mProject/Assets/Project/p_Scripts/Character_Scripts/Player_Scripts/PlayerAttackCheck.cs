using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackCheck : MonoBehaviour
{
    public bool isEnable = false;
    [SerializeField] private Monster monster;

    private void OnTriggerEnter(Collider other)
    {
        if (isEnable)
        {
            if (other.gameObject.tag == "Monster")
            {

                Debug.Log("hit monster");
                monster = other.GetComponentInParent<Monster>();
                if (monster != null)
                    monster.GetDamage(3);
                else
                    Debug.LogError("몬스터 : null");

            }
            else
            {

            }
        }

    }
}
