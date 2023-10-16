using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcAreaController : MonoBehaviour
{
    DialogueManager theDM; 
    GameInfo gameInfo;

    GameObject interObject;
    Item interaction_Item; 
    private void Start()
    {
        theDM = FindObjectOfType<DialogueManager>();
        gameInfo=GetComponent<GameInfo>();
        interObject = gameObject;
        interaction_Item = gameObject.GetComponent<Item>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player") //플레이어가 들어가면 대화창 활성화
        {
            //theDM.ShowDialogue(gameObject.transform.GetComponent<InterectionEvent>().GetDialogue
            
        }
    }

    public void SettingUI(bool p_flag)
    {
        //대화창 비활성화 false => 다른 ui, 커서등 비활성화
        //대화창 활성화 true => 다른 ui, 커서등 활성화

        //나중에 코드 추가해야함. 
    }

   
}
