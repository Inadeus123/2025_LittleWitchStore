using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;

public class PoleAttachTrigger : MonoBehaviour
{
    public Transform topCapsule;   
    public Transform attachPoint;     // 附着点，位于 topCapsule 顶端
    public CharacterActor player;
    public bool PlayerAttached { get; private set; } = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Attach player");
            AttachPlayer();
        }
    }

    public void AttachPlayer()
    {
        //if (PlayerAttached) return; // 已经附着了
        
        PlayerAttached = true;
        // 让玩家进入“附着状态”
        Debug.Log("Attach player");
        player.Teleport(attachPoint.position, attachPoint.rotation);
        player.transform.SetParent(topCapsule);  // 让玩家跟随顶端移动
        // 可以禁用 CCP 移动脚本等，以进入“准备弹射”状态
        
        //禁用相机旋转
        Camera3D cam = Camera.main.GetComponent<Camera3D>();
        cam.updatePitch = false;
        cam.updateYaw = false;
    }
}

