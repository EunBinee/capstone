using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.Timeline;

public class CameraController : MonoBehaviour
{
    public CameraInfo cameraInfo;
    public CinemachineBrain cinemachineBrain;
    public SignalReceiver signalReceiver;
    public CameraShake cameraShake;
    public PlayerController playerController;
    public Transform playerHeadPos; //* 벽체크에쓰임
    public Transform playerBackPos; //* 조준 할때 쓰임

    [Header("카메라 오브젝트.")]
    public GameObject playerCamera;      //카메라 오브젝트
    public GameObject playerCameraPivot; //카메라 피봇
    public Camera cameraObj;             //카메라.
    public Transform cameraTrans;

    [Header("스피트")]
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
    public bool stopRotation = false;

    [Header("스크롤")]
    public float scrollSpeed = 20;// 스크롤 속도
    public float default_FieldOfView = 35f; //기본 zoom

    [Header("주목 기능")]
    public bool banAttention = false; // 주목 금지
    public bool isBeingAttention = false;
    public Monster curTargetMonster = null;
    public Transform targetTrans;

    [Header("주목 대기")]
    public bool awaitAttention = false;
    public List<Monster> awaitMonster;
    //주목 후 카메라 리셋
    Coroutine resetCameraZ_co = null;

    //"벽 체크"
    [Header("벽체크 후, 앞으로 가는 속도")]
    public float frontOfTheWall_Speed = 20;
    //* 주목X Z값
    float minZ = -0.9f; //벽 앞이라 Z값을 땡겼을때
    float maxZ = -5f;
    float minY = 1.2f; //Pivot의 Y값 고정
    //* 주목O Z값
    float minZ_Attention = -0.9f; //벽 앞이라 Z값을 땡겼을때
    float maxZ_Attention = -7f;
    float minY_Attention = 1.4f; //벽 앞이라 Z값을 땡겼을때 Pivot의 Y값
    float maxY_Attention = 1.7f;
    float time_Z = 0;

    [Header("화살 스킬 관련 카메라")]
    public bool use_aimCamera = false; // 조준 사용여부



    private void Awake()
    {
        cameraTrans = cameraObj.gameObject.GetComponent<Transform>();
        cameraShake = GetComponent<CameraShake>();
    }

    private void Start()
    {
        playerController = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
        playerHeadPos = GameManager.Instance.gameData.playerHeadPos;
        playerBackPos = GameManager.instance.gameData.playerBackPos;
        cameraObj.fieldOfView = default_FieldOfView;
        Check_Z();
        CamReset();
        stopRotation = false;
    }

    private void Update()
    {
        CameraInput();
    }

    //* 카메라 Input
    public void CameraInput()
    {
        if (!use_aimCamera)
        {
            //*주목
            if (GameManager.instance.monsterUnderAttackList.Count > 0 && !banAttention) // - 현재 플레이어와 싸우고 있는 몬스터가 있고.
            {
                if (Input.GetMouseButtonUp(2)) //* 다른 몬스터로 주목 옮김
                {
                    if (!isBeingAttention) // - 만약에 아무것도 주목이 안되어잇는상태면?
                    {
                        //주목
                        AttentionMonster();
                    }
                    else //이미 주목 중인 상태라면?
                    {
                        UndoAttention();
                    }
                }
                if (Input.GetKeyDown(KeyCode.Tab)) //* 다른 몬스터로 주목 옮김
                {
                    if (isBeingAttention) // 주목되있는 상태라면?
                    {
                        //다른 몬스터로 다시 주목
                        if (GameManager.instance.monsterUnderAttackList.Count > 1)
                        {
                            ChangeAttentionMonster();
                        }
                    }
                }
            }
            //* 조준모드
            if (Input.GetKeyDown(KeyCode.F4))
            {
                SetAimCamera();
            }

            //* 마우스 휠 줌인줌아웃
            float scroll = -Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
            if (scroll != 0)
            {
                ScrollZoomInOut(scroll);
            }
        }
        else if (use_aimCamera)
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                OffAimCamera();
            }
        }
    }

    //* 스크롤 줌인 줌아웃
    public void ScrollZoomInOut(float scroll)
    {
        if (cameraObj.fieldOfView <= 20f && scroll < 0)
        {
            cameraObj.fieldOfView = 20f;
        }
        else if (cameraObj.fieldOfView >= 50f && scroll > 0)
        {
            cameraObj.fieldOfView = 50f;
        }
        else
        {
            cameraObj.fieldOfView += scroll;
        }
    }

    private void LateUpdate()
    {
        if (!use_aimCamera)
        {
            //* 조준 카메라 아닐때
            CameraActions();
        }
        else if (use_aimCamera)
        {
            //* 조준 카메라
            AimCameraActions();
        }
    }

    #region 일반 카메라 (조준X, 주목X)
    //* 카메라 움직임
    private void CameraActions()
    {
        CameraFollowPlayer(); //*플레이어를 따라다니는 카메라

        if (isBeingAttention) //* 주목 카메라
        {
            TargetRotate();
            FixCamZ();
        }
        else
        {
            if (!stopRotation)
                CameraRotate();  //마우스 방향에 따른 카메라 방향
            float camPosZ = WallInFrontOfCamera(minZ, maxZ);
            Vector3 targetPos = new Vector3(0, 0, camPosZ);
            cameraObj.gameObject.transform.localPosition = Vector3.Lerp(cameraObj.gameObject.transform.localPosition, targetPos, frontOfTheWall_Speed * Time.deltaTime);

            if (playerCameraPivot.transform.localPosition.y != minY)
            {
                playerCameraPivot.transform.localPosition = new Vector3(0, minY, 0);
            }
        }
    }

    private void CameraFollowPlayer()
    {
        //플레이어를 따라다니는 카메라
        //ref는 call by reference를 하겠다는 것.
        Vector3 cameraPos;
        cameraPos = Vector3.Lerp(playerCamera.transform.position, playerController.gameObject.transform.position, 17f * Time.deltaTime);
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

        //가로 세로 => 카메라 최상위 부모
        cameraRot = Vector3.zero;
        cameraRot.y = left_right_LookAngle;
        targetCameraRot = Quaternion.Euler(cameraRot);
        playerCamera.transform.rotation = targetCameraRot;
        //위아래 => cameraPivot
        cameraRot = Vector3.zero;
        cameraRot.x = up_down_LookAngle;
        targetCameraRot = Quaternion.Euler(cameraRot);
        playerCameraPivot.transform.localRotation = targetCameraRot;
    }

    #endregion

    #region 주목
    //* 주목 금지 (매개변수 true => 주목 금지, false => 주목 금지 해제)
    public void AttentionBan(bool ban)
    {
        if (ban) //주목 금지
        {
            //* 현재 주목 중이면? 주목 풀기
            if (isBeingAttention)
            {
                //주목중.
                UndoAttention();
                banAttention = ban;
                isBeingAttention = false; //주목해제
            }
            else
            {
                banAttention = ban;
            }

        }
        else if (!ban) //주목 금지 해제
        {
            banAttention = false;
            //AttentionMonster();
        }
    }

    //* 처음 주목
    public void AttentionMonster()
    {
        if (GameManager.instance.monsterUnderAttackList.Count > 0)
        {
            if (resetCameraZ_co != null)
                StopCoroutine(resetCameraZ_co);

            cameraObj.fieldOfView = default_FieldOfView;
            Vector3 camPos = cameraTrans.localPosition;
            float curZ = WallInFrontOfCamera(minZ_Attention, maxZ_Attention);
            camPos.z = curZ;
            cameraTrans.localPosition = camPos;
            playerController._currentState.isStrafing = true;
            //처음 주목한 경우
            isBeingAttention = true;
            //* 처음에 주목할 때는 가장 가까이에 있는 몬스터부터 주목
            GameManager.instance.SortingMonsterList();
            curTargetMonster = GameManager.instance.monsterUnderAttackList[0];
        }
    }

    //*주목 대상 바꾸기
    public void ChangeAttentionMonster()
    {
        GameManager.instance.SortingMonsterList();

        if (GameManager.instance.monsterUnderAttackList.Count >= 2)//1마리 이상이면?
        {
            if (curTargetMonster == GameManager.instance.monsterUnderAttackList[0])
            {
                curTargetMonster = GameManager.instance.monsterUnderAttackList[1];
            }
            else
            {
                curTargetMonster = GameManager.instance.monsterUnderAttackList[0];
            }
        }
        else if (GameManager.instance.monsterUnderAttackList.Count == 1)
        {
            curTargetMonster = GameManager.instance.monsterUnderAttackList[0];
        }
    }

    //*주목 풀기
    public void UndoAttention()
    {
        ResetCameraZ();
        cameraObj.fieldOfView = default_FieldOfView;
        playerController._currentState.isStrafing = false;

        Vector3 playerCam = playerCamera.transform.rotation.eulerAngles;
        left_right_LookAngle = playerCam.y;
        up_down_LookAngle = playerCam.x;

        banAttention = false;
        isBeingAttention = false;
        curTargetMonster = null;
    }

    //* 주목 대상쪽으로 회전
    private void TargetRotate()
    {
        if (curTargetMonster == null)
        {
            Debug.Log("카메라. 타겟 몬스터 null이다.");
            return;
        }

        Vector3 cameraRot; //camera pivot의 회전 Euler (위아래)
        Quaternion targetCameraRot = Quaternion.identity; //camera의 회전 (좌우)
        Vector3 targetPos = curTargetMonster.gameObject.transform.position;

        //* 카메라와 몬스터의 방향 벡터
        Vector3 directionToTarget_camera = targetPos - playerCamera.transform.position;
        Vector3 directionToTarget_pivot = targetPos - playerCameraPivot.transform.position;

        //* 카메라 좌우\
        directionToTarget_camera.y *= -1f;
        targetCameraRot = Quaternion.LookRotation(directionToTarget_camera);
        if (QuaternionAngleDifference(targetCameraRot, playerCamera.transform.rotation) < 1f)
        {
            //각도가 1보다 작으면.. 같은걸로 간주
            playerCamera.transform.rotation = targetCameraRot;
        }
        else if (targetCameraRot != Quaternion.identity)
            playerCamera.transform.rotation = Quaternion.Slerp(playerCamera.transform.rotation, targetCameraRot, 4f * Time.deltaTime);

        cameraRot = Vector3.zero;

        cameraRot.x = directionToTarget_pivot.y * -3;
        if (cameraRot.x < 0)
            cameraRot.x = 0;

        //* Camera Pivot 위아래
        targetCameraRot = Quaternion.Euler(cameraRot);
        playerCameraPivot.transform.localRotation = targetCameraRot;
        SetCameraZ_Attention();
    }

    bool curTouchWall = false;

    //* 주목 할때 Z값
    private void SetCameraZ_Attention()
    {
        float originZ = WallInFrontOfCamera(minZ_Attention, maxZ_Attention);
        Vector3 cameraPivotPos = playerCameraPivot.transform.localPosition;
        float duration = 1f;

        if (originZ != maxZ_Attention)
        {
            //* 장애물 존재
            if (!curTouchWall)
            {
                curTouchWall = true;
                time_Z = 0;
            }
            if (cameraPivotPos.y != minY_Attention)
            {
                time_Z += Time.deltaTime;
                float value = Mathf.Lerp(cameraPivotPos.y, minY_Attention, time_Z / duration);
                cameraPivotPos.y = value;
                playerCameraPivot.transform.localPosition = cameraPivotPos;
            }
        }
        else
        {
            //* 장애물 없음.
            if (curTouchWall)
            {
                curTouchWall = false;
                time_Z = 0;
            }
            if (cameraPivotPos.y != maxY_Attention && !use_aimCamera)
            {
                time_Z += Time.deltaTime;
                float value = Mathf.Lerp(cameraPivotPos.y, maxY_Attention, time_Z / duration);
                cameraPivotPos.y = value;
                playerCameraPivot.transform.localPosition = cameraPivotPos;
            }
        }
        // 일정한 속도로 이동하기 위한 보간
        Vector3 targetPos = new Vector3(0, 0, originZ);
        cameraObj.gameObject.transform.localPosition = Vector3.Lerp(cameraObj.gameObject.transform.localPosition, targetPos, frontOfTheWall_Speed * Time.deltaTime);
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
        Check_Z();
        Vector3 camPos = cameraTrans.localPosition;
        Vector3 camPivotPos = playerCameraPivot.transform.localPosition;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;

            float value = Mathf.Lerp(camPivotPos.y, minY, time / duration);
            camPivotPos.y = value;
            playerCameraPivot.transform.localPosition = camPivotPos;

            yield return null;
        }

        camPos = cameraTrans.localPosition;

        float curZ = WallInFrontOfCamera(minZ, maxZ);
        camPos.z = curZ;
        cameraTrans.localPosition = camPos;

        camPivotPos = playerCameraPivot.transform.localPosition;
        camPivotPos.y = minY;
        playerCameraPivot.transform.localPosition = camPivotPos;
    }

    public void Check_Z()
    {
        if (!GameManager.instance.bossBattle)
        {
            //! 보스전때 Z값 바꿀때 여기서 바꾸기 
        }
        else
        {
            //* 주목X Z값
            minZ = -0.9f; //벽 앞이라 Z값을 땡겼을때
            maxZ = -5f;
            minY = 1.2f; //Pivot의 Y값 고정
                         //* 주목O Z값
            minZ_Attention = -0.9f; //벽 앞이라 Z값을 땡겼을때
            maxZ_Attention = -7f;
            minY_Attention = 1.4f; //벽 앞이라 Z값을 땡겼을때 Pivot의 Y값
            maxY_Attention = 1.7f;
            time_Z = 0;
        }
    }

    #endregion

    #region 조준 카메라
    public void SetAimCamera()
    {
        //! 아래 코드 임시로 주목
        playerController._currentState.isStrafing = true;
        cameraObj.fieldOfView = default_FieldOfView;
        use_aimCamera = true;
        Vector3 rotZero = new Vector3(0, 0, 0);
        playerCameraPivot.transform.localRotation = Quaternion.Euler(rotZero);
    }
    public void OffAimCamera()
    {
        //! 아래 코드 임시로 주목
        playerController._currentState.isStrafing = false;
        use_aimCamera = false;
        CameraRecovery();

    }

    private void AimCameraActions()
    {
        AimCameraSetZ();
        AimCameraFollowPlayer();
        AimCameraRotate();

        if (playerController._currentValue.moveAmount == 0)
        {
            AimCameraLeftRightRotate_moveAmount();
        }
        else
        {
            AimCameraLeftRightRotate();
        }
        AimCameraUpDownRotate();
    }

    private void AimCameraSetZ()
    {
        Vector3 cameraSetVec = new Vector3(0.15f, 0.4f, -1.5f);

        cameraObj.transform.localPosition = cameraSetVec;
    }

    //* 방향 전환
    private void AimCameraRotate()
    {
        Vector3 targetDirection = playerController.gameObject.transform.position - playerCamera.gameObject.transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        //Quaternion tempRot = Quaternion.Slerp(transform.rotation, targetRotation, 30 * Time.deltaTime);
        Quaternion tempRot = targetRotation;
        Vector3 tempRot_Euler = tempRot.eulerAngles;
        tempRot_Euler = new Vector3(0, tempRot_Euler.y, 0);

        playerCamera.gameObject.transform.rotation = Quaternion.Euler(tempRot_Euler);
    }

    //* 위아래 카메라
    private void AimCameraUpDownRotate()
    {
        //마우스 방향에 따른 카메라 방향
        Vector3 cameraRot;
        Quaternion targetCameraRot;
        up_down_LookAngle -= (playerController._input.mouseY * up_down_LookSpeed) * 0.4f * Time.deltaTime;

        up_down_LookAngle = Mathf.Clamp(up_down_LookAngle, -40, maxPivot); //위아래 고정
        //위아래 => cameraPivot
        cameraRot = Vector3.zero;
        cameraRot.x = up_down_LookAngle;
        targetCameraRot = Quaternion.Euler(cameraRot);
        playerCameraPivot.transform.localRotation = targetCameraRot;
    }
    //* 좌우 카메라
    private void AimCameraLeftRightRotate()
    {
        //마우스 방향에 따른 카메라 방향
        Vector3 cameraRot;
        Quaternion targetCameraRot;
        left_right_LookAngle += (playerController._input.mouseX * left_right_LookSpeed) * 0.4f * Time.deltaTime;

        //좌우 => playerCamera
        cameraRot = Vector3.zero;
        cameraRot.y = left_right_LookAngle;
        targetCameraRot = Quaternion.Euler(cameraRot);
        playerCamera.transform.localRotation = targetCameraRot;
    }
    //* 좌우 카메라
    private void AimCameraLeftRightRotate_moveAmount()  //안움직일때
    {
        //마우스 방향에 따른 카메라 방향
        Vector3 cameraRot;
        Quaternion targetCameraRot;
        left_right_LookAngle += playerController._input.mouseX * left_right_LookSpeed * Time.deltaTime;

        cameraRot = Vector3.zero;
        cameraRot.y = left_right_LookAngle;
        targetCameraRot = Quaternion.Euler(cameraRot);
        playerController.transform.localRotation = targetCameraRot;
    }

    private void AimCameraFollowPlayer()
    {
        //Vector3 cameraPos = Vector3.Lerp(playerCamera.transform.position, playerBackPos.position, 17f * Time.deltaTime);
        Vector3 cameraPos = playerBackPos.position;
        playerCamera.transform.position = playerBackPos.position;
    }

    #endregion

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

    void CamReset()
    {
        float resetZ = WallInFrontOfCamera(minZ, maxZ);
        cameraObj.gameObject.transform.localPosition = new Vector3(0, 0, resetZ);
        cameraObj.gameObject.transform.localRotation = Quaternion.identity;
        cameraObj.fieldOfView = default_FieldOfView;
    }
    void CamReset_Lerp()
    {
        float resetZ = WallInFrontOfCamera(minZ, maxZ);
        // cameraObj.gameObject.transform.localPosition = new Vector3(0, 0, resetZ);
        Vector3 resetVector = new Vector3(0, 0, resetZ);
        cameraObj.gameObject.transform.localRotation = Quaternion.identity;
        cameraObj.fieldOfView = default_FieldOfView;
        while (cameraObj.gameObject.transform.localPosition != resetVector)
        {
            cameraObj.gameObject.transform.localPosition = Vector3.Lerp(cameraObj.gameObject.transform.localPosition, resetVector, 2 * Time.deltaTime);
            if (resetVector == cameraObj.gameObject.transform.localPosition)
            {
                break;
            }
        }

    }

    //* 보스전 끝난 후 주목 풀기.
    public void BossCameraReset(float stopTime)
    {
        StartCoroutine(BossCameraReset_Co(stopTime));
    }

    IEnumerator BossCameraReset_Co(float stopTime)
    {
        yield return new WaitForSeconds(stopTime);
        GameManager.instance.bossBattle = false;
        Check_Z();
        ResetCameraZ();
        UndoAttention();

    }

    //*------------------------------------------------------------------------------------------//
    //* 카메라 벽체크
    public float WallInFrontOfCamera(float max = -0.9f, float min = -5f)
    {
        if (playerController._currentState.isStartComboAttack || playerController._currentState.isDodgeing)
            return cameraObj.gameObject.transform.localPosition.z;

        int monsterLayerMask = 1 << LayerMask.NameToLayer("Monster"); //몬스터 제외
        Vector3 curDirection = cameraObj.gameObject.transform.position - playerHeadPos.position;
        Debug.DrawRay(playerHeadPos.position, curDirection * 20, Color.magenta);
        Ray ray = new Ray(playerHeadPos.position, curDirection);
        RaycastHit hit;
        // Ray와 충돌한 경우
        if (Physics.Raycast(ray, out hit, 20, ~monsterLayerMask)) //몬스터 제외
        {
            float dist = Vector3.Distance(hit.point, cameraObj.gameObject.transform.position);//  (hit.point - cameraObj.gameObject.transform.position).magnitude;

            bool isbehind = CheckObj_behindCamera(hit.point);

            float camPosZ = 0;
            if (isbehind)
            {
                //앞에있음
                camPosZ = cameraObj.gameObject.transform.localPosition.z + dist;
            }
            else
            {
                //뒤에 있음.
                camPosZ = cameraObj.gameObject.transform.localPosition.z - dist;
            }

            if (camPosZ >= max)
            {
                camPosZ = max;
            }
            if (camPosZ <= min)
            {
                camPosZ = min;
            }
            return camPosZ;
        }
        return min;
    }

    //* 감지된 객체가 카메라의 뒤에 있는지 앞에 있는지 확인용 함수
    public bool CheckObj_behindCamera(Vector3 objPos)
    {
        // 충돌 지점이 현재 객체의 앞쪽에 있는지 뒷쪽에 있는지 확인
        Vector3 directionToHitPoint = objPos - cameraObj.gameObject.transform.position;
        float dotProduct = Vector3.Dot(directionToHitPoint, cameraObj.gameObject.transform.forward);

        if (dotProduct > 0)
        {
            // 충돌 지점이 현재 객체의 앞쪽에 있음
            //Debug.Log("충돌 지점이 앞에 있습니다.");
            return true;
        }
        else
        {
            // 충돌 지점이 현재 객체의 뒷쪽에 있음
            //Debug.Log("충돌 지점이 뒤에 있습니다.");
            return false;
        }
    }

    //*------------------------------------------------------------------------------------------//
    //*시네머신 이후 카메라 복구
    public void CameraRecovery()
    {
        Check_Z();
        CamReset(); //rotation, position 변경
    }

    public void CameraRecovery_Lerp()
    {
        Check_Z();
        CamReset_Lerp();
    }

    public void CinemachineSetting(bool _value)
    {
        cinemachineBrain.enabled = _value;
        signalReceiver.enabled = _value;
    }
    //*------------------------------------------------------------------------------------------//
    void OnPreCull() => GL.Clear(true, true, Color.black);
}
