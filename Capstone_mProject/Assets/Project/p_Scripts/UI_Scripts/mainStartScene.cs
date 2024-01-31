using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class mainStartScene : MonoBehaviour
{
    public Button startBtn;
    public Button loadBtn;

    void Start()
    {
        SetButton();
    }

    public void SetButton()
    {
        startBtn.onClick.AddListener(() => GameManager.instance.loadScene.LoadMainScene());
        loadBtn.onClick.AddListener(() => GameManager.instance.loadScene.LoadDataScene());
    }
}
