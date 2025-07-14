using System;
using UnityEngine;
using DG.Tweening;

public class ScaleAnimation : MonoBehaviour
{
    [SerializeField] private Transform sphereTransform; // 球体的Transform
    [SerializeField] private float maxScale = 2f; // 最大缩放值
    [SerializeField] private float scaleDuration = 1f; // 缩放动画持续时间
    [SerializeField] private float holdDuration = 2f; // 保持最大缩放的时间
    [SerializeField] private Ease easeType = Ease.InOutSine; // 缓动类型
    [SerializeField] private ShaderInteractorPosition shaderInteractorPosition; 

    private Vector3 originalScale; // 初始缩放值
    private float originalInteractorRadius; // 初始Shader交互位置
    private bool isAnimating = false; // 防止重复动画

    void Start()
    {
        // 存储初始缩放值
        if (sphereTransform != null)
            originalScale = sphereTransform.localScale;
        
        sphereTransform.gameObject.SetActive(false);
        shaderInteractorPosition.radius = sphereTransform.localScale.x /2 ;
        originalInteractorRadius = shaderInteractorPosition.radius;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) //暂时使用鼠标上的按键T
        {
            StartScaleAnimation();
        }
    }

    // 按钮调用此方法
    public void StartScaleAnimation()
    {
        if (isAnimating || sphereTransform == null) return;

        isAnimating = true;

        sphereTransform.gameObject.SetActive(true);
        // 创建DOTween序列
        Sequence scaleSequence = DOTween.Sequence();

        // 放大动画
        scaleSequence.Append(sphereTransform.DOScale(maxScale, scaleDuration)
            .SetEase(easeType));
        scaleSequence.Join(DOTween.To(()=> shaderInteractorPosition.radius,
            value => shaderInteractorPosition.radius = value, 
            maxScale / 2, scaleDuration).SetEase(easeType));    

        // 保持最大缩放
        scaleSequence.AppendInterval(holdDuration);

        // 缩小动画
        scaleSequence.Append(sphereTransform.DOScale(originalScale, scaleDuration)
            .SetEase(easeType));
        scaleSequence.Join(DOTween.To(() => shaderInteractorPosition.radius,
            value => shaderInteractorPosition.radius = value, 
            originalInteractorRadius, scaleDuration).SetEase(easeType));

        // 动画完成回调
        
        scaleSequence.OnComplete(() =>
        {
            isAnimating = false;
            sphereTransform.gameObject.SetActive(false);
        });
    }
}