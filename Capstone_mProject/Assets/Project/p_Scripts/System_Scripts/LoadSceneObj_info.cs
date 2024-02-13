using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneObj_info : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    public string sceneName = "";

    public void PreLoadSceneSetting()
    {
        //* 이동전 할일 

        //* 현재 공격중인 몬스터들 전부 주목 풀기

        GameManager.instance.Stop_AllMonster();

        if (GameManager.instance.gameData.GetPlayerController()._currentState.isStrafing)
        {
            GameManager.instance.gameData.GetPlayerController()._currentState.isStrafing = false;
        }

        GameManager.instance.RemoveMonster();

    }
}
