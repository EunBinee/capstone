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
    public Transform timelineParent;
    public List<PlayableDirector> timelines;
    Dictionary<string, PlayableDirector> timelineDic;

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
    }

    void Update()
    {

    }

    private void SceneSetting()
    {
        //* 처음 불러와졌을때 세팅

        Cursor.visible = false;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정
        Time.timeScale = 1f;
        UIManager.gameIsPaused = false;

        //나중에 씬마다 BGM동적으로 바뀌도록 설정하기
        SoundManager.Instance.Play_BGM(SoundManager.BGM.Ingame, true);
        // GameManager.instance.cameraController.cameraInfo.ResetCamera();

        GameManager.instance.gameData.player.GetComponent<PlayerInputHandle>().KeyRebind();
        SetPlayerPos();

        GameManager.instance.GetGameInfo();
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
        Debug.Log("!! Ready And Play");

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

        //   timelines.Clear();
        //   for (int i = 0; i < timelinesName.Count; i++)
        //   {
        //       GameObject timeLine = Resources.Load<GameObject>("TimeLinesResource/" + timelinesName[i]);
        //       GameObject timeLineObj = UnityEngine.Object.Instantiate(timeLine);
        //       timeLineObj.transform.SetParent(timelineParent);
        //       PlayableDirector director = timeLineObj.GetComponent<PlayableDirector>();
        //
        //
        //       // TimelineAsset track = timeLineObj.GetComponent<TimelineAsset>();
        //
        //       var outputs = director.playableAsset.outputs;
        //       foreach (var itm in outputs)
        //       {
        //           Debug.Log(itm.sourceObject.name);
        //       }
        //
        //       //director.playableAsset.GetOutputTrack(0);
        //       // director.SetGenericBinding(track, GameManager.instance.cameraController.gameObject.GetComponent<Camera>());
        //
        //
        //       // timelines.Add(director);
        //   }
        //
    }

}
