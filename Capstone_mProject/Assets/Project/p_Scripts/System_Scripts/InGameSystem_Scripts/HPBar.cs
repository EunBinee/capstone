using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    private double monsterMaxHP = 0;
    public bool isReset = false;
    private Slider m_slider;
    public Monster m_Monster;
    public Transform m_HPBarPos;

}
