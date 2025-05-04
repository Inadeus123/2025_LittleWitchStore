using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStoppingState :PlayerWithOutCarryingItemMovementState
{
    public PlayerStoppingState(PlayerMovementStateMachine playerMovementStateMachine) : base(playerMovementStateMachine)
    {
        
    }

    public override void Enter()
    {
        base.Enter();
        StartAnimation(stateMachine.Player.AnimationData.StoppingParameterHash);
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation(stateMachine.Player.AnimationData.StoppingParameterHash);
    }

    public override void Update()
    {
        base.Update();
        // Check if the player is still moving
        if (stateMachine.PlayerReusableData.MovementInput != Vector2.zero)
        {
            // If the player is still moving, change to the walking state
            stateMachine.ChangeState(stateMachine.WalkingState);
        }
    }

    public override void OnAnimationTransitionEvent()
    {
        Debug.Log("StoppingOnAnimationTransitionEvent Called");
        base.OnAnimationTransitionEvent();
        stateMachine.ChangeState(stateMachine.IdlingState);
    }
}
