using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraInfo : MonoBehaviour
{
    public CameraController cameraController;

    void Start()
    {
        CameraSetting();
    }

    private void CameraSetting()
    {
        Debug.Log("hi");
        //GameManager
        GameManager.instance.gameData.playerCamera = cameraController.playerCamera;
        GameManager.instance.gameData.playerCameraPivot = cameraController.playerCameraPivot;
        GameManager.instance.gameData.cameraObj = cameraController.cameraObj;
        GameManager.instance.cameraController = cameraController;

        UIManager.Instance.damageManager.m_Camera = cameraController.cameraObj;
        UIManager.Instance.hPBarManager.m_Camera = cameraController.cameraObj;

        //플레이어
        PlayerController playerController = GameManager.instance.gameData.GetPlayerController();
        playerController._playerFollowCamera.playerCamera = cameraController.playerCamera;
        playerController._playerFollowCamera.playerCameraPivot = cameraController.playerCameraPivot;
        playerController._playerFollowCamera.cameraObj = cameraController.cameraObj;

        GameManager.instance.startActionCam?.Invoke(cameraController);
        GameManager.instance.startActionCam = null;
    }

    public void ResetCamera()
    {
        //GameManager
        GameManager.instance.gameData.playerCamera = null;
        GameManager.instance.gameData.playerCameraPivot = null;
        GameManager.instance.gameData.cameraObj = null;
        //플레이어
        PlayerController playerController = GameManager.instance.gameData.GetPlayerController();
        playerController._playerFollowCamera.playerCamera = null;
        playerController._playerFollowCamera.playerCameraPivot = null;
        playerController._playerFollowCamera.cameraObj = null;
    }
}
