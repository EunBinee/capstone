using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupUI : MonoBehaviour
{
    //아이템 버리기 팝업
    [Header("Throw away")]
    [SerializeField] private GameObject throwPopup;
    [SerializeField] private Button throwOkBtn;
    [SerializeField] private Button thorwCancleBtn;

    //아이템 사용 팝업
    [Header("Use")]
    [SerializeField] private GameObject usePopup;
    [SerializeField] private Button useOkBtn;
    [SerializeField] private Button useCancleBtn;

    private event Action OnOkBtn; // 확인 버튼 누를 경우 실행할 이벤트
    private void ShowPanel() => gameObject.SetActive(true);
    private void HidePanl() => gameObject.SetActive(false);
    private void HideThrowPopup() => gameObject.SetActive(false);

    private void Awake()
    {
        throwOkBtn.onClick.AddListener(HidePanl);
        throwOkBtn.onClick.AddListener(HideThrowPopup);
        throwOkBtn.onClick.AddListener(() => OnOkBtn?.Invoke());
    }


}
