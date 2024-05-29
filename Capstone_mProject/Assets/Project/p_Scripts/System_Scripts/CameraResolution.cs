using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Reference https://giseung.tistory.com/19
public class CameraResolution : MonoBehaviour
{
    public int setWidth = 1920; // 사용자 설정 너비
    public int setHeight = 1080; // 사용자 설정 높이

    CanvasScaler canvasScaler;

    private void Awake()
    {
        canvasScaler = GetComponent<CanvasScaler>();
        SetResolution(); // 초기에 게임 해상도 고정
        if (canvasScaler != null)
        {
            FixScales();
        }
        
    }

    /* 해상도 설정하는 함수 */
    public void SetResolution()
    {
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        if (canvasScaler == null)
            canvasScaler = GetComponent<CanvasScaler>();

        int deviceWidth = Screen.width; // 기기 너비 저장
        int deviceHeight = Screen.height; // 기기 높이 저장

                //Default 해상도 비율
        float fixedAspectRatio = setWidth / setHeight;

        //현재 해상도의 비율
        float currentAspectRatio = (float)Screen.width / (float)Screen.height;

        //Debug.Log($"{currentAspectRatio}            {fixedAspectRatio}");
        //현재 해상도 가로 비율이 더 길 경우
        // if (currentAspectRatio > fixedAspectRatio) canvasScaler.matchWidthOrHeight = 1;
        // //현재 해상도의 세로 비율이 더 길 경우
        // else if (currentAspectRatio < fixedAspectRatio) canvasScaler.matchWidthOrHeight = 0;


        Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true); // SetResolution 함수 제대로 사용하기

        if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
        {
            float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
        }
        else // 게임의 해상도 비가 더 큰 경우
        {
            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
        }
    }
    public void FixScales()
    {
        foreach (Transform child in transform)
        {
            FixScaleRecursive(child);
        }
    }

    private void FixScaleRecursive(Transform obj)
    {
        obj.localScale = Vector3.one; // scale을 1로 설정
        // foreach (Transform child in obj)
        // {
        //     FixScaleRecursive(child); // 자식들에 대해 재귀적으로 호출
        // }
    }




}