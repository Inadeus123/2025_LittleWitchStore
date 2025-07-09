using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation; // CCP 命名空间
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPoleAttachState : CharacterState
{
    [Header("引用")]
    public PoleController pole;                 // Inspector 直接拖
    public CharacterStateController controller;
    public CharacterBrain brain;
    public Transform attachTransform;           // 在 Awake 里通过 pole.attachPoint 更新
    public CharacterActor actor;
    Camera playerCam;                           // 如有需要

    [Header("发射参数")]
    public float upwardBoost = 0.6f;            // 垂直分量
    bool _isAttached;
    

    void Awake()
    {
        //actor = GetComponent<CharacterActor>();
        controller = actor.GetComponentInChildren<CharacterStateController>();
        brain = actor.GetComponentInChildren<CharacterBrain>();
        if (controller == null)
        {
            Debug.Log("Controller not exist");
        }
       
    }

    /* ---------- 状态逻辑 ---------- */
    public override void UpdateBehaviour(float dt)
    {
        // ① 跟随杆顶
        if (_isAttached)
        {
            attachTransform = pole.attachPoint;
            actor.Teleport(attachTransform.position);
        }
    }

    /* ---------- 进入/离开 状态 ---------- */

    public override void EnterBehaviour(float dt, CharacterState prev)
    {
        // 把玩家锁定到AttachPoint
        _isAttached = true;
        pole.RegisterAttach();

        actor.Velocity = Vector3.zero;
        //actor.ForceGrounded();
        //actor.PlanarMovement = false; // 禁用水平输入
        
        pole.OnShoot += OnPoleShoot; // 订阅杆子松手事件
        
    }

    public override void ExitBehaviour(float dt,CharacterState toState)
    {
        _isAttached = false;
        pole.RegisterDetach();
        //actor.PlanarMovement = true;
        
        pole.OnShoot -= OnPoleShoot; // 取消订阅杆子松手事件
    }

    
    /* ---------- 收到 Pole 的松手事件 ---------- */

    void OnPoleShoot()
    {
        // 防呆：若方向丢失则不发射
        if (pole.LastBendDirWS.sqrMagnitude < 0.001f)
            return;

        Vector3 dir = (-pole.LastBendDirWS + Vector3.up * upwardBoost).normalized;
        //actor.SetVelocity(dir * pole.launchForce);
        actor.Velocity = dir * pole.launchForce;
        controller.EnqueueTransition<NormalMovement>();
        
    }
}