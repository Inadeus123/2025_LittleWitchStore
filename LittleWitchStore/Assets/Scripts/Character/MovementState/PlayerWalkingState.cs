using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWalkingState :PlayerWithOutCarryingItemMovementState
{
    public PlayerWalkingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
    {
    }
}
