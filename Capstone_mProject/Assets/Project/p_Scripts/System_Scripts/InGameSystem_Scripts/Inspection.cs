using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.Reach;

public class Inspection : MonoBehaviour
{
    public GameObject inspectionPrefab;
    Button interactionBtn;
    GameObject interactionNotice;
    bool isCanInspection = false;

    public GameObject inventory;
    Inventory inventory_;
    public ItemData[] itemDataArray;
    
    private void Start() 
    {
        inspectionPrefab = CanvasManager.instance.inspectionUI;
        interactionBtn =  inspectionPrefab.transform.Find("interactionBtn").GetComponent<Button>();

        Transform interactionNoticeTransform = inspectionPrefab.transform.Find("interactionNotice");
        interactionNotice = interactionNoticeTransform.gameObject;
        //inspectionPrefab.gameObject.SetActive(false);
        interactionBtn.gameObject.SetActive(false); 
        interactionNotice.SetActive(false); 

        isCanInspection = false;

        inventory = CanvasManager.instance.inventoryUI;
        inventory_=inventory.GetComponent<Inventory>();

    }

    private void Update() {
        if(isCanInspection && Input.GetKeyDown(KeyCode.F))
        {
            OnClickButton();
        }
    
    }

    private void OnTriggerEnter(Collider other)
    {
         if (other.CompareTag("Player")) // 플레이어 태그가 있는 오브젝트가 콜라이더에 들어오면
        {
            ButtonManager button = interactionBtn.GetComponent<ButtonManager>();
            button.buttonText = "조사";
        
            interactionBtn.gameObject.SetActive(true);

            isCanInspection = true;

            //Debug.Log(isCanInspection + "들어감");
        }
    }
    private void OnTriggerExit(Collider other)
    {
         if (other.CompareTag("Player")) // 플레이어 태그가 있는 오브젝트가 콜라이더에 들어오면
        {
            interactionBtn.gameObject.SetActive(false); // interactionBtn 활성화
            isCanInspection = false;
            //Debug.Log(isCanInspection + "나감");
        }
    }
    
    public void OnClickButton()
    {
        //Debug.Log("Button Click!");
        // Debug.Log(interactionNotice.transform.GetSiblingIndex());
        // Debug.Log(inventory.transform.GetSiblingIndex());
        // interactionNotice.transform.SetSiblingIndex(inventory.transform.GetSiblingIndex() + 5);
        // Debug.Log(interactionNotice.transform.GetSiblingIndex());
        // Debug.Log(inventory.transform.GetSiblingIndex());
    
        string itemName = itemDataArray[0].Name;

        NotificationManager notification = interactionNotice.GetComponent<NotificationManager>();
        notification.notificationText=itemName+"를 획득하였습니다.";

        inventory.transform.SetSiblingIndex(10);
    
        interactionNotice.SetActive(true);
        interactionBtn.gameObject.SetActive(false);
        //StartCoroutine(UIFalse());
        notification.AnimateNotification();

        inventory_.Add(itemDataArray[0]);
        Destroy(this.gameObject);

       
    }
    
}
