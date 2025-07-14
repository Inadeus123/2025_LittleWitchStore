using UnityEngine;
using DG.Tweening;

/// <summary>
/// 平滑动画控制器 - 处理杆子弯曲和玩家移动的平滑动画
/// </summary>
public class SmoothAnimationController : MonoBehaviour
{
    [Header("动画设置")]
    [SerializeField] private float bendAnimationDuration = 0.2f;    // 弯曲动画持续时间
    [SerializeField] private float returnAnimationDuration = 0.5f;  // 返回动画持续时间
    [SerializeField] private Ease bendEase = Ease.OutQuad;          // 弯曲缓动
    [SerializeField] private Ease returnEase = Ease.InOutQuad;      // 返回缓动
    
    [Header("玩家移动")]
    [SerializeField] private float teleportAnimationDuration = 0.3f; // 传送动画持续时间
    [SerializeField] private Ease teleportEase = Ease.OutBack;       // 传送缓动
    
    private BendablePole currentPole;
    private Sequence currentBendSequence;
    private Tween teleportTween;
    
    /// <summary>
    /// 动画化弯曲
    /// </summary>
    public void AnimateBending(BendablePole pole, Vector3 direction, float intensity)
    {
        currentPole = pole;
        
        // 结束当前动画
        currentBendSequence?.Kill();
        
        // 创建新的弯曲序列
        currentBendSequence = DOTween.Sequence();
        
        // 应用弯曲动画
        currentBendSequence.AppendCallback(() => {
            pole.ApplyBendingForce(direction, intensity);
        });
        
        // 设置动画属性
        currentBendSequence.SetEase(bendEase)
                          .SetRecyclable(true)
                          .SetUpdate(UpdateType.Fixed);
        
        // 播放动画
        currentBendSequence.Play();
    }
    
    /// <summary>
    /// 动画化返回
    /// </summary>
    public void AnimateReturn()
    {
        if (currentPole == null) return;
        
        // 结束当前动画
        currentBendSequence?.Kill();
        
        // 创建返回序列
        currentBendSequence = DOTween.Sequence();
        
        // 应用返回动画
        currentBendSequence.AppendCallback(() => {
            currentPole.ReleaseBending();
        });
        
        // 设置动画属性
        currentBendSequence.SetEase(returnEase)
                          .SetRecyclable(true)
                          .SetUpdate(UpdateType.Fixed);
        
        // 播放动画
        currentBendSequence.Play();
    }
    
    /// <summary>
    /// 动画化传送
    /// </summary>
    public void AnimateTeleport(Transform target, Vector3 destination, System.Action onComplete = null)
    {
        // 结束当前传送动画
        teleportTween?.Kill();
        
        // 创建传送动画
        teleportTween = target.DOMove(destination, teleportAnimationDuration)
                            .SetEase(teleportEase)
                            .OnComplete(() => {
                                onComplete?.Invoke();
                            });
        
        // 播放动画
        teleportTween.Play();
    }
    
    /// <summary>
    /// 停止所有动画
    /// </summary>
    public void StopAllAnimations()
    {
        currentBendSequence?.Kill();
        teleportTween?.Kill();
    }
    
    void OnDestroy()
    {
        StopAllAnimations();
    }
}