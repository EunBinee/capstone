using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysicsCheck : MonoBehaviour
{
    public PlayerController _controller;// = new PlayerController();
    private PlayerController P_Controller => _controller;
    private PlayerComponents P_Com => P_Controller._playerComponents;
    private CheckOption P_COption => P_Controller._checkOption;
    private CurrentState P_States => P_Controller._currentState;
    private CurrentValue P_Value => P_Controller._currentValue;

    private float _castRadius; //레이캐스트 반지름
    private float _castRadiusDiff; //그냥 캡슐 콜라이더 radius와 castRadius의 차이
    private float _capsuleRadiusDiff;
    private float _fixedDeltaTime; //물리 업데이트 발생주기
    public float rayCastHeightOffset = 1;
    //캡슐 가운데 가장 위쪽
    private Vector3 CapsuleTopCenterPoint
    => new Vector3(transform.position.x, transform.position.y + P_Com.capsuleCollider.height - P_Com.capsuleCollider.radius, transform.position.z);
    //캡슐 가운데 가장 아래쪽
    private Vector3 CapsuleBottomCenterPoint
    => new Vector3(transform.position.x, transform.position.y + P_Com.capsuleCollider.radius, transform.position.z);

    public List<Collider> forwardHit;

    void FixedUpdate(){
        if (UIManager.gameIsPaused == false)
        {
            _fixedDeltaTime = Time.fixedDeltaTime;

            Update_Physics();
            CheckedForward();
            CheckedBackward();
            CheckedGround();
        }
    }

    public void InitCapsuleCollider()
    {
        P_Com.capsuleCollider = GetComponent<CapsuleCollider>();
        _castRadius = P_Com.capsuleCollider.radius * 0.9f;
        _castRadiusDiff = P_Com.capsuleCollider.radius - _castRadius + 0.05f;
        //그냥 캡슐 콜라이더 radius와 castRadius의 차이
    }

    //* 물리(중력)
    private void Update_Physics()
    {
        if (P_States.isGround && !P_States.isJumping)
        {
            //지면에 잘 붙어있을 경우
            P_Value.gravity = 0f;
        }
        else if (!P_States.isGround && !P_States.isJumping)
        {
            P_Value.gravity += _fixedDeltaTime * P_COption.gravity;
        }
        else if (!P_States.isGround && P_States.isJumping)
        {
            //Debug.Log("Here");
            P_Value.gravity = P_COption.gravity * P_COption.jumpGravity;
        }

    }

    //* 전방체크
    public void CheckedForward()
    {
        //Debug.Log("CheckedForward()");
        //캐릭터가 이동하는 방향으로 막힘 길이 있는가?
        // 함수 파라미터 : Capsule의 시작점, Capsule의 끝점,
        // Capsule의 크기(x, z 중 가장 큰 값이 크기가 됨), Ray의 방향,
        // RaycastHit 결과, Capsule의 회전값, CapsuleCast를 진행할 거리
        /*bool cast = Physics.SphereCast(transform.position, 5f, transform.forward,
        out var hit, Mathf.Infinity, 0);*/

        bool cast = Physics.CapsuleCast(CapsuleBottomCenterPoint, CapsuleTopCenterPoint,
        _castRadius, P_Value.moveDirection,// + Vector3.down * 0.25f,
        out var hit, P_COption.forwardCheckDistance, -1, QueryTriggerInteraction.Ignore);

        //Debug.Log("cast : " + cast);
        // QueryTriggerInteraction.Ignore 란? 트리거콜라이더의 충돌은 무시한다는 뜻
        P_Value.hitDistance = hit.distance;
        P_States.isForwardBlocked = false;
        if (cast)
        {
            P_States.isForwardBlocked = true;
            forwardHit.Add(hit.collider);    //* 전방체크 해서 걸린 거 리스트에 추가 
                                             //Debug.Log("if (cast)");
            float forwardObstacleAngle = Vector3.Angle(hit.normal, Vector3.up);
            P_States.isForwardBlocked = forwardObstacleAngle >= P_COption.maxSlopAngle;
            //if (P_States.isForwardBlocked)
            //Debug.Log("앞에 장애물있음!" + forwardObstacleAngle + "도");
            //Debug.Log("P_Value.hitDistance : " + P_Value.hitDistance);
        }
        else
        {
            forwardHit.Clear(); //* P_Controller.forwardHit == null
        }
    }
    //* 후방체크
    public void CheckedBackward()
    {
        bool cast = Physics.CapsuleCast(CapsuleBottomCenterPoint, CapsuleTopCenterPoint,
        _castRadius, P_Value.moveDirection + Vector3.down * -0.25f,
        out var hit, P_COption.forwardCheckDistance, -1, QueryTriggerInteraction.Ignore);

        //Debug.Log("cast : " + cast);
        // QueryTriggerInteraction.Ignore 란? 트리거콜라이더의 충돌은 무시한다는 뜻
        P_Value.hitDistance = hit.distance;
        P_States.isBackwardBlocked = false;
        if (cast)
        {
            P_States.isBackwardBlocked = true;
            //Debug.Log("if (cast)");
            float forwardObstacleAngle = Vector3.Angle(hit.normal, Vector3.up);
            P_States.isBackwardBlocked = forwardObstacleAngle >= P_COption.maxSlopAngle;
            //if (P_States.isForwardBlocked)
            //Debug.Log("앞에 장애물있음!" + forwardObstacleAngle + "도");
            //Debug.Log("P_Value.hitDistance : " + P_Value.hitDistance);
        }
    }

    //*바닥 체크
    public void CheckedGround()
    {
        //캐릭터와 지면사이의 높이
        P_Value.groundDistance = float.MaxValue; //float의 최대값을 넣어준다.
        P_Value.groundNormal = Vector3.up;      //현재 바닥의 노멀 값. 
        P_Value.groundSlopeAngle = 0f;          //바닥의 경사면.
        P_Value.forwardSlopeAngle = 0f;         // 플레이어가 이동하는 방향의 바닥의 경사면.
        bool cast = Physics.SphereCast(CapsuleBottomCenterPoint, _castRadius, Vector3.down,
        out var hit, P_COption.groundCheckDistance, P_COption.groundLayerMask, QueryTriggerInteraction.Ignore);
        if (cast)
        {
            //지면의 노멀값
            P_Value.groundNormal = hit.normal;
            //지면의 경사각(기울기)
            P_Value.groundSlopeAngle = Vector3.Angle(P_Value.groundNormal, Vector3.up);
            //캐릭터 앞의 경사각
            P_Value.forwardSlopeAngle = Vector3.Angle(P_Value.groundNormal, P_Value.moveDirection) - 90f;
            //가파른 경사 있는지 체크
            P_States.isOnSteepSlop = P_Value.groundSlopeAngle >= P_COption.maxSlopAngle;
            P_Value.groundDistance = Mathf.Max((hit.distance - _capsuleRadiusDiff - P_COption.groundCheckThreshold), -10f);
            P_States.isGround = (P_Value.groundDistance <= 0.03f) && !P_States.isOnSteepSlop;
        }
        P_Value.groundCross = Vector3.Cross(P_Value.groundNormal, Vector3.up);
        //경사면의 회전축벡터 => 플레이어가 경사면을 따라 움직일수있도록 월드 이동 벡터를 회전
    }
}
