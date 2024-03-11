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
    [SerializeField] private Button throwCancleBtn;

    //아이템 사용 팝업
    // [Header("Use")]
    // [SerializeField] private GameObject usePopup;
    // [SerializeField] private Button useOkBtn;
    // [SerializeField] private Button useCancleBtn;

    //private event Action OnOkBtn; // 확인 버튼 누를 경우 실행할 이벤트
    private event Action OnUseBtn; //사용버튼
    private event Action OnThrowBtn; //삭제버튼
    private void ShowPanel() => gameObject.SetActive(true);
    private void HidePanl() => gameObject.SetActive(false);
    public void HideThrowPopup() => gameObject.SetActive(false);
    //private void HideUsePopup() => gameObject.SetActive(false);

    private void Awake()
    {
        init();
        HidePanl();
        HideThrowPopup();
        //HideUsePopup();


    }
    private void init()
    {
        throwOkBtn.onClick.AddListener(HidePanl);
        throwOkBtn.onClick.AddListener(HideThrowPopup);
        throwOkBtn.onClick.AddListener(() => OnUseBtn?.Invoke());

        throwCancleBtn.onClick.AddListener(HidePanl);
        throwCancleBtn.onClick.AddListener(HideThrowPopup);
        throwCancleBtn.onClick.AddListener(() => OnThrowBtn?.Invoke());

        // useOkBtn.onClick.AddListener(HidePanl);
        // useOkBtn.onClick.AddListener(HideUsePopup);
        // useOkBtn.onClick.AddListener(() => OnOkBtn?.Invoke());

        // useCancleBtn.onClick.AddListener(HidePanl);
        // useCancleBtn.onClick.AddListener(HideUsePopup);
    }

    // public void OpenThrowPopup(Action okCallback)
    // {
    //     ShowPanel();
    //     ShowThrowPopup();
    //     OnThrowBtn = okCallback;
    // }
    public void OpenPopup(Action okCallback, Action removeCallback)
    {
        ShowPanel();
        ShowThrowPopup();
        OnUseBtn = okCallback;
        OnThrowBtn = removeCallback;
    }

    // public void OpenUsePopup(Action okCallback)
    // {
    //     ShowPanel();
    //     ShowUsePopup();
    //     OnOkBtn = okCallback;
    //     Debug.Log("open");
    // }

    private void ShowThrowPopup()
    {
        throwPopup.SetActive(true);
        //usePopup.SetActive(false);
    }
    // private void ShowUsePopup()
    // {
    //     usePopup.SetActive(true);
    //     throwPopup.SetActive(false);
    // }
}
