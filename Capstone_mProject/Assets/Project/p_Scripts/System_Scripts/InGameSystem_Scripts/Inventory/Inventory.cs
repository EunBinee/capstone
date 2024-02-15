using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    //아이템 개수 제한
    public int capacity { get; private set; }


    [SerializeField, Range(8, 50)]
    private int initalCapacity = 32; //초기 아이템 한도


    [SerializeField, Range(8, 50)]
    private int maxCapacity = 50; // 최대 수용 한도(아이템 배열 크기)

    [SerializeField]
    private InventoryUI inventoryUI;
    public GameObject inventory;
    private bool openInventory;

    [SerializeField]
    private Item[] items;
    private readonly static Dictionary<Type, int> sortWeight = new Dictionary<Type, int>
    {
        {typeof(PortionItemData),100}
        //!아이템 종류 추가되면 여기에
    };
    private class ItemComparer : IComparer<Item>
    {
        public int Compare(Item a, Item b)
        {
            return (a.Data.ID + sortWeight[a.Data.GetType()])
            - (b.Data.ID + sortWeight[b.Data.GetType()]);
        }
    }
    private static readonly ItemComparer itemComparer = new ItemComparer();


    private void Awake()
    {
        items = new Item[maxCapacity];
        capacity = initalCapacity;
        inventory.SetActive(false);
    }

    private void Start()
    {
        UpdateAccessAll();
    }

    public void UpdateAccessAll()
    {
        inventoryUI.SetAccessSlotRange(capacity);
        //inventory.SetActive(false);
        openInventory = false;
    }

    private void Update()
    {
        OpenCloseInventory();
    }

    public void OpenCloseInventory()
    {
        if (!openInventory && Input.GetKeyDown(KeyCode.I))
        {
            GameManager.instance.cameraController.stopRotation = true;

            inventory.SetActive(true);
            openInventory = true;

            UIManager.Instance.Pause();
        }
        else if (openInventory && Input.GetKeyDown(KeyCode.I))
        {
            GameManager.instance.cameraController.stopRotation = false;

            inventory.SetActive(false);
            openInventory = false;

            UIManager.Instance.Resume();
        }
    }
    public void CloseBtn()
    {
        GameManager.instance.cameraController.stopRotation = false;

        inventory.SetActive(false);
        openInventory = false;

        UIManager.Instance.Resume();
    }

    //인덱스 슬롯상태 및 ui 업뎃
    public void UpdateSlot(int index)
    {
        if (!IsValidIndex(index)) return;

        Item item = items[index];

        //아이템이 슬롯에 존재하는 경우
        if (item != null)
        {
            inventoryUI.SetItemIcon(index, item.Data.IconSprite); //아이콘 등록

            //셀 수 있는 아이템
            if (item is CountableItem ci)
            {
                //수량이 0인경우
                if (ci.isEmpty)
                {
                    items[index] = null;
                    RemoveIcon();
                    return;
                }
                //수량 텍스트에 표시
                else
                {
                    inventoryUI.SetItemAmountText(index, ci.Amount);
                }
            }
            //셀 수 없는 아이템인 경우 수량 텍스트 제거
            else
            {
                inventoryUI.HideItemAmountText(index);
            }

            inventoryUI.UpdateSlotFilterState(index, item.Data);
        }
        //빈 슬롯인 경우 -> 아이콘 제거
        else
        {
            RemoveIcon();
        }

        //로컬: 아이콘 제거하기
        void RemoveIcon()
        {
            inventoryUI.RemoveItem(index);
            inventoryUI.HideItemAmountText(index);
        }
    }
    private void UpdateSlot(params int[] indices)
    {
        foreach (var i in indices)
        {
            UpdateSlot(i);
        }
    }
    private void UpdateAllSlot()
    {
        for (int i = 0; i < capacity; i++)
        {
            UpdateSlot(i);
        }
    }

    //인덱스가 수용 범위내에 있는지
    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < capacity;
    }

    //앞에서부터 비어있는 슬롯 인덱스 탐색
    private int FindEmptySlot(int index = 0)
    {
        for (int i = index; i < capacity; i++)
        {
            if (items[i] == null)
                return i;
        }
        return -1;
    }
    //앞에서부터 개수 여유가 있는 Countable 아이템의 슬롯 인덱스 탐색
    private int FindCountableItemSlotIndex(CountableItemData target, int startIndex = 0)
    {
        for (int i = startIndex; i < capacity; i++)
        {
            var current = items[i];
            if (current == null)
                continue;

            // 아이템 종류 일치, 개수 여유 확인
            if (current.Data == target && current is CountableItem ci)
            {
                if (!ci.isMax)
                {
                    return i;
                }

            }
        }
        return -1;
    }

    //슬롯 ui 접근 가능여부 업뎃
    public void UpdateAccessSlot()
    {
        inventoryUI.SetAccessSlotRange(capacity);
    }

    //해당 슬롯이 아이템을 갖고있는지 여부
    public bool HaveItem(int index)
    {
        return IsValidIndex(index) && items[index] != null;
    }

    //해당 슬롯이 셀 수 있는 아이템인지 여부
    public bool IsCountalbleItem(int index)
    {
        return HaveItem(index) && items[index] is CountableItem;
    }

    //잘못된 인덱스 : -1, 빈 슬롯: 0, 셀 수 없는 아이템: 1
    public int GetCurrentAmount(int index)
    {
        if (!IsValidIndex(index)) return -1;
        if (items[index] == null) return 0;

        CountableItem ci = items[index] as CountableItem;

        if (ci == null)
        {
            return 1;
        }

        return ci.Amount;
    }

    //해당 슬롯 아이템데이터 정보
    public ItemData GetItemData(int index)
    {
        if (!IsValidIndex(index)) return null;
        if (items[index] == null) return null;

        return items[index].Data;
    }
    //아이템 이름 
    public string GetItemName(int index)
    {
        if (!IsValidIndex(index)) return "";
        if (items[index] == null) return "";

        return items[index].Data.Name;
    }

    public void Swap(int indexA, int indexB)
    {
        if (!IsValidIndex(indexA)) return;
        if (!IsValidIndex(indexB)) return;

        Item itemA = items[indexA];
        Item itemB = items[indexB];

        //셀 수 있는 아이템이고 동일한 아이템일 경우
        if (itemA != null && itemB != null && itemA.Data == itemB.Data &&
            itemA is CountableItem ciA && itemB is CountableItem ciB)
        {
            int maxAmount = ciB.maxAmount;
            int sum = ciA.Amount + ciB.Amount;

            if (sum <= maxAmount)
            {
                ciA.SetAmount(0);
                ciB.SetAmount(sum);
            }
            else
            {
                ciA.SetAmount(sum - maxAmount);
                ciB.SetAmount(maxAmount);
            }
        }

        //일반적인 경우 
        else
        {
            items[indexA] = itemB;
            items[indexB] = itemA;
        }
        //두 슬롯 정보 갱신
        UpdateSlot(indexA, indexB);
    }

    public int Add(ItemData itemData, int amount = 1)
    {
        int index;

        //셀 수 있는 아이템
        if (itemData is CountableItemData ciData)
        {
            bool findNextCountable = true;
            index = -1;

            while (amount > 0)
            {
                //해당 아이템이 인벤토리에 존재하고 개수 여유가 있는지 검사
                if (findNextCountable)
                {
                    index = FindCountableItemSlotIndex(ciData, index + 1);
                    //개수 여유있는 슬롯이 더이상 없다고 판단될 경우 -> 빈 슬롯부터 탐색 시작
                    if (index == -1)
                    {
                        findNextCountable = false;
                    }
                    //슬롯을 찾은 경우 -> 개수 증가, 초과되었으면 amount 초기화
                    else
                    {
                        CountableItem ci = items[index] as CountableItem;
                        amount = ci.AddAmountAndGetExcess(amount);

                        UpdateSlot(index);
                    }
                }
                //빈 슬롯 검색
                else
                {
                    index = FindEmptySlot(index + 1);

                    //빈 슬롯이 없는 경우 -> 종료 
                    if (index == -1)
                    {
                        Debug.Log("빈슬롯 없음");
                        break;
                    }
                    //빈 슬롯이 있는 경우 -> 슬롯에 아이템 추가, 잉어량 계산
                    else
                    {
                        CountableItem ci = ciData.CreateItem() as CountableItem;
                        ci.SetAmount(amount);

                        //슬롯에 추가
                        items[index] = ci;

                        //남은 개수 계산
                        amount = (amount > ciData.MaxAmount) ? (amount - ciData.MaxAmount) : 0;
                        UpdateSlot(index);
                    }
                }
            }
        }
        //셀 수 없는 아이템
        else
        {
            //1개만 넣는 경우
            if (amount == 1)
            {
                index = FindEmptySlot();
                if (index != -1)
                {
                    // 아이템을 생성하여 슬롯에 추가
                    items[index] = itemData.CreateItem();
                    amount = 0;

                    UpdateSlot(index);
                }
            }

            //2개 이상의 수량없는 아이템을 동시에 추가하는 경우
            index = -1;
            for (; amount > 0; amount--)
            {
                // 아이템 넣은 인덱스의 다음 인덱스부터 슬롯 탐색
                index = FindEmptySlot(index + 1);

                // 다 넣지 못한 경우 루프 종료
                if (index == -1)
                {
                    break;
                }

                // 아이템을 생성하여 슬롯에 추가
                items[index] = itemData.CreateItem();

                UpdateSlot(index);
            }
        }
        //Debug.Log(amount);
        return amount;
    }
    public void Use(int index)
    {
        if (!IsValidIndex(index)) return;
        if (items[index] == null) return;

        if (items[index] is IUsableItem uItem)
        {
            bool useItem = uItem.Use();

            if (useItem)
            {
                UpdateSlot(index);
            }
        }
    }
    public void Remove(int index)
    {
        if (!IsValidIndex(index)) return;

        items[index] = null;
        inventoryUI.RemoveItem(index);
    }

    public void SortAll()
    {
        //Trim
        int i = -1;
        while (items[++i] != null) ;
        int j = i;

        while (true)
        {
            while (++j < capacity && items[j] == null) ;

            if (j == capacity)
                break;

            items[i] = items[j];
            items[j] = null;
            i++;
        }

        //sort
        Array.Sort(items, 0, i, itemComparer);

        UpdateAllSlot();
        inventoryUI.UpdateAllSlotFilters();
    }

}

