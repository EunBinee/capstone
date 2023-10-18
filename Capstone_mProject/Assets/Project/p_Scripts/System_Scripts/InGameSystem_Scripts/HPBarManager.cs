using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBarManager : MonoBehaviour
{
    //! 몬스터 HP바를 관리하는 스크립트입니다.
    public string HPBar_name = "";
    public Transform HPBar_Parent;

    private MonsterUI_Info HPBar_Prefab;
    private List<MonsterUI_Info> HPBarInUse;
    private List<MonsterUI_Info> hpBarPools;
    private int hpBarPoolsCount = 30;

    private Camera m_Camera;

    private void Awake()
    {
        InitHPBar();
    }

    private void InitHPBar()
    {
        HPBarInUse = new List<MonsterUI_Info>();
        hpBarPools = new List<MonsterUI_Info>();

        if (HPBar_Prefab == null)
        {

            HPBar_Prefab = Resources.Load<MonsterUI_Info>("SystemPrefabs/" + HPBar_name);
        }

        m_Camera = Camera.main;
    }

    private void Update()
    {
        if (HPBarInUse.Count > 0)
        {
            for (int i = 0; i < HPBarInUse.Count; i++)
            {
                if (HPBarInUse[i].isReset) // 리셋을 안시킨 경우
                {
                    HPBarInUse[i].gameObject.transform.position = m_Camera.WorldToScreenPoint(HPBarInUse[i].m_HPBarPos.position);
                }
            }
        }
    }

    //*----------------------------------------------------------------------------//
    //* hp바 오브젝트 풀링//
    //HP바 받기.
    public MonsterUI_Info Get_HPBar()
    {
        MonsterUI_Info curHPBar = null;

        //오브젝트 풀에 
        if (hpBarPools.Count > 0)
        {
            curHPBar = hpBarPools[0];

            HPBarInUse.Add(curHPBar);
            hpBarPools.Remove(curHPBar);
        }
        else
        {
            curHPBar = UnityEngine.Object.Instantiate(HPBar_Prefab);
            HPBarInUse.Add(curHPBar);
        }

        curHPBar.gameObject.transform.SetParent(HPBar_Parent);
        curHPBar.gameObject.SetActive(true);
        return curHPBar;
    }

    //HP바 반납.
    public void Add_HPBarPool(MonsterUI_Info HPBar)
    {
        if (hpBarPools.Count >= hpBarPoolsCount)
        {
            //만약 풀이 가득 찼다면, 그냥 삭제.
            UnityEngine.Object.Destroy(HPBar.gameObject);
        }
        else
        {
            HPBarInUse.Remove(HPBar);
            hpBarPools.Add(HPBar);

            HPBar.gameObject.SetActive(false);
        }
    }





}
