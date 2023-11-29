using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredictedPath : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public int pointsCount = 10;

    private void Start()
    {
        DrawPath();
    }

    private void DrawPath()
    {
        Vector3[] points = CalculatePathPoints();
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }

    private Vector3[] CalculatePathPoints()
    {
        Vector3[] points = new Vector3[pointsCount];

        // 현재 위치와 초기 속도
        Vector3 currentPosition = transform.position;
        Vector3 initialVelocity = GetComponent<Rigidbody>().velocity;

        for (int i = 0; i < pointsCount; i++)
        {
            // 시간에 따른 예상 위치 계산
            float time = i * 0.1f; // 간격을 조절하여 적절한 시간 간격 설정
            Vector3 nextPosition = currentPosition + initialVelocity * time + 0.5f * Physics.gravity * time * time;

            points[i] = nextPosition;
        }

        return points;
    }
}
