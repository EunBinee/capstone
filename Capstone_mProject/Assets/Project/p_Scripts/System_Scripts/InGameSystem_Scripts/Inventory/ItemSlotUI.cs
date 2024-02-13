using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [Tooltip("아이템 아이콘 이미지")]
    [SerializeField] private Image iconImg;

    [Tooltip("아이템 개수 텍스트")]
    [SerializeField] private TMP_Text amountText;
    [Tooltip("하이라이트 이미지")]
    [SerializeField] private Image highlightImage;

    [Space]
    [Tooltip("하이라이트 이미지 알파값")]
    [SerializeField] private float highlightAlpha = 0.5f;
    [Tooltip("하이라이트 이미지 소요 시간")]
    [SerializeField] private float highlightImgDuration = 0.2f;

    public int Index { get; private set; } //슬롯 인덱스
    public bool HaveItem => iconImg.sprite != null; //아이템 보유 여부
    public bool IsAccess => isAccessSlot && isAccessItem;

    public RectTransform SlotRect => slotRect;
    public RectTransform IconRect => iconRect;

    private InventoryUI inventoryUI;

    private RectTransform slotRect; //슬롯
    private RectTransform iconRect; //슬롯아이템이미지
    private RectTransform highlightRect; //하이라이트 이미지

    private GameObject objIcon; //슬롯아이템이미지 오브젝트
    private GameObject objText;
    private GameObject objHighlight;

    private Image slotImage; //슬롯 이미지
    private float curHighlightAlpha = 0f; //현재 하이라이트이미지 알파값
    private bool isAccessSlot = true; // 슬롯 접근가능 여부
    private bool isAccessItem = true; // 아이템 접근가능 여부

    private static readonly Color inaccessSlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f); //비활성화 슬롯이미지 색장
    /// <summary> 비활성화된 아이콘 색상 </summary>
    private static readonly Color inaccessIconColor = new Color(0.5f, 0.5f, 0.5f, 0.5f); //비활성화 아이콘이미지 색상

    private void ShowIcon() => objIcon.SetActive(true);
    private void HideIcon() => objIcon.SetActive(false);

    private void ShowText() => objText.SetActive(true);
    private void HideText() => objText.SetActive(false);
    public void SetSlotIndex(int index) => Index = index;


    private void Awake()
    {
        InitComponent();
        InitValue();
    }

    private void InitComponent()
    {
        inventoryUI = GetComponent<InventoryUI>();

        slotRect = GetComponent<RectTransform>();
        iconRect = iconImg.rectTransform;
        highlightRect = highlightImage.rectTransform;

        objIcon = iconRect.gameObject;
        objText = amountText.gameObject;
        objHighlight = highlightImage.gameObject;

        slotImage = GetComponent<Image>();
    }
    private void InitValue()
    {
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;

        iconImg.raycastTarget = false;
        highlightImage.raycastTarget = false;

        HideIcon();
        objHighlight.SetActive(false);

    }

    //슬롯 활성화비활성화
    public void SetSlotState(bool value)
    {
        if (isAccessSlot == value) return;

        if (value) //슬롯o -> 슬롯이미지 블랙으로 
        {
            slotImage.color = Color.black;
        }
        else //슬롯x -> 아이콘, 텍스트 비활성화
        {
            slotImage.color = inaccessSlotColor;
            HideIcon();
            HideText();
        }

        isAccessSlot = value; //슬롯 접근가능 여부 = value
    }

    public void SetItemState(bool value)
    {
        if (isAccessItem == value) return; //아이템 접근가능 여부

        if (value) //아이템 o
        {
            iconImg.color = Color.white;
            amountText.color = Color.white;
        }
        else
        {
            iconImg.color = inaccessIconColor;
            amountText.color = inaccessIconColor;
        }

        isAccessItem = value;
    }

    public void SwapOrMoveIcon(ItemSlotUI other)
    {
        if (other == null) return;
        if (other == this) return; //자기자신이랑은 교환불가
        if (!this.IsAccess) return;
        if (!other.IsAccess) return;

        var temp = iconImg.sprite;

        //교환할 슬롯에 아이템이 있을 경우 -> 교환
        if (other.HaveItem)
            SetItem(other.iconImg.sprite);


        // " 없을경우 -> 이동
        else RemoveItem();

        other.SetItem(temp); //슬롯에 있는 아이템을 삭제하고 아이템등록.
    }

    //슬롯에 아이템 등록
    public void SetItem(Sprite itemSprite)
    {
        if (itemSprite != null)
        {
            iconImg.sprite = itemSprite;
            ShowIcon();
        }
        else
        {
            RemoveItem();
        }
    }

    //슬롯에 있는 아이템 삭제
    public void RemoveItem()
    {
        iconImg.sprite = null;
        HideIcon();
        HideText();
    }

    //아이템 이미지 알파값
    public void SetIconAlpha(float alpha)
    {
        iconImg.color = new Color(iconImg.color.r, iconImg.color.g, iconImg.color.b, alpha);
    }

    //아이템 개수 텍스트 
    public void SetItemAmount(int amount)
    {
        if (HaveItem && amount > 1) //아이템을 가지고 있고 개수가 1보다 크면
        {
            ShowText();
        }
        else
        {
            HideText();
        }

        amountText.text = amount.ToString();
    }

    //슬롯에 하이라이트 이미지 표시와 해제 
    public void Highlight(bool show)
    {
        if (!this.IsAccess) return;

        if (show)
        {
            StartCoroutine(nameof(HighlightFadeInRoutine));
        }
        else
        {
            StartCoroutine(nameof(HighlightFadeOutRoutine));
        }
    }
    private IEnumerator HighlightFadeInRoutine()
    {
        StopCoroutine(nameof(HighlightFadeOutRoutine));
        objHighlight.SetActive(true);

        float unit = highlightAlpha / highlightImgDuration;

        for (; curHighlightAlpha <= highlightAlpha; curHighlightAlpha += unit * Time.deltaTime)
        {
            highlightImage.color = new Color
            (
                highlightImage.color.r,
                highlightImage.color.g,
                highlightImage.color.b,
                curHighlightAlpha
            );

            yield return null;
        }
    }
    private IEnumerator HighlightFadeOutRoutine()
    {
        StopCoroutine(nameof(HighlightFadeInRoutine));

        float unit = highlightAlpha / highlightImgDuration;

        for (; curHighlightAlpha >= 0f; curHighlightAlpha -= unit * Time.deltaTime)
        {
            highlightImage.color = new Color
            (
                highlightImage.color.r,
                highlightImage.color.g,
                highlightImage.color.b,
                curHighlightAlpha
            );

            yield return null;
        }

        objHighlight.SetActive(false);
    }
}



