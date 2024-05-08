using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    public KeyState playerKeyState;
    public static ButtonManager instance = null;

    public enum Btns
    {
        SkipBtn
    }

    public Button skipBtn;
    bool skipBtnActive = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);


    }
    void Start()
    {
        Init();
    }
    // Update is called once per frame
    void Init()
    {
        playerKeyState = GameManager.instance.gameData.GetPlayerController()._keyState;
        SetActiveBtn(Btns.SkipBtn, false);
    }

    void Update()
    {
        if (skipBtnActive)
        {
            //*skip버튼이있을경우
            if (playerKeyState.ZDown)
            {
                skipBtnActive = false;
                skipBtn.onClick.Invoke();
            }
        }
    }


    public void SetActiveBtn(Btns btn, bool active)
    {
        switch (btn)
        {
            case Btns.SkipBtn:
                skipBtn.gameObject.SetActive(active);
                skipBtnActive = active;
                break;
            default:
                break;
        }
    }
}
