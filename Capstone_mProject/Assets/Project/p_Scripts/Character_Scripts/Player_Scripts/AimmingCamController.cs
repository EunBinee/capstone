using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class AimmingCamController : MonoBehaviour
{
    public Transform campos;
    public GameObject player;

    public float left_right_LookAngle;
    public float up_down_LookAngle;
    public float left_right_LookSpeed = 500; //왼 오 돌리는 스피드
    public float up_down_LookSpeed = 500;    //위아래로 돌리는 스피드

    void Start()
    {
        player = GameManager.instance.gameData.player;
    }

    void Update()
    {
        this.transform.position = campos.position;  //* 위치 고정
    }
    private void FixedUpdate()
    {
        //CameraRotate();
    }
    private void CameraRotate() // 일반 카메라
    {
        //마우스 방향에 따른 카메라 방향
        Vector3 cameraRot;
        Quaternion targetCameraRot;
        left_right_LookAngle += (player.GetComponent<PlayerController>()._input.mouseX * left_right_LookSpeed) * Time.deltaTime;
        up_down_LookAngle -= (player.GetComponent<PlayerController>()._input.mouseY * up_down_LookSpeed) * Time.deltaTime;

        //left_right_LookAngle = Mathf.Clamp(left_right_LookAngle, -70, 50); //좌우 고정
        up_down_LookAngle = Mathf.Clamp(up_down_LookAngle, -25, 25); //위아래 고정

        //가로 세로
        cameraRot = Vector3.zero;
        cameraRot.y = left_right_LookAngle;
        targetCameraRot = Quaternion.Euler(cameraRot);
        transform.rotation = targetCameraRot;
        //위아래
        cameraRot = Vector3.zero;
        cameraRot.x = up_down_LookAngle;
        targetCameraRot = Quaternion.Euler(cameraRot);
        campos.transform.localRotation = targetCameraRot;
    }
}
