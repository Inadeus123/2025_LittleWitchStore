using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdlingState:PlayerWithOutCarryingItemMovementState
{
    public PlayerIdlingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
    {
        
    }
}
