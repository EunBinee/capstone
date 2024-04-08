using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reasoning
{
    public int reasoningID; //추리ID

    public string question; //추리 질문
    public List<string> reasoningChoice; //선택지
    public List<string> reasoningDialogue; //추리 후 대사 
    public string answer;

     public string firstOption; //1번 선택지 Text
    public string secondOption; //2번 선택지 Text
    public string thirdOption; //3번 선택지 Text
    public string fourthOption; //4번 선택지 Text
    public string fifthOption; //5번 선택지 Text

    public string firstOptDialogue; //1번째 선택지를 선택했을 경우, 그다음 대사 단락 번호
    public string secondOptDialogue;//2번째 선택지를 선택했을 경우, 그다음 대사 단락 번호
    public string thirdOptDialogue;//3번째 선택지를 선택했을 경우, 그다음 대사 단락 번호
    public string fourthOptDialogue;//4번째 선택지를 선택했을 경우, 그다음 대사 단락 번호
    public string fifthOptDialogue;//5번째 선택지를 선택했을 경우, 그다음 대사 단락 번호
}
