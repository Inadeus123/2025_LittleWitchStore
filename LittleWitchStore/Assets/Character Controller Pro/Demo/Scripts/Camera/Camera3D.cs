using System.Collections.Generic;
using UnityEngine;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Implementation;
using Lightbug.Utilities;

namespace Lightbug.CharacterControllerPro.Demo
{

    [AddComponentMenu("Character Controller Pro/Demo/Camera/Camera 3D")]
    [DefaultExecutionOrder(ExecutionOrder.CharacterGraphicsOrder + 100)]  // <--- Do your job after everything else
    public class Camera3D : MonoBehaviour
    {
        // ───── 3D 平台跳跃特化 ─────
        [Header("Platformer Zoom Presets")]
        public bool platformerZoomMode = true;          // 开关
        [Tooltip("预设的缩放档位（从近到远）")]
        public float[] zoomSteps = new float[] { 3f, 6f, 9f };
        [Tooltip("摇杆从中档推到最上/下需要的最小幅度")]
        public float stickThreshold = 0.4f;
        [Tooltip("同方向连续换挡的冷却时间（秒）")]
        public float zoomCooldown = 0.15f;
        float zoomCooldownTimer = 0f;
        int currentZoomIndex = 1;                       // 默认中档

        private bool isMoving;
        
        [Header("Platformer Pitch-Zoom Link")]
        public bool linkPitchToZoom = true; // 开关

        [Tooltip("最远 -> 最近 的俯仰角（度）。例：从 10° 抬到 35°")]
        public float farPitch = 10f;        // 对应 maxZoom
        public float nearPitch = 35f;       // 对应 minZoom

        [Tooltip("自定义插值曲线（横轴 = 0~1 的缩放归一化，纵轴 = 0~1 的插值因子）")]
        public AnimationCurve pitchCurve = AnimationCurve.EaseInOut(0,0,1,1); 

        
        [Header("Inputs")]

        [SerializeField]
        public InputHandlerSettings inputHandlerSettings = new InputHandlerSettings();

        [SerializeField]
        string axes = "Camera";

        [SerializeField]
        string zoomAxis = "Camera Zoom";

        [Header("Target")]


        [Tooltip("Select the graphics root object as your target, the one containing all the meshes, sprites, animated models, etc. \n\nImportant: This will be the considered as the actual target (visual element).")]
        [SerializeField]
        public Transform targetTransform = null;

        [SerializeField]
        Vector3 offsetFromHead = Vector3.zero;

        [Tooltip("The interpolation speed used when the height of the character changes.")]
        [SerializeField]
        float heightLerpSpeed = 10f;

        [Header("View")]

        public CameraMode cameraMode = CameraMode.ThirdPerson;

        [Header("First Person")]

        public bool hideBody = true;

        [SerializeField]
        public GameObject bodyObject = null;

        [Header("Yaw")]

        public bool updateYaw = true;

        public float yawSpeed = 180f;
        
        public float cameraAutoYawSpeed = 150f; // 移动相机自动转向的最大速度（度/秒）
        
        [Tooltip("移动相机自动转向的延迟时间（秒）")]
        float autoYawDelay = 0f;      // 持续移动后等待的时间
        [Tooltip("移动相机自动转向的计时器（秒）")]
        public float autoYawDelayMax = 1f;


        [Header("Pitch")]

        public bool updatePitch = true;

        [SerializeField]
        float initialPitch = 45f;

        public float pitchSpeed = 180f;

        [Range(1f, 85f)]
        public float maxPitchAngle = 80f;

        [Range(1f, 85f)]
        public float minPitchAngle = 80f;


        [Header("Roll")]
        public bool updateRoll = false;


        [Header("Zoom (Third person)")]

        public bool updateZoom = true;

        [Min(0f)]
        [SerializeField]
        float distanceToTarget = 5f;

        [Min(0f)]
        public float zoomInOutSpeed = 40f;

        [Min(0f)]
        public float zoomInOutLerpSpeed = 5f;

        [Min(0f)]
        public float minZoom = 2f;

        [Min(0.001f)]
        public float maxZoom = 12f;


        [Header("Collision")]

        public bool collisionDetection = true;
        public bool collisionAffectsZoom = false;
        public float detectionRadius = 0.5f;
        public LayerMask layerMask = 0;
        public bool considerKinematicRigidbodies = true;
        public bool considerDynamicRigidbodies = true;

        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────


        CharacterActor characterActor = null;
        Rigidbody characterRigidbody = null;

        float currentDistanceToTarget;
        float smoothedDistanceToTarget;

        float deltaYaw = 0f;
        float deltaPitch = 0f;
        float deltaZoom = 0f;
        
        Vector2 movementAxes = Vector3.up; //用于相机方向自动跟随移动
        
        Vector3 lerpedCharacterUp = Vector3.up;       

        Transform viewReference = null;
        Renderer[] bodyRenderers = null;
        RaycastHit[] hitsBuffer = new RaycastHit[10];
        RaycastHit[] validHits = new RaycastHit[10];
        Vector3 characterPosition = default(Vector3);
        float lerpedHeight;


        public enum CameraMode
        {
            FirstPerson,
            ThirdPerson,
        }


        public void ToggleCameraMode()
        {
            cameraMode = cameraMode == CameraMode.FirstPerson ? CameraMode.ThirdPerson : CameraMode.FirstPerson;
        }

        
        
        void OnValidate()
        {
            initialPitch = Mathf.Clamp(initialPitch, -minPitchAngle, maxPitchAngle);
        }

        void Awake()
        {
            Initialize(targetTransform);
        }

        public bool Initialize(Transform targetTransform)
        {
            if (targetTransform == null)
                return false;

            characterActor = targetTransform.GetComponentInBranch<CharacterActor>();

            if (characterActor == null || !characterActor.isActiveAndEnabled)
            {
                Debug.Log("The character actor component is null, or it is not active/enabled.");
                return false;
            }

            characterRigidbody = characterActor.GetComponent<Rigidbody>();

            inputHandlerSettings.Initialize(gameObject);

            GameObject referenceObject = new GameObject("Camera reference");
            viewReference = referenceObject.transform;

            if (bodyObject != null)
                bodyRenderers = bodyObject.GetComponentsInChildren<Renderer>();

            return true;
        }

        void OnEnable()
        {
            if (characterActor == null)
                return;

            characterActor.OnTeleport += OnTeleport;
        }

        void OnDisable()
        {
            if (characterActor == null)
                return;

            characterActor.OnTeleport -= OnTeleport;
        }

       

        void Start()
        {

            characterPosition = targetTransform.position;

            previousLerpedCharacterUp = targetTransform.up;
            lerpedCharacterUp = previousLerpedCharacterUp;


            currentDistanceToTarget = distanceToTarget;
            
            // 若平台模式，把初始距离匹配到最近的档位
            if (platformerZoomMode && zoomSteps.Length > 0)
            {
                float minDiff = float.MaxValue;
                for (int i = 0; i < zoomSteps.Length; i++)
                {
                    float diff = Mathf.Abs(zoomSteps[i] - currentDistanceToTarget);
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        currentZoomIndex = i;
                    }
                }
                currentDistanceToTarget = distanceToTarget = zoomSteps[currentZoomIndex];
            }

            
            smoothedDistanceToTarget = currentDistanceToTarget;

            viewReference.rotation = targetTransform.rotation;
            viewReference.Rotate(Vector3.right, initialPitch);

            lerpedHeight = characterActor.BodySize.y;
        }


        void Update()
        {
            if (targetTransform == null)
            {
                this.enabled = false;
                return;
            }

            Vector2 cameraAxes = inputHandlerSettings.InputHandler.GetVector2(axes); //读取axes的输入
             movementAxes = inputHandlerSettings.InputHandler.GetVector2("Movement");
            //Debug.Log("Movement Axes X: " + movementAxes.x);
            //Debug.Log("Movement Axes Y: " + movementAxes.y);
            /*if (updatePitch)
                deltaPitch = -cameraAxes.y;

            if (updateZoom)
                deltaZoom = -inputHandlerSettings.InputHandler.GetFloat(zoomAxis);*/
            
            // ─── Pitch 只在第一人称或非 Platformer 模式下允许 ───
            if (updatePitch && !platformerZoomMode)
                deltaPitch = -cameraAxes.y;
            
            if (updateYaw)
                deltaYaw = cameraAxes.x;

            // ─── Platformer Zoom：纵向摇杆换挡 ───
            if (platformerZoomMode && cameraMode == CameraMode.ThirdPerson)
                TryPlatformerZoom(cameraAxes.y);
            else if (updateZoom)                          // 仍保留普通滚轮缩放
                deltaZoom = -inputHandlerSettings.InputHandler.GetFloat(zoomAxis);


            // An input axis value (e.g. mouse x) usually gets accumulated over time. So, the higher the frame rate the smaller the value returned.
            // In order to prevent inconsistencies due to frame rate changes, the camera movement uses a fixed delta time, instead of the old regular
            // delta time.
            float dt = Time.fixedDeltaTime;

            UpdateCamera(dt);
        }

        private void TryPlatformerZoom(float stickY)
        {
            zoomCooldownTimer -= Time.unscaledDeltaTime;
            if (Mathf.Abs(stickY) < stickThreshold || zoomCooldownTimer > 0f)
                return;

            int dir = stickY > 0f ? -1 : 1;              // 上推 => 拉近（index--）
            int nextIndex = Mathf.Clamp(currentZoomIndex + dir, 0, zoomSteps.Length - 1);

            if (nextIndex != currentZoomIndex)
            {
                currentZoomIndex = nextIndex;
                currentDistanceToTarget = zoomSteps[currentZoomIndex];
                zoomCooldownTimer = zoomCooldown;
            }
        }


        void OnTeleport(Vector3 position, Quaternion rotation)
        {
            viewReference.rotation = rotation;
            transform.rotation = viewReference.rotation;

            lerpedCharacterUp = characterActor.Up;
            previousLerpedCharacterUp = lerpedCharacterUp;

        }


        Vector3 previousLerpedCharacterUp = Vector3.up;

        void HandleBodyVisibility()
        {
            if (cameraMode == CameraMode.FirstPerson)
            {
                if (bodyRenderers != null)
                    for (int i = 0; i < bodyRenderers.Length; i++)
                    {
                        if (bodyRenderers[i].GetType().IsSubclassOf(typeof(SkinnedMeshRenderer)))
                        {
                            SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)bodyRenderers[i];
                            if (skinnedMeshRenderer != null)
                                skinnedMeshRenderer.forceRenderingOff = hideBody;
                        }
                        else
                        {
                            bodyRenderers[i].enabled = !hideBody;
                        }
                    }

            }
            else
            {
                if (bodyRenderers != null)
                    for (int i = 0; i < bodyRenderers.Length; i++)
                    {
                        if (bodyRenderers[i] == null)
                            continue;

                        if (bodyRenderers[i].GetType().IsSubclassOf(typeof(SkinnedMeshRenderer)))
                        {
                            SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)bodyRenderers[i];
                            if (skinnedMeshRenderer != null)
                                skinnedMeshRenderer.forceRenderingOff = false;
                        }
                        else
                        {
                            bodyRenderers[i].enabled = true;
                        }


                    }
            }
        }

        

        void UpdateCamera(float dt)
        {
            // Body visibility ---------------------------------------------------------------------
            HandleBodyVisibility();

            // Rotation -----------------------------------------------------------------------------------------
            lerpedCharacterUp = targetTransform.up;

            // Rotate the reference based on the lerped character up vector 
            Quaternion deltaRotation = Quaternion.FromToRotation(previousLerpedCharacterUp, lerpedCharacterUp);
            previousLerpedCharacterUp = lerpedCharacterUp;

            viewReference.rotation = deltaRotation * viewReference.rotation;



            // Yaw rotation -----------------------------------------------------------------------------------------        
            viewReference.Rotate(lerpedCharacterUp, deltaYaw * yawSpeed * dt, Space.World);

            //镜头的自动校正
            if (Mathf.Abs(deltaYaw) > 0.01f)
            {
                autoYawDelay = autoYawDelayMax; // 有输入则重置计时器
            }
            else
            {
                autoYawDelay -= dt;
            }

            if (autoYawDelay <= 0f)
            {
                viewReference.Rotate(lerpedCharacterUp, movementAxes.x * cameraAutoYawSpeed * dt, Space.World);
            }

            
            
            // Pitch rotation -----------------------------------------------------------------------------------------            

            float angleToUp = Vector3.Angle(viewReference.forward, lerpedCharacterUp);


            float minPitch = -angleToUp + (90f - minPitchAngle);
            float maxPitch = 180f - angleToUp - (90f - maxPitchAngle);

            float pitchAngle = Mathf.Clamp(deltaPitch * pitchSpeed * dt, minPitch, maxPitch);
            viewReference.Rotate(Vector3.right, pitchAngle);

            // Roll rotation -----------------------------------------------------------------------------------------    
            if (updateRoll)
            {
                viewReference.up = lerpedCharacterUp;//Quaternion.FromToRotation( viewReference.up , lerpedCharacterUp ) * viewReference.up;
            }

            // Position of the target -----------------------------------------------------------------------
            characterPosition = targetTransform.position;

            lerpedHeight = Mathf.Lerp(lerpedHeight, characterActor.BodySize.y, heightLerpSpeed * dt);
            Vector3 targetPosition = characterPosition + targetTransform.up * lerpedHeight + targetTransform.TransformDirection(offsetFromHead);
            viewReference.position = targetPosition;

            Vector3 finalPosition = viewReference.position;

            // ------------------------------------------------------------------------------------------------------
            if (cameraMode == CameraMode.ThirdPerson)
            {
                currentDistanceToTarget += deltaZoom * zoomInOutSpeed * dt;
                currentDistanceToTarget = Mathf.Clamp(currentDistanceToTarget, minZoom, maxZoom);

                smoothedDistanceToTarget = Mathf.Lerp(smoothedDistanceToTarget, currentDistanceToTarget, zoomInOutLerpSpeed * dt);
              
                if (platformerZoomMode && linkPitchToZoom)
                {
                    float zoomNorm = Mathf.InverseLerp(maxZoom , minZoom , smoothedDistanceToTarget);   // 0=最远,1=最近
                    float t = pitchCurve.Evaluate(zoomNorm);
                    float targetPitch = Mathf.Lerp(farPitch , nearPitch , t);

                    // 用当前 right 轴旋转；保留 yaw/roll
                    Quaternion pitchRot = Quaternion.AngleAxis(targetPitch , viewReference.right);

                    // “把 forward 投影到水平面再抬头” 可避免累积误差
                    Vector3 upDir  = lerpedCharacterUp;
                    Vector3 fwdHor = Vector3.ProjectOnPlane(viewReference.forward , upDir).normalized;

                    viewReference.rotation = Quaternion.LookRotation(
                        pitchRot * fwdHor ,
                        upDir );
                }
                
                Vector3 displacement = -viewReference.forward * smoothedDistanceToTarget;

                if (collisionDetection)
                {
                    bool hit = DetectCollisions(ref displacement, targetPosition);

                    if (collisionAffectsZoom && hit)
                    {
                        currentDistanceToTarget = smoothedDistanceToTarget = displacement.magnitude;
                    }
                }

                finalPosition = targetPosition + displacement;
            }


            transform.position = finalPosition;
            transform.rotation = viewReference.rotation;

        }


        

        bool DetectCollisions(ref Vector3 displacement, Vector3 lookAtPosition)
        {
            int hits = Physics.SphereCastNonAlloc(
                lookAtPosition,
                detectionRadius,
                Vector3.Normalize(displacement),
                hitsBuffer,
                currentDistanceToTarget,
                layerMask,
                QueryTriggerInteraction.Ignore
            );

            // Order the results
            int validHitsNumber = 0;
            for (int i = 0; i < hits; i++)
            {
                RaycastHit hitBuffer = hitsBuffer[i];

                Rigidbody detectedRigidbody = hitBuffer.collider.attachedRigidbody;

                // Filter the results ---------------------------
                if (hitBuffer.distance == 0)
                    continue;

                if (detectedRigidbody != null)
                {
                    if (considerKinematicRigidbodies && !detectedRigidbody.isKinematic)
                        continue;

                    if (considerDynamicRigidbodies && detectedRigidbody.isKinematic)
                        continue;

                    if (detectedRigidbody == characterRigidbody)
                        continue;
                }

                //----------------------------------------------            
                validHits[validHitsNumber] = hitBuffer;
                validHitsNumber++;
            }

            if (validHitsNumber == 0)
                return false;


            float distance = Mathf.Infinity;
            for (int i = 0; i < validHitsNumber; i++)
            {
                RaycastHit hitBuffer = validHits[i];

                if (hitBuffer.distance < distance)
                    distance = hitBuffer.distance;
            }

            displacement = CustomUtilities.Multiply(Vector3.Normalize(displacement), distance);


            return true;
        }

        
    }

}
