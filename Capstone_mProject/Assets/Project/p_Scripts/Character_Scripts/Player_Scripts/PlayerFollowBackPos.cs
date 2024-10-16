using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollowBackPos : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CameraController cameraController;
    private Vector3 playerPos;
    private Vector3 offset = new Vector3(0.5f, 0.12f, -0.65f);
    void Start(){
        Debug.Log("start");
        DontDestroyOnLoad(this.gameObject);
        //playerController = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
        //cameraController = playerController._playerFollowCamera.cameraObj.GetComponent<CameraController>();
        //if (cameraController == null) cameraController = GameManager.Instance.cameraController.GetComponent<CameraController>();
    }
    void FixedUpdate()
    {
        Debug.Log("fixed");
        playerPos = playerController.transform.position;
        playerPos = playerPos + offset;
        this.transform.position = playerPos;

        //!
        //this.transform.rotation = Quaternion.Euler(playerController.transform.forward);
        // AimCameraRotate();
        // if (playerController._currentValue.moveAmount == 0)
        // {
        //    AimCameraLeftRightRotate_moveAmount();
        // }
        // else
        // {
        //    AimCameraLeftRightRotate();
        // }
        this.transform.RotateAround(playerPos, Vector3.up, playerController._input.mouseX * cameraController.left_right_LookSpeed * Time.deltaTime);
    }
    
    //* 방향 전환
    private void AimCameraRotate()
    {
        Vector3 targetDirection = playerController.transform.position - this.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        Vector3 tempRot_Euler = targetRotation.eulerAngles;
        tempRot_Euler = new Vector3(0, tempRot_Euler.y, 0);

        this.transform.rotation = Quaternion.Euler(tempRot_Euler);
    }
    
    //* 좌우 카메라
    private void AimCameraLeftRightRotate()
    {
        //마우스 방향에 따른 카메라 방향
        Vector3 cameraRot;
        Quaternion targetCameraRot;
        cameraController.left_right_LookAngle += (playerController._input.mouseX * cameraController.left_right_LookSpeed) * 0.4f * Time.deltaTime;

        //좌우 => playerCamera
        cameraRot = Vector3.zero;
        cameraRot.y = cameraController.left_right_LookAngle;
        targetCameraRot = Quaternion.Euler(cameraRot);
        this.transform.rotation = targetCameraRot;
    }
    //* 좌우 카메라
    private void AimCameraLeftRightRotate_moveAmount()  //안움직일때
    {
        //마우스 방향에 따른 카메라 방향
        Vector3 cameraRot;
        Quaternion targetCameraRot;
        cameraController.left_right_LookAngle += playerController._input.mouseX * cameraController.left_right_LookSpeed * Time.deltaTime;

        cameraRot = Vector3.zero;
        cameraRot.y = cameraController.left_right_LookAngle;
        targetCameraRot = Quaternion.Euler(cameraRot);
        playerController.transform.localRotation = targetCameraRot;
    }
}
