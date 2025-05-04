using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerWithOutCarryingItemMovementData
{
    [Header("Walk / Run")]
    [Tooltip("Ground move speed (u/s)")] public float walkSpeed = 4f;
    [Tooltip("Time to reach full walk speed (s). 0 = instant")] public float walkAccelTime = 0.1f;
    [Tooltip("Time to full stop after releasing input (s)")] public float walkDecelTime = 0.05f;

    [Header("Dash")]
    [Tooltip("Dash speed (u/s)")] public float dashSpeed = 8f;
    [Tooltip("Dash duration (s)")] public float dashDuration = 0.30f;
    [Tooltip("Cooldown before next dash (s)")] public float dashCooldown = 1.0f;
    [Tooltip("Prone/hard‑stop time after dash (s)")] public float proneRecoverTime = 0.60f;

    [Header("Turn")]
    [Tooltip("Max yaw change per second (deg/s)")] public float turnSpeed = 720f;

    [Header("开启重力")]
    public bool useGravity = false;
    public float gravity = -30f;

    // Helper to compute accel/decel values (cached at runtime)
    [NonSerialized] public float walkAccel ;   // units / s²
    [NonSerialized] public float walkDecel ;

    public void CacheRuntime() {
        walkAccel = walkAccelTime <= 0 ? float.PositiveInfinity : walkSpeed / walkAccelTime;
        walkDecel = walkDecelTime <= 0 ? float.PositiveInfinity : walkSpeed / walkDecelTime;
    }
}
