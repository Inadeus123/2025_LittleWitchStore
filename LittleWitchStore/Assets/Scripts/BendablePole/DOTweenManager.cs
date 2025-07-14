using UnityEngine;
using DG.Tweening;

/// <summary>
/// DOTween管理器 - 初始化和管理DOTween设置
/// </summary>
public class DOTweenManager : MonoBehaviour
{
    [Header("DOTween设置")]
    [SerializeField] private bool recycleAllByDefault = true;    // 默认回收
    [SerializeField] private bool useSafeMode = true;            // 安全模式
    [SerializeField] private LogBehaviour logBehaviour = LogBehaviour.ErrorsOnly;
    
    [Header("容量设置")]
    [SerializeField] private int maxTweeners = 500;              // 最大补间器数量
    [SerializeField] private int maxSequences = 100;            // 最大序列数量
    
    void Awake()
    {
        // 初始化DOTween
        DOTween.Init(recycleAllByDefault, useSafeMode, logBehaviour)
            .SetCapacity(maxTweeners, maxSequences);
        
        // 设置全局时间缩放
        DOTween.timeScale = 1f;
        
        // 启用安全模式
        DOTween.useSafeMode = useSafeMode;
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        // 应用暂停时暂停所有补间
        if (pauseStatus)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }
    
    void OnDestroy()
    {
        // 清理所有补间
        DOTween.KillAll();
    }
}