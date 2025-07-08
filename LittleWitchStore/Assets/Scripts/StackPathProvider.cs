using System.Collections.Generic;
using UnityEngine;

public class StackPathProvider : MonoBehaviour
{
    public List<Transform> nodes = new();   // 底→顶，长度 = 7
    public bool IsActive { get; set; } 
    public bool IsValid => nodes.Count >= 2;

    float[] cumulative;                     // 弧长表
    float totalLen;

    void LateUpdate()                       // 每帧重新计算节点位置 & 弧长
    {
        //Debug.Log("测试 StackPathProvider");
        if (!IsValid)
        {
            Debug.Log("Is not valid");
            return;
        }

        int n = nodes.Count;
        if (cumulative == null || cumulative.Length != n)
            cumulative = new float[n];

        cumulative[0] = 0;
        for (int i = 1; i < n; i++)
        {
            float seg = Vector3.Distance(nodes[i - 1].position, nodes[i].position);
            cumulative[i] = cumulative[i - 1] + seg;
        }
        totalLen = cumulative[n - 1];
    }

    /// <summary> progress∈[0,1] → 世界坐标、朝向</summary>
    public void Sample(float progress, out Vector3 pos, out Vector3 tangent)
    {
        pos = nodes[0].position;
        tangent = nodes[1].position - pos;

        if (!IsValid) return;

        float target = progress * totalLen;

        // 找落在哪一段
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
}