using UnityEngine;

/// <summary>
/// 摆荡视觉效果
/// </summary>
public class SwingVisuals : MonoBehaviour
{
    [Header("= 锁链设置 =")]
    [SerializeField] private Material chainMaterial;
    [SerializeField] private float chainWidth = 0.15f;
    [SerializeField] private AnimationCurve chainWidthCurve = AnimationCurve.Linear(0, 1, 1, 0.5f);
    
    [Header("= 特效 =")]
    [SerializeField] private ParticleSystem deployEffect;     // 部署特效
    [SerializeField] private ParticleSystem swingTrailEffect; // 摆荡拖尾
    [SerializeField] private ParticleSystem recallEffect;     // 召回特效
    [SerializeField] private ParticleSystem impactEffect;     // 撞击特效
    
    private LineRenderer chainRenderer;
    private FamiliarController familiarController;
    private SwingPhysics swingPhysics;
    
    void Awake()
    {
        familiarController = GetComponent<FamiliarController>();
        swingPhysics = GetComponent<SwingPhysics>();
        
        SetupChainRenderer();
        SubscribeEvents();
    }
    
    void SetupChainRenderer()
    {
        // 获取或创建锁链渲染器
        chainRenderer = GetComponentInChildren<LineRenderer>();
        if (chainRenderer == null)
        {
            GameObject chainObj = new GameObject("ChainVisual");
            chainObj.transform.SetParent(transform);
            chainRenderer = chainObj.AddComponent<LineRenderer>();
        }
        
        // 设置锁链外观
        if (chainMaterial != null)
            chainRenderer.material = chainMaterial;
        
        chainRenderer.startWidth = chainWidth;
        chainRenderer.endWidth = chainWidth * 0.5f;
        chainRenderer.numCapVertices = 5;
        chainRenderer.numCornerVertices = 5;
        chainRenderer.widthCurve = chainWidthCurve;
    }
    
    void SubscribeEvents()
    {
        if (familiarController != null)
        {
            familiarController.OnFamiliarDeployed += OnDeploy;
            familiarController.OnRecallImpact += OnImpact;
        }
    }
    
    void OnDeploy(Vector3 position)
    {
        if (deployEffect != null)
        {
            deployEffect.transform.position = position;
            deployEffect.Play();
        }
        
        if (swingTrailEffect != null)
        {
            swingTrailEffect.Play();
        }
    }
    
    void OnImpact(Vector3 force)
    {
        if (impactEffect != null)
        {
            impactEffect.transform.position = transform.position;
            impactEffect.Play();
        }
        
        // 相机震动（如果有）
        //CameraShakeManager.Instance?.ShakeCamera(0.3f, force.magnitude * 0.02f);
    }
    
    void Update()
    {
        // 更新锁链弯曲
        if (chainRenderer != null && familiarController != null && familiarController.IsDeployed)
        {
            UpdateChainCurve();
        }
    }
    
    void UpdateChainCurve()
    {
        // 创建弯曲的锁链效果
        int segments = 10;
        chainRenderer.positionCount = segments;
        
        Vector3 start = transform.position + Vector3.up * 0.5f;
        Vector3 end = familiarController.AnchorPosition;
        
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);
            Vector3 point = Vector3.Lerp(start, end, t);
            
            // 添加一点下垂效果
            float sag = Mathf.Sin(t * Mathf.PI) * 0.3f;
            point.y -= sag;
            
            chainRenderer.SetPosition(i, point);
        }
    }
}