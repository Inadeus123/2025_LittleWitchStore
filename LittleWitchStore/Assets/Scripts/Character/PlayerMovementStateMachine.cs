using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementStateMachine : StateMachine
{
    public Player Player { get; private set; }
    public PlayerIdlingState IdlingState { get; private set; }
    public PlayerWalkingState WalkingState { get; private set; }
    public PlayerStoppingState StoppingState { get; private set; }
    
    //Player被击中的几种State
    public PlayerStunnedState StunnedState { get; private set; }
    public PlayerHitReactState HitReactState { get; private set; }
    
    public PlayerMovementStateMachine(Player player)
    {
        Player = player;
        IdlingState = new PlayerIdlingState();
        WalkingState = new PlayerWalkingState();
        StoppingState = new PlayerStoppingState();
        
        StunnedState = new PlayerStunnedState();
        HitReactState = new PlayerHitReactState();
    }
}
