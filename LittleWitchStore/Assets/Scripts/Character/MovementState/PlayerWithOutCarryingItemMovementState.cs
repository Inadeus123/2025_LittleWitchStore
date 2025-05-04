using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWithOutCarryingItemMovementState : PlayerMovementState
{
    public PlayerWithOutCarryingItemMovementState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
    {
        
    }
    
    protected virtual void OnMove()
    {
        stateMachine.ChangeState(stateMachine.WalkingState);
    }
}
