using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerIdlingState:PlayerWithOutCarryingItemMovementState
{
    public PlayerIdlingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
    {
        
    }
    
    public override void Enter()
    {
        base.Enter();
        stateMachine.PlayerReusableData.MovementSpeedModifier = 0f;
        StartAnimation(stateMachine.Player.AnimationData.IdlingParameterHash);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.Player.AnimationData.IdlingParameterHash);
    }

    public override void Update()
    {
        base.Update();
        if (stateMachine.PlayerReusableData.MovementInput == Vector2.zero)
        {
            return;
        }
        OnMove();
        
    }
}
