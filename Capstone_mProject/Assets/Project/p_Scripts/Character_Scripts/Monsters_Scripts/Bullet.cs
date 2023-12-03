using System;
using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
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
    [Header("맞으면 무조건 넘어지는 총알인지")]
    public bool isFallDown = false;

    private bool isdisappear = false;
    private bool isReset = false;
    private bool firstUpdate = true;

    public Action OnHitPlayerEffect = null;

    private float targetDistance = 0;
    private bool attackPlayer = false;

    private Vector3 curOriginPos;
    private Vector3 targetDir;

    // [Header("맞은 부위 체크")]
    private Vector3 hitPoint;
    private Vector3 normalHitPoint;

    string hitEffectName = "";

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
        if (trailRenderer != null)
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
            //* 유지 시간이 지났는데 아직 안사라지고 움직이고 있다면 없애기
            if (time > disappearTime && !isdisappear)
            {
                isdisappear = true;
                DisappearBullet(false);
            }

            //* 현재 Bullt이 이동한 거리
            float curBulletDistance = Vector3.Distance(curOriginPos, this.gameObject.transform.position);

            //처음 시작할 때의 Ray 체크 무시 안되도록
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
                DisappearBullet(true);
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
        RaycastHit shortHit;
        hits = Physics.RaycastAll(curOriginPos, targetDir, range);

        float shortDist = 1000f;
        bool isPass = false;
        if (hits.Length != 0)
        {
            shortHit = hits[0];

            foreach (RaycastHit hit in hits)
            {
                isPass = false;
                if (hit.collider.tag == "Monster")
                {
                    //자기 자신인지확인
                    Transform _transform = FindTopParent(hit.collider.gameObject.GetComponent<Transform>());
                    if (_transform.name == monster.gameObject.name)
                    {
                        isPass = true;
                    }
                }

                if (hit.collider.name != this.gameObject.name && !isPass)
                {
                    Vector3 hitPoint = hit.point;

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
            {
                targetDistance = shortDist;
                hitPoint = shortHit.point;
                normalHitPoint = shortHit.normal;
            }
        }

    }

    public void SetInfo(Vector3 _targetDir, string _hitEffectName = "")
    {
        targetDir = _targetDir;
        hitEffectName = _hitEffectName;
    }

    public void AttackPlayer()
    {
        if (isFallDown)
        {
            monster.OnHit_FallDown(3, 50, OnHitPlayerEffect);
        }
        else
            monster.OnHit(3, OnHitPlayerEffect);
    }

    private void DisappearBullet(bool isHitDisappear = false)
    {
        if (isReset)
        {
            //사라지기 전 이펙트
            if (isHitDisappear)
            {
                //*보스전일때만
                if (Vector3.Distance(hitPoint, playerController.gameObject.transform.position) < 4)
                {
                    //가까운곳에 떨어졌을때. 
                    GameManager.Instance.cameraShake.ShakeCamera(0.2f, 0.75f, 0.75f);
                }
                if (hitEffectName != "")
                {
                    Quaternion rot = Quaternion.FromToRotation(Vector3.up, normalHitPoint);
                    Vector3 pos = hitPoint;

                    Effect effect = GameManager.Instance.objectPooling.ShowEffect(hitEffectName);
                    effect.gameObject.transform.position = pos;
                    effect.gameObject.transform.rotation = rot;
                }

            }

            //풀링
            rigid.velocity = Vector3.zero;

            GameManager.Instance.objectPooling.AddProjectilePool(projectileName, this.gameObject);
            OnHitPlayerEffect = null;
            isReset = false;
        }
        else
        {
            Debug.LogError("HI");
        }
    }

    Transform FindTopParent(Transform childTransform)
    {
        Transform topParent = childTransform;

        while (topParent.parent != null)
        {
            topParent = topParent.parent;
        }

        return topParent;
    }


    public void FinishEffect()
    {

    }

}
