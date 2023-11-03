using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackCheck : MonoBehaviour
{
    [SerializeField] private Monster monster;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Monster")
        {
            Debug.Log("hit monster");
            monster = other.GetComponent<Monster>();
            if (monster != null)
                monster.GetDamage(3);
        }
        else
        {
            //Debug.Log("���� ���ؤ�");
        }
    }
}
