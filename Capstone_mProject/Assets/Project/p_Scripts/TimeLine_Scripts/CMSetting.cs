using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CMSetting : MonoBehaviour
{
    public bool endSetting = false;
    public List<CMInfo> useCMInfo;

    private void Awake()
    {

    }
    public void Init()
    {
        endSetting = true;
        if (useCMInfo.Count > 0)
        {
            GameManager.instance.cameraController.CinemachineSetting(true);
            foreach (CMInfo cmInfo in useCMInfo)
            {
                cmInfo.gameObject.SetActive(true);
                cmInfo.Init();
                cmInfo.gameObject.SetActive(false);
            }
            GameManager.instance.cameraController.CinemachineSetting(false);
            GameManager.instance.cameraController.CameraRecovery();
        }
    }

}
