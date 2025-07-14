using UnityEngine;
using DG.Tweening;

/// <summary>
/// 程序化变形增强器 - 为杆子添加更平滑的视觉效果
/// </summary>
public class ProceduralPoleEnhancer : MonoBehaviour
{
    [Header("变形设置")]
    [SerializeField] private float deformationIntensity = 0.5f;  // 变形强度
    [SerializeField] private AnimationCurve deformationCurve;     // 变形曲线
    [SerializeField] private float animationSpeed = 2f;          // 动画速度
    
    private BendablePole bendablePole;
    private Vector3[] originalPositions;
    private Transform[] segmentTransforms;
    
    void Start()
    {
        bendablePole = GetComponent<BendablePole>();
        CacheOriginalPositions();
    }
    
    void CacheOriginalPositions()
    {
        segmentTransforms = new Transform[transform.childCount];
        originalPositions = new Vector3[transform.childCount];
        
        for (int i = 0; i < transform.childCount; i++)
        {
            segmentTransforms[i] = transform.GetChild(i);
            originalPositions[i] = segmentTransforms[i].localPosition;
        }
    }
    
    /// <summary>
    /// 应用程序化变形
    /// </summary>
    public void ApplyProceduralDeformation(Vector2 bendInput)
    {
        for (int i = 0; i < segmentTransforms.Length; i++)
        {
            // 计算归一化高度
            float normalizedHeight = (float)i / segmentTransforms.Length;
            
            // 应用变形曲线
            float deformAmount = deformationCurve.Evaluate(normalizedHeight) * deformationIntensity;
            
            // 计算偏移
            Vector3 offset = new Vector3(
                bendInput.x * deformAmount,
                0,
                bendInput.y * deformAmount
            );
            
            // 应用DOTween平滑动画
            segmentTransforms[i].DOLocalMove(originalPositions[i] + offset, animationSpeed)
                .SetEase(Ease.OutQuad);
        }
    }
    
    /// <summary>
    /// 重置变形
    /// </summary>
    public void ResetDeformation()
    {
        for (int i = 0; i < segmentTransforms.Length; i++)
        {
            segmentTransforms[i].DOLocalMove(originalPositions[i], animationSpeed)
                .SetEase(Ease.InOutQuad);
        }
    }
}