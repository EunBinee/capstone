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
    private int eventNum;


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
    public int LineNum
    {
        get { return eventNum; }
        set { eventNum = value; }
    }
}
