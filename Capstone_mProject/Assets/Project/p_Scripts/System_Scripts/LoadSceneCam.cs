using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadSceneCam : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }


}
