using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Hoop : MonoBehaviour
{
    [SerializeField] private float m_hoopSuccess = 0.5f;

    [SerializeField] private Vector3 m_boxSize;
    [SerializeField] private Vector3 m_boxOffset;

    [SerializeField] private float m_height;
    [SerializeField] private float m_radius;
    [SerializeField] private float m_inner_radius;
    [SerializeField] private float m_fov;

    private Vector3 m_hoopGridIndex;

    public Vector3 HoopGridIndex
    {
        get
        {
            return m_hoopGridIndex;
        }
        set
        {
            m_hoopGridIndex = value;
        }
    }

    public Vector3 BoxSize
    {
        get
        {
            return m_boxSize;
        }
    }

    private void Update()
    {
        var collided = Physics.OverlapBox(transform.position + m_boxOffset, m_boxSize / 2);
        Vector3 norm = transform.up;
        Vector3 minLeftVector = FindWidthVectorByFov(m_fov, -1, m_radius, transform.forward, transform.right);
        Vector3 maxRightVector = FindWidthVectorByFov(m_fov, 1, m_radius, transform.forward, transform.right);

        Vector3 bottomVector = ((transform.position + (transform.forward * m_radius)) - transform.position).normalized;
        Vector3 topVector = ((transform.position + (norm * m_height) + (transform.forward * m_radius)) - (transform.position + (norm * m_height))).normalized;        

        foreach (var collide in collided)
        {
            Vector3 ovBottom = (collide.transform.position - transform.position);
            Vector3 ovTop = (collide.transform.position - (transform.position + (norm * m_height)));

            bool isWithinRight = CrossSign(ovBottom.normalized, maxRightVector, norm) > 0;
            bool isWithinLeft = CrossSign(ovBottom.normalized, minLeftVector, norm) < 0;
            bool isWithinBottom = CrossSign(ovBottom.normalized, bottomVector, transform.right, true) > 0;
            bool isWithinTop = CrossSign(ovTop.normalized, topVector, transform.right) < 0;
            bool isInsideRadius = IsInsideCylinderRadius(transform.position, collide.transform.position, norm, m_radius);
            bool isOutsideInnerRadius = !IsInsideCylinderRadius(transform.position, collide.transform.position, norm, m_inner_radius);
            bool isFromTop = IsFromTop(collide.GetComponentInParent<Ball>().CurrentVelocity, m_hoopSuccess);

            if (isWithinRight && isWithinLeft && isWithinBottom && isWithinTop && isInsideRadius && isOutsideInnerRadius && isFromTop)
            {
                collide.GetComponentInParent<Ball>().Reset();
                GameManager.Instance.CurrentScore += 1;
                SpawnManager.Instance.Remove(this);                
                // TODO: spawn effects!

                // TODO: music effects!
            }
        }        
    }

    private void OnDrawGizmos()
    {
        var collided = Physics.OverlapBox(transform.position + m_boxOffset, m_boxSize / 2);
        foreach (var collide in collided)
        {            
            DrawCylinderTrigger(collide.gameObject);
        }

        if (collided.Length <= 0)
        {            
            DrawCylinderTrigger(null);
        }

        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + m_boxOffset, m_boxSize);
    }

    private bool IsInsideCylinderRadius(Vector3 self, Vector3 other, Vector3 normal, float radius)
    {
        // take transform, project it down to radial plane, defined by plane normal and position
        Vector3 planeNormal = normal;
        Vector3 otherDirection = other - self;

        float otherDotNormal = Vector3.Dot(otherDirection, planeNormal);
        float vecProj = (planeNormal * otherDotNormal).magnitude;
        float radialDist = Mathf.Sqrt(Mathf.Pow(otherDirection.magnitude, 2) - Mathf.Pow(vecProj, 2));

        return radialDist <= radius;
    }

    private bool IsFromTop(Vector3 otherDir, float success)
    {                
        var dot = Vector3.Dot(otherDir, -transform.up);

        return dot > success;
    }

    float CrossSign(Vector3 ov, Vector3 edge, Vector3 comparison, bool drawDebug = false)
    {
        Vector3 ovCrossEdge = Vector3.Cross(ov, edge);        

        float crossSign = Mathf.Sign(Vector3.Dot(ovCrossEdge, comparison));

        return crossSign;
    }

    Vector3 FindWidthVector(float width, float range, Vector3 forward, Vector3 right)
    {
        Vector3 widthRange = Vector3.zero;

        var projVec = width * right;
        widthRange = projVec + (forward * (1 - Mathf.Abs(width)) * range);

        return widthRange.normalized;
    }

    Vector3 FindWidthVectorByFov(float fov, float sign, float range, Vector3 forward, Vector3 right)
    {
        Vector3 widthRange = Vector3.zero;

        fov = Mathf.Deg2Rad * fov;

        widthRange = FindWidthVector(sign * Mathf.Cos(fov / 2), range, forward, right);

        return widthRange;
    }

    private void DrawCylinderTrigger(GameObject o)
    {
        Vector3 norm = transform.up;

        Vector3 minLeftVector = FindWidthVectorByFov(m_fov, -1, m_radius, transform.forward, transform.right);
        Vector3 maxRightVector = FindWidthVectorByFov(m_fov, 1, m_radius, transform.forward, transform.right);

        Vector3 bottomVector = ((transform.position + (transform.forward * m_radius)) - transform.position).normalized;
        Vector3 topVector = ((transform.position + (norm * m_height) + (transform.forward * m_radius)) - (transform.position + (norm * m_height))).normalized;

        if (o != null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position, o.transform.position);
            Gizmos.DrawLine(transform.position + (norm * m_height), o.transform.position);

            // find cross product with minleft and max right, < 0 outside, > 0 inside
            Vector3 ovBottom = (o.transform.position - transform.position);
            Vector3 ovTop = (o.transform.position - (transform.position + (norm * m_height)));

            bool isWithinRight = CrossSign(ovBottom.normalized, maxRightVector, norm) > 0;
            bool isWithinLeft = CrossSign(ovBottom.normalized, minLeftVector, norm) < 0;
            bool isWithinBottom = CrossSign(ovBottom.normalized, bottomVector, transform.right, true) > 0;
            bool isWithinTop = CrossSign(ovTop.normalized, topVector, transform.right) < 0;
            bool isInsideRadius = IsInsideCylinderRadius(transform.position, o.transform.position, norm, m_radius);
            bool isOutsideInnerRadius = !IsInsideCylinderRadius(transform.position, o.transform.position, norm, m_inner_radius);
            bool isFromTop = IsFromTop(o.GetComponentInParent<Ball>().CurrentVelocity, m_hoopSuccess);

            if (isWithinRight && isWithinLeft && isWithinBottom && isWithinTop && isInsideRadius && isOutsideInnerRadius && isFromTop) Handles.color = Color.green;
            else Handles.color = Color.red;

            Handles.DrawWireCube(o.transform.position, Vector3.one);
        }

        // draw cylinder here
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position + (minLeftVector * m_inner_radius), transform.position + (minLeftVector * m_radius));
        Gizmos.DrawLine(transform.position + (maxRightVector * m_inner_radius), transform.position + (maxRightVector * m_radius));

        Gizmos.DrawLine(transform.position + (norm * m_height) + (minLeftVector * m_inner_radius), transform.position + (norm * m_height) + (minLeftVector * m_radius));
        Gizmos.DrawLine(transform.position + (norm * m_height) + (maxRightVector * m_inner_radius), transform.position + (norm * m_height) + (maxRightVector * m_radius));

        Handles.color = Color.white;
        Handles.DrawWireArc(transform.position, norm, maxRightVector, -Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(maxRightVector, transform.forward)), m_radius);
        Handles.DrawWireArc(transform.position, norm, minLeftVector, Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(minLeftVector, transform.forward)), m_radius);

        Handles.DrawWireArc(transform.position, norm, maxRightVector, -Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(maxRightVector, transform.forward)), m_inner_radius);
        Handles.DrawWireArc(transform.position, norm, minLeftVector, Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(minLeftVector, transform.forward)), m_inner_radius);

        Handles.DrawWireArc(transform.position + (norm * m_height), norm, maxRightVector, -Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(maxRightVector, transform.forward)), m_radius);
        Handles.DrawWireArc(transform.position + (norm * m_height), norm, minLeftVector, Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(minLeftVector, transform.forward)), m_radius);

        Handles.DrawWireArc(transform.position + (norm * m_height), norm, maxRightVector, -Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(maxRightVector, transform.forward)), m_inner_radius);
        Handles.DrawWireArc(transform.position + (norm * m_height), norm, minLeftVector, Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(minLeftVector, transform.forward)), m_inner_radius);

        //Gizmos.DrawLine(transform.position, transform.position + (norm * m_height));

        Gizmos.DrawLine(transform.position + (minLeftVector * m_inner_radius), transform.position + (minLeftVector * m_inner_radius) + (norm * m_height));
        Gizmos.DrawLine(transform.position + (maxRightVector * m_inner_radius), transform.position + (maxRightVector * m_inner_radius) + (norm * m_height));

        Gizmos.DrawLine(transform.position + (minLeftVector * m_radius), transform.position + (minLeftVector * m_radius) + (norm * m_height));
        Gizmos.DrawLine(transform.position + (maxRightVector * m_radius), transform.position + (maxRightVector * m_radius) + (norm * m_height));

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (norm * 2));
    }
}