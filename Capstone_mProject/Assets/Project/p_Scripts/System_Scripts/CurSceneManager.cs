using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CurSceneManager : MonoBehaviour
{
    public static CurSceneManager instance = null;

    public List<Transform> spawnPoints;
    public List<string> timelinesName;

    public CMSetting curCMSetting;
    public List<PlayableDirector> timelines;
    Dictionary<string, PlayableDirector> timelineDic;

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
    }

    void Update()
    {

    }

    private void SceneSetting()
    {
        //* 처음 불러와졌을때 세팅
        if (CanvasManager.instance.mainStartScene.activeSelf)
            CanvasManager.instance.mainStartScene.SetActive(false);
        Cursor.visible = false;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정
        Time.timeScale = 1f;
        UIManager.gameIsPaused = false;

        //나중에 씬마다 BGM동적으로 바뀌도록 설정하기
        SoundManager.Instance.Play_BGM(SoundManager.BGM.Ingame, true);
        // GameManager.instance.cameraController.cameraInfo.ResetCamera();

        SetPlayerPos();

        GameManager.instance.GetGameInfo();
    }

    private void TimelineSetting()
    {
        timelineDic = new Dictionary<string, PlayableDirector>();

        for (int i = 0; i < timelinesName.Count; i++)
            timelineDic.Add(timelinesName[i], timelines[i]);
    }

    private void SetPlayerPos()
    {
        GameManager.instance.gameData.player.transform.position = spawnPoints[0].position;
        GameManager.instance.gameData.player.transform.rotation = Quaternion.identity;
    }

    public void PlayTimeline(string timelineName)
    {
        timelineDic[timelineName].Play();
    }

}
