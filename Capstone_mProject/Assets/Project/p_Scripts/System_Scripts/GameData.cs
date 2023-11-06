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

    public GameObject playerCamera;      //카메라 오브젝트
    public GameObject playerCameraPivot; //카메라 피봇
    public Camera cameraObj;             //카메라.

}
