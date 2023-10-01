using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GameData
{
    public GameObject player;

    public Transform GetPlayerTransform()
    {
        return player.transform;
    }
}
