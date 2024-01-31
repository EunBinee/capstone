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

    private List<DamageUI_Info> damageUI_InUse;
    private List<DamageUI_Info> damage_Pools;

    private int damagePoolsCount = 30;

    public Camera m_Camera;

    void Start()
    {
        InitDamageUI();
    }

    private void InitDamageUI()
    {
        damage_Pools = new List<DamageUI_Info>();
        damageUI_InUse = new List<DamageUI_Info>();
        if (damage_Prefab == null)
        {
            damage_Prefab = Resources.Load<DamageUI_Info>("SystemPrefabs/" + damage_name);
            if (damage_Prefab == null)
                Debug.LogError("데미지 프리펩 없음.");
        }

        m_Camera = GameManager.instance.gameData.cameraObj;
    }

    void Update()
    {
        //데미지 위치
        if (damageUI_InUse.Count > 0)
        {
            for (int i = 0; i < damageUI_InUse.Count; i++)
            {
                // 오브젝트와 카메라 간의 거리를 계산
                Vector3 cameraToObj = damageUI_InUse[i].m_DamagePos - m_Camera.transform.position;
                // 카메라 정면 방향과 오브젝트 간의 각도를 계산
                float angle = Vector3.Angle(m_Camera.transform.forward, cameraToObj);
                if (angle < 90f)
                {
                    Vector3 targetScreenPos = m_Camera.WorldToScreenPoint(damageUI_InUse[i].m_DamagePos);
                    if (damageUI_InUse[i].isReset) // 리셋을 시킨 경우
                    {
                        damageUI_InUse[i].gameObject.transform.position = targetScreenPos;
                    }
                }
            }
        }
    }

    //*----------------------------------------------------------------------------//
    //* 데미지 오브젝트 풀링//

    //HP바 받기.
    public DamageUI_Info Get_DamageUI()
    {
        DamageUI_Info curDamageUI = null;

        //오브젝트 풀에 
        if (damage_Pools.Count > 0)
        {
            curDamageUI = damage_Pools[0];

            damageUI_InUse.Add(curDamageUI);
            damage_Pools.Remove(curDamageUI);
        }
        else
        {
            curDamageUI = UnityEngine.Object.Instantiate(damage_Prefab);
            damageUI_InUse.Add(curDamageUI);
        }

        curDamageUI.gameObject.transform.SetParent(damage_Parent);
        curDamageUI.gameObject.SetActive(true);

        return curDamageUI;
    }

    //HP바 반납.
    public void Add_DamageUI(DamageUI_Info damageUI)
    {
        damageUI.gameObject.SetActive(false);

        if (damage_Pools.Count >= damagePoolsCount)
        {
            //만약 풀이 가득 찼다면, 그냥 삭제.
            UnityEngine.Object.Destroy(damageUI.gameObject);
        }
        else
        {
            damageUI_InUse.Remove(damageUI);
            damage_Pools.Add(damageUI);
        }
    }





}
