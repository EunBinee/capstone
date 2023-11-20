using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    //! 하는 일
    // 1. 몬스터 데미지 UI 관리
    // 2. 몬스터 데미지 연산 (여기서 안할 수도 있음.)

    //UI 풀링
    public string damage_name = "";
    public Transform damage_Parent;
    private DamageUI_Info damage_Prefab;
    private List<DamageUI_Info> damage_Pools;

    private int DamagePoolsCount = 30;

    private Camera m_Camera;

    void Awake()
    {
        InitDamageUI();
    }

    private void InitDamageUI()
    {
        damage_Pools = new List<DamageUI_Info>();

        if (damage_Prefab == null)
        {
            damage_Prefab = Resources.Load<DamageUI_Info>("SystemPrefabs/" + damage_name);
            if (damage_Prefab == null)
                Debug.LogError("데미지 프리펩 없음.");
        }

        m_Camera = Camera.main;
    }

    void Update()
    {
        //데미지 위치
    }

    //*----------------------------------------------------------------------------//
    //* 데미지 오브젝트 풀링//

}
