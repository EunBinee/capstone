using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerProjectile
{
    [SerializeField]
    private GameObject player;
    private PlayerController playerController;
    public int poolSize = 10;
    // 레이 길이
    public float rayDistance = 100f;

    public string arrowName = "Arrow"; // Prefabs 폴더에 Arrow 프리팹
    public int arrowsPoolCount = 50;
    public Dictionary<string, GameObject> arrowPrefabs;
    public Dictionary<string, List<GameObject>> arrowPools;

    public string bulletName = "PlayerBullet";
    public int bulletPoolCount = 100;
    public Dictionary<string, GameObject> bulletPrefabs;
    public Dictionary<string, List<GameObject>> bulletPools;


    public void Init()
    {
        player = GameManager.instance.gameData.player;
        playerController = player.GetComponent<PlayerController>();

        arrowPrefabs = new Dictionary<string, GameObject>();
        arrowPools = new Dictionary<string, List<GameObject>>();
        arrowPrefabs.Clear();
        arrowPools.Clear();

        bulletPrefabs = new Dictionary<string, GameObject>();
        bulletPools = new Dictionary<string, List<GameObject>>();
        bulletPrefabs.Clear();
        bulletPools.Clear();
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

    GameObject curBulletObj = null;
    public GameObject GetBulletPrefab()
    {
        //GameObject curBulletObj = null;
        //프리펩 찾기
        if (bulletPrefabs.ContainsKey(bulletName))    //화살이 있으면
        {
            curBulletObj = bulletPrefabs[bulletName];  //최근 오브젝트에 그대로 넣기
        }
        else        //아니면(화살이 없으면)
        {
            curBulletObj = Resources.Load<GameObject>("ProjectilePrefabs/" + bulletName); //리소스 불러오기
            if (curBulletObj != null)    //리소스를 성공적으로 받아오면
            {
                //프리펩 추가
                bulletPrefabs.Add(bulletName, curBulletObj);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("Projectile 프리펩 없음. 오류.");
#endif
            }
        }

        if (curBulletObj != null)    //최근 오브젝트를 성공적으로 받아오면
        {
            //오브젝트 풀에 
            if (bulletPools.ContainsKey(bulletName))  //풀에서 화살 있으면
            {
                if (bulletPools[bulletName].Count > 0)    //화살 풀에서 화살 갯수가 1개 이상이라면
                {
                    curBulletObj = bulletPools[bulletName][0]; //최근 오브젝트에 첫번째부터 넣고
                    bulletPools[bulletName].RemoveAt(0);  //제거
                }
                else
                {
                    curBulletObj = UnityEngine.Object.Instantiate(curBulletObj);
                }
            }
            else    //화살 풀에서 화살 갯수가 0개 이하라면
            {
                bulletPools.Add(bulletName, new List<GameObject>());  //풀에 새로 추가
                curBulletObj = UnityEngine.Object.Instantiate(curBulletObj);  //최근 오브젝트에 그대로 생성
            }

            curBulletObj.gameObject.transform.SetParent(GameManager.Instance.transform); //부모 설정
        }

        return curBulletObj; //최근 오브젝트 반환
    }

    public void AddBulletPool(GameObject bulletObj)
    {
        if (bulletPools[bulletName].Count >= bulletPoolCount)
        {
            //만약 풀이 가득 찼다면, 그냥 삭제.
            UnityEngine.Object.Destroy(bulletObj);
        }
        else    //화살 풀에서 화살의 갯수가 풀 카운트보다 작다면
        {
            //정상적으로 추가
            bulletObj.transform.SetParent(GameManager.Instance.transform);
            bulletPools[bulletName].Add(bulletObj);
            bulletObj.SetActive(false);
        }
    }

    public void PlayerBulletRay()
    {
        // 화면 중앙 좌표 계산 (스크린 해상도 기준)
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // 화면 중앙에서 카메라 방향으로 레이 생성
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);

        // 레이캐스트 결과 저장할 변수
        RaycastHit[] hit;
        hit = Physics.RaycastAll(ray, rayDistance, LayerMask.GetMask("Monster"));

        GameObject nearMon = hit[0].collider.gameObject;
        float monDist = hit[0].distance;
        int monIndex = 0;

        if (hit.Length > 1){
        for (int i = 1; i < hit.Length; i++)
        {
            if (monDist < hit[i].distance) 
            {
                nearMon = hit[i].collider.gameObject;
                monDist = hit[i].distance;
                monIndex = i;
            }
        }}
        //Debug.Log($"[player test] monDist = {monDist}");
        Monster monster = nearMon.GetComponentInParent<Monster>();
        if (monster)
        {
            Vector3 objHit = hit[monIndex].point;
            Debug.DrawRay(player.transform.position, objHit - player.transform.position, Color.yellow, 5f);
            player.GetComponent<PlayerSkills>().GetBulletDir(objHit - player.transform.position);

            string objtag = nearMon.tag;
            //Debug.Log($"[player test] ray tag = {objtag}");

            if (monster)
            {
                Vector3 collisionPoint = hit[monIndex].collider.ClosestPoint(objHit);
                Quaternion otherQuaternion = Quaternion.FromToRotation(Vector3.up, objHit.normalized);

                curBulletObj.GetComponent<PlayerAttackCheck>().playerHitMonster(collisionPoint, otherQuaternion, monster, objtag == "BossWeakness");
            }
        }
    }
}