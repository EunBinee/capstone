using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundObject : MonoBehaviour
{
    //오브젝트를 공격했는지
    public bool attackSoundObj = false;
    //충돌위치
    public Vector3 collisionPos;

    private void Start() {
        attackSoundObj = false;
    }
   private void OnTriggerEnter(Collider other)
   {
        attackSoundObj = true;
        collisionPos = other.transform.position; // 충돌한 위치 저장
        SoundManager.Instance.Play_SfxSound(SoundManager.SfxSound.SoundObject);
        Debug.Log("소리 오브젝트");
   }
}
