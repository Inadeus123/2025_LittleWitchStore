using UnityEngine;
using DG.Tweening; // DOTween命名空间

[SerializeField]
public class SearchlightSweep : MonoBehaviour
{
    [Header("基本设置")]
    public float speedMultiplier = 1f; // 整体速度倍率（在Inspector调整加速/减速）
    public Vector3 rotationAxis = Vector3.up; // 扫射轴（默认Y轴，左右扫；可改成Vector3.right为X轴上下扫）

    [Header("自定义扫射路径")]
    [Tooltip("旋转目标点数组（欧拉角）。例如：[0,0,0] 到 [90,0,0]。至少2个点来回扫射。")]
    public Vector3[] rotationPoints = new Vector3[] { new Vector3(0, -45, 0), new Vector3(0, 45, 0) }; // 默认简单左右扫

    [Tooltip("每个段的持续时间（秒）。数组长度应匹配rotationPoints-1。")]
    public float[] segmentDurations = new float[] { 2f }; // 默认2秒从一个点到下一个

    [Tooltip("每个段的缓动类型（动画曲线）。")]
    public Ease[] segmentEases = new Ease[] { Ease.Linear }; // 默认线性，可在Inspector选InOutSine等

    [Header("循环设置")]
    public LoopType loopType = LoopType.Yoyo; // 默认Yoyo（来回），可选Restart（单向循环）或Incremental
    public int loops = -1; // -1为无限循环

    [Header("高级")]
    public float startDelay = 0f; // 开始延迟（秒）
    public bool autoStart = true; // 是否自动开始

    private Sequence sweepSequence;

    void Start()
    {
        if (autoStart)
        {
            StartSweep();
        }
    }

    public void StartSweep()
    {
        // 杀死旧序列（防止重复）
        if (sweepSequence != null && sweepSequence.IsActive())
        {
            sweepSequence.Kill();
        }

        // 创建DOTween序列
        sweepSequence = DOTween.Sequence();

        // 添加起始延迟
        sweepSequence.AppendInterval(startDelay);

        // 当前旋转作为起点
        Vector3 currentRotation = transform.localEulerAngles;

        // 循环添加每个段的旋转Tween
        for (int i = 0; i < rotationPoints.Length; i++)
        {
            // 计算目标旋转（相对或绝对？这里用绝对欧拉角，便于Inspector编辑）
            Vector3 targetRotation = rotationPoints[i];

            // 持续时间：用segmentDurations[i]，如果数组短则用最后一个
            float duration = (i < segmentDurations.Length) ? segmentDurations[i] : segmentDurations[segmentDurations.Length - 1];
            duration /= speedMultiplier; // 应用速度倍率（更高倍率=更快）

            // 缓动：类似
            Ease ease = (i < segmentEases.Length) ? segmentEases[i] : segmentEases[segmentEases.Length - 1];

            // 添加旋转Tween（用DORotate，围绕指定轴）
            sweepSequence.Append(transform.DOLocalRotate(targetRotation, duration, RotateMode.Fast).SetEase(ease));
        }

        // 设置循环
        sweepSequence.SetLoops(loops, loopType);
    }

    // 可选：停止扫射（例如从其他脚本调用）
    public void StopSweep()
    {
        if (sweepSequence != null && sweepSequence.IsActive())
        {
            sweepSequence.Kill();
        }
    }

    // 编辑器调试：如果在Inspector改变参数，预览更新
#if UNITY_EDITOR
    private void OnValidate()
    {
        // 确保数组长度合理（至少2点）
        if (rotationPoints.Length < 2)
        {
            Debug.LogWarning("Rotation points need at least 2 for sweeping.");
        }
        if (segmentDurations.Length < 1)
        {
            Debug.LogWarning("Segment durations need at least 1.");
        }
    }
#endif
}