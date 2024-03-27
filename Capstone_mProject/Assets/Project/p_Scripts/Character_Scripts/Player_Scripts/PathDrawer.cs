using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathDrawer : MonoBehaviour
{
    public Transform objectToTrack;
    public LineRenderer lineRenderer;
    public int maxPositions = 100; // 저장할 최대 위치 수
    private Vector3[] positions;

    void Start()
    {
        objectToTrack = this.GetComponent<Transform>();
        positions = new Vector3[maxPositions];
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        // 오브젝트의 현재 위치 저장
        SavePosition(objectToTrack.position);
        // 저장된 위치로 경로 표시
        DrawPath();
    }

    void SavePosition(Vector3 newPosition)
    {
        // 배열의 처음에 새 위치 추가
        for (int i = positions.Length - 1; i > 0; i--)
        {
            positions[i] = positions[i - 1];
        }
        positions[0] = newPosition;

        // 최대 위치 수를 초과하지 않도록 조절
        int count = Mathf.Min(lineRenderer.positionCount + 1, maxPositions);
        lineRenderer.positionCount = count;
    }

    void DrawPath()
    {
        // 저장된 위치를 LineRenderer로 그리기
        lineRenderer.SetPositions(positions);
    }
}
