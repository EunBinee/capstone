using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }
    public GameData gameData;

    public ObjectPooling objectPooling;
    public DamageCalculator damageCalculator;

    //대화
    public GameInfo gameInfo; //* 게임 정보
    public LoadScene loadScene;

    //*---------------------------------------------//
    public Canvas m_canvas;
    //* 카메라 제어---------------------------------//
    //! 씬이동 후, GameManager에서 CameraController는 null
    //! Awake 나 start에서 GameManager의 CameraController를 쓰고 싶을 때 아래 Action 사용. 
    public Action<CameraController> startActionCam = null; //
    public CameraController cameraController;
    //* 현재 몬스터 --------------------------------//
    public List<Monster> monsterUnderAttackList; //*현재 공격중인 몬스터들 리스트
                                                 //TODO: 나중에 몬스터 스폰 될때 자동으로 넣고 빼도록.
    public Monster[] cur_monsters; //위에 꺼할때 배열은 지워도 될듯염
    public List<Monster> monsters;

    public bool bossBattle = false;
    public bool isLoading = false;
    public bool curCutScene_ing = false;
    //* --------------------------------------------//
    //ESC 세팅 값
    public float cameraSensitivity = 1.0f; //카메라 감도




    void Awake()
    {
        Init();
    }
    void Start()
    {
        startInit();
    }
    private void Init()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);

        objectPooling.InitPooling();

        //cameraController = gameData.cameraObj.GetComponent<CameraController>();
        monsterUnderAttackList = new List<Monster>();

        //* 게임에 대한 전반적인 정보를 가지고 있는 스크립트. ex. 현재 게임의 엔딩 번호, 이벤트 번호
        gameInfo = GetComponent<GameInfo>();
        GetGameInfo();
    }

    public void GetGameInfo()
    {
        cur_monsters = GameObject.FindObjectsOfType<Monster>();
        monsters = new List<Monster>();
        for (int i = 0; i < cur_monsters.Length; i++)
        {
            monsters.Add(cur_monsters[i]);
        }

    }

    public void startInit()
    {
        loadScene.Init();
    }

    static public GameManager GetInstance()
    {
        return instance;
    }

    // 기본 메서드 
    public Color HexToColor(string hex)
    {
        Color color = Color.white;

        if (UnityEngine.ColorUtility.TryParseHtmlString(hex, out color))
        {
            return color;
        }
        else
        {
            Debug.LogError("Invalid hexadecimal color value: " + hex);
            return Color.white; // 기본값으로 흰색을 반환하거나 다른 처리를 추가할 수 있습니다.
        }
    }

    public void PadeIn_Alpha(GameObject image, bool padeIn, float alphaValue, float speed = 0.15f, bool isImage = true)
    {
        //pade In 변수  true => Pade In, false => Pade Out
        alphaValue /= 255;

        if (padeIn)
        {
            StartCoroutine(padeIn_Alpha(image, alphaValue, speed, isImage));
        }
        else
        {
            StartCoroutine(padeOut_Alpha(image, alphaValue, speed, isImage));
        }
    }

    IEnumerator padeIn_Alpha(GameObject image, float alphaValue, float speed = 0.15f, bool isImage = true)
    {
        if (isImage)
        {
            Image obj_img = image.GetComponent<Image>();

            while (true)
            {
                obj_img.color = new Color(obj_img.color.r, obj_img.color.g, obj_img.color.b, obj_img.color.a + (speed * Time.deltaTime));
                // 페이드 인이 완료되면 플래그를 false로 설정
                if (obj_img.color.a >= alphaValue)
                {
                    obj_img.color = new Color(obj_img.color.r, obj_img.color.g, obj_img.color.b, alphaValue);
                    break;
                }
                yield return null;
            }
        }
        else
        {
            TMP_Text obj_text = image.GetComponent<TMP_Text>();

            while (true)
            {
                obj_text.color = new Color(obj_text.color.r, obj_text.color.g, obj_text.color.b, obj_text.color.a + (speed * Time.deltaTime));
                // 페이드 인이 완료되면 플래그를 false로 설정
                if (obj_text.color.a >= alphaValue)
                {
                    obj_text.color = new Color(obj_text.color.r, obj_text.color.g, obj_text.color.b, alphaValue);
                    break;
                }
                yield return null;
            }
        }


    }

    IEnumerator padeOut_Alpha(GameObject image, float alphaValue, float speed = 0.15f, bool isImage = true)
    {
        if (isImage)
        {
            Image obj_img = image.GetComponent<Image>();
            while (true)
            {
                obj_img.color = new Color(obj_img.color.r, obj_img.color.g, obj_img.color.b, obj_img.color.a - (speed * Time.deltaTime));

                // 페이드 인이 완료되면 플래그를 false로 설정
                if (obj_img.color.a <= alphaValue)
                {
                    obj_img.color = new Color(obj_img.color.r, obj_img.color.g, obj_img.color.b, alphaValue);
                    break;
                }
                yield return null;
            }
            image.SetActive(false);
        }
        else
        {
            TMP_Text obj_text = image.GetComponent<TMP_Text>();

            while (true)
            {
                obj_text.color = new Color(obj_text.color.r, obj_text.color.g, obj_text.color.b, obj_text.color.a - (speed * Time.deltaTime));

                // 페이드 인이 완료되면 플래그를 false로 설정
                if (obj_text.color.a <= alphaValue)
                {
                    obj_text.color = new Color(obj_text.color.r, obj_text.color.g, obj_text.color.b, alphaValue);
                    break;
                }
                yield return null;
            }
            image.SetActive(false);
        }
    }

    public void SortingMonsterList()
    {
        //TODO: monsterUnderAttackList 리스트를 새로 정렬하기
        //플레이어와 거리 순으로.
        if (monsterUnderAttackList.Count > 1)
        {
            monsterUnderAttackList.Sort((monster01, monster02) =>
            {
                float distance1 = Vector3.Distance(monster01.transform.position, transform.position);
                float distance2 = Vector3.Distance(monster02.transform.position, transform.position);

                // 오름차순으로 정렬
                return distance1.CompareTo(distance2);
            });
        }
    }

    public void RemoveMonster()
    {
        //* 씬이동할때 현재 몬스터들 지움.
        monsterUnderAttackList.Clear();
        cur_monsters = new Monster[0];
        monsters.Clear();
    }

    public void Stop_AllMonster()
    {
        //* 구현
        // 모든 몬스터 roaming상태, 공격 중이던 몬스터는 자기자리로 돌아간 후 roaming상태
        // 몬스터가 플레이어 인지 못함.
        foreach (var m in monsters)
        {
            m.monsterPattern.StopMonster();
        }
    }

    public void Start_AllMonster()
    {
        foreach (var m in monsters)
        {
            m.monsterPattern.StartMonster();
        }
    }


    //*---------------------------------------------------------------------------//
    //* 사이 각 구하기 (0~180)
    public float GetAngleSeparation(Vector3 center, Vector3 point1, Vector3 point2)
    {
        Vector3 v1 = (point1 - center).normalized;
        Vector3 v2 = (point2 - center).normalized;

        float dotProduct = Vector3.Dot(v1, v2);
        float angle = Mathf.Acos(Mathf.Clamp(dotProduct, -1f, 1f));
        float angleInDegrees = Mathf.Rad2Deg * angle;

        return angleInDegrees;
    }

    //*---------------------------------------------------------------------------//
    public void CutSceneSetting(bool start)
    {
        if (start)
        {
            if (CurSceneManager.instance.curCMSetting != null)
            {
                if (!CurSceneManager.instance.curCMSetting.endSetting)
                {
                    CurSceneManager.instance.curCMSetting.Init();
                }
            }
            curCutScene_ing = true;
            StopGame();
        }
        else if (!start)
        {
            curCutScene_ing = false;

            StartGame();
        }

    }
    //*---------------------------------------------------------------------------//

    //* 세팅값 바꿔주기
    public void ChangeSettingValue()
    {
        cameraController.left_right_LookSpeed = cameraController.left_right_DefaultSpeed * cameraSensitivity;
        cameraController.up_down_LookSpeed = cameraController.up_down_DefaultSpeed * cameraSensitivity;

    }

    public void StopGame()
    {
        PlayerController playerController = gameData.GetPlayerController();
        playerController.PlayerUI_SetActive(false);
        Stop_AllMonster();

        UIManager.gameIsPaused = true;
    }
    public void StartGame()
    {
        PlayerController playerController = gameData.GetPlayerController();
        playerController.PlayerUI_SetActive(true);
        Start_AllMonster();

        UIManager.gameIsPaused = false;
    }
}

