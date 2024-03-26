using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerArrows
{
    private GameObject player;
    private PlayerController playerController;
    public string arrowName = "Arrow"; // Prefabs 폴더에 Arrow 프리팹
    public int poolSize = 5;
    public int arrowsPoolCount = 50;
    public Dictionary<string, GameObject> arrowPrefabs;
    public Dictionary<string, List<GameObject>> arrowPools;



    public void Init()
    {
        player = GameManager.instance.gameData.player;
        playerController = player.GetComponent<PlayerController>();

        arrowPrefabs = new Dictionary<string, GameObject>();
        arrowPools = new Dictionary<string, List<GameObject>>();
        arrowPrefabs.Clear();
        arrowPools.Clear();
    }

    public GameObject GetArrowPrefab()
    {
        GameObject curArrowObj = null;
        //프리펩 찾기
        if (arrowPrefabs.ContainsKey(arrowName))    //화살이 있으면
        {
            curArrowObj = arrowPrefabs[arrowName];  //최근 오브젝트에 그대로 넣기
        }
        else        //아니면(화살이 없으면)
        {
            curArrowObj = Resources.Load<GameObject>("ProjectilePrefabs/" + arrowName); //리소스 불러오기
            if (curArrowObj != null)    //리소스를 성공적으로 받아오면
            {
                //프리펩 추가
                arrowPrefabs.Add(arrowName, curArrowObj);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("Projectile 프리펩 없음. 오류.");
#endif
            }
        }

        if (curArrowObj != null)    //최근 오브젝트를 성공적으로 받아오면
        {
            //오브젝트 풀에 
            if (arrowPools.ContainsKey(arrowName))  //풀에서 화살 있으면
            {
                if (arrowPools[arrowName].Count > 0)    //화살 풀에서 화살 갯수가 1개 이상이라면
                {
                    curArrowObj = arrowPools[arrowName][0]; //최근 오브젝트에 첫번째부터 넣고
                    arrowPools[arrowName].RemoveAt(0);  //제거
                }
                else
                {
                    curArrowObj = UnityEngine.Object.Instantiate(curArrowObj);
                }
            }
            else    //화살 풀에서 화살 갯수가 0개 이하라면
            {
                arrowPools.Add(arrowName, new List<GameObject>());  //풀에 새로 추가
                curArrowObj = UnityEngine.Object.Instantiate(curArrowObj);  //최근 오브젝트에 그대로 생성
            }

            curArrowObj.gameObject.transform.SetParent(GameManager.Instance.transform); //부모 설정
        }

        return curArrowObj; //최근 오브젝트 반환
    }

    public void AddArrowPool(GameObject arrowObj)
    {
        if (arrowPools[arrowName].Count >= arrowsPoolCount) //화살 풀에서 화살의 갯수가 풀 카운트보다 크다면
        {
            //만약 풀이 가득 찼다면, 그냥 삭제.
            UnityEngine.Object.Destroy(arrowObj);
        }
        else    //화살 풀에서 화살의 갯수가 풀 카운트보다 작다면
        {
            //정상적으로 추가
            arrowObj.transform.SetParent(GameManager.Instance.transform);
            arrowPools[arrowName].Add(arrowObj);
            arrowObj.SetActive(false);
        }
    }
}
