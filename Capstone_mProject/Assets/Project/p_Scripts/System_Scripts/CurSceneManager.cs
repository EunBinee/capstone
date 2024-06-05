using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CurSceneManager : MonoBehaviour
{
    public static CurSceneManager instance = null;

    public string curSceneName = "";
    public List<Transform> spawnPoints;
    public List<string> timelinesName;

    public CMSetting curCMSetting;
    public Transform timelineParent;
    public List<PlayableDirector> timelines;
    public Dictionary<string, PlayableDirector> timelineDic;

    public bool haveCsv_Npc;
    public string csvFileName_NPC;
    public bool haveCsv_Quest;
    public string csvFileName_Quest;

    bool isReady = false;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }



    }

    void Start()
    {
        SceneSetting();
        TimelineSetting();
        if (haveCsv_Npc)
        {
            DatabaseManager.instance.csvFileName_NPC = csvFileName_NPC;
            DatabaseManager.instance.DialogueParser(csvFileName_NPC, true);

            if (csvFileName_NPC == "Tutorial")
                DialogueManager.instance.questManager.isTutorial = true;
            else
                DialogueManager.instance.questManager.isTutorial = false;

            GameManager.Instance.gameInfo.EventNum = 1;
        }
        if (haveCsv_Quest)
        {
            DatabaseManager.instance.csvFileName_Quest = csvFileName_Quest;
            DatabaseManager.instance.QuestParser(csvFileName_Quest, 0);
            QuestManager.instance.currentQuestValue_ = 0;
        }
    }

    void Update()
    {

    }

    private void SceneSetting()
    {
        //* 처음 불러와졌을때 세팅

        if (UIManager.instance.uiPrefabs.playerDieWindow != null)
            UIManager.instance.uiPrefabs.playerDieWindow.gameObject.SetActive(false);

        if (UIManager.instance.uiPrefabs.finishUIWindow != null)
            UIManager.instance.uiPrefabs.finishUIWindow.gameObject.SetActive(false);

        Cursor.visible = false;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정
        Time.timeScale = 1f;
        UIManager.gameIsPaused = false;

        //나중에 씬마다 BGM동적으로 바뀌도록 설정하기
        SoundManager.Instance.Play_BGM(SoundManager.BGM.Ingame, true);

        GameManager.instance.gameData.player.GetComponent<PlayerInputHandle>().KeyRebind();
        SetPlayerPos();

        GameManager.instance.GetGameInfo();

        GameManager.instance.gameData.GetPlayerController().player_loadScene = false;

        if (CanvasManager.instance.fadeImg != null)
        {
            CanvasManager.instance.fadeImg.SetActive(true);
            Fade fade = CanvasManager.instance.fadeImg.GetComponent<Fade>();
            fade.FadeOut();
        }


        if (CanvasManager.instance.cameraResolution == null)
        {
            CanvasManager.instance.cameraResolution = CanvasManager.instance.gameObject.GetComponent<CameraResolution>();
        }
        CanvasManager.instance.cameraResolution.SetResolution();
    }

    private void TimelineSetting()
    {

        LoadTimeLine();

        timelineDic = new Dictionary<string, PlayableDirector>();

        for (int i = 0; i < timelinesName.Count; i++)
            timelineDic.Add(timelinesName[i], timelines[i]);

        isReady = true;
    }

    private void SetPlayerPos()
    {
        GameManager.instance.gameData.player.transform.position = spawnPoints[0].position;
        GameManager.instance.gameData.player.transform.rotation = Quaternion.identity;
    }

    public void PlayTimeline(string timelineName)
    {
        StartCoroutine("PlayTimeLineRoutine", timelineName);
    }

    IEnumerator PlayTimeLineRoutine(string timelineName)
    {
        if (isReady == false)
        {
            Debug.Log("!! Not Ready");
        }
        while (isReady == false)
        {
            yield return null;
        }
        // Debug.Log("!! Ready And Play");

        timelineDic[timelineName].Play();
    }

    public PlayableDirector GetTimeLine(string timelineName)
    {
        return timelineDic[timelineName];
    }

    private void LoadTimeLine()
    {
        if (timelines.Count != timelinesName.Count)
        {
            timelines.Clear();
            foreach (Transform child in timelineParent)
            {
                // 자식 오브젝트의 작업을 수행합니다.
                Debug.Log("Child Object Name: " + child.name);
                timelines.Add(child.gameObject.GetComponent<PlayableDirector>());

            }
        }

    }

}
