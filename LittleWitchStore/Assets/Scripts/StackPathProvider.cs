using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 为七段“叠高高”栈提供路径采样 & 弯曲功能。
/// 节点顺序：nodes[0] = 底部，nodes[^1] = 顶部。
/// </summary>
public class StackPathProvider : MonoBehaviour
{
    [Header("Rope Nodes (Bottom → Top)")]
    public List<Transform> nodes = new(); // 建议长度 = 7

    public bool IsActive { get; set; } = true;
    public bool IsValid => nodes.Count >= 2;

    [Header("Bend Settings")]
    [Tooltip("摇杆全开时，绳子中点最大的侧向偏移量（米）")]
    [SerializeField] float maxBendDistance = 0.6f;

    /* ────────── 弧长缓存 ────────── */
    float[] cumulative; // 每节点到 bottom 的累长
    float totalLen;

    /* ────────── 每帧更新弧长表 ────────── */
    void LateUpdate()
    {
        if (!IsValid) return;

        int n = nodes.Count;
        if (cumulative == null || cumulative.Length != n)
            cumulative = new float[n];

        cumulative[0] = 0f;
        for (int i = 1; i < n; i++)
        {
            float seg = Vector3.Distance(nodes[i - 1].position, nodes[i].position);
            cumulative[i] = cumulative[i - 1] + seg;
        }
        totalLen = cumulative[n - 1];
    }

    /* ────────── 采样（0~1） ────────── */
    public void Sample(float progress, out Vector3 pos, out Vector3 tangent)
    {
        if (!IsValid)
        {
            pos = transform.position;
            tangent = transform.up;
            return;
        }

        progress = Mathf.Clamp01(progress);
        float target = progress * totalLen;

        // 找所在段
        int seg = 0;
        while (seg < cumulative.Length - 1 && cumulative[seg + 1] < target)
            seg++;

        float t = Mathf.InverseLerp(cumulative[seg], cumulative[seg + 1], target);
        Vector3 p0 = nodes[seg].position;
        Vector3 p1 = nodes[seg + 1].position;

        pos = Vector3.Lerp(p0, p1, t);
        tangent = (p1 - p0).normalized;
    }

    public float TotalLength => totalLen;

    /* ────────── 核心：根据右摇杆弯曲 ────────── */
    public void SetBendInput(Vector2 input)
    {
        if (!IsValid || nodes.Count < 3) return;

        int    n        = nodes.Count;
        Vector3 bottom  = nodes[0].position;
        Vector3 top     = nodes[n - 1].position;

        /* 1. 计算弯曲强度与方向 -------------------------------------------- */
        Vector3 bendDir = transform.right * input.x + transform.forward * input.y;
        float   strength = Mathf.Clamp01(bendDir.magnitude);
        Vector3 dirNorm  = bendDir.sqrMagnitude < 1e-6f ? Vector3.zero : bendDir.normalized;
        float   offsetMag = strength * maxBendDistance;

        /* 2. 给所有节点（1‥n-1）施加偏移；t² 曲线 → 越高偏移越大 ---------- */
        for (int i = 1; i < n; i++)
        {
            float   t       = (float)i / (n - 1);               // 0→1
            Vector3 basePos = Vector3.Lerp(bottom, top, t);     // 直线插值
            float   curve   = Mathf.Pow(t, 2f);                 // 上端更狠
            Vector3 offset  = dirNorm * offsetMag * curve;

            nodes[i].position = basePos + offset;
        }

        /* 3. 更新每节朝向：让本节 +Z 指向“下一节” -------------------------- */
        for (int i = 0; i < n - 1; i++)
        {
            Vector3 dir = nodes[i + 1].position - nodes[i].position;
            if (dir.sqrMagnitude > 1e-6f)
                nodes[i].rotation = Quaternion.LookRotation(dir);
        }
        /* 顶端指向倒数第二节 */
        Vector3 backDir = nodes[n - 2].position - nodes[n - 1].position;
        if (backDir.sqrMagnitude > 1e-6f)
            nodes[n - 1].rotation = Quaternion.LookRotation(backDir);
    }

}
