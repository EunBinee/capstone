using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public CameraController cameraController;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
