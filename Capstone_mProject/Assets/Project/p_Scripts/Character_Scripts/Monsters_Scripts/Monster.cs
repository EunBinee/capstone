using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Monster : MonoBehaviour
{
    public MonsterData monsterData;




    public virtual void Movement()
    {
        //움직임.
    }

    public virtual void Hit()
    {

    }
    public virtual void Death()
    {

    }

}
