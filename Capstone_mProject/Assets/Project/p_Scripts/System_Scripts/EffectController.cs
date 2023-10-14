using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectController : MonoBehaviour
{
    [Header("이펙트 사이즈 조절 크기")]
    public float size = 1;
    ParticleSystem[] particleSystemList;
    void Start()
    {
        particleSystemList = GetComponentsInChildren<ParticleSystem>(true);
        ChangeSize();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeSize()
    {
        for (int i = 0; i < particleSystemList.Length; i++)
        {
            float originStartSize = particleSystemList[i].startSize;

            float curStartSize = originStartSize * size;
            curStartSize = (float)Math.Round(curStartSize, 1);
            particleSystemList[i].startSize = curStartSize;
        }
    }
}
