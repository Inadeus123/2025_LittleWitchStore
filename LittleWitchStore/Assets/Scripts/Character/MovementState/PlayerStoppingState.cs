using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStoppingState :PlayerWithOutCarryingItemMovementState
{
    public PlayerStoppingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
    {
    }
}
