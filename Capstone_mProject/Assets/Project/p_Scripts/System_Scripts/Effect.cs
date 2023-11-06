using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    //이펙트 실행
    //이펙트 끄기
    //OnEnable() //풀링할거라서 
    private ParticleSystem mEffect;
    public event Action callBack = null;
    public Action finishAction; //이펙트가 끝났을때 필요한 것

    public void ShowEffect()
    {
        mEffect = GetComponent<ParticleSystem>();
        mEffect.gameObject.SetActive(true);
        mEffect.Play();
    }

    public void StopEffect()
    {
        mEffect.Stop();
    }

    private void Update()
    {
        if (!mEffect.IsAlive())
        {
            //이펙트가 종료.

            mEffect.gameObject.SetActive(false);
            finishAction?.Invoke();
            finishAction = null;
            callBack?.Invoke();
        }
    }
}
