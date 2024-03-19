using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class Skill_Indicator : MonoBehaviour
{
    //! 스킬 표시
    public Transform tempPoint;
    public BoxCollider boxCollider;

    public bool checkTrigger = false; //* 현재 이 콜라이더 안에 있으면  플레이어가 공격 받는다는 뜻.
    public bool insideBox = false;
    public List<Effect> electricity_Effects;
    public List<Effect> lightningStrike_Effects;
    Bounds originBounds;
    float angle = 0;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        electricity_Effects = new List<Effect>();
    }

    public void Init()
    {
        checkTrigger = false;
        insideBox = false;
    }

    public void CheckTrigger(bool enabled)
    {
        checkTrigger = enabled;

    }

    public void SetBounds()
    {
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                Debug.LogError("BoxCollider없음");
                return;// 에러 처리: BoxCollider가 없으면 원점 반환
            }
        }
        originBounds = boxCollider.bounds;
    }
    public void SetAngle(float m_angle)
    {
        angle = m_angle;
    }

    public Vector3 GetRandomPos(bool getBounds = false)
    {
        // BoxCollider 내부의 랜덤 좌표를 반환하는 함수
        if (boxCollider == null)
        {
            Debug.LogError("BoxCollider없음");
            return Vector3.zero; // 에러 처리: BoxCollider가 없으면 원점 반환
        }
        if (getBounds)
        {
            SetBounds();
        }
        // BoxCollider의 bounds 가져오기
        float randomX = Random.Range(originBounds.min.x, originBounds.max.x);
        float randomY = 0.5f;
        float randomZ = Random.Range(originBounds.min.z, originBounds.max.z);
        return new Vector3(randomX, randomY, randomZ);
    }

}
