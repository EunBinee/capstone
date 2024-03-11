using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NIcknameInput : MonoBehaviour
{
    public TMP_InputField nicknameInput; //닉네임 입력 inputfield
    public GameObject object_Nickname;
    private string nickname = null;

    private void Awake()
    {
        nickname = nicknameInput.GetComponent<TMP_InputField>().text;
    }

    private void Update()
    {
        //! 여긴 나중에 넣을 곳 수정만 하면 됨. 
        if (Input.GetKeyDown(KeyCode.O))
        {
            object_Nickname.SetActive(true);
            DialogueManager.instance.player_InteractingTrue();
            nicknameInput.Select();
        }

        //키보드 
        if (nickname.Length > 0 && Input.GetKeyDown(KeyCode.Return))
        {
            InputNickname();
        }

    }

    //마우스
    public void InputNickname()
    {
        nickname = nicknameInput.text;
        GameManager.Instance.gameInfo.Nickname = nickname;
        object_Nickname.SetActive(false);
        DialogueManager.instance.player_InteractingFalse();
        Debug.Log(nickname);
    }

}
