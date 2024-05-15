using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class PlayerSkillTooltip : MonoBehaviour
{
    [SerializeField]
    private TMP_Text skillName; //스킬 이름
    [SerializeField]
    private Image skillImage; //스킬 아이콘이미지

    [SerializeField]
    private TMP_Text skillText; //아이템 설명

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

    //툴팁UI에 스킬 정보 등록
    public void SetSkillInfo(SOSkill data)
    {
        skillName.text = data.skillName;
        skillImage.sprite = data.icon;
        skillText.text = data.skillDetail;
    }

    //툴팁의 위치 조정
    public void SetRectPosition(RectTransform slotRect)
    {
        canvasScaler = GetComponentInParent<CanvasScaler>();

        //해상도따라 다르게
        float wRatio = Screen.width / canvasScaler.referenceResolution.x;
        float hRatio = Screen.height / canvasScaler.referenceResolution.y;
        float ratio = wRatio * (0.5f - canvasScaler.matchWidthOrHeight) + hRatio * (canvasScaler.matchWidthOrHeight);

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
