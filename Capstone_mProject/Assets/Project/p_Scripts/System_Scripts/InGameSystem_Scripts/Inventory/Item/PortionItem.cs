using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortionItem : CountableItem, IUsableItem
{
    public PortionItem(PortionItemData data, int amount = 1) : base(data, amount) { }

    public bool Use()
    {
        // 임시 : 개수 하나 감소
        Amount--;

        return true;
    }
}
