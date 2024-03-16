using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerArrows
{
    private GameObject player;
    private PlayerController playerController;
    public string arrowPrefabName = "Arrow"; // Prefabs 폴더에 Arrow 프리팹
    public int poolSize = 5;
    private GameObject[] arrowPool;
    private int currentArrowIndex = 0;



    public void Init()
    {
        player = GameManager.instance.gameData.player;
        playerController = player.GetComponent<PlayerController>();
        InitializeArrowPool();
    }

    void InitializeArrowPool()
    {
        arrowPool = new GameObject[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            GameObject arrowPrefab = Resources.Load<GameObject>("ProjectilePrefabs/" + arrowPrefabName);
            if (arrowPrefab == null)
            {
                Debug.Log("arrowPrefab = null");
            }
            //arrowPool[i] = UnityEngine.Object.Instantiate(arrowPrefab, playerController.transform.position, Quaternion.identity);
            arrowPool[i] = UnityEngine.Object.Instantiate(arrowPrefab, playerController.shootPoint.position, playerController.shootPoint.rotation);
            //arrowPool[i].gameObject.transform.SetParent(playerController.shootPoint);
            arrowPool[i].gameObject.transform.SetParent(GameManager.Instance.transform);
            arrowPool[i].SetActive(false);
        }
    }

    public GameObject GetArrowFromPool()
    {
        GameObject arrow = arrowPool[currentArrowIndex];
        currentArrowIndex = (currentArrowIndex + 1) % poolSize;
        arrow.SetActive(false);
        return arrow;
    }
}
