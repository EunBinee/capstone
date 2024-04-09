using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Setting : UIBase
{
    public SettingUI_Info settingInfo;

    private Image imageSettingBtn;
    private Image imgSaveBtn;
    private Color enableBtnColor;
    private Color disableBtnColor;
    private SaveData loadData;

    public override void Init()
    {
        base.Init();

        SetPanel();
        SetButtonValue();
    }

    private void SetPanel()
    {
        enableBtnColor = new Color32(255, 255, 255, 255);  //흰색
        imageSettingBtn = settingInfo.settingPanelBtn.GetComponent<Image>();
        imageSettingBtn.color = enableBtnColor;
        settingInfo.settingPanel.SetActive(true);

        disableBtnColor = new Color32(145, 145, 145, 255); //회색
        imgSaveBtn = settingInfo.savePanelBtn.GetComponent<Image>();
        imgSaveBtn.color = disableBtnColor;
        settingInfo.savePanel.SetActive(false);
    }

    public void SetButtonValue()
    {
        //* 버튼에 값을 넣어주는 함수.

        //close 닫기 버튼
        settingInfo.closeBtn.onClick.RemoveAllListeners();
        settingInfo.closeBtn.onClick.AddListener(() =>
        {
            SaveChangedData();

            if (UIManager.gameIsPaused == true)
            {
                //게임이 멈춰있으면 다시 재생.
                UIManager.Instance.Resume();
            }

            CloseBtn();
        });

        // 세팅 패널 버튼
        settingInfo.settingPanelBtn.onClick.RemoveAllListeners();
        settingInfo.settingPanelBtn.onClick.AddListener(() =>
        {
            if (!settingInfo.settingPanel.activeSelf)
            {
                //세팅 패널이 비활성화 되었을때만 진행
                imageSettingBtn.color = enableBtnColor;
                settingInfo.settingPanel.SetActive(true);

                imgSaveBtn.color = disableBtnColor;
                settingInfo.savePanel.SetActive(false);
            }
        });

        // 저장 패널 버튼
        settingInfo.savePanelBtn.onClick.RemoveAllListeners();
        settingInfo.savePanelBtn.onClick.AddListener(() =>
        {
            if (!settingInfo.savePanel.activeSelf)
            {
                //저장 패널이 비활성화 되었을때만 진행
                imageSettingBtn.color = disableBtnColor;
                settingInfo.settingPanel.SetActive(false);
                imgSaveBtn.color = enableBtnColor;
                settingInfo.savePanel.SetActive(true);
            }
        });

        //*----------------------------------------------------------------------//
        //세팅 패널 안 버튼
        //*----------------------------------------------------------------------//
        //저장 패널 안 버튼
        //저장 버튼

        settingInfo.saveBtn.onClick.RemoveAllListeners();
        settingInfo.saveBtn.onClick.AddListener(() =>
        {
            //TODO:저장 기능
            SaveData dialogue = new SaveData();
            dialogue.eventNum = GameManager.Instance.gameInfo.eventNum;
            dialogue.endingNum = GameManager.Instance.gameInfo.EndingNum;
            dialogue.questNum = GameManager.Instance.gameInfo.QuestNum;
            dialogue.doQuest = DialogueManager.instance.DoQuest;
            dialogue.dialogueNum = GameManager.Instance.gameInfo.DialogueNum;
            dialogue.currentQuestValue = DialogueManager.Instance.questManager.currentQuestValue_;
            dialogue.nickname = GameManager.Instance.gameInfo.Nickname;

            SaveSystem.Save(dialogue, "GameData");

        });

        //불러오기 버튼
        settingInfo.loadBtn.onClick.RemoveAllListeners();
        settingInfo.loadBtn.onClick.AddListener(() =>
        {
            //TODO: 불러오기 기능
            loadData = SaveSystem.Load("GameData");
            GameManager.instance.loadScene.DialogueLoad();
        });

        // 게임 나가기 기능
        settingInfo.quitBtn.onClick.RemoveAllListeners();
        settingInfo.quitBtn.onClick.AddListener(() =>
        {

            GameObject popup = UIManager.Instance.GetUIPrefab(UIManager.UI.PopupWindow);
            Popup_Window popup_Window = popup.GetComponent<Popup_Window>();
            popup_Window.SetButtonValue("Quit", "Do you really want to quit game?", () =>
            {
                //TODO: 나가기 기능 
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
                Debug.Log("나가기");
            });
        });

        //*----------------------------------------------------------------------//
    }

    private void SaveChangedData()
    {
        // TODO : 닫기전 세팅에서 바뀐 데이터 저장

    }

    //퀘스트, 대화 데이터 로드 함수 
    public void DialogueLoad()
    {
        GameManager.Instance.gameInfo.eventNum = loadData.eventNum;
        GameManager.Instance.gameInfo.EndingNum = loadData.endingNum;
        GameManager.Instance.gameInfo.QuestNum = loadData.questNum;
        DialogueManager.instance.DoQuest = loadData.doQuest;
        GameManager.Instance.gameInfo.DialogueNum = loadData.dialogueNum;
        DialogueManager.Instance.questManager.currentQuestValue_ = loadData.currentQuestValue;
        GameManager.Instance.gameInfo.Nickname = loadData.nickname;
        DialogueManager.Instance.questManager.UpdateQuest(loadData.questNum);
    }
}
