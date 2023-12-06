using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.VersionControl.Asset;

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
    public HPBarManager hPBarManager;
    public DamageManager damageManager;
    //대화
    public DialogueInfo dialogueInfo;
    public DialogueManager dialogueManager;
    public GameInfo gameInfo;
    //public Item item;
    public QuestManager questManager;
    //탐라
    public TimeLineController timeLineController;
    //씬 로드
    public LoadScene loadScene;

    //*---------------------------------------------//
    public Canvas m_canvas;
    //* 카메라 제어---------------------------------//
    public CameraShake cameraShake;
    public CameraController cameraController;
    //* 현재 몬스터 --------------------------------//

    public List<Monster> monsterUnderAttackList; //*현재 공격중인 몬스터들 리스트
                                                 //TODO: 나중에 몬스터 스폰 될때 자동으로 넣고 빼도록.
    public Monster[] cur_monsters; //위에 꺼할때 배열은 지워도 될듯염
    public List<Monster> monsters;

    public bool bossBattle = false;
    public bool isLoading = false;
    //* --------------------------------------------//
    void Awake()
    {
        Init();
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

        hPBarManager = GetComponent<HPBarManager>();
        damageManager = GetComponent<DamageManager>();

        cameraShake = GetComponent<CameraShake>();
        cameraController = gameData.cameraObj.GetComponent<CameraController>();
        monsterUnderAttackList = new List<Monster>();

        //instance = this;
        dialogueInfo = new DialogueInfo();
        dialogueManager = GetComponent<DialogueManager>(); //대사 시스템을 위한 스크립트
        //게임에 대한 전반적인 정보를 가지고 있는 스크립트. ex. 현재 게임의 엔딩 번호, 이벤트 번호
        gameInfo = GetComponent<GameInfo>();
        //item = GetComponent<Item>();
        questManager = new QuestManager();
        cur_monsters = GameObject.FindObjectsOfType<Monster>();
        monsters = new List<Monster>();
        for (int i = 0; i < cur_monsters.Length; i++)
        {
            monsters.Add(cur_monsters[i]);
        }
        timeLineController = GetComponent<TimeLineController>();
        loadScene = new LoadScene();
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

}

