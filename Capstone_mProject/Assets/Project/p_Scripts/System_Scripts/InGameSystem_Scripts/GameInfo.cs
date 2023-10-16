using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo : MonoBehaviour
{
    //[SerializeField]
    //private int endingNum; //엔딩 번호

    //[SerializeField]
    //private int eventNum; //이벤트 번호

    //public int EndingNum
    //{
    //    get { return endingNum; }
    //    set { endingNum = value; }
    //}
    //public int EventNum
    //{
    //    get { return eventNum; }
    //    set { eventNum = value; }
    //}

    //2차수정========================================================
    //게임 엔딩
    [SerializeField]
    private int endingNum;

    //이벤트 번호
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
    //public int LineNum
    //{
    //    get { return eventNum; }
    //    set { eventNum = value; }
    //}
}
