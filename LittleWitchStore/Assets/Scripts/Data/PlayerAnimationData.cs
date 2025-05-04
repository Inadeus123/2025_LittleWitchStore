using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerAnimationData
{
    [Header("State Group Parameter Names")]
    [SerializeField] private string idlingParameterName = "isIdling";
    [SerializeField] private string walkingParameterName = "isWalking";
    [SerializeField] private string stoppingParameterName = "isStopping";
    [SerializeField] private string hitParameterName = "isHit";
    [SerializeField] private string stunnedParameterName = "isStunned";
    [SerializeField] private string withoutCarryingItemParameterName = "bWithoutCarryingItem";
    [SerializeField] private string withCarryingItemParameterName = "bCarryingItem";

    public int IdlingParameterHash  { get; private set; }
    public int WalkingParameterHash  { get; private set; }
    public int StoppingParameterHash { get; private set; }
    public int HitParameterHash  { get; private set; }
    public int StunnedParameterHash  { get; private set; }

    public int WithoutCarryingItemParameterHash  { get; private set; }
    public int WithCarryingItemParameterHash  { get; private set; }

    public void Initialize()
    {
        IdlingParameterHash = Animator.StringToHash(idlingParameterName);
        WalkingParameterHash = Animator.StringToHash(walkingParameterName);
        StoppingParameterHash = Animator.StringToHash(stoppingParameterName);
        HitParameterHash = Animator.StringToHash(hitParameterName);
        StunnedParameterHash = Animator.StringToHash(stunnedParameterName);

        WithoutCarryingItemParameterHash = Animator.StringToHash(withoutCarryingItemParameterName);
        WithCarryingItemParameterHash = Animator.StringToHash(withCarryingItemParameterName);
        
    }
}