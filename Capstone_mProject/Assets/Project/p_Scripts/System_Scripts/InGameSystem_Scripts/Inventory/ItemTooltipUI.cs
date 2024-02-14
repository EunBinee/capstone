using JetBrains.Annotations;
using TMPro;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ItemTooltipUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text itemName; //아이템 이름
    [SerializeField]
    private Image itemImage; //아이템 아이콘이미지

    [SerializeField]
    private TMP_Text itemType; //아이템 용도

    [SerializeField]
    private TMP_Text itemText; //아이템 설명

    [SerializeField]
    private TMP_Text itemEffect; //아이템 효과

    private RectTransform rt;
    private CanvasScaler canvasScaler;


    private void Awake()
    {
        Init();
        Hide();
    }
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    private void Init()
    {
        TryGetComponent(out rt);
        rt.pivot = new Vector2(0f, 1f); //left top
        //canvasScaler = GetComponentInParent<CanvasScaler>();

        DisableChildRaycast(transform);

    }
    private void Start()
    {
        canvasScaler = GetComponentInParent<CanvasScaler>();
    }

    //모든 자식 UI에 레이케스트 타깃 해제 
    private void DisableChildRaycast(Transform tr)
    {
        tr.TryGetComponent(out Graphic gr); //본인이 Graphic을 상속하면 레이케스트 타겟 해제 
        if (gr != null)
        {
            gr.raycastTarget = false;
        }

        //자식이 없는 경우 -> 종료
        int childCount = tr.childCount;
        if (childCount == 0) return;

        for (int i = 0; i < childCount; i++)
        {
            DisableChildRaycast(tr.GetChild(i));
        }
    }

    //툴팁UI에 아이템 정보 등록
    public void SetItemInfo(ItemData data)
    {
        itemName.text = data.Name;
        itemType.text = data.Type;
        itemImage.sprite = data.IconSprite;
        itemText.text = data.Tooltip;
        itemEffect.text = data.Effect;
    }
    //툴팁의 위치 조정
    public void SetRectPosition(RectTransform slotRect)
    {
        canvasScaler = GetComponentInParent<CanvasScaler>();
        Debug.Log(canvasScaler);
        Debug.Log(canvasScaler.referenceResolution.x);

        //해상도따라 다르게
        float wRatio = Screen.width / canvasScaler.referenceResolution.x;
        float hRatio = Screen.height / canvasScaler.referenceResolution.y;
        float ratio = wRatio * (1f - canvasScaler.matchWidthOrHeight) + hRatio * (canvasScaler.matchWidthOrHeight);

        float slotWidth = slotRect.rect.width * ratio;
        float slotHeight = slotRect.rect.height * ratio;

        //초기위치(슬롯의 우하단으로) 설정
        rt.position = slotRect.position + new Vector3(slotWidth, -slotHeight);
        Vector2 pos = rt.position;

        //크기
        float width = rt.rect.width * ratio;
        float height = rt.rect.height * ratio;

        //우측 또는 하단이 잘렸는지 여부
        bool rightcut = pos.x + width > Screen.width;
        bool bottomcut = pos.y - height < 0f;

        ref bool R = ref rightcut;
        ref bool B = ref bottomcut;

        //오른쪽만 잘림 -> 슬롯의 왼쪽밑 방향으로 표시
        if (R && !B)
        {
            rt.position = new Vector2(pos.x - width - slotWidth, pos.y);
        }
        //아래쪽만 잘림 -> 슬롯의 오른쪽위 방향으로 표시
        else if (!R && B)
        {
            rt.position = new Vector2(pos.x, pos.y + height + slotHeight);
        }
        //오른쪽 아래 모두 잘림 -> 슬롯의 왼쪽위 방향으로 표시
        else if (R && B)
        {
            rt.position = new Vector2(pos.x - width - slotWidth, pos.y + height + slotHeight);
        }
    }
}
