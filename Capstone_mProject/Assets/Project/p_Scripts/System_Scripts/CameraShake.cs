using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private Transform cam;

    bool isShaking = false;

    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    public void ShakeCamera(float duration, float shakeSpeed, float shakeAmount)
    {
        if (!isShaking)
        {
            isShaking = true;

            StartCoroutine(Shake(duration, shakeSpeed, shakeAmount));
        }
    }

    IEnumerator Shake(float duration, float shakeSpeed, float shakeAmount)
    {
        Vector3 originPos = cam.localPosition;
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
    }
}
