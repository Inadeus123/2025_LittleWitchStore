using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CaptureVolume : MonoBehaviour
{
    private BoxCollider captureBoxCollider;
    private Vector3 captureBoxSize;
    
    void OnDrawGizmos() {
        Gizmos.color = new Color(0,1,0,0.3f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(5,3,5));
    }
    
    private Vector3 GetCaptureBoxColliderSize()
    {
        if (captureBoxCollider == null)
        {
            captureBoxCollider = GetComponent<BoxCollider>();
            Vector3 worldSize = Vector3.Scale(captureBoxCollider.size, captureBoxCollider.transform.lossyScale);
            return worldSize;
        }

        return Vector3.zero;
    }
}
