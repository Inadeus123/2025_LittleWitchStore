using System;
using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;

public class Player : MonoBehaviour
{

    private CharacterStateController controller;

    private void Start()
    {
        controller = GetComponentInChildren<CharacterStateController>();
        if (controller == null)
        {
            Debug.LogError("No CharacterStateController found");
            return;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            controller.EnqueueTransition<RunFastState>();
        }
        
        
        // 检查是否按下了使魔键（假设用F键）
        if (Input.GetKeyDown(KeyCode.O))
        {
            controller.EnqueueTransition<FamiliarSwingState>();
        }
    }
}
