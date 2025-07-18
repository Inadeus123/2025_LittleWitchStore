using System;
using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;


public class Player : MonoBehaviour
{
    /// <summary>
    /// Gets the CharacterActor component of the gameObject.
    /// </summary>
    public CharacterActor characterActor { get; private set; }

    /// <summary>
    /// Gets the CharacterBrain component of the gameObject.
    /// </summary>
    // public CharacterBrain CharacterBrain{ get; private set; }
    CharacterBrain characterBrain = null;

    /// <summary>
    /// Gets the current brain actions CharacterBrain component of the gameObject.
    /// </summary>
    private CharacterActions characterActions;
    
    private CharacterStateController controller;
    public InputSystemHandler inputSystemHandler;
    private void Start()
    {
        controller = GetComponentInChildren<CharacterStateController>();
        characterActor = GetComponentInChildren<CharacterActor>();
        characterBrain = GetComponentInChildren<CharacterBrain>();
        characterActions = characterBrain.CharacterActions;
        
        if (controller == null || characterActor == null || characterBrain == null)
        {
            Debug.LogError("No CharacterStateController/characterActor/characterBrain found");
            return;
        }
    }

    void Update()
    {
        //Debug.Log("Player update");
        if (Input.GetKeyDown(KeyCode.Y))
        {
            controller.EnqueueTransition<RunFastState>();
        }
        
        // 检查是否按下了摆荡
        if (inputSystemHandler.GetButtonDown("Swing"))
        {
            Debug.Log("Press B, Ready for swing");
            controller.EnqueueTransition<FamiliarSwingState>();
        }
    }
    
}
