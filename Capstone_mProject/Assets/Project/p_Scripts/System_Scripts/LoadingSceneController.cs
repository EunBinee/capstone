using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneController : MonoBehaviour
{
    public static string nextScene;
    [SerializeField] private Image progressBar;

    private void Start()
    {
        StartCoroutine(LoadScene_co());
    }

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene"); //로딩씬으로 바로 이동
    }

    IEnumerator LoadScene_co()
    {
        yield return null;
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false; //씬을 90프로만 불러오고 멈춤. 바로 다 안불러옴
        float timer = 0;

        while (!op.isDone) //씬 로드가 끝나기 전까지
        {
            yield return null;

            if (op.progress < 0.9f)
            {
                //씬로드가 90프로 이하일때
                progressBar.fillAmount = op.progress;
            }
            else
            {
                //90이상 로드 됐다면?
                //* 여기서 페이크 로딩. 
                //* 여러가지 처리하기
                timer += Time.unscaledDeltaTime; ;
                progressBar.fillAmount = Mathf.Lerp(0.9f, 1f, timer);
                if (progressBar.fillAmount >= 1.0f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }

}
