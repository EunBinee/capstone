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
            if (particleSystemList[i].main.startSize3D)
            {
                Debug.Log("변경");
                ParticleSystem.MainModule mainModule = particleSystemList[i].main;
                mainModule.startSizeXMultiplier = (float)Math.Round((mainModule.startSizeXMultiplier * size), 1);
                mainModule.startSizeYMultiplier = (float)Math.Round((mainModule.startSizeYMultiplier * size), 1);
                mainModule.startSizeZMultiplier = (float)Math.Round((mainModule.startSizeZMultiplier * size), 1);
            }
            else
            {
                Debug.Log("변경");
                ParticleSystem.MainModule mainModule = particleSystemList[i].main;
                float originStartSize = mainModule.startSizeMultiplier;

                float curStartSize = originStartSize * size;
                curStartSize = (float)Math.Round(curStartSize, 1);
                mainModule.startSizeMultiplier = curStartSize;
            }
        }
    }
}
