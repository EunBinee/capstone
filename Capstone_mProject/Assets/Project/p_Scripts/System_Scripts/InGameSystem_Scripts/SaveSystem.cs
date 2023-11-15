using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class SaveSystem : MonoBehaviour
{
    // static GameObject container;

    // static SaveSystem instance;
    // public static SaveSystem Instance
    // {
    //     get
    //     {
    //         if (!instance)
    //         {
    //             container = new GameObject();
    //             container.name = "SaveSystem";
    //             instance = container.AddComponent(typeof(SaveSystem)) as SaveSystem;
    //             DontDestroyOnLoad(container);
    //         }
    //         return instance;
    //     }
    // }

    // //게임데이터 파일이름 설정(원하는 이름(영문).json)
    // string GameDataFileName = "GameData.json";

    // //저장용 클래스 변수
    // public SaveData data = new SaveData();
    // //public GameInfo gameInfo = new GameInfo();

    // //불러오기
    // public void LoadGameData()
    // {
    //     string filePath = Application.persistentDataPath + "/" + GameDataFileName;

    //     //저장된 게임이 있다면
    //     if (File.Exists(filePath))
    //     {
    //         //저장된 파일을 읽어오고 json을 클래스 형식으로 전환해서 할당
    //         string FromJsonData = File.ReadAllText(filePath);
    //         data = JsonUtility.FromJson<SaveData>(FromJsonData);
    //         //gameInfo = JsonUtility.FromJson<GameInfo>(FromJsonData);
    //         Debug.Log("불러오기 완료, 이벤트 번호: " + data.id);
    //     }
    // }

    // //저장하기
    // public void SaveGameData()
    // {
    //     //클래스를 json 형식으로 전환(true : 가독성 좋게 작성)
    //     string ToJsonData = JsonUtility.ToJson(data, true);
    //     string filiePath = Application.persistentDataPath + "/" + GameDataFileName;

    //     //이미 저장된 파일이 있다면 덮어쓰고, 없다면 새로 만들어서 저장. 
    //     File.WriteAllText(filiePath, ToJsonData);

    //     //올바르게 저장됐는지 확인
    //     Debug.Log("저장 완료, 이벤트 번호: " + data.id);
    // }\


    private static string SavePath => Application.persistentDataPath + "/saves/";

    public static void Save(SaveData saveData, string saveFileName)
    {
        if (!Directory.Exists(SavePath))
        {
            Directory.CreateDirectory(SavePath);
        }

        string saveJson = JsonUtility.ToJson(saveData, true);

        string saveFilePath = SavePath + saveFileName + ".json";
        File.WriteAllText(saveFilePath, saveJson);
        Debug.Log("Save Success: " + saveFilePath);
    }

    public static SaveData Load(string saveFileName)
    {
        string saveFilePath = SavePath + saveFileName + ".json";

        if (!File.Exists(saveFilePath))
        {
            Debug.LogError("No such saveFile exists");
            return null;
        }
        Debug.Log("Load Success");
        string saveFile = File.ReadAllText(saveFilePath);
        SaveData saveData = JsonUtility.FromJson<SaveData>(saveFile);
        return saveData;
    }
}
