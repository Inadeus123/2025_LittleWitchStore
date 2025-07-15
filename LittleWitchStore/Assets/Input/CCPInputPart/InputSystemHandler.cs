using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using Lightbug.CharacterControllerPro.Implementation;


public class InputSystemHandler : InputHandler
{
    [SerializeField]
    InputActionAsset inputActionsAsset = null;

    [SerializeField]
    bool filterByActionMap = false;

    [SerializeField]
    string gameplayActionMap = "Gameplay";

    [SerializeField]
    bool filterByControlScheme = false;

    [SerializeField]
    string controlSchemeName = "Keyboard Mouse";


    Dictionary< string , InputAction> inputActionsDictionary = new Dictionary<string, InputAction>();

    protected virtual void Awake()
    {
        
        if( inputActionsAsset == null )
        {
            Debug.Log("No input actions asset found!");
            return;
        }

        inputActionsAsset.Enable();

        if( filterByControlScheme )
        {
            string bindingGroup = inputActionsAsset.controlSchemes.First( x => x.name == controlSchemeName ).bindingGroup;
            inputActionsAsset.bindingMask = InputBinding.MaskByGroup( bindingGroup );
        }

        ReadOnlyArray<InputAction> rawInputActions = new ReadOnlyArray<InputAction>();
        
        if( filterByActionMap )
        {
            rawInputActions = inputActionsAsset.FindActionMap( gameplayActionMap ).actions;

            for( int i = 0 ; i < rawInputActions.Count ; i++ )
                inputActionsDictionary.Add( rawInputActions[i].name , rawInputActions[i] );
        
        }
        else
        {
            for( int i = 0 ; i < inputActionsAsset.actionMaps.Count ; i++ )
            {
                InputActionMap actionMap = inputActionsAsset.actionMaps[i];

                for( int j = 0 ; j < actionMap.actions.Count ; j++ )
                {
                    InputAction action = actionMap.actions[j];
                    inputActionsDictionary.Add( action.name , action );
                }

            }

            
        }
        

        for( int i = 0 ; i < rawInputActions.Count ; i++ )
        {
            inputActionsDictionary.Add( rawInputActions[i].name , rawInputActions[i] );
        }

    }

    public override bool GetBool( string actionName )
    { 
        InputAction inputAction;

        if( !inputActionsDictionary.TryGetValue( actionName , out inputAction ) )
            return false;

        return inputActionsDictionary[actionName].ReadValue<float>() >= InputSystem.settings.defaultButtonPressPoint;
    }

    public override float GetFloat( string actionName )
    {       
        InputAction inputAction;

        if( !inputActionsDictionary.TryGetValue( actionName , out inputAction ) )
            return 0f;
        
        return inputAction.ReadValue<float>();
    }

    

    public override Vector2 GetVector2( string actionName )
    {
        InputAction inputAction;

        if( !inputActionsDictionary.TryGetValue( actionName , out inputAction ) )
            return Vector2.zero;
        
        return inputActionsDictionary[actionName].ReadValue<Vector2>(); 
    }

    /// <summary>
    /// 检测按钮是否在这一帧按下
    /// </summary>
    /*public bool GetButtonDown(string actionName)
    {
        if (!inputActionsDictionary.TryGetValue(actionName, out InputAction inputAction))
        {
            Debug.LogWarning($"Input action '{actionName}' not found.");
            return false;
        }
        Debug.Log("Does Button pressed?" +inputAction.WasPressedThisFrame() );
        return inputAction.WasPressedThisFrame();
    }*/

    /// <summary>
    /// 检测按钮是否在这一帧被释放
    /// </summary>
    /*public bool GetButtonUp(string actionName)
    {
        if (!inputActionsDictionary.TryGetValue(actionName, out InputAction inputAction))
        {
            Debug.LogWarning($"Input action '{actionName}' not found.");
            return false;
        }
        Debug.Log("Does Button released?" +inputAction.WasReleasedThisFrame() );
        return inputAction.WasReleasedThisFrame();
    }*/
    
    
    Dictionary<string, float> lastValue = new Dictionary<string, float>();

    public bool GetButtonDown(string actionName)
    {
        if (!inputActionsDictionary.TryGetValue(actionName, out var action))
            return false;

        float curr = action.ReadValue<float>();
        lastValue.TryGetValue(actionName, out float prev);

        lastValue[actionName] = curr;                 // 记录供下一帧用
        Debug.Log( curr >= 0.5f && prev < 0.5f );
        return curr >= 0.5f && prev < 0.5f;           // 0→1
    }

    public bool GetButtonUp(string actionName)
    {
        if (!inputActionsDictionary.TryGetValue(actionName, out var action))
            return false;

        float curr = action.ReadValue<float>();
        lastValue.TryGetValue(actionName, out float prev);

        lastValue[actionName] = curr;                 // 记录供下一帧用
        Debug.Log( curr < 0.5f && prev >= 0.5f);
        return curr < 0.5f && prev >= 0.5f;           // 1→0
    }

}