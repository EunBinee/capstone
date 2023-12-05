using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private Transform cam;

    bool isShaking = false;
    Vector3 originPos;
    Coroutine shake_co = null;

    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    public void ShakeCamera(float duration, float shakeSpeed, float shakeAmount)
    {
        cam = GameManager.instance.cameraController.cameraObj.transform;

        if (isShaking)
        {
            if (shake_co != null)
            {
                StopCoroutine(shake_co);
                cam.localPosition = originPos;
            }
        }
        isShaking = true;
        shake_co = StartCoroutine(Shake(duration, shakeSpeed, shakeAmount));
    }

    IEnumerator Shake(float duration, float shakeSpeed, float shakeAmount)
    {
        originPos = cam.localPosition;

        float time = 0;
        while (time < duration)
        {
            Vector3 randomPos = originPos + Random.insideUnitSphere * shakeAmount;
            cam.localPosition = Vector3.Lerp(cam.localPosition, randomPos, Time.deltaTime * shakeSpeed);
            yield return null;
            time += Time.deltaTime;
        }
        cam.localPosition = originPos;

        isShaking = false;
        shake_co = null;
    }
}
