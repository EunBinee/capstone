using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance = null;
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
        PopupWindow
    };

    public UIPrefabs uiPrefabs;
    public List<GameObject> uiPrefabsInGame;

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
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gameIsPaused)
            {
                GetUIPrefab(UI.SettingMenu);
                Pause();
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
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
                    prefab = Resources.Load<GameObject>("SystemPrefabs/" + "Popup_Settings");
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
            default:
                break;
        }

        if (prefab != null)
        {
            if (!uiPrefabsInGame.Contains(prefab))
            {
                prefab = Instantiate(prefab, GameManager.Instance.m_canvas.transform);
                prefab.SetActive(true);
                uiPrefabsInGame.Add(prefab);

                switch (ui)
                {
                    case UI.SettingMenu:
                        uiPrefabs.settingUI = prefab;
                        break;
                    case UI.Inventory:
                        break;
                    case UI.Quest:
                        break;
                    case UI.PopupWindow:
                        uiPrefabs.popupWindow = prefab;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                int index = uiPrefabsInGame.IndexOf(prefab);
                uiPrefabsInGame[index].SetActive(true);
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
            GameManager.instance.gameData.player.transform.position = new Vector3(-12, 0.5f, 30);
            GameManager.instance.gameData.player.transform.rotation = Quaternion.identity;
        }
        // BossFieldScene으로 씬 이동
        GameManager.instance.loadSceneManager.ChangeScene("BossFieldScene");
        //        SceneManager.LoadScene("BossFieldScene");
        yield return new WaitForSeconds(1.5f);
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
        GameManager.Instance.PadeIn_Alpha(loadingImg.gameObject, true, 255, 0.65f, true);
    }
    IEnumerator PadeInBlack_Co(float delay)
    {
        yield return new WaitForSeconds(delay);
        GameManager.Instance.PadeIn_Alpha(loadingImg.gameObject, true, 255, 0.65f, true);
        yield return new WaitForSeconds(2);
        GameManager.instance.loadSceneManager.ChangeScene("StartScene");
    }
    public void PadeOutBlack()
    {
        GameManager.Instance.PadeIn_Alpha(loadingImg.gameObject, false, 0, 0.65f, true);
    }

}






