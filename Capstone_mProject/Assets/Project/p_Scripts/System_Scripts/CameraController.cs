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
    public Transform cameraTrans;

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
    public bool banAttention = false;
    public bool isBeingAttention = false;
    public Monster curTargetMonster = null;

    public float normal_Z = -5f;
    public float attention_Z = -6.5f;
    public float longAttention_Z = -7.5f;

    bool isNormal_Z = false;
    bool isAttention_Z = false;
    bool isLongAttention_Z = false;

    float time_Z = 0;
    float duration = 1f;

    Coroutine resetCameraZ_co = null;


    private void Start()
    {
        playerController = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
        cameraTrans = cameraObj.gameObject.GetComponent<Transform>();
    }

    private void Update()
    {
        //TODO: 주목 Input =>나중에 InputManager로 옮기기
        if (Input.GetKeyDown(KeyCode.Tab) && !banAttention)
        {
            //주목 기능
            if (GameManager.instance.monsterUnderAttackList.Count > 0)
            {
                if (!isBeingAttention)
                {
                    if (resetCameraZ_co != null)
                        StopCoroutine(resetCameraZ_co);
                    Vector3 camPos = cameraTrans.localPosition;
                    camPos.z = attention_Z;
                    cameraTrans.localPosition = camPos;
                    playerController._currentState.isStrafing = true;
                    //처음 주목한 경우
                    isBeingAttention = true;
                    //* 처음에 주목할 때는 가장 가까이에 있는 몬스터부터 주목
                    GameManager.instance.SortingMonsterList();
                    curTargetMonster = GameManager.instance.monsterUnderAttackList[0];
                }
                else
                {
                    //다른 몬스터로 다시 주목
                    if (GameManager.instance.monsterUnderAttackList.Count > 1)
                    {
                        GameManager.instance.SortingMonsterList();
                        if (curTargetMonster == GameManager.instance.monsterUnderAttackList[0])
                        {
                            curTargetMonster = GameManager.instance.monsterUnderAttackList[1];
                        }
                        else
                        {
                            curTargetMonster = GameManager.instance.monsterUnderAttackList[0];
                        }
                    }

                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (isBeingAttention)
            {
                UndoAttention();
            }
        }
    }

    public void UndoAttention()
    {
        ResetCameraZ();
        playerController._currentState.isStrafing = false;

        Vector3 playerCam = playerCamera.transform.rotation.eulerAngles;
        left_right_LookAngle = playerCam.y;
        up_down_LookAngle = playerCam.x;

        isBeingAttention = false;
        curTargetMonster = null;
    }


    private void LateUpdate()
    {
        CameraActions();

    }
    private void FixedUpdate()
    {

    }

    void OnPreCull() => GL.Clear(true, true, Color.black);

    //카메라 움직임
    private void CameraActions()
    {
        CameraFollowPlayer(); //플레이어를 따라다니는 카메라

        if (isBeingAttention)
        {
            TargetRotate();
            FixCamZ();
        }
        else
        {
            CameraRotate();  //마우스 방향에 따른 카메라 방향
        }
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

        //가로 세로
        cameraRot = Vector3.zero;
        cameraRot.y = left_right_LookAngle;
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

        SetCameraZ_AccDistance();

        Vector3 cameraRot;
        Quaternion targetCameraRot = Quaternion.identity;
        // 타겟의 위치로 향하는 방향 벡터를 구함
        Vector3 directionToTarget = curTargetMonster.gameObject.transform.position - playerCameraPivot.transform.position;
        cameraRot = Vector3.zero;
        cameraRot.y = directionToTarget.x;

        if (directionToTarget.sqrMagnitude >= 3)
        {
            targetCameraRot = Quaternion.LookRotation(directionToTarget);// 방향 벡터를 바라보도록 하는 Quaternion을 생성    
        }
        if (QuaternionAngleDifference(targetCameraRot, playerCamera.transform.rotation) < 1f)
        {
            //각도가 1보다 작으면.. 같은걸로 간주
            playerCamera.transform.rotation = targetCameraRot;
        }
        else if (targetCameraRot != Quaternion.identity)
            playerCamera.transform.rotation = Quaternion.Slerp(playerCamera.transform.rotation, targetCameraRot, 3f * Time.deltaTime); /* x speed */

        //playerCamera.transform.position = target.position - transform.forward * dis;
        //위아래
        cameraRot = Vector3.zero;
        targetCameraRot = Quaternion.Euler(cameraRot);
        playerCameraPivot.transform.localRotation = targetCameraRot;


    }


    public void ResetCameraZ()
    {
        if (cameraTrans == null)
        {
            cameraTrans = cameraObj.gameObject.GetComponent<Transform>();
        }
        if (resetCameraZ_co != null)
        {
            StopCoroutine(resetCameraZ_co);
        }
        resetCameraZ_co = StartCoroutine(ResetCameraZ_co(5f));
    }

    IEnumerator ResetCameraZ_co(float duration)
    {
        Vector3 camPos = cameraTrans.localPosition;
        Vector3 camPivotPos = playerCameraPivot.transform.localPosition;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            //camPos.z = -5f;
            float value = Mathf.Lerp(camPos.z, normal_Z, time / duration);
            camPos.z = value;
            cameraTrans.localPosition = camPos;

            value = Mathf.Lerp(camPivotPos.y, 1.7f, time / duration);
            camPivotPos.y = value;
            playerCameraPivot.transform.localPosition = camPivotPos;

            yield return null;
        }

        camPos = cameraTrans.localPosition;
        camPos.z = normal_Z;
        cameraTrans.localPosition = camPos;

        camPivotPos = playerCameraPivot.transform.localPosition;
        camPivotPos.y = 1.7f;
        playerCameraPivot.transform.localPosition = camPivotPos;
    }

    private void SetCameraZ_AccDistance()
    {
        //플레이어와 몬스터의 거리에 따른 z값 변경
        float distance = Vector3.Distance(playerController.gameObject.transform.position, curTargetMonster.gameObject.transform.position);

        Vector3 camPos = cameraTrans.localPosition;
        Vector3 camPivotPos = playerCameraPivot.transform.localPosition;

        float normalDistance = 10;
        float longAttention_Distance = 4;
        if (GameManager.instance.bossBattle)
        {
            normalDistance = 20;
            longAttention_Distance = 12;
        }


        if (distance > normalDistance)
        {
            //z 를 normal_Z(-5)로 변경
            if (camPos.z != normal_Z)
            {
                if (isNormal_Z == false)
                {
                    time_Z = 0;
                    isNormal_Z = true;
                    isAttention_Z = false;
                    isLongAttention_Z = false;
                }
                time_Z += Time.deltaTime;

                float value = Mathf.Lerp(camPos.z, normal_Z, time_Z / duration);
                camPos.z = value;
                cameraTrans.localPosition = camPos;

                value = Mathf.Lerp(camPivotPos.y, 1.7f, time_Z / duration);
                camPivotPos.y = value;
                playerCameraPivot.transform.localPosition = camPivotPos;
            }
        }
        else if (distance < longAttention_Distance)
        {
            //z를 longAttention_Z(-9)으로 변경
            if (camPos.z != longAttention_Z)
            {
                if (isLongAttention_Z == false)
                {
                    time_Z = 0;
                    isNormal_Z = false;
                    isAttention_Z = false;
                    isLongAttention_Z = true;
                }
                time_Z += Time.deltaTime;

                float value = Mathf.Lerp(camPos.z, longAttention_Z, time_Z / duration);
                camPos.z = value;
                cameraTrans.localPosition = camPos;

                if (camPivotPos.y != 1.2f)
                {
                    value = Mathf.Lerp(camPivotPos.y, 1.2f, time_Z / duration);
                    camPivotPos.y = value;
                    playerCameraPivot.transform.localPosition = camPivotPos;
                }
            }
        }
        else
        {
            //z를 -6으로 변경
            if (camPos.z != attention_Z)
            {
                if (isAttention_Z == false)
                {
                    time_Z = 0;
                    isNormal_Z = false;
                    isAttention_Z = true;
                    isLongAttention_Z = false;
                }
                time_Z += Time.deltaTime;

                float value = Mathf.Lerp(camPos.z, attention_Z, time_Z / duration);
                camPos.z = value;
                cameraTrans.localPosition = camPos;

                if (camPivotPos.y != 1.2f)
                {
                    value = Mathf.Lerp(camPivotPos.y, 1.2f, time_Z / duration);
                    camPivotPos.y = value;
                    playerCameraPivot.transform.localPosition = camPivotPos;
                }
            }
        }
    }

    public void Check_Z()
    {
        if (!GameManager.instance.bossBattle)
        {
            normal_Z = -5f;
            attention_Z = -6.5f;
            longAttention_Z = -7.5f;
        }
        else
        {
            normal_Z = -7f;
            attention_Z = -10f;
            longAttention_Z = -13f;
        }
    }
    //*----------------------------------------------------------------------------------------//
    void FixCamZ()
    {
        Vector3 pos = playerCamera.transform.rotation.eulerAngles;
        pos.z = 0;

        playerCamera.transform.rotation = Quaternion.Euler(pos);
    }
    // 두 Quaternion 간의 각도 차이를 반환하는 함수
    float QuaternionAngleDifference(Quaternion a, Quaternion b)
    {
        // Quaternion.Angle 함수를 사용하여 각도 차이 계산
        return Quaternion.Angle(a, b);
    }

}
