using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
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
    public MonoBehaviour monoBehaviour { get; private set; }

    public ObjectPooling objectPooling;

    [SerializeField] private Button poolingBtn;

    void Awake()
    {
        InitGameManager();
    }

    private void InitGameManager()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);

        objectPooling.InitPooling();

        poolingBtn.onClick.RemoveAllListeners();
        poolingBtn.onClick.AddListener(() =>
        {
            objectPooling.ShowEffect("Prefab");

        });
    }


}
