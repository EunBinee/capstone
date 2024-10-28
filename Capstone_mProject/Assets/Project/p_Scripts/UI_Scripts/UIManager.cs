using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null;
    public static UIManager Instance
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

    public Image loadingImg;
    public static bool gameIsPaused = false;

    public enum UI
    {
        SettingMenu,
        Inventory,
        Quest,
        PopupWindow,
        PlayerDieWindow,
        FinishGameWindow
    };

    public enum ButtonUI
    {
        StartSceneBtn,
        Btn
    }

    public UIPrefabs uiPrefabs;
    public List<GameObject> uiPrefabsInGame;

    [Header("몬스터관련 UI 관리")]
    public HPBarManager hPBarManager;
    public DamageManager damageManager;

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

        hPBarManager = GetComponent<HPBarManager>();
        damageManager = GetComponent<DamageManager>();
    }

    void Start()
    {
        if (CanvasManager.instance.loadingImg == null)
        {
            CanvasManager.instance.loadingImg = CanvasManager.instance.GetCanvasUI(CanvasManager.instance.loadingImgName);
            if (CanvasManager.instance.loadingImg == null)
                Debug.LogError("CanvasManager.instance.loadingImg 없음");
        }
        else
            loadingImg = CanvasManager.instance.loadingImg.GetComponent<Image>();

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Setting"))
        {
            if (!gameIsPaused && !GameManager.instance.isLoading)
            {
                GetUIPrefab(UI.SettingMenu);
                Pause();
            }
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (!GameManager.instance.bossBattle)
            {
                GoBossField(true);
            }
        }

        //왼쪽Alt키를 누르고 있는동안 마우스 커서 활성화
        if (Input.GetKey(KeyCode.LeftAlt))
        {

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None; //마우스 커서 위치 고정
        }
        else if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

    }

    public void Resume()
    {
        //! 다시 시작
        Cursor.visible = false;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.Locked; //마우스 커서 위치 고정
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    public void Pause(bool useTimeScale = true)
    {
        //! 멈춤
        Cursor.visible = true;     //마우스 커서를 보이지 않게
        Cursor.lockState = CursorLockMode.None; //마우스 커서 위치 고정
        gameIsPaused = true;
        if (useTimeScale)
            Time.timeScale = 0f;

    }

    public GameObject GetUIPrefab(UI ui)
    {
        GameObject prefab = null;

        switch (ui)
        {
            case UI.SettingMenu:
                if (uiPrefabs.settingUI == null)
                    prefab = Resources.Load<GameObject>("CanvasPrefabs/" + "SettingsUI");
                else
                    prefab = uiPrefabs.settingUI;
                break;
            case UI.Inventory:
                break;
            case UI.Quest:
                break;
            case UI.PopupWindow:
                if (uiPrefabs.popupWindow == null)
                    prefab = Resources.Load<GameObject>("SystemPrefabs/" + "Popup_Window");
                else
                    prefab = uiPrefabs.popupWindow;
                break;
            case UI.PlayerDieWindow:
                if (uiPrefabs.playerDieWindow == null)
                {
                    prefab = Resources.Load<GameObject>("CanvasPrefabs/" + "PlayerDieUI");
                }
                else
                    prefab = uiPrefabs.playerDieWindow;
                break;
            case UI.FinishGameWindow:
                if (uiPrefabs.finishUIWindow == null)
                {
                    prefab = Resources.Load<GameObject>("CanvasPrefabs/" + "FinishUI");
                }
                else
                    prefab = uiPrefabs.finishUIWindow;
                break;
            default:
                break;
        }

        if (prefab != null)
        {
            if (!uiPrefabsInGame.Contains(prefab))
            {
                if (GameManager.Instance.m_canvas == null)
                {
                    GameManager.instance.m_canvas = CanvasManager.instance.gameObject.GetComponent<Canvas>();
                }
                prefab = Instantiate(prefab, GameManager.Instance.m_canvas.transform);
                prefab.SetActive(true);

                uiPrefabsInGame.Add(prefab);

                switch (ui)
                {
                    case UI.SettingMenu:
                        uiPrefabs.settingUI = prefab;
                        //uiPrefabsInGame.Add(prefab);
                        break;
                    case UI.Inventory:
                        //uiPrefabsInGame.Add(prefab);
                        break;
                    case UI.Quest:
                        //uiPrefabsInGame.Add(prefab);
                        break;
                    case UI.PopupWindow:
                        uiPrefabs.popupWindow = prefab;
                        //uiPrefabsInGame.Add(prefab);
                        break;
                    case UI.PlayerDieWindow:
                        uiPrefabs.playerDieWindow = prefab;
                        break;
                    case UI.FinishGameWindow:
                        uiPrefabs.finishUIWindow = prefab;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                int index = uiPrefabsInGame.IndexOf(prefab);
                GameObject curObj = uiPrefabsInGame[index];
                curObj.SetActive(true);

                switch (ui)
                {
                    case UI.SettingMenu:
                        curObj.GetComponent<SettingUI>().ShowSettingUI();
                        break;
                    case UI.Inventory:
                        break;
                    case UI.Quest:
                        break;
                    case UI.PopupWindow:
                        uiPrefabs.popupWindow = prefab;
                        break;
                    case UI.PlayerDieWindow:
                        curObj.GetComponent<FinishUI>().ShowFinishUI();
                        break;
                    case UI.FinishGameWindow:
                        curObj.GetComponent<FinishUI>().ShowFinishUI();
                        break;
                    default:
                        break;
                }
            }
        }

        return prefab;
    }

    public void GoBossField(bool changeP = false)
    {
        GameManager.instance.isLoading = true;
        PadeInBlack();
        //Pause(false);

        StartCoroutine(LoadSceneAfterDelay(changeP));
    }

    IEnumerator LoadSceneAfterDelay(bool changeP = false)
    {
        // 0.5초 대기
        yield return new WaitForSeconds(1.5f);
        if (changeP)
        {
            GameManager.instance.gameData.player.transform.position = new Vector3(11.5f, 0.4f, -23);
            GameManager.instance.gameData.player.transform.rotation = Quaternion.identity;
        }
        // BossFieldScene으로 씬 이동
        GameManager.instance.loadScene.ChangeScene("BossFieldScene 1");
        //        SceneManager.LoadScene("BossFieldScene");
        SoundManager.Instance.Stop_BGM(SoundManager.BGM.Ingame);
        yield return new WaitForSeconds(1.5f);
        SoundManager.Instance.Play_BGM(SoundManager.BGM.BossIngame, true);
        PadeOutBlack();
        yield return new WaitForSeconds(0.5f);
        GameManager.instance.isLoading = false;
    }

    public void PadeInBlack(float delay = 0)
    {
        if (delay != 0)
        {
            StartCoroutine(PadeInBlack_Co(delay));
        }
        else
        {
            if (loadingImg.gameObject.activeSelf == false)
                loadingImg.gameObject.SetActive(true);
            GameManager.Instance.PadeIn_Alpha(loadingImg.gameObject, true, 255, 0.65f, true);
        }

    }
    IEnumerator PadeInBlack_Co(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (loadingImg.gameObject.activeSelf == false)
            loadingImg.gameObject.SetActive(true);
        GameManager.Instance.PadeIn_Alpha(loadingImg.gameObject, true, 255, 0.65f, true);
    }
    public void PadeOutBlack()
    {
        GameManager.Instance.PadeIn_Alpha(loadingImg.gameObject, false, 0, 0.65f, true);
    }


    //*-----------------------------------------------------------------------------------//
    public void FinishGame()
    {
        //* 게임 끝났을 때 UI
        StartCoroutine(FinishGame_co());
    }
    IEnumerator FinishGame_co()
    {
        yield return new WaitForSeconds(3f);
        GetUIPrefab(UI.FinishGameWindow);
        Pause();
    }
    public void PlayerDie()
    {
        //* 플레이어 죽었을 때 UI
        StartCoroutine(PlayerDie_co());
    }
    IEnumerator PlayerDie_co()
    {
        yield return new WaitForSeconds(3f);
        GetUIPrefab(UI.PlayerDieWindow);
        Pause();
    }


}






