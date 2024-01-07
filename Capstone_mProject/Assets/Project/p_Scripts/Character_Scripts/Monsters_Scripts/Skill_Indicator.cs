using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class Skill_Indicator : MonoBehaviour
{
    public Transform[] vertices;  // 사다리꼴의 4개 꼭지점 좌표 배열

    private void Update()
    {
        // bool insideTrapezoid = IsPlayerInsideTrapezoid(GameManager.instance.gameData.player.transform.position);
        // Debug.Log(insideTrapezoid);
    }

    public bool IsPlayerInsideTrapezoid(Vector3 playerPosition)
    {
        // 3D 사다리꼴 내부에 있는지 확인
        bool inside = false;

        for (int i = 0, j = vertices.Length - 1; i < vertices.Length; j = i++)
        {
            if (((vertices[i].position.z > playerPosition.z) != (vertices[j].position.z > playerPosition.z)) &&
                (playerPosition.x < (vertices[j].position.x - vertices[i].position.x) * (playerPosition.z - vertices[i].position.z) / (vertices[j].position.z - vertices[i].position.z) + vertices[i].position.x))
            {
                inside = !inside;
            }
        }

        return inside;
    }

}
