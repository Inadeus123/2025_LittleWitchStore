using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;   // 新输入系统

/// <summary>
/// 负责：
/// 1. 根据右摇杆让杆子产生弯曲形变
/// 2. 检测“松杆”瞬间，返回最后方向给 PlayerAttachState 发射
/// </summary>
public class PoleController : MonoBehaviour
{
    public float deadZone = 0.15f;          // 静止阈值
    public float releaseThreshold = 0.12f;  // 触发发射阈值
    public Camera cam;                      // 拖主摄像机

    bool stickActive;
    Vector3 holdDirWS;

    [Header("杆段列表：底→顶")]
    public List<Transform> segments = new(); // 按 Inspector 手动塞 7 个段

    [Header("形变参数")]
    [Tooltip("最大弯曲角（度）— 摇杆满档时杆顶相对竖直能转多少度")]
    public float maxBendAngle = 55f;

    [Tooltip("杆段间距 / 单段长度")]
    public float segmentLength = 1f;

    [Header("发射参数")]
    public float launchForce = 28f;   // 根据你 CCP 角色质量来调
    public Transform attachPoint;     // 段顶 AttachPoint

    /* ————— 私有状态 ————— */
    Vector2 _prevStick;       // 上一帧右摇杆
    Vector2 _currentStick;    // 当前帧
    Vector3 _bendDirWS;       // 世界空间弯曲方向
    bool _isAnyPlayerAttached;

    public bool IsPlayerAttached => _isAnyPlayerAttached;            // 给外部读
    public Vector3 LastBendDirWS { get; private set; } = Vector3.zero;

    /*void Update()
    {
        // ① 读取右摇杆（没有手柄则默认为零）
        _currentStick = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;

        // ② 若有人附着才允许弯曲
        if (_isAnyPlayerAttached)
        {
            BendPole();
            DetectRelease();
        }

        _prevStick = _currentStick;
    }*/

    private void FixedUpdate()
    {
        Vector2 raw = Gamepad.current?.rightStick.ReadValue() ?? Vector2.zero;
        float mag   = raw.magnitude;

        // ───── 检测“开始拉” ─────
        if (!stickActive && mag > deadZone)
        {
            stickActive = true;
            holdDirWS   = StickToWorld(raw);   // 一次性记录
        }

        // ───── 检测“松手” ─────
        if (stickActive && mag < releaseThreshold)
        {
            LastBendDirWS = holdDirWS;
            stickActive   = false;
            OnPoleReleased();                 // 触发事件
        }

        // 用于实时弯曲，只在 stickActive 时才弯
        _currentStick = stickActive ? raw : Vector2.zero;
        if (stickActive && _isAnyPlayerAttached)
            BendPole();   // *可改到 FixedUpdate 内*
    }
    
    Vector3 StickToWorld(Vector2 stick)
    {
        // 相机水平坐标系
        Vector3 camF = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 camR = Vector3.ProjectOnPlane(cam.transform.right,   Vector3.up).normalized;
        return (camR * stick.x + camF * stick.y).normalized;
    }

    /* ---------- 对外接口 ---------- */

    /// <summary>某玩家附着 & 锁定。可重复调用（多玩家时改 list）</summary>
    public void RegisterAttach()
    {
        _isAnyPlayerAttached = true;
    }

    /// <summary>玩家离开杆子</summary>
    public void RegisterDetach()
    {
        _isAnyPlayerAttached = false;
        // 把杆子缓慢回弹到竖直
        StartCoroutine(RecoverPole());
    }

    /* ---------- 杆子弯曲 & 松杆检测 ---------- */

    void BendPole()
    {
        // 2D 摇杆 → 水平世界向量
        //Vector3 horizontal = new Vector3(_currentStick.x, 0f, _currentStick.y);
        Vector3 horizontal = StickToWorld(_currentStick); 
        float mag = Mathf.Clamp01(horizontal.magnitude);   // 0~1
        if (mag < 0.01f) return;                           // 微动忽略

        _bendDirWS = horizontal.normalized;                // 保留方向
        float bendAngle = maxBendAngle * mag;

        // 以底段为锚点，分段 Slerp 形成平滑弧线
        Vector3 basePos = segments[0].position;
        Quaternion baseRot = Quaternion.identity; // 默认世界 +Y 竖直

        for (int i = 0; i < segments.Count; i++)
        {
            float t = (i + 1) / (float)segments.Count;          // 0→1
            float thisAngle = bendAngle * t;                    // 越往顶越弯
            Quaternion rot = Quaternion.AngleAxis(
                thisAngle,
                Vector3.Cross(Vector3.up, _bendDirWS));         // 绕垂直轴旋转

            // 旋转后的“上方向”
            Vector3 upDir = rot * Vector3.up;

            // 设置段 Transform
            if (i == 0)
            {
                segments[i].position = basePos;
                segments[i].rotation = rot;
            }
            else
            {
                segments[i].position = segments[i - 1].position + upDir * segmentLength;
                segments[i].rotation = rot;
            }
        }

        // 更新 AttachPoint 位置
        attachPoint.position = segments[^1].position + (segments[^1].up * 0.2f);
        attachPoint.rotation = segments[^1].rotation;
    }

    void DetectRelease()
    {
        // 上帧有量 & 本帧几乎为零 = 松开
        if (_prevStick.sqrMagnitude > 0.05f && _currentStick.sqrMagnitude < 0.01f)
        {
            LastBendDirWS = _bendDirWS;   // 记录最后方向
            OnPoleReleased();
        }
    }

    public event System.Action OnShoot;
    void OnPoleReleased()
    {
       OnShoot?.Invoke();
    }

    /* ---------- 回弹协程 ---------- */

    System.Collections.IEnumerator RecoverPole()
    {
        // 用 Lerp 把所有段慢慢拉直
        float t = 0f;
        List<Quaternion> startRots = new();
        foreach (var s in segments) startRots.Add(s.rotation);

        while (t < 1f)
        {
            t += Time.deltaTime * 3f; // 回弹速度
            for (int i = 0; i < segments.Count; i++)
            {
                segments[i].rotation = Quaternion.Slerp(startRots[i], Quaternion.identity, t);
                if (i == 0) segments[i].position = segments[0].position; // 底段位置保持
                else segments[i].position = segments[i - 1].position + segments[i].up * segmentLength;
            }
            attachPoint.position = segments[^1].position + segments[^1].up * 0.2f;
            yield return null;
        }
    }
}
