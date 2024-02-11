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
        //처음 불러와졌을때 세팅
        CanvasManager.instance.mainStartScene.SetActive(false);
        Cursor.visible = false;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정
        Time.timeScale = 1f;
        UIManager.gameIsPaused = false;

        SoundManager.Instance.Play_BGM(SoundManager.BGM.Ingame, true);
        // GameManager.instance.cameraController.cameraInfo.ResetCamera();

        SetPlayerPos();
    }

    private void SetPlayerPos()
    {
        GameManager.instance.gameData.player.transform.position = spawnPoints[0].position;
        GameManager.instance.gameData.player.transform.rotation = Quaternion.identity;
    }
}
