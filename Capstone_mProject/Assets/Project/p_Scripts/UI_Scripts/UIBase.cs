using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        Init();
    }

    public virtual void Init()
    {
    }

    //* ----------------------------------------------------------------------------//
    // close 버튼
    public void CloseBtn()
    {
        this.gameObject.SetActive(false);

    }
    //* ----------------------------------------------------------------------------//
    //TODO: Save Load Quit 는 다른 곳에서도 많이 쓰이니 여기서 관리
}
