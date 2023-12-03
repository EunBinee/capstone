using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackCheck : MonoBehaviour
{
    public bool isEnable = false;
    [SerializeField] private Monster monster;
    private Rigidbody rigid;

    public PlayerController _playerController;// = new PlayerController();
    public PlayerController P_Controller => _playerController;
    private CurrentValue P_Value => _playerController._currentValue;
    private CurrentState P_States => _playerController._currentState;

    private GameObject player;
    private bool isArrow = false;
    private bool goShoot = false;
    Vector3 dir = Vector3.zero;
    Vector3 arrpos = Vector3.zero;
    Quaternion arrrot = Quaternion.identity;
    Transform nowArrow;

    bool attackEnemy = false;

    void Start()
    {
        player = GameManager.Instance.gameData.player;
        // Transform currentTransform = transform;
        // while (currentTransform.parent != null)
        // {
        //     currentTransform = currentTransform.parent;
        // }
        _playerController = player.GetComponent<PlayerController>();
        rigid = GetComponent<Rigidbody>();
        if (gameObject.tag == "Arrow")  //* 화살인지 확인을 해
        {
            isArrow = true;
        }
        //currentTransform.GetComponent<PlayerController>();
    }
    void FixedUpdate()
    {
        //! 발사 후에 위치랑 회전이 계속 저장되서 화살이 유도탄마냥 움직임

        if (isArrow && !goShoot)
        {
            //nowArrow.position = P_Controller.shootPoint.position;   //* 위치 방향 저장
            //nowArrow.rotation = player.transform.rotation;
            this.transform.position = P_Controller.shootPoint.position;
            this.transform.rotation = player.transform.rotation;
            if (!P_Controller.returnIsAim())    //* isAim이 거짓이 되면
            {
                //* 키네매틱 끄기
                GetComponent<Rigidbody>().isKinematic = false;
                //Vector3 dir = GameManager.Instance.gameData.player.transform.forward;
                if (dir == Vector3.zero)    //* 방향 지정
                {
                    //dir = P_Controller._playerFollowCamera.cameraObj.transform.forward;
                    dir = P_Controller.AimmingCam.transform.forward;
                }
                //transform.position += dir * 0.1f;
                rigid.velocity = dir.normalized * 40f; ; //* 발사
                goShoot = true;
            }
        }
    }

    private void isBouncingToFalse()
    {
        P_States.isBouncing = false;
        P_Value.maxHitScale = 1.2f;
        P_Value.minHitScale = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isEnable)
        {
            if (other.gameObject.tag == "Monster")
            {
                monster = other.GetComponentInParent<Monster>();

                if (monster.monsterPattern.GetCurMonsterState() != MonsterPattern.MonsterState.Death)
                {
                    //Debug.Log($"hit monster ,  curState  {monster.monsterPattern.GetCurMonsterState()}");
                    if (monster != null && !P_States.hadAttack)
                    {
                        P_States.hadAttack = true;
                        // 충돌한 객체의 Transform을 얻기
                        Transform collidedTransform = other.transform;
                        // 충돌 지점의 좌표를 얻기
                        Vector3 collisionPoint = other.ClosestPoint(transform.position);
                        Quaternion otherQuaternion = Quaternion.FromToRotation(Vector3.up, collisionPoint.normalized);
                        playerHitMonster(collisionPoint, otherQuaternion);
                    }
                    else if (monster != null && P_States.hadAttack)
                    {
                        //이미 한번 때린 상태
                    }
                    else
                        Debug.LogError("몬스터 : null");
                }
            }
            else
            {

            }
        }

    }
    private void playerHitMonster(Vector3 collisionPoint, Quaternion otherQuaternion)
    {
        //TODO: 나중에 연산식 사용.
        monster.GetDamage(700, collisionPoint, otherQuaternion);

        P_Value.nowEnemy = monster.gameObject;  //* 몬스터 객체 저장
        P_Value.curHitTime = Time.time; //* 현재 시간 저장

        P_Controller.CheckHitTime();
        P_Value.hits = P_Value.hits + 1;    //* 히트 수 증가

        P_States.isBouncing = true;     //* 히트 UI 출력효과
        Invoke("isBouncingToFalse", 0.3f);  //* 히트 UI 출력효과 초기화
    }

    private void ArrowRay(float curArrowDistance)
    {
        float range = 100f;
        RaycastHit[] hits;
        hits = Physics.RaycastAll(this.transform.position, this.transform.forward, range);

        float shortDist = 1000f;
        RaycastHit shortHit = hits[0];
        RaycastHit m_Hit;
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.name != this.gameObject.name)
            {
                //자기 자신은 패스
                float distance = hit.distance;
                if (curArrowDistance < distance && shortDist > distance)
                {
                    shortHit = hit;
                    shortDist = distance;

                    if (hit.collider.tag == "Monster")
                    {
                        attackEnemy = true;
                        m_Hit = hit;
                        Vector3 collisionPoint = hit.point;
                        Quaternion otherQuaternion = Quaternion.FromToRotation(Vector3.up, hit.normal);

                        playerHitMonster(collisionPoint, otherQuaternion);
                    }
                    else
                        attackEnemy = false;
                }
            }
        }

        // if (shortDist != 1000)
        //     targetDistance = shortDist;
    }
    private void OnCollisionEnter(Collision other)
    {

        if (isArrow)
        {
            //! 사용 금지 Bullet.cs에 BulletRay()참고해서 
            //! 충돌 처리 다시..하기.....
            this.gameObject.SetActive(false);
        }
    }
}
