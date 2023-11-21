using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    private SaveData loadData;

    void Start()
    {
        Cursor.visible = true;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.None; //마우스 커서 위치 고정
        Time.timeScale = 0f;
        UIManager.gameIsPaused = true;
    }

    public void LoadMainScene()
    {
        //SceneManager.LoadScene("mid_SampleScene_01");
        gameObject.SetActive(false);
        Cursor.visible = false;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정
        Time.timeScale = 1f;
        UIManager.gameIsPaused = false;
        Debug.Log("임무 수행");
    }

    public void LoadDataScene()
    {
        Debug.Log("불러오기");
        //SceneManager.LoadScene("mid_SampleScene_01");

        loadData = SaveSystem.Load("GameData");
        DialogueLoad();
        gameObject.SetActive(false);
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
        GameManager.Instance.dialogueManager.DoQuest = loadData.doQuest;
        GameManager.Instance.gameInfo.DialogueNum = loadData.dialogueNum;
        GameManager.Instance.questManager.currentQuestValue_ = loadData.currentQuestValue;
        GameManager.Instance.questManager.UpdateQuest(loadData.questNum);
    }
}
