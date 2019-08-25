using System;
using UnityEngine;

[Serializable]
public struct InputKeyMap
{
    [Header("KeyMap")]
    public string AttackButton;
    public string ToogleRun;
    public string RollButton;
    [Header("Unity Default KeyMap")]
    public string HorizontalAxis;
    public string VerticalAxis;
    public string MouseHorizontal;
}
