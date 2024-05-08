using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.EventSystems;

public class ButtonController : MonoBehaviour
{
    public UIManager.ButtonUI btnUI;

    private void OnMouseEnter()
    {
        //* 마우스가 오브젝트 위에 있다.

        switch (btnUI)
        {
            case UIManager.ButtonUI.StartSceneBtn:
                Debug.Log("enter");
                break;
            default:
                break;
        }
    }

    private void OnMouseExit()
    {
        //*  마우스가 오브젝트에서 벗어났습니다.
        switch (btnUI)
        {
            case UIManager.ButtonUI.StartSceneBtn:
                Debug.Log("exit");
                break;
            default:
                break;
        }

    }

}
