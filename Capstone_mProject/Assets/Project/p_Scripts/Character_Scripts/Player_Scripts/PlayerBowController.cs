using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBowController : MonoBehaviour
{
    public PlayerController _controller;
    private PlayerController P_Controller => _controller;
    private CurrentState P_States => P_Controller._currentState;

    public Animator animator;

    void Start()
    {
        _controller = GameManager.instance.gameData.player.GetComponent<PlayerController>();

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        animator.SetBool("isAim", P_States.isAim);
    }
}
