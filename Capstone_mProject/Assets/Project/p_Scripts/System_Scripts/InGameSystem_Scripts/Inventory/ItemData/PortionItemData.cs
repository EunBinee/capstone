using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Item_Portion_", menuName = "Item Data/Portion", order = 3)]
public class PortionItemData : CountableItemData
{
    //포션 등등 소비아이템 
    public float Value => _value;
    [SerializeField] private float _value;
    public override Item CreateItem()
    {
        return new PortionItem(this);
    }
}
