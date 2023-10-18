using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //! 몬스터 공격의 총알입니다. (플레이어용 X)

    public Monster monster;
    private PlayerController playerController;
    private TrailRenderer trailRenderer;
    private Rigidbody rigid;

    public float disappearTime = 30f;
    public string projectileName = "";

    private float time = 0;
    private bool isdisappear = false;
    private bool isReset = false;
    private bool firstUpdate = true;

    public Action<Vector3> OnHitPlayerEffect = null;

    private float targetDistance = 0;
    private bool attackPlayer = false;

    private Vector3 curOriginPos;
    private Vector3 targetDir;


    private void Start()
    {
    }

    public void Reset(Monster _monster = null, string _projectileName = "", Transform muzzlePos = null)
    {
        trailRenderer = GetComponent<TrailRenderer>();
        rigid = GetComponent<Rigidbody>();
        this.gameObject.SetActive(true);

        this.gameObject.transform.position = muzzlePos.position;
        this.gameObject.transform.Rotate(Vector3.zero);
        curOriginPos = muzzlePos.position;

        playerController = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
        time = 0;
        isdisappear = false;
        targetDistance = 0;
        attackPlayer = false;

        monster = _monster;
        projectileName = _projectileName;

        trailRenderer.Clear();
        isReset = true;
        firstUpdate = true;
    }

    private void Update()
    {
        if (isReset)
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

            float curBulletDistance = Vector3.Distance(curOriginPos, this.gameObject.transform.position);

            if (firstUpdate)
            {
                firstUpdate = false;
                BulletRay(curBulletDistance);
            }

            if (curBulletDistance >= targetDistance && !isdisappear)
            {
                if (attackPlayer)
                {
                    AttackPlayer();
                }
                isdisappear = true;
                DisappearBullet();
            }
            else
            {
                BulletRay(curBulletDistance);
            }

        }
    }

    private void BulletRay(float curBulletDistance)
    {
        float range = 100f;
        RaycastHit[] hits;
        hits = Physics.RaycastAll(curOriginPos, targetDir, range);

        float shortDist = 1000f;
        RaycastHit shortHit = hits[0];

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.name != this.gameObject.name)
            {
                //자기 자신은 패스
                float distance = hit.distance;
                if (curBulletDistance < distance && shortDist > distance)
                {
                    shortHit = hit;
                    shortDist = distance;

                    if (hit.collider.tag == "Player")
                        attackPlayer = true;
                    else
                        attackPlayer = false;
                }
            }
        }

        if (shortDist != 1000)
            targetDistance = shortDist;
    }



    public void GetDistance(Vector3 _targetDir)
    {
        targetDir = _targetDir;
    }

    public void AttackPlayer()
    {
        GameManager.Instance.cameraShake.ShakeCamera(0.2f, 2, 1);
        OnHitPlayerEffect?.Invoke(this.gameObject.transform.position);

        if (!playerController._currentState.isGettingHit)
        {
            monster.OnHit();
        }
    }

    private void DisappearBullet()
    {
        //풀링

        GameManager.Instance.objectPooling.AddProjectilePool(projectileName, this.gameObject);
        OnHitPlayerEffect = null;
        isReset = false;
    }

}
