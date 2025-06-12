using UnityEngine;
using System.Collections.Generic;

public class CaptureVolumeProjection : MonoBehaviour
{
    [Header("Volume References")]
    public Transform projectionVolume;       // Reference to the ProjectionVolume transform
    public Material projectionMaterial;      // Reference to the Material with VolumeClippingShader

    // Dictionary to keep track of original objects and their clone objects
    private Dictionary<GameObject, GameObject> cloneMap = new Dictionary<GameObject, GameObject>();

    // We’ll use the BoxCollider on this object (CaptureVolume) as trigger
    private BoxCollider triggerCollider;
    // Cached transform of CaptureVolume for convenience
    private Transform captureTransform;

    void Start()
    {
        captureTransform = this.transform;
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null || !triggerCollider.isTrigger)
        {
            Debug.LogError("CaptureVolume requires a BoxCollider set as Trigger.");
        }

        // Initialize any objects that start inside the volume
        Collider[] initialColliders = Physics.OverlapBox(captureTransform.position, triggerCollider.size * 0.5f, captureTransform.rotation);
        // Physics.OverlapBox returns all colliders overlapping an AABB; we provide half-extent (size*0.5) and rotation for oriented volume
        foreach (Collider col in initialColliders)
        {
            TryAddObject(col.gameObject);
        }
    }

    // When an object enters the capture volume trigger
    private void OnTriggerEnter(Collider other)
    {
        TryAddObject(other.gameObject);
    }

    // When an object exits the capture volume trigger
    private void OnTriggerExit(Collider other)
    {
        RemoveObject(other.gameObject);
    }

    // Tries to add (capture) an object entering the volume
    private void TryAddObject(GameObject obj)
    {
        // Ignore the ProjectionVolume itself or any clones to prevent recursive capturing
        if (obj == projectionVolume.gameObject) return;
        if (obj.CompareTag("ProjectionClone")) return; // We'll mark clones with this tag
        if (cloneMap.ContainsKey(obj)) return;  // Already have a clone for this object

        // Create a clone of the object and set it up
        GameObject clone = Instantiate(obj);  // duplicate the entire object hierarchy
        clone.name = obj.name + "_ProjectionClone";
        // Mark clone with a tag to identify it (create a tag "ProjectionClone" in Unity editor first)
        clone.tag = "ProjectionClone";

        // Remove any scripts or components on the clone that might cause unintended behavior
        CleanupCloneComponents(clone);

        // Parent the clone under the ProjectionVolume for organizational clarity (optional)
        clone.transform.SetParent(projectionVolume, worldPositionStays: true);

        // Initialize the clone's transform to match the relative position/rotation of original within the volumes
        UpdateCloneTransform(obj.transform, clone.transform);

        // Apply the projection material to all renderers of the clone, preserving textures
        ApplyProjectionMaterialToClone(clone);

        // Ensure clone colliders are configured for interaction (make kinematic if moving)
        SetupCloneCollisions(clone);

        // Store the mapping
        cloneMap[obj] = clone;
    }

    // Removes (destroys) the clone when the original leaves the volume
    private void RemoveObject(GameObject obj)
    {
        if (!cloneMap.ContainsKey(obj)) return;
        GameObject clone = cloneMap[obj];
        Destroy(clone);
        cloneMap.Remove(obj);
    }

    // Update is called once per frame to sync clones
    void LateUpdate()
    {
        // Update volume info to shader (so clipping region matches CV current transform)
        UpdateShaderVolumeParams();

        // Update each clone's transform to follow its original
        List<GameObject> originals = new List<GameObject>(cloneMap.Keys);
        foreach (GameObject orig in originals)
        {
            // Original might be destroyed or deactivated
            if (orig == null || !orig.activeInHierarchy)
            {
                RemoveObject(orig);
            }
            else
            {
                GameObject clone = cloneMap[orig];
                UpdateCloneTransform(orig.transform, clone.transform);
            }
        }
    }

    // Synchronize a clone's position/rotation/scale to match the original relative to the volumes
    private void UpdateCloneTransform(Transform origTransform, Transform cloneTransform)
    {
        // Calculate the object's position relative to CaptureVolume, then project that into ProjectionVolume
        Vector3 localPos = captureTransform.InverseTransformPoint(origTransform.position);
        Vector3 projectedWorldPos = projectionVolume.TransformPoint(localPos);
        // Calculate relative rotation: rotation difference from CV to the object, then apply to PV
        Quaternion localRot = Quaternion.Inverse(captureTransform.rotation) * origTransform.rotation;
        Quaternion projectedWorldRot = projectionVolume.rotation * localRot;
        // Calculate relative scale if volumes have different scale factors (assuming uniform scaling for simplicity)
        Vector3 origScale = origTransform.lossyScale;
        Vector3 captureScale = captureTransform.lossyScale;
        Vector3 projScale = projectionVolume.lossyScale;
        // Compute clone scale such that origScale is transferred relative to volume scales
        Vector3 relativeScaleFactor = new Vector3(
            origScale.x * (projScale.x / captureScale.x),
            origScale.y * (projScale.y / captureScale.y),
            origScale.z * (projScale.z / captureScale.z)
        );

        // Apply to clone
        cloneTransform.position = projectedWorldPos;
        cloneTransform.rotation = projectedWorldRot;
        cloneTransform.localScale = relativeScaleFactor;
    }

    // Apply the projection (clipping) material to all renderers on the clone, copying textures/colors from the original's materials
    private void ApplyProjectionMaterialToClone(GameObject clone)
    {
        // Find all MeshRenderer or SkinnedMeshRenderer components in the clone
        Renderer[] renderers = clone.GetComponentsInChildren<Renderer>(includeInactive: true);
        foreach (Renderer rend in renderers)
        {
            // Get the original materials from this renderer (from the source object)
            // We assume the clone's renderer materials are identical copies of the original's at this point
            Material[] originalMats = rend.sharedMaterials;
            Material[] newMats = new Material[originalMats.Length];
            for (int m = 0; m < originalMats.Length; m++)
            {
                Material origMat = originalMats[m];
                // Create an instance of the projection material for this sub-material
                Material projMatInstance = new Material(projectionMaterial);
                // If the original material has a main texture, copy it
                if (origMat.HasProperty("_MainTex"))
                    projMatInstance.mainTexture = origMat.mainTexture;
                // If original has a base color (common in URP Lit), copy that too
                if (origMat.HasProperty("_BaseColor"))
                    projMatInstance.SetColor("_Color", origMat.GetColor("_BaseColor"));
                else if (origMat.HasProperty("_Color"))
                    projMatInstance.SetColor("_Color", origMat.GetColor("_Color"));
                // Assign the new material to the array
                newMats[m] = projMatInstance;
            }
            // Set the clone's renderer materials to the new projection materials
            rend.materials = newMats;
        }
    }

    //需要修改
    // Remove or adjust components on the clone to prevent unwanted behavior (like AI, duplicate scripts, etc.)
    private void CleanupCloneComponents(GameObject clone)
    {
        // We will disable or destroy non-essential components on clones, keeping only Transform, Renderers, Colliders, etc.
        // For example:
        MonoBehaviour[] scripts = clone.GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour mb in scripts)
        {
            // If the script is this same projection script (shouldn't be on clone) or other game logic scripts, disable them.
            // You can refine this check as needed for your project (e.g., exclude specific components).
            mb.enabled = false;
        }
        // Remove any AudioListener or duplicate Camera if present (just in case)
        foreach (var aud in clone.GetComponentsInChildren<AudioListener>()) Destroy(aud);
        foreach (var cam in clone.GetComponentsInChildren<Camera>()) Destroy(cam);
        // Note: We do not remove Colliders or Renderers here, since we need them.
    }

    // Setup colliders and rigidbodies on the clone for correct physics interaction
    private void SetupCloneCollisions(GameObject clone)
    {
        // Add a kinematic Rigidbody to each root collider on the clone, if not already
        // This ensures moving colliders don't act as static (for physics stability):contentReference[oaicite:3]{index=3}
        Rigidbody rb = clone.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = clone.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;
        // Also, for any child colliders that were on the original, ensure they are not triggers (so player can collide) unless intended.
        Collider[] colliders = clone.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            // If the original collider was a trigger (for some gameplay reason), you might mirror that. Otherwise, we keep them solid.
            // Here we simply ensure the clone colliders are enabled (they should be by default from Instantiate).
            col.enabled = true;
        }
    }

    // Update the shader parameters (_CV_WorldToLocal matrix and _HalfSize) each frame (or whenever volume moves)
    private void UpdateShaderVolumeParams()
    {
        // Compute the world-to-local matrix of the capture volume
        Matrix4x4 worldToLocal = captureTransform.worldToLocalMatrix;
        // Compute half-size (half extents) of the capture volume in its local space
        Vector3 halfSize = Vector3.one * 0.5f;
        // If using the scale of the capture cube to define volume size:
        // The BoxCollider's size property is likely (1,1,1) with scaling applied via transform,
        // so use the scaled extents: captureTransform.lossyScale * 0.5f
        halfSize = Vector3.Scale(triggerCollider.size, captureTransform.lossyScale) * 0.5f;
        // Set these as global shader parameters so all projection materials use them
        Shader.SetGlobalMatrix("_CV_WorldToLocal", worldToLocal);
        Shader.SetGlobalVector("_HalfSize", new Vector4(halfSize.x, halfSize.y, halfSize.z, 0));
    }
}

