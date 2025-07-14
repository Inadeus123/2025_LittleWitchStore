using UnityEngine;

/// <summary>
/// 杆子检测器 - 处理玩家与杆子的交互
/// </summary>
public class PoleDetector : MonoBehaviour
{
    [Header("检测设置")]
    [SerializeField] private float detectionRadius = 1f;         // 检测半径
    [SerializeField] private LayerMask poleLayerMask;            // 杆子层掩码
    [SerializeField] private string poleTag = "Pole";            // 杆子标签
    
    // 事件委托
    public System.Action<BendablePole> OnPoleDetected;
    public System.Action OnPoleLeft;
    
    private BendablePole currentPole;
    private bool isNearPole;
    
    void Update()
    {
        CheckPoleProximity();
    }
    
    /// <summary>
    /// 检查杆子接近度
    /// </summary>
    private void CheckPoleProximity()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, poleLayerMask);
        
        bool foundPole = false;
        
        foreach (var collider in colliders)
        {
            if (collider.CompareTag(poleTag))
            {
                BendablePole pole = collider.GetComponentInParent<BendablePole>();
                if (pole != null)
                {
                    if (currentPole != pole)
                    {
                        currentPole = pole;
                        isNearPole = true;
                        OnPoleDetected?.Invoke(currentPole);
                    }
                    foundPole = true;
                    break;
                }
            }
        }
        
        // 如果之前靠近杆子但现在不靠近了
        if (!foundPole && isNearPole)
        {
            isNearPole = false;
            currentPole = null;
            OnPoleLeft?.Invoke();
        }
    }
    
    /// <summary>
    /// 获取当前杆子
    /// </summary>
    public BendablePole GetCurrentPole()
    {
        return currentPole;
    }
    
    /// <summary>
    /// 是否靠近杆子
    /// </summary>
    public bool IsNearPole()
    {
        return isNearPole;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}