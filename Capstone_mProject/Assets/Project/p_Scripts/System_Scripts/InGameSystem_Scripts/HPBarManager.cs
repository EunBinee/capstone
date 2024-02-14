using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBarManager : MonoBehaviour
{
    //! 몬스터 HP바를 관리하는 스크립트입니다.
    public string HPBar_name = "";
    public string bossHPBar_name = "";
    public Transform HPBar_Parent;

    //* 일반 몬스터 HP바
    private HPBarUI_Info HPBar_Prefab;
    private List<HPBarUI_Info> HPBarInUse;
    private List<HPBarUI_Info> hpBarPools;
    private int hpBarPoolsCount = 30;

    //* 보스 몬스터 HP바
    private HPBarUI_Info curBossHPBar;

    public Camera m_Camera;

    private void Awake()
    {
        InitHPBar();
    }
    private void Start()
    {
        if (CanvasManager.instance.monster_HPBarUI == null)
        {
            CanvasManager.instance.monster_HPBarUI = CanvasManager.instance.GetCanvasUI(CanvasManager.instance.monster_HPBarName);
            if (CanvasManager.instance.monster_HPBarUI == null)
                Debug.LogError("오브젝트 없음");
        }
        HPBar_Parent = CanvasManager.instance.monster_HPBarUI.GetComponent<Transform>();
        m_Camera = GameManager.instance.gameData.cameraObj;
    }
    private void InitHPBar()
    {
        HPBarInUse = new List<HPBarUI_Info>();
        hpBarPools = new List<HPBarUI_Info>();

        if (HPBar_Prefab == null)
        {
            HPBar_Prefab = Resources.Load<HPBarUI_Info>("SystemPrefabs/" + HPBar_name);
        }
    }

    private void Update()
    {
        if (HPBarInUse.Count > 0)
        {
            for (int i = 0; i < HPBarInUse.Count; i++)
            {
                // 오브젝트와 카메라 간의 거리를 계산
                Vector3 cameraToObj = HPBarInUse[i].m_HPBarPos.position - m_Camera.transform.position;
                // 카메라 정면 방향과 오브젝트 간의 각도를 계산
                float angle = Vector3.Angle(m_Camera.transform.forward, cameraToObj);
                if (angle < 90f)
                {
                    Vector3 targetScreenPos = m_Camera.WorldToScreenPoint(HPBarInUse[i].m_HPBarPos.position);
                    if (HPBarInUse[i].isReset) // 리셋을 안시킨 경우
                    {
                        HPBarInUse[i].gameObject.transform.position = targetScreenPos;
                    }
                }
            }
        }
    }
    //*----------------------------------------------------------------------------//
    //* hp바 오브젝트 풀링//
    //HP바 받기.
    public HPBarUI_Info Get_HPBar()
    {
        HPBarUI_Info curHPBar = null;

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
    public void Add_HPBarPool(HPBarUI_Info HPBar)
    {
        HPBar.gameObject.SetActive(false);

        if (hpBarPools.Count >= hpBarPoolsCount)
        {
            //만약 풀이 가득 찼다면, 그냥 삭제.
            UnityEngine.Object.Destroy(HPBar.gameObject);
        }
        else
        {
            HPBarInUse.Remove(HPBar);
            hpBarPools.Add(HPBar);
        }
    }

    //--------------------------------------------------------------------------------------------
    //HP바 받기.
    public HPBarUI_Info Get_BossHPBar()
    {
        //보스전의 HP바 
        if (curBossHPBar == null)
        {
            HPBarUI_Info HPBar_Prefab = Resources.Load<HPBarUI_Info>("SystemPrefabs/" + bossHPBar_name);
            curBossHPBar = UnityEngine.Object.Instantiate(HPBar_Prefab);

        }
        curBossHPBar.gameObject.transform.SetParent(HPBar_Parent);
        curBossHPBar.gameObject.SetActive(true);

        curBossHPBar.gameObject.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        return curBossHPBar;
    }

    public void Return_BossHPBar()
    {
        //보스 HP바 반납
        curBossHPBar.gameObject.SetActive(false);
    }



}
