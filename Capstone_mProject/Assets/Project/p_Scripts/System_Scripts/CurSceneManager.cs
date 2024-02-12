using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurSceneManager : MonoBehaviour
{
    public List<Transform> spawnPoints;

    void Start()
    {
        SceneSetting();
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

    private void SetPlayerPos()
    {
        GameManager.instance.gameData.player.transform.position = spawnPoints[0].position;
        GameManager.instance.gameData.player.transform.rotation = Quaternion.identity;
    }
}
