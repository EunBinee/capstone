using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerSkillUI : MonoBehaviour
{
    private bool showTooltip = true;
    [SerializeField] private PlayerSkillName pointerOverSlot; //현재 포인터가 위치한 곳의 슬롯
    // private bool isAccessSlot = true; // 슬롯 접근가능 여부
    // private bool isAccessSkill= true; // 스킬 접근가능 여부
    // public bool IsAccess => isAccessSlot && isAccessSkill;
    [SerializeField] private PlayerSkillTooltip skillTooltip; //아이템 정보 보여줄 툴팁 UI
    //아이템 드래그앤드랍
    private GraphicRaycaster gr;
    private PointerEventData ped;
    [SerializeField] private List<RaycastResult> rList;

    void Start()
    {

        TryGetComponent(out gr);
        if (gr == null)
        {
            gr = gameObject.AddComponent<GraphicRaycaster>();
        }
        ped = new PointerEventData(EventSystem.current);
        rList = new List<RaycastResult>(10);
    }


    void Update()
    {

        if (EventSystem.current != null)
        {
            // 마우스 입력을 기반으로 위치 설정
            ped.position = Input.mousePosition;
            OnPointerEnterExit();
            if (showTooltip) ShowHideItemTooltip();
            //OnPointerDown();
            //OnPointerDrag();
            //OnPointerUp();
        }

    }

    //아이템 드래그앤드롭
    private T RaycastAndGetFirstComponent<T>() where T : Component
    {
        rList.Clear(); //레이캐스트 결과 저장할 리스트 초기화

        gr.Raycast(ped, rList);
        Debug.DrawRay(ped.position,Input.mousePosition,Color.red);

        if (rList.Count == 0) {
            return null;} //히트한 결과가 없으면 null

        Transform aa = rList[0].gameObject.transform;
        T a = aa.GetComponent<T>();
        if (a == null)
        {
            a = aa.GetComponentInParent<T>();
        }
        
        return a;
    }
    private void OnPointerEnterExit()
    {
        //이전 프레임 슬롯
        //var preSlot = pointerOverSlot;

        //현재 프레임의 슬롯
        //var curSlot = 
        pointerOverSlot = RaycastAndGetFirstComponent<PlayerSkillName>();
    }

    //아이템 정보 툴팁 보여주고 숨기기
    private void ShowHideItemTooltip()
    {
        //마우스 커서가 유효한 아이템 아이콘위에 올라가 있으면 툴팁 보이게하기
        bool isValid = pointerOverSlot != null;

        if (isValid)
        {
            UpdateTooltipUI(pointerOverSlot);
            skillTooltip.Show();
        }
        else
        {
            skillTooltip.Hide();
        }

    }
    //툴팁 UI 슬롯 데이터 업데이트
    private void UpdateTooltipUI(PlayerSkillName slot)
    {
        //if (!slot.IsAccess || !slot.HaveItem)
        //    return;

        skillTooltip.SetSkillInfo(slot.skillData);
        skillTooltip.SetRectPosition(slot.GetComponent<RectTransform>());
        // Debug.Log(slot.SlotRect);

    }
}
