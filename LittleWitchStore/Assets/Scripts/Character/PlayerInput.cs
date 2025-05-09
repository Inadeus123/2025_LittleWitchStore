using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    //Player输入部分
    public PlayerInputAction InputActions { get; private set; }
    public PlayerInputAction.PlayerNormalInputActions PlayerNormalInputActions { get; private set; }

    private void Awake()
    {
        InputActions = new PlayerInputAction();
        PlayerNormalInputActions = InputActions.PlayerNormalInput;
    }

    private void OnEnable()
    {
        InputActions.Enable();
    }
    
    private void OnDisable()
    {
        InputActions.Disable();
    }
    
    public void DisableActionFor(InputAction action, float seconds)
    {
        StartCoroutine(DisableAction(action, seconds));
    }

    private IEnumerator DisableAction(InputAction action, float seconds)
    {
        action.Disable();

        yield return new WaitForSeconds(seconds);

        action.Enable();
    }
}
