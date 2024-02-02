using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Npc : MonoBehaviour
{
    public string Name; //Npc 이름
    public int id; //Npc ID

    public bool isNpc;

    //대화 
    public int dialogueNum; //단락번호, //처음은 항상 1
    public int questNum; //퀘스트 번호
    //public int lineNum;

    //ending이나 Event가 변한 경우를 체크하기 위함
    //--> 엔딩이나 이벤트가 변한 경우, DialogueNum이 다시 1이 되어야 한다.
    // 그것을 체크하기 위함.
    public int preEventNum;
    public int preEndingNum;
}
