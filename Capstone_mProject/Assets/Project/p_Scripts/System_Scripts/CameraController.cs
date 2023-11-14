using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerController playerController;
    [Header("카메라 오브젝트.")]
    public GameObject playerCamera;      //카메라 오브젝트
    public GameObject playerCameraPivot; //카메라 피봇
    public Camera cameraObj;             //카메라.

    [Header("회전 스피트")]
    public float left_right_LookSpeed = 500; //왼 오 돌리는 스피드
    public float up_down_LookSpeed = 500;    //위아래로 돌리는 스피드
    [Header("위아래 고정 비율  >> 0이면 위아래로 카메라 안움직임")]
    public float minPivot = -35;              //위아래 고정 시키기 위한 Pivot -35로 아래 고정
    public float maxPivot = 35;               //35로 위 고정

    [Header("Camera Debug")]
    //카메라가 캐릭터를 쫒아가는 데 속력. zero로 초기화
    public Vector3 cameraFllowVelocity = Vector3.zero;
    public float left_right_LookAngle;
    public float up_down_LookAngle;

    [Header("주목 기능")]
    public bool isBeingAttention = false;
    private Monster curTargetMonster = null;


    private void Start()
    {
        playerController = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
    }

    private void Update()
    {
        //TODO: 주목 Input =>나중에 InputManager로 옮기기
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            //주목 기능
            if (playerController.monsterUnderAttackList.Count > 0)
            {
                if (!isBeingAttention)
                {
                    //처음 주목한 경우
                    isBeingAttention = true;
                    curTargetMonster = playerController.monsterUnderAttackList[0];
                }
                else
                {
                    //다른 몬스터로 다시 주목
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isBeingAttention)
            {
                isBeingAttention = false;
                curTargetMonster = null;
            }
        }
    }

    private void LateUpdate()
    {
        CameraActions();
    }

    void OnPreCull() => GL.Clear(true, true, Color.black);

    //카메라 움직임
    private void CameraActions()
    {
        CameraFollowPlayer(); //플레이어를 따라다니는 카메라
        CameraRotate();
        //if (isBeingAttention)
        //{
        //    TargetRotate();
        //}
        //else
        //{
        //           //마우스 방향에 따른 카메라 방향
        //}
    }

    private void CameraFollowPlayer()
    {
        //플레이어를 따라다니는 카메라
        //ref는 call by reference를 하겠다는 것.
        Vector3 cameraPos = Vector3.SmoothDamp(playerCamera.transform.position, playerController.gameObject.transform.position, ref cameraFllowVelocity, 0.1f);
        playerCamera.transform.position = cameraPos;
    }

    private void CameraRotate() // 일반 카메라
    {
        //마우스 방향에 따른 카메라 방향
        Vector3 cameraRot;
        Quaternion targetCameraRot;
        left_right_LookAngle += (playerController._input.mouseX * left_right_LookSpeed) * Time.deltaTime;
        up_down_LookAngle -= (playerController._input.mouseY * up_down_LookSpeed) * Time.deltaTime;

        up_down_LookAngle = Mathf.Clamp(up_down_LookAngle, minPivot, maxPivot); //위아래 고정

        cameraRot = Vector3.zero;
        cameraRot.y = left_right_LookAngle;
        //y에서 up_down_LookAngle을 안쓰고 left_right_LookAngle을 쓰는이유
        //*(중요)마우스가 위로 올라갈때, 유니티 좌표계에서는 좌표가 뒤바뀐다.
        // 마우스 X좌표가 위아래 좌표가 된다.
        //그래서 카메라rot y(위아래,세로)에는 마우스의 x축(가로)을 넣어주고
        // 카메라rot x축(좌우,가로)에는  마우스의 y축(세로)를 넣어준다

        //가로 세로
        targetCameraRot = Quaternion.Euler(cameraRot);
        playerCamera.transform.rotation = targetCameraRot;

        //위아래
        cameraRot = Vector3.zero;
        cameraRot.x = up_down_LookAngle;
        targetCameraRot = Quaternion.Euler(cameraRot);
        playerCameraPivot.transform.localRotation = targetCameraRot;
    }



    private void TargetRotate()
    {
        if (curTargetMonster == null)
        {
            Debug.Log("카메라. 타겟 몬스터 null이다.");
            return;
        }

    }

    private void TargetRotate_()
    {
        if (curTargetMonster == null)
        {
            Debug.Log("카메라. 타겟 몬스터 null이다.");
            return;
        }
        Vector3 cameraRot;
        Quaternion targetCameraRot;

        // 타겟의 위치로 향하는 방향 벡터를 구함
        Vector3 directionToTarget = curTargetMonster.gameObject.transform.position - transform.position;
        targetCameraRot = Quaternion.LookRotation(directionToTarget);// 방향 벡터를 바라보도록 하는 Quaternion을 생성
        // 현재 객체의 회전을 조절
        playerCamera.transform.rotation = targetCameraRot;
    }

}
