using UnityEngine;
// [CreateAssetMenu]
// public class Item : ScriptableObject
// {
//     public string itmeName; //아이템 이름
//     public Sprite itemImage; //아이템 이미지 
//     public string tooltip; //아이템 설명

// }

public abstract class Item
{
    public ItemData Data { get; private set; }

    public Item(ItemData data) => Data = data;
}