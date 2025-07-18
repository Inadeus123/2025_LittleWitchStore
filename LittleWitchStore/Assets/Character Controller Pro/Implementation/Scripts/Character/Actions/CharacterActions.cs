namespace Lightbug.CharacterControllerPro.Implementation
{

/// <summary>
/// This struct contains all the inputs actions available for the character to interact with.
/// </summary>
[System.Serializable]
public struct CharacterActions 
{
    
    // Bool actions
	public BoolAction @jump;
	public BoolAction @run;
	public BoolAction @interact;
	public BoolAction @jetPack;
	public BoolAction @dash;
	public BoolAction @crouch;
	public BoolAction @attack;
	public BoolAction @openwheel;
	public BoolAction @familiaract;
	public BoolAction @swing;


    // Float actions
	public FloatAction @pitch;
	public FloatAction @roll;


    // Vector2 actions
	public Vector2Action @movement;
	public Vector2Action @camera;


    public static CharacterActions CreateDefaultActions()
    {
        var actions = new CharacterActions();
        actions.InitializeActions();
        return actions;
    }

    /// <summary>
    /// Reset all the actions.
    /// </summary>
	public void Reset()
	{
		@jump.Reset();
		@run.Reset();
		@interact.Reset();
		@jetPack.Reset();
		@dash.Reset();
		@crouch.Reset();
		@attack.Reset();
		@openwheel.Reset();
		@familiaract.Reset();
		@swing.Reset();

		@pitch.Reset();
		@roll.Reset();

		@movement.Reset();
		@camera.Reset();

	}

    /// <summary>
    /// Initializes all the actions by instantiate them. Each action will be instantiated with its specific type (Bool, Float or Vector2).
    /// </summary>
    public void InitializeActions()
    {
		@jump = new BoolAction();
		@jump.Initialize();

		@run = new BoolAction();
		@run.Initialize();

		@interact = new BoolAction();
		@interact.Initialize();

		@jetPack = new BoolAction();
		@jetPack.Initialize();

		@dash = new BoolAction();
		@dash.Initialize();

		@crouch = new BoolAction();
		@crouch.Initialize();

		@attack = new BoolAction();
		@attack.Initialize();

		@openwheel = new BoolAction();
		@openwheel.Initialize();

		@familiaract = new BoolAction();
		@familiaract.Initialize();

		@swing = new BoolAction();
		@swing.Initialize();


		@pitch = new FloatAction();
		@roll = new FloatAction();

		@movement = new Vector2Action();
		@camera = new Vector2Action();

    }

    /// <summary>
    /// Updates the values of all the actions based on the current input handler (human).
    /// </summary>
    public void SetValues( InputHandler inputHandler )
    {
        if( inputHandler == null )
			return;
        
		@jump.value = inputHandler.GetBool( "Jump" );
		@run.value = inputHandler.GetBool( "Run" );
		@interact.value = inputHandler.GetBool( "Interact" );
		@jetPack.value = inputHandler.GetBool( "Jet Pack" );
		@dash.value = inputHandler.GetBool( "Dash" );
		@crouch.value = inputHandler.GetBool( "Crouch" );
		@attack.value = inputHandler.GetBool( "Attack" );
		@openwheel.value = inputHandler.GetBool( "OpenWheel" );
		@familiaract.value = inputHandler.GetBool( "FamiliarAct" );
		@swing.value = inputHandler.GetBool( "Swing" );

		@pitch.value = inputHandler.GetFloat( "Pitch" );
		@roll.value = inputHandler.GetFloat( "Roll" );

		@movement.value = inputHandler.GetVector2( "Movement" );
		@camera.value = inputHandler.GetVector2( "Camera" );

    }

    /// <summary>
    /// Copies the values of all the actions from an existing set of actions.
    /// </summary>
    public void SetValues( CharacterActions characterActions )
    {	
		@jump.value = characterActions.jump.value;
		@run.value = characterActions.run.value;
		@interact.value = characterActions.interact.value;
		@jetPack.value = characterActions.jetPack.value;
		@dash.value = characterActions.dash.value;
		@crouch.value = characterActions.crouch.value;
		@attack.value = characterActions.attack.value;
		@openwheel.value = characterActions.openwheel.value;
		@familiaract.value = characterActions.familiaract.value;
		@swing.value = characterActions.swing.value;

		@pitch.value = characterActions.pitch.value;
		@roll.value = characterActions.roll.value;

		@pitch.value = characterActions.pitch.value;
		@roll.value = characterActions.roll.value;
		@movement.value = characterActions.movement.value;
		@camera.value = characterActions.camera.value;

    }

    /// <summary>
	/// Update all the actions internal states.
	/// </summary>
    public void Update( float dt )
    {
		@jump.Update( dt );
		@run.Update( dt );
		@interact.Update( dt );
		@jetPack.Update( dt );
		@dash.Update( dt );
		@crouch.Update( dt );
		@attack.Update( dt );
		@openwheel.Update( dt );
		@familiaract.Update( dt );
		@swing.Update( dt );

    }


}


}