using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine;

public class PoleEnterTrigger : MonoBehaviour
{
    public PlayerPoleAttachState playerState;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterStateController controller = other.GetComponentInChildren<CharacterStateController>();
            controller.EnqueueTransition<PlayerPoleAttachState>();
        }
    }
}

