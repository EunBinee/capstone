using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class LoadScene
{
    //! - 씬 로드 (아직 구현 X)
    //! - 불러오기 저장하기
    private SaveData loadData;
    public GameObject mainScene;

    public void Init()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
        UIManager.gameIsPaused = true;
        //GameManager.instance.m_canvas.GetComponent<CanvasManager>().mainStartScene.GetComponent<mainStartScene>().SetButton();
    }

    //* 씬 불러오기
    public void ChangeScene(string sceneName)
    {
        GameManager.instance.cameraController.cameraInfo.ResetCamera();
        SceneManager.LoadScene(sceneName);
    }

    public void ReloadSetting()
    {
        //새로 불러왔을때 세팅.
    }

    public void LoadMainScene(string sceneName) //* 메인씬 로드
    {
        if (mainScene == null)
            mainScene = CanvasManager.instance.mainStartScene;

        mainScene.gameObject.SetActive(false);
        Cursor.visible = false;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정
        Time.timeScale = 1f;
        UIManager.gameIsPaused = false;

        SoundManager.Instance.Play_BGM(SoundManager.BGM.Ingame, true);
        ChangeScene(sceneName);
    }



    public void LoadDataScene() //* 불러오기
    {
        loadData = SaveSystem.Load("GameData");
        DialogueLoad();

        if (mainScene == null)
            mainScene = CanvasManager.instance.mainStartScene;

        mainScene.gameObject.SetActive(false);
        Cursor.visible = false;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정
        Time.timeScale = 1f;
        UIManager.gameIsPaused = false;
    }

    public void DialogueLoad()
    {
        GameManager.Instance.gameInfo.eventNum = loadData.eventNum;
        GameManager.Instance.gameInfo.EndingNum = loadData.endingNum;
        GameManager.Instance.gameInfo.QuestNum = loadData.questNum;
        DialogueManager.instance.DoQuest = loadData.doQuest;
        GameManager.Instance.gameInfo.DialogueNum = loadData.dialogueNum;
        DialogueManager.Instance.questManager.currentQuestValue_ = loadData.currentQuestValue;
        DialogueManager.Instance.questManager.UpdateQuest(loadData.questNum);
    }

}
