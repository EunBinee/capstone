using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameInfo : MonoBehaviour
{
    //게임 엔딩
    [SerializeField]
    private int endingNum;

    //이벤트 id
    [SerializeField]
    public int eventNum;

    //퀘스트 번호
    [SerializeField]
    private int questNum;
    //대사 단락 번호
    [SerializeField]
    private int dialogueNum;
    //플레이어 닉네임
    [SerializeField]
    private string nickname;


    public int EndingNum
    {
        get { return endingNum; }
        set { endingNum = value; }
    }

    public int EventNum
    {
        get { return eventNum; }
        set { eventNum = value; }
    }
    public int QuestNum
    {
        get { return questNum; }
        set { questNum = value; }
    }
    public int DialogueNum
    {
        get { return dialogueNum; }
        set { dialogueNum = value; }
    }
    public string Nickname
    {
        get { return nickname; }
        set { nickname = value; }

    }
}
