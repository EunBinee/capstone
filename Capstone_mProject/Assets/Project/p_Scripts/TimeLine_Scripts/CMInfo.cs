using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CMInfo : MonoBehaviour
{
    CinemachineVirtualCamera m_Cam;
    public enum FollowSomething
    {
        None,
        playerTrans,
        playerHead
    }
    public enum LookAtSomething
    {
        None
    }

    public FollowSomething followSomething;
    public LookAtSomething lookAtSomething;

    public void Init()
    {
        m_Cam = GetComponent<CinemachineVirtualCamera>();
        SettingCM();
    }


    void SettingCM()
    {
        SettingFollowSomeThing();
        SettingLookAtSomeThing();
    }

    void SettingFollowSomeThing()
    {
        switch (followSomething)
        {
            case FollowSomething.None:
                break;
            case FollowSomething.playerTrans:
                m_Cam.Follow = GameManager.instance.gameData.GetPlayerTransform();
                break;
            case FollowSomething.playerHead:
                Debug.Log("dddd");
                m_Cam.Follow = GameManager.instance.gameData.playerHeadPos;
                break;
            default:
                break;
        }
    }
    void SettingLookAtSomeThing()
    {
        switch (lookAtSomething)
        {
            case LookAtSomething.None:
                break;
            default:
                break;
        }
    }

}
