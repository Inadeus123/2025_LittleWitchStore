using UnityEngine;
using System.Collections;

/// <summary>
/// 使魔控制器 - 管理使魔的所有行为
/// </summary>
public class FamiliarController : MonoBehaviour
{
    [Header("= 基础设置 =")]
    [SerializeField] private GameObject familiarPrefab;       // 使魔预制体
    [SerializeField] private float anchorHeight = 5f;         // 挂点高度（相对于角色）
    [SerializeField] private float anchorForwardOffset = 2f;  // 挂点前方偏移
    
    [Header("= 召回设置 =")]
    [SerializeField] private float recallSpeed = 30f;         // 召回飞行速度
    [SerializeField] private float recallImpactForce = 15f;  // 撞击力度
    [SerializeField] private float recallImpactRadius = 1f;  // 撞击检测半径
    
    // 组件引用
    private GameObject currentFamiliar;
    private Transform playerTransform;
    private LineRenderer chainRenderer;
    
    // 状态
    public bool IsDeployed { get; private set; }
    public bool IsRecalling { get; private set; }
    public Vector3 AnchorPosition { get; private set; }
    
    // 事件
    public System.Action<Vector3> OnFamiliarDeployed;
    public System.Action OnFamiliarRecalled;
    public System.Action<Vector3> OnRecallImpact;
    
    void Awake()
    {
        playerTransform = transform;
        SetupChainRenderer();
    }
    
    /// <summary>
    /// 设置锁链渲染器
    /// </summary>
    private void SetupChainRenderer()
    {
        GameObject chainObj = new GameObject("ChainRenderer");
        chainObj.transform.SetParent(transform);
        chainRenderer = chainObj.AddComponent<LineRenderer>();
        
        // 设置锁链外观
        chainRenderer.startWidth = 0.1f;
        chainRenderer.endWidth = 0.1f;
        chainRenderer.material = new Material(Shader.Find("Sprites/Default"));
        chainRenderer.startColor = Color.gray;
        chainRenderer.endColor = Color.gray;
        chainRenderer.enabled = false;
    }
    
    /// <summary>
    /// 部署使魔到挂点
    /// </summary>
    public void DeployFamiliar()
    {
        if (IsDeployed) return;
        
        // 计算挂点位置
        Vector3 deployDirection = playerTransform.forward;
        deployDirection.y = 0.5f; // 稍微向上
        deployDirection.Normalize();
        
        AnchorPosition = playerTransform.position + 
                        Vector3.up * anchorHeight + 
                        deployDirection * anchorForwardOffset;
        
        // 创建使魔
        if (familiarPrefab != null)
        {
            currentFamiliar = Instantiate(familiarPrefab, AnchorPosition, Quaternion.identity);
        }
        else
        {
            // 如果没有预制体，创建默认球体
            currentFamiliar = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            currentFamiliar.transform.position = AnchorPosition;
            currentFamiliar.transform.localScale = Vector3.one * 0.5f;
            
            // 设置为触发器
            Collider col = currentFamiliar.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }
        
        currentFamiliar.name = "Familiar_Anchor";
        
        // 启用锁链
        chainRenderer.enabled = true;
        
        IsDeployed = true;
        OnFamiliarDeployed?.Invoke(AnchorPosition);
        
        Debug.Log($"使魔已部署到: {AnchorPosition}");
    }
    
    /// <summary>
    /// 召回使魔
    /// </summary>
    public void RecallFamiliar()
    {
        if (!IsDeployed || IsRecalling) return;
        
        IsRecalling = true;
        StartCoroutine(RecallCoroutine());
    }
    
    /// <summary>
    /// 召回协程
    /// </summary>
    private IEnumerator RecallCoroutine()
    {
        if (currentFamiliar == null) yield break;
        
        Vector3 startPos = currentFamiliar.transform.position;
        float startTime = Time.time;
        
        while (currentFamiliar != null)
        {
            // 计算飞向玩家的方向
            Vector3 toPlayer = playerTransform.position - currentFamiliar.transform.position;
            float distance = toPlayer.magnitude;
            
            // 检查是否撞击到玩家
            if (distance < recallImpactRadius)
            {
                // 计算撞击力方向
                Vector3 impactDirection = toPlayer.normalized;
                impactDirection.y = 0.5f; // 稍微向上的撞击
                impactDirection.Normalize();
                
                // 触发撞击事件
                OnRecallImpact?.Invoke(impactDirection * recallImpactForce);
                
                // 清理使魔
                ReleaseFamiliar();
                break;
            }
            
            // 移动使魔
            currentFamiliar.transform.position = Vector3.MoveTowards(
                currentFamiliar.transform.position,
                playerTransform.position,
                recallSpeed * Time.deltaTime
            );
            
            yield return null;
        }
        
        IsRecalling = false;
        OnFamiliarRecalled?.Invoke();
    }
    
    /// <summary>
    /// 释放使魔
    /// </summary>
    public void ReleaseFamiliar()
    {
        if (currentFamiliar != null)
        {
            Destroy(currentFamiliar);
            currentFamiliar = null;
        }
        
        chainRenderer.enabled = false;
        IsDeployed = false;
        IsRecalling = false;
        AnchorPosition = Vector3.zero;
    }
    
    /// <summary>
    /// 更新锁链渲染
    /// </summary>
    void LateUpdate()
    {
        if (IsDeployed && chainRenderer != null && currentFamiliar != null)
        {
            chainRenderer.SetPosition(0, playerTransform.position + Vector3.up * 0.5f);
            chainRenderer.SetPosition(1, currentFamiliar.transform.position);
        }
    }
    
    void OnDestroy()
    {
        ReleaseFamiliar();
    }
}