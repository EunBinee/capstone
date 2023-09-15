using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class ObjectPooling
{
    public int PoolCount = 50;
    public Dictionary<string, Effect> effectPrefabs;
    public Dictionary<string, List<Effect>> effectPools;
    public List<Effect> loofEffectPools;


    public void InitPooling()
    {
        //오브젝트 풀링.
        effectPrefabs = new Dictionary<string, Effect>();
        effectPools = new Dictionary<string, List<Effect>>();
        loofEffectPools = new List<Effect>();

        effectPrefabs.Clear();
        effectPools.Clear();
        loofEffectPools.Clear();
    }

    public Effect ShowEffect(string effectName, Transform parent = null) //effectName은 경로의 역할도 함
    {
        Effect curEffect = null;

        //프리펩 찾기
        if (effectPrefabs.ContainsKey(effectName))
        {
            curEffect = effectPrefabs[effectName];
            //curEffect = UnityEngine.Object.Instantiate(curEffect);
        }
        else
        {
            curEffect = Resources.Load<Effect>(effectName);
            if (curEffect != null)
            {
                effectPrefabs.Add(effectName, curEffect);
                //curEffect = UnityEngine.Object.Instantiate(curEffect);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("ObjectPooling 프리펩 없음. 오류.");
#endif
            }
        }

        if (curEffect != null)
        {
            //오브젝트 풀에 
            if (effectPools.ContainsKey(effectName))
            {
                if (effectPools[effectName].Count > 0)
                {
                    curEffect = effectPools[effectName][0];
                    effectPools[effectName].RemoveAt(0);
                }
                else
                {
                    curEffect = UnityEngine.Object.Instantiate(curEffect);
                }
            }
            else
            {
                effectPools.Add(effectName, new List<Effect>());
                curEffect = UnityEngine.Object.Instantiate(curEffect);
            }

            Transform effecParent = (parent == null) ? GameManager.Instance.transform : parent;
            curEffect.gameObject.transform.SetParent(effecParent);
            curEffect.ShowEffect();
            curEffect.callBack += () =>
            {
                AddEffectPool(effectName, curEffect);
            };
        }

        return curEffect;
    }

    public void AddEffectPool(string effectName, Effect effect)
    {
        if (effectPools[effectName].Count >= PoolCount)
        {
            //만약 풀이 가득 찼다면, 그냥 삭제.
            UnityEngine.Object.Destroy(effect.gameObject);
        }
        else
        {
            effect.gameObject.transform.SetParent(GameManager.Instance.transform);
            effectPools[effectName].Add(effect);
        }
    }
}
