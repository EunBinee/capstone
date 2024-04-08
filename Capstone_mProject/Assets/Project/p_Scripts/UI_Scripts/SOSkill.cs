using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SOSkill : ScriptableObject
{
    public float damage;
    public float cool;
    public bool isTwice;

    public string animationName;
    public Sprite icon;
}