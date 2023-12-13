using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimeLineController : MonoBehaviour
{
    public PlayableDirector playableDirector;
    public TimelineAsset timeline;
    public bool runTimeline;

    private void Start()
    {
        runTimeline = false;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {

            Debug.Log("탐라");

            runTimeline = true;
            playableDirector.Play(timeline);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {

            runTimeline = true;


            //playableDirector.Pause(timeline);
        }
    }
    // public void Play()
    // {
    //     // 현재 playableDirector에 등록되어 있는 타임라인을 실행
    //     playableDirector.Play();
    // }

    // public void PlayFromTimeline()
    // {
    //     // 새로운 timeline을 시작
    //     playableDirector.Play(timeline);
    // }
}
