using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GameData
{
    public GameObject player;
    public Transform playerTargetPos;
    public Transform GetPlayerTransform()
    {
        return player.transform;
    }

}
