using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerAttackCheck : MonoBehaviour
{
    public bool isEnable = false;
    public Monster monster;
    private Rigidbody rigid;

    public PlayerController _playerController;
    public PlayerMovement _playerMovement;
    private PlayerController P_Controller => _playerController;
    private CurrentValue P_Value => _playerController._currentValue;
    private CurrentState P_States => _playerController._currentState;
    private PlayerSkills P_Skills => P_Controller.P_Skills;
    private PlayerProjectile P_Projectile => _playerController._playerProjectile;

    // HashSet을 사용하여 이미 처리된 몬스터를 추적합니다.
    HashSet<GameObject> seenMonsters = new HashSet<GameObject>();

    private GameObject player;
    private bool isArrow = false;
    private bool isBullet = false;
    private bool goShoot = false;
    private bool incoArrow = false;
    Vector3 dir = Vector3.zero;
    Transform nowArrow;
    public float deltaShootTime = 0.0f;

    public SoundObject soundObject;

    //데미지
    public double damageValue = 0;



    void Start()
    {
        player = GameManager.Instance.gameData.player;
        _playerController = player.GetComponent<PlayerController>();
        _playerMovement = player.GetComponent<PlayerMovement>();
        rigid = GetComponent<Rigidbody>();

        if (this.gameObject.tag == "Arrow")  //* 화살인지 확인을 해
        {
            isArrow = true;
        }
        if (this.gameObject.tag == "Bullet")  //* 총알인지 확인을 해
        {
            isBullet = true;
        }
        _playerController.hitMonsters.Clear();


    }
    void FixedUpdate()
    {
        if (_playerController.hitMonsters.Count > 1)
            checkMon();

        if (isArrow || isBullet)    // 활이거나 총알이면
        {
            if (!goShoot && (P_States.isAim || P_States.startAim || P_States.isShortArrow))
            {
                transform.position = P_Controller.shootPoint.position;
                transform.rotation = P_Controller.shootPoint.rotation;

                if (!incoArrow)
                    StartCoroutine(Arrowing());
            }
        }
    }
    public void StrongArrowEffect_co()
    {
        StartCoroutine(StrongArrowEffect());
    }
    IEnumerator StrongArrowEffect()
    {
        Effect effect = GameManager.Instance.objectPooling.ShowEffect("Bow_Attack_ChargingLoop");
        effect.transform.rotation = Quaternion.LookRotation(this.transform.forward);
        while (!goShoot || !P_States.colliderHit)
        {
            effect.gameObject.transform.position = this.gameObject.transform.position; // 오브젝트에 이펙트 부착

            yield return null;
        }

        effect.StopEffect();
        yield return null;
    }
    IEnumerator Arrowing()
    {
        incoArrow = true;
        dir = Vector3.zero;
        yield return new WaitUntil(() => (!P_States.isAim || P_States.isShortArrow || !P_States.isClickDown) || P_States.isGunMode);  //* isAim이 거짓이 되거나 단타라면

        if (!goShoot)
        {
            _playerMovement.playerArrowList.Add(this);

            transform.position = P_Controller.shootPoint.position;
            transform.rotation = P_Controller.shootPoint.rotation;
            //* 키네매틱 끄기
            rigid.isKinematic = false;
            if (dir == Vector3.zero)    //* 방향 지정
            {
                if (!P_States.isShortArrow)
                    dir = GameManager.Instance.gameData.cameraObj.transform.forward;
                else
                    dir = player.transform.forward;
            }
            if (isArrow) //* 발사
                rigid.velocity = dir.normalized * (P_States.isShortArrow ? 40f : 88f);
            else if (isBullet)
            {
                PlayerBulletRay(); // 레이 쏘고 데미지 계산 
            }
            //P_States.isShortArrow = false;
            goShoot = true;
            //P_States.isShortArrow = false;
        }
        while (!(P_States.colliderHit == true || P_States.hadAttack == true || deltaShootTime >= 4.0f))
        {
            deltaShootTime = deltaShootTime + Time.deltaTime;
            if (isArrow) ArrowRay();
            yield return null;
        }
        //yield return new WaitUntil(() => P_States.colliderHit == true || P_States.hadAttack == true || shootDeltaTime() >= 5.0f);
        resetArrow();
        yield return null;
    }

    public void resetArrow()
    {
        incoArrow = false;
        goShoot = false;

        _playerMovement.playerArrowList.Remove(this);

        P_States.hadAttack = false;
        P_States.colliderHit = false;
        P_States.isShortArrow = false;
        //P_States.isClickDown = false;
        P_Value.aimClickDown = 0;
        deltaShootTime = 0.0f;
        GetComponent<Rigidbody>().isKinematic = true;

        if (P_States.isBowMode) P_Projectile.AddArrowPool(this.gameObject);
        else if (P_States.isGunMode) P_Projectile.AddBulletPool(this.gameObject);

        this.gameObject.SetActive(false);

    }
    IEnumerator DelayAfterAction()
    {
        yield return new WaitForSeconds(10.0f); // 1초의 딜레이 추가

        // 여기에 1초 딜레이 이후에 실행할 코드를 넣으세요.
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
            if (other.gameObject.CompareTag("Monster"))
            {
                monster = other.GetComponentInParent<Monster>();

                if (monster == null)
                {
                    Debug.LogError("몬스터 : null");
                    return;
                }

                if (monster.monsterPattern.GetCurMonsterState() != MonsterPattern.MonsterState.Death)
                {
                    //attackShield = false;
                    _playerController.hitMonsters.Add(other.gameObject);

                    //Debug.Log($"hit monster ,  curState  {monster.monsterPattern.GetCurMonsterState()}");
                    if (P_States.hadAttack == false || P_States.notSameMonster)
                    {
                        // 충돌한 객체의 Transform을 얻기
                        Transform collidedTransform = other.transform;
                        // 충돌 지점의 좌표를 얻기
                        Vector3 collisionPoint = other.ClosestPoint(transform.position);
                        Quaternion otherQuaternion = Quaternion.FromToRotation(Vector3.up, collisionPoint.normalized);

                        if (monster.monsterData.isShieldMonster && monster.monsterPattern.isShield)
                        {
                            monster.monsterPattern.isShield = false;
                            playerHitShield(collisionPoint, otherQuaternion);
                        }
                        else
                        {
                            playerHitMonster(collisionPoint, otherQuaternion, monster);
                        }
                        //사운드
                        SoundManager.Instance.Play_PlayerSound(SoundManager.PlayerSound.Hit, false);
                    }

                }
                else
                {
                    //Debug.Log("[attack test]몬스터 상태 : " + monster.monsterPattern.GetCurMonsterState());
                }
            }
            else
            {
                //Debug.Log("[attack test]몬스터 아님 : " + other.gameObject.tag);  
            }
        }
    }
    // private bool isShield = false;
    public void checkMon()
    {
        // 리스트를 거꾸로 순회합니다. 이렇게 하는 이유는 리스트를 순회하면서 항목을 제거할 때 문제가 발생하지 않도록 하기 위함입니다.
        for (int i = _playerController.hitMonsters.Count - 1; i >= 0; i--)
        {
            GameObject curmon = _playerController.hitMonsters[i];

            if (seenMonsters.Contains(curmon))
            {
                //P_States.notSameMonster = false;
                _playerController.hitMonsters.RemoveAt(i);
            }
            else
            {
                // 처음 보는 몬스터이면 HashSet에 추가합니다.
                seenMonsters.Add(curmon);
                P_States.notSameMonster = true;
                //P_States.hasAttackSameMonster = true;
            }
        }
    }

    public bool playerHitMonster(Vector3 collisionPoint, Quaternion otherQuaternion, Monster monster, bool HitWeakness = false)
    {
        if (!monster.monsterPattern.noAttack)
        {
            //TODO: 나중에 연산식 사용.
            // if (isArrow)                    //* 활
            // {
            //     if (P_States.isStrongArrow) //* 예스 차징
            //     {
            //         if (HitWeakness && monster.monsterData.useWeakness)
            //         {
            //             damageValue = monster.monsterData.MaxHP * monster.monsterData.weaknessDamageRate;
            //         }
            //         else
            //             damageValue = 550;
            //         //P_States.isStrongArrow = false;
            //     }
            //     else                        //* 노 차징
            //     {
            //         damageValue = 250;
            //     }
            // }
            if (isBullet)              //* 총
            {
                // damageValue = 3;
                if (HitWeakness && monster.monsterData.useWeakness && monster.monsterData.weaknessHP_ == 0) //약점공격 
                {
                    // damageValue = monster.monsterData.MaxHP * monster.monsterData.weaknessDamageRate;
                    //Debug.Log(damageValue);
                }
                // else if (monster.monsterData.useWeakness)
                //     damageValue = 0;
                else damageValue = 10;
            }
            else                            //* 검
            {
                damageValue = 350;

                if (P_Value.hits % 5 != 0)
                {
                    GameManager.instance.damageCalculator.damageExpression = (P_Value.index + 1 > 3) ? "A+D" : "A+B"; // 350+370 : 350+125
                    GameManager.instance.damageCalculator.CalculateAndPrint();
                    damageValue = GameManager.instance.damageCalculator.result;
                }
                else if (P_Value.hits % 5 == 0 && P_Value.hits != 0)
                {
                    GameManager.instance.damageCalculator.damageExpression = (P_Value.index + 1 > 3) ? "A+D" : "A+C"; // 350+370 : 350+500
                    GameManager.instance.damageCalculator.CalculateAndPrint();
                    damageValue = GameManager.instance.damageCalculator.result;
                }

            }


            // if (isBullet && !monster.monsterData.useWeakness)
            //         damageValue = 3;

            // 20% 확률로 데미지에 100을 추가
            if (Random.value < 0.25f)
            {
                damageValue *= 1.5;
            }

            //P_Value.damageOnHits = P_Value.damageOnHits + (int)damageValue;

            monster.GetDamage(damageValue, collisionPoint, otherQuaternion, HitWeakness);

            if (!P_States.isBowMode && !P_States.isGunMode)
            {
                _playerController.playAttackEffect("Attack_Combo_Hit"); //* 히트 이펙트 출력
            }

            P_Value.nowEnemy = monster.gameObject;  //* 몬스터 객체 저장
            P_Value.curHitTime = Time.time; //* 현재 시간 저장

            P_Controller.CheckHitTime();
            //P_Value.hits = P_Value.hits + 1;    //* 히트 수 증가
            P_States.hadAttack = true;
            P_States.notSameMonster = false;

            //P_States.isBouncing = true;     //* 히트 UI 출력효과
            if (P_States.isBouncing) Invoke("isBouncingToFalse", 0.3f);  //* 히트 UI 출력효과 초기화

            if (isBullet) this.gameObject.SetActive(false);

            return true;
        }
        else
        {
            if (isBullet) this.gameObject.SetActive(false);
            return false;
        }
    }

    public void playerHitShield(Vector3 collisionPoint, Quaternion otherQuaternion)
    {
        int damageValue;

        GameManager.instance.damageCalculator.damageExpression = "A+B";
        GameManager.instance.damageCalculator.CalculateAndPrint();
        damageValue = 0;

        monster.GetDamage(damageValue, collisionPoint, otherQuaternion);
        _playerController.playAttackEffect("Attack_Combo_Hit"); //* 히트 이펙트 출력
    }

    private void ArrowRay()
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(this.transform.position, this.transform.forward, Mathf.Infinity);

        float shortDist = 1000f;

        if (hits.Length == 0) return;   // 레이 히트 없으면 바로 리턴

        RaycastHit shortHit = hits[0];
        //RaycastHit m_Hit;

        //todo: hits 들어온 것들 중 제일 거리가 제일 짧은 것만 체크
        float hitsDist = hits[0].distance;
        int shortIndex = 0;
        for (int i = 1; i < hits.Length; i++)
        {
            if (hits[i].point != null)
            {
                if (hits[i].distance < hitsDist)
                {
                    hitsDist = hits[i].distance;
                    shortIndex = i;
                }
            }
        }

        if (hits[shortIndex].collider.name != this.gameObject.name)
        {
            RaycastHit hit = hits[shortIndex];
            //자기 자신은 패스
            //float distance = hitsDist;
            if (shortDist > hitsDist)    //범위 내 라면
            {
                shortHit = hit;
                shortDist = hitsDist;

                if (hit.collider.CompareTag("SoundObject"))
                {
                    P_States.colliderHit = true;
                    soundObject = hit.collider.gameObject.GetComponent<SoundObject>();
                    //Debug.Log(soundObject);
                    soundObject.attackSoundObj = true;
                    soundObject.collisionPos = hit.transform.position;
                }

                if (hit.collider.tag == "BossWeakness")
                {
                    P_States.colliderHit = true;
                    //* 보스 약점
                    BossWeakness bossWeakness = hit.collider.GetComponent<BossWeakness>();
                    monster = bossWeakness.m_monster;
                    if (monster != null)
                    {
                        //Debug.Log($"약점 맞음! 몬스터 : {monster.gameObject.name}");
                    }

                    if (!bossWeakness.destroy_BossWeakness)
                    {
                        P_States.hadAttack = true;

                        Vector3 collisionPoint = hit.point;
                        Quaternion otherQuaternion = Quaternion.FromToRotation(Vector3.up, hit.normal);

                        bool successfulAttack = playerHitMonster(collisionPoint, otherQuaternion, monster, true);
                        if (successfulAttack)
                        {

                            bossWeakness.WeaknessGetDamage(shortHit.normal, shortHit.point);
                        }

                    }
                }
                else if (hit.collider.tag == "Monster")
                {
                    P_States.colliderHit = true;
                    //Debug.Log("[arrow test] arrow hit");
                    monster = hit.collider.GetComponentInParent<Monster>();
                    if (monster == null)
                    {
                        Debug.LogError("몬스터 : null");
                        return;
                    }
                    if (monster.monsterPattern.GetCurMonsterState() != MonsterPattern.MonsterState.Death
                        && P_States.hadAttack == false)
                    {
                        //attackEnemy = true;
                        P_States.hadAttack = true;
                        //m_Hit = hit;
                        Vector3 collisionPoint = hit.point;
                        Quaternion otherQuaternion = Quaternion.FromToRotation(Vector3.up, hit.normal);


                        playerHitMonster(collisionPoint, otherQuaternion, monster);

                    }
                }
                if (hit.collider.tag == "Shield")
                {
                    monster = hit.collider.GetComponentInParent<Monster>();

                    if (monster.monsterData.isShieldMonster && monster.monsterPattern.isShield)
                    {
                        Vector3 collisionPoint = hit.point;
                        Quaternion otherQuaternion = Quaternion.FromToRotation(Vector3.up, hit.normal);

                        monster.monsterPattern.isShield = false;
                        playerHitShield(collisionPoint, otherQuaternion);
                        //monster.monsterPattern.isShield = false;
                    }
                }
            }
        }

    }

    public void PlayerBulletRay()
    {
        // 화면 중앙 좌표 계산 (스크린 해상도 기준)
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        // 화면 중앙에서 카메라 방향으로 레이 생성
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);

        // 레이캐스트 결과 저장할 변수
        RaycastHit hit;

        int layerMask = (1 << LayerMask.NameToLayer("Player"));  // Everything에서 Player 레이어만 제외하고 충돌 체크함
        layerMask = ~layerMask;

        //hit = Physics.RaycastAll(ray, rayDistance, LayerMask.GetMask("Monster"));
        bool currentIsHit = Physics.Raycast(ray, out hit, 100f, layerMask);

        if (!currentIsHit) return;

        P_Skills.GetBulletDir(hit.point - player.transform.position);
        transform.Translate(P_Skills.bulletDir * 10f);

        //if (hit.collider.CompareTag("SoundObject"))
        //{
        //    P_States.colliderHit = true;
        //    soundObject = hit.collider.gameObject.GetComponent<SoundObject>();
        //    //Debug.Log(soundObject);
        //    soundObject.attackSoundObj = true;
        //    soundObject.collisionPos = hit.transform.position;
        //
        if (hit.collider.tag == "BossWeakness")
        {
            P_States.colliderHit = true;
            //* 보스 약점
            BossWeakness bossWeakness = hit.collider.GetComponent<BossWeakness>();
            monster = bossWeakness.m_monster;
            if (monster != null)
            {
                //Debug.Log($"약점 맞음! 몬스터 : {monster.gameObject.name}");
            }

            if (!bossWeakness.destroy_BossWeakness)
            {
                P_States.hadAttack = true;

                Vector3 collisionPoint = hit.point;
                Quaternion otherQuaternion = Quaternion.FromToRotation(Vector3.up, hit.normal);
                player.GetComponent<PlayerSkills>().GetBulletDir(collisionPoint - player.transform.position);

                bool successfulAttack = playerHitMonster(collisionPoint, otherQuaternion, monster, true);
                if (successfulAttack)
                {

                    bossWeakness.WeaknessGetDamage(hit.normal, hit.point);
                }

            }
        }
        else if (hit.collider.tag == "Monster")
        {
            P_Controller._currentState.colliderHit = true;
            monster = hit.collider.GetComponentInParent<Monster>();

            if (monster == null)
            {
                Debug.LogError("몬스터 : null");
                return;
            }
            if (monster.monsterPattern.GetCurMonsterState() != MonsterPattern.MonsterState.Death
                && P_States.hadAttack == false)
            {
                //attackEnemy = true;
                P_States.hadAttack = true;
                //m_Hit = hit;
                Vector3 collisionPoint = hit.point;
                Quaternion otherQuaternion = Quaternion.FromToRotation(Vector3.up, hit.normal);


                playerHitMonster(collisionPoint, otherQuaternion, monster);
            }
        }
        if (hit.collider.tag == "Shield")
        {
            monster = hit.collider.GetComponentInParent<Monster>();

            if (monster.monsterData.isShieldMonster && monster.monsterPattern.isShield)
            {
                Vector3 collisionPoint = hit.point;
                Quaternion otherQuaternion = Quaternion.FromToRotation(Vector3.up, hit.normal);

                monster.monsterPattern.isShield = false;
                playerHitShield(collisionPoint, otherQuaternion);
                //monster.monsterPattern.isShield = false;
            }
        }
    }
}