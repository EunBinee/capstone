using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemData : ScriptableObject
{
    //아이템 공동 데이터
    public int ID => _id;
    public string Name => _name;
    public string Tooltip => _tooltip;
    public Sprite IconSprite => _iconSprite;
    public string Type => _type;
    public string Effect => _effect;

    [SerializeField] private int _id;
    [SerializeField] private string _name;    // 아이템 이름
    [SerializeField] private string _type;  // 아이템 용도
    [SerializeField] private string _tooltip; // 아이템 설명
    [SerializeField] private string _effect;    // 아이템 효과
    [SerializeField] private Sprite _iconSprite; // 아이템 아이콘
    [SerializeField] private GameObject dropItemPrefab; // 바닥에 떨어질 때 생성할 프리팹

    public abstract Item CreateItem(); //아이템 타입에 따른 아이템생성


}
