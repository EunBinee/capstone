using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CountableItem : Item
{
    public CountableItemData countableData { get; set; }
    public int Amount { get; protected set; } //현재 아이템 개수
    public int maxAmount => countableData.MaxAmount;  //최대 아이템 개수
    public bool isMax => Amount >= countableData.MaxAmount; //최대수량인지 아닌지
    public bool isEmpty => Amount <= 0; //아이템 개수가 있는지 없는지

    public CountableItem(CountableItemData data, int amount = 1) : base(data)
    {
        countableData = data;
        SetAmount(amount);
    }

    //아이템 개수 지정(범위)
    public void SetAmount(int amount)
    {
        Amount = Mathf.Clamp(amount, 0, maxAmount); //최소 최대값설정하여 범위넘지않도록
    }

    //아이템 개수 추가, 최대치 초과시 
    public int AddAmountAndGetExcess(int amount)
    {
        int nextAmount = Amount + amount;
        SetAmount(nextAmount);

        return (nextAmount > maxAmount) ? (nextAmount - maxAmount) : 0; //초과량반환, 초과x-> 0반환 
    }
}
