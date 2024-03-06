using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillIconTrans : MonoBehaviour
{
    public Image bow;
    public Image sword;

    public PlayerController playerController;

    void Start()
    {
        playerController = GameManager.Instance.gameData.player.GetComponent<PlayerController>();
        bow.gameObject.SetActive(true);
        sword.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        if (playerController.returnIsBowMode()) //활 사용중
        {
            bow.gameObject.SetActive(false);
            sword.gameObject.SetActive(true);
        }
        else
        {
            bow.gameObject.SetActive(true);
            sword.gameObject.SetActive(false);
        }
    }
}
