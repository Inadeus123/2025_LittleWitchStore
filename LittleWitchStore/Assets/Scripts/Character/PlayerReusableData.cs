using System.Collections.Generic;
using UnityEngine;

public class PlayerReusableData
{
    public Vector2 MovementInput {get; set; }
    public float MovementSpeedModifier { get; set; } // 用来控制角色的移动速度
    public float MovementDecelerationForce { get; set; } // 用来控制角色的减速
}
