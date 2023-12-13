using Unity.VisualScripting;
using UnityEngine;

public class LoadScene : MonoBehaviour
{
    private SaveData loadData;
    string gameManagerPrefabName = "GameManager";
    string soundManagerPrefabName = "SoundManager";
    string playerName = "Player";
    string playerCameraName = "PlayerCamera";

    void Start()
    {

        Cursor.visible = true;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.None; //마우스 커서 위치 고정
        Time.timeScale = 0f;
        UIManager.gameIsPaused = true;

        CheckData();
    }

    public void LoadMainScene()
    {


        gameObject.SetActive(false);
        Cursor.visible = false;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정
        Time.timeScale = 1f;
        UIManager.gameIsPaused = false;
        //Debug.Log("임무 수행");
        SoundManager.Instance.Play_BGM(SoundManager.BGM.Ingame, true);

    }

    public void LoadDataScene()
    {
        //Debug.Log("불러오기");
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

    public void CheckData()
    {
        GameManager gameManager = GameObject.FindObjectOfType<GameManager>();

        if (gameManager == null)
        {
            //아무것도 없는 상태
            //* GameManager 생성
            gameManager = Resources.Load<GameManager>("SystemPrefabs/BasicPrefabs/" + gameManagerPrefabName);
            gameManager = UnityEngine.Object.Instantiate(gameManager);

            //* SoundManager 생성
            SoundManager soundManager = Resources.Load<SoundManager>("SystemPrefabs/BasicPrefabs/" + soundManagerPrefabName);
            soundManager = UnityEngine.Object.Instantiate(soundManager);

            //* - Player 생성
            PlayerController player = Resources.Load<PlayerController>("SystemPrefabs/BasicPrefabs/" + playerName);
            player = UnityEngine.Object.Instantiate(player);

            //* - PlayerCamera 생성
            GameObject playerCamera = Resources.Load<GameObject>("SystemPrefabs/BasicPrefabs/" + playerCameraName);
            playerCamera = UnityEngine.Object.Instantiate(playerCamera);
            CameraController cameraController = playerCamera.GetComponent<PlayerCamera>().cameraController;

            //* GameManager 세팅
            //*
            gameManager.gameData.player = player.gameObject;
            gameManager.gameData.playerTargetPos = player._playerComponents.playerTargetPos;

            gameManager.gameData.playerCamera = cameraController.playerCamera;
            gameManager.gameData.playerCameraPivot = cameraController.playerCameraPivot;
            gameManager.gameData.cameraObj = cameraController.cameraObj;

            //* 카메라 세팅
            cameraController.playerController = player;

            //* 플레이어 세팅
            player._playerFollowCamera.playerCamera = cameraController.playerCamera;
            player._playerFollowCamera.playerCameraPivot = cameraController.playerCameraPivot;
            player._playerFollowCamera.cameraObj = cameraController.cameraObj;


        }








    }
}



