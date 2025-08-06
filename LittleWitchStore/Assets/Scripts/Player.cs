using System;
using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
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
    public Camera3D camera;
    public GameObject playerHead; //丢出去的头
    public float playerHeadOffset;
    private void Start()
    {
        controller = GetComponentInChildren<CharacterStateController>();
        characterActor = GetComponentInChildren<CharacterActor>();
        characterBrain = GetComponentInChildren<CharacterBrain>();
        characterActions = characterBrain.CharacterActions;
        camera = Camera.main.GetComponent<Camera3D>();
        
        if (controller == null || characterActor == null || characterBrain == null || camera == null)
        {
            Debug.LogError("No CharacterStateController/characterActor/characterBrain/Camera3D not found");
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

        if (inputSystemHandler.GetButtonDown("ThrowHead"))
        {
            Debug.Log("Press RB, Ready for throwhead");
            ThrowHead();
        }
    }


    void ThrowHead()
    {
        //----------------setactive头部，传送头部到位------------------------
        if (!playerHead.activeSelf)
        {
            playerHead.transform.position = transform.position + new Vector3(0, playerHeadOffset, 0);
            playerHead.SetActive(true);
        }
        //Debug.Log("Player head position: " + playerHead.transform.position);
        CharacterActor playerHeadCharacterActor = playerHead.GetComponent<CharacterActor>();
        playerHeadCharacterActor.Teleport(transform.position + new Vector3(0, playerHeadOffset, 0));
        
        //--------------------------------相机控制权给playerHead--------------------------------
        camera.targetTransform = playerHead.transform;
        camera.bodyObject = playerHead;
        camera.inputHandlerSettings.InputHandler = playerHead.GetComponentInChildren<InputSystemHandler>();
        //--------------------------------本体设置为inactive--------------------------------
        
        this.gameObject.SetActive(false);
    }
}
