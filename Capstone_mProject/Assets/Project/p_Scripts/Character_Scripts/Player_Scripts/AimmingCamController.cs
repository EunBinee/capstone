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
}
