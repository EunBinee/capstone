using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerSkills
{
    PlayerController playerController;
    public string arrowPrefabPath = "Arrow"; // Prefabs 폴더에 Arrow 프리팹
    public int poolSize = 5;
    private GameObject[] arrowPool;
    private int currentArrowIndex = 0;



    public void Init()
    {
        playerController = GameManager.instance.gameData.player.GetComponent<PlayerController>();
        InitializeArrowPool();
    }

    void InitializeArrowPool()
    {
        Debug.Log("비상");
        arrowPool = new GameObject[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            GameObject arrowPrefab = Resources.Load<GameObject>(arrowPrefabPath);
            if (arrowPrefab == null)
            {
                Debug.Log("arrowPrefab = null");
            }
            arrowPool[i] = UnityEngine.Object.Instantiate(arrowPrefab, playerController.transform.position, Quaternion.identity);
            arrowPool[i].gameObject.transform.SetParent(GameManager.Instance.transform);
            arrowPool[i].SetActive(false);
        }
    }

    public GameObject GetArrowFromPool()
    {
        GameObject arrow = arrowPool[currentArrowIndex];
        currentArrowIndex = (currentArrowIndex + 1) % poolSize;
        arrow.SetActive(true);
        return arrow;
    }
}
