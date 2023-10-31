using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackCheck : MonoBehaviour
{
    [SerializeField] private Monster monster;

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Monster")
        {
            monster.GetDamage(5);
        }
    }
}
