using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private PlayerController playerController;
    public Monster monster;

    public float disappearTime = 30f;
    public string projectileName = "";
    private float time = 0;
    private bool isdisappear = false;

    TrailRenderer trailRenderer;

    private void Start()
    {
    }

    public void Reset(Monster _monster = null, string _projectileName = "", Transform muzzlePos = null)
    {
        trailRenderer = GetComponent<TrailRenderer>();

        this.gameObject.SetActive(true);

        this.gameObject.transform.position = muzzlePos.position;
        this.gameObject.transform.Rotate(Vector3.zero);

        playerController = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
        time = 0;
        isdisappear = false;

        monster = _monster;
        projectileName = _projectileName;

        trailRenderer.Clear();
    }

    private void Update()
    {
        if (time <= disappearTime)
        {
            time += Time.deltaTime;
        }
        if (time > disappearTime && !isdisappear)
        {
            isdisappear = true;
            DisappearBullet();
        }
    }

    private void DisappearBullet()
    {
        //풀링
        GameManager.Instance.objectPooling.AddProjectilePool(projectileName, this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!playerController._currentState.isGettingHit)
            {
                monster.OnHit();
            }
            isdisappear = true;
            DisappearBullet();
        }
        else
        {
            isdisappear = true;
            DisappearBullet();
        }
    }


    // ! n초가 지나면 사라지도록
    // ! 어딘가에 부딪치면 사라지도록
    // ! 플레이어에 맞으면 사라지고 플레이어 피격



}
