using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Options")]
    [Range(0, 10)]
    [SerializeField] private int horizontalSlot = 8; //가로 슬롯 개수 
    [Range(0, 10)]
    [SerializeField] private int verticalSlot = 8; //세로 슬롯 개수 
    [SerializeField] private float slotMargin = 8f; //한 슬롯 상하좌우 여백
    [SerializeField] private float contentAreaPadding = 20f; //인벤토리 내부 영역 여백
    [Range(32, 128)]
    [SerializeField] private float slotSize = 64f; //슬롯 칸 사이즈 

    [Space]
    [SerializeField] private bool showHighlight = true;

    [Header("Connected Objects")]
    [SerializeField] private RectTransform content; //슬롯 부모 = content
    [SerializeField] private GameObject slotPrefab; //슬롯 프리팹

    //아이템 드래그앤드랍
    private GraphicRaycaster gr;
    private PointerEventData ped;
    private List<RaycastResult> rList;

    private ItemSlotUI pointerOverSlot; //현재 포인터가 위치한 곳의 슬롯
    private ItemSlotUI beginDragSlot; //드래그 시작한 슬롯
    private Transform beginDragIconTransform; //해당 슬롯아이콘

    private Vector3 beginDragIconPoint; //드래그 시작시 슬롯위치 
    private Vector3 beginDragCusorPoint; //드래그 시작 마우스 위치
    private int beginDragSlotIndex;


    private List<ItemSlotUI> slotUIList = new List<ItemSlotUI>();
    private Inventory inventory;

    //인벤토리 ui 내 아이템 필터링 옵션
    private enum FilterOption
    {
        All, Equipment, Portion
    }
    private FilterOption curFilterOption = FilterOption.All;
    // public GameObject contentUI;

    private void Awake()
    {
        InitSlots();
        Init();
    }

    private void Update()
    {

        if (EventSystem.current != null)
        {
            // 마우스 입력을 기반으로 위치 설정
            ped.position = Input.mousePosition;
            //OnPointerEnterExit();
            OnPointerDown();
            OnPointerDrag();
            OnPointerUp();
        }

    }

    private void Init()
    {
        TryGetComponent(out gr);
        if (gr == null)
        {
            gr = gameObject.AddComponent<GraphicRaycaster>();
        }

        ped = new PointerEventData(EventSystem.current);
        rList = new List<RaycastResult>(10);


    }

    //슬롯 동적 생성 
    private void InitSlots()
    {
        // 슬롯 프리팹 설정
        slotPrefab.TryGetComponent(out RectTransform slotRect);
        slotRect.sizeDelta = new Vector2(slotSize, slotSize);

        slotPrefab.TryGetComponent(out ItemSlotUI itemSlot);
        if (itemSlot == null)
            slotPrefab.AddComponent<ItemSlotUI>();

        slotPrefab.SetActive(false);

        Vector2 beginPos = new Vector2(contentAreaPadding, -contentAreaPadding);
        Vector2 curPos = beginPos;

        slotUIList = new List<ItemSlotUI>(verticalSlot * horizontalSlot);

        for (int j = 0; j < verticalSlot; j++)
        {
            for (int i = 0; i < horizontalSlot; i++)
            {
                int slotIndex = (horizontalSlot * j) + i;

                var slotRT = CloneSlot();
                slotRT.pivot = new Vector2(0f, 1f); // Left Top
                slotRT.anchoredPosition = curPos;
                slotRT.gameObject.SetActive(true);
                slotRT.gameObject.name = $"Item Slot [{slotIndex}]";

                var slotUI = slotRT.GetComponent<ItemSlotUI>();
                slotUI.SetSlotIndex(slotIndex);
                slotUIList.Add(slotUI);

                curPos.x += (slotMargin + slotSize);
            }
            curPos.x = beginPos.x;
            curPos.y -= (slotMargin + slotSize);
        }

        if (slotPrefab.scene.rootCount != 0)
            Destroy(slotPrefab);

        RectTransform CloneSlot()
        {
            GameObject objSlot = Instantiate(slotPrefab); //슬롯 프리팹 생성
            RectTransform rt = objSlot.GetComponent<RectTransform>();
            rt.SetParent(content);

            return rt;
        }
    }

    //접근 가능한 슬롯 범위 설정
    public void SetAccessSlotRange(int accessibleSlotCount)
    {
        for (int i = 0; i < slotUIList.Count; i++)
        {
            slotUIList[i].SetSlotState(i < accessibleSlotCount);
        }
    }

    //특정 슬롯의 필터상태 업뎃
    public void UpdateSlotFilterState(int index, ItemData itemData)
    {
        bool isFiltered = true;

        if (itemData != null)
        {
            switch (curFilterOption)
            {
                case FilterOption.Equipment:
                    //isFiltered = (itemData is EquipmentItemData);
                    break;

                case FilterOption.Portion:
                    isFiltered = itemData is PortionItemData;
                    break;
            }

            slotUIList[index].SetItemState(isFiltered);

        }
    }

    //아이템 드래그앤드롭
    private T RaycastAndGetFirstComponent<T>() where T : Component
    {
        rList.Clear(); //레이캐스트 결과 저장할 리스트 초기화

        gr.Raycast(ped, rList);

        if (rList.Count == 0) return null; //히트한 결과가 없으면 null

        return rList[0].gameObject.GetComponent<T>(); //있으면 첫번째 히트된 게임오브젝트 가져옴. 
    }
    //슬롯에 마우스 포인터가 올라가는 경우와 슬롯에서 포인터가 빠져나가는 경우
    private void OnPointerEnterExit()
    {
        //이전 프레임 슬롯
        var preSlot = pointerOverSlot;

        //현재 프레임의 슬롯
        var curSlot = pointerOverSlot = RaycastAndGetFirstComponent<ItemSlotUI>();

        if (preSlot == null)
        {
            if (curSlot == null)
            {
                OnCurrentEnter();
            }
        }
        else
        {
            if (curSlot == null)
            {
                OnPreExit();
            }
            else if (preSlot != curSlot)
            {
                OnPreExit();
                OnCurrentEnter();
            }
        }


        void OnCurrentEnter()
        {
            if (showHighlight)
            {
                curSlot.Highlight(true);
            }
        }
        void OnPreExit()
        {
            preSlot.Highlight(false);
        }

    }

    private void OnPointerDown()
    {
        //마우스 좌클릭시 
        if (Input.GetMouseButtonDown(0))
        {
            beginDragSlot = RaycastAndGetFirstComponent<ItemSlotUI>();

            //아이템을 가지고 있는 슬롯의 경우
            if (beginDragSlot != null && beginDragSlot.HaveItem && beginDragSlot.IsAccess)
            {
                EditorLog($"Drag Begin : Slot [{beginDragSlot.Index}]");

                // //위치 기억, 참조 등록
                beginDragIconTransform = beginDragSlot.IconRect.transform;
                beginDragIconPoint = beginDragIconTransform.position;
                beginDragCusorPoint = Input.mousePosition;

                // //맨 위에 보이기
                beginDragSlotIndex = beginDragSlot.transform.GetSiblingIndex();
                beginDragSlot.transform.SetAsLastSibling();

                //해당 슬롯 하이라이트 이미지를 아이콘보다 뒤에 위치
                //! 나중에 추가요 
                //beginDragSlot.SetHighlight(false);
            }
            else
            {
                beginDragSlot = null;
            }
        }

        // else if (Input.GetMouseButtonDown(1))
        // {
        //     ItemSlotUI slot = RaycastAndGetFirstComponent<ItemSlotUI>();

        //     if (slot != null && slot.HaveItem && slot.IsAccess)
        //     {
        //         TryUseItem(slot.Index);
        //     }
        // }
    }
    //아이템 드래그 하는 도중
    private void OnPointerDrag()
    {
        if (beginDragSlot == null) return;

        if (Input.GetMouseButton(0))
        {
            //beginDragIconTransform.position = beginDragIconPoint + (Input.mousePosition - beginDragCusorPoint);
        }
    }
    //드래그 끝 = 마우스 클릭 뗄 경우
    private void OnPointerUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log(beginDragSlot);
            if (beginDragSlot != null)
            {
                beginDragIconTransform.position = beginDragIconPoint; //위치 복원
                beginDragSlot.transform.SetSiblingIndex(beginDragSlotIndex); //UI 순서복원
                EndDrag(); //드래그 완료처리

                //beginDragSlot.SetHighlight(true);

                //참조 초기화
                beginDragSlot = null;
                beginDragIconTransform = null;
            }
        }
    }

    private void EndDrag()
    {
        ItemSlotUI endDragSlot = RaycastAndGetFirstComponent<ItemSlotUI>();

        if (endDragSlot != null && endDragSlot.IsAccess)
        {
            TrySwapItems(beginDragSlot, endDragSlot);
        }
    }
    private void TryUseItem(int index)
    {
        inventory.Use(index);
    }
    //두 슬롯의 아이템 교환
    private void TrySwapItems(ItemSlotUI from, ItemSlotUI to)
    {
        if (from == to)
        {
            EditorLog($"UI - Try Swap Items: Same Slot [{from.Index}]");
            return;
        }
        EditorLog($"UI - Try Swap Items: Slot [{from.Index} -> {to.Index}]");

        from.SwapOrMoveIcon(to);
        inventory.Swap(from.Index, to.Index);
        //! swap 오류 고치기~

    }

    //슬롯에 아이템 아이콘 등록
    public void SetItemIcon(int index, Sprite icon)
    {
        EditorLog($"Set Item Icon : Slot [{index}]");

        slotUIList[index].SetItem(icon);
        //! 리스트는 add해줘야함 initslots보기 
    }
    //해당 슬롯 아이템 개수 텍스트 지정
    public void SetItemAmountText(int index, int amount)
    {
        EditorLog($"Set Item Amount Text : Slot [{index}], Amount [{amount}]");

        // NOTE : amount가 1 이하일 경우 텍스트 미표시
        slotUIList[index].SetItemAmount(amount);
    }

    //아이템 개수 텍스트 숨기기
    public void HideItemAmountText(int index)
    {
        EditorLog($"Hide Item Amount Text : Slot [{index}]");

        slotUIList[index].SetItemAmount(1);
    }

    // 슬롯에서 아이템 아이콘 제거, 개수 텍스트 숨기기
    public void RemoveItem(int index)
    {
        EditorLog($"Remove Item : Slot [{index}]");

        slotUIList[index].RemoveItem();
    }

#if UNITY_EDITOR
    [Header("Editor Options")]
    [SerializeField] private bool _showDebug = true;
#endif
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void EditorLog(object message)
    {
        if (!_showDebug) return;
        UnityEngine.Debug.Log($"[InventoryUI] {message}");
    }
}
