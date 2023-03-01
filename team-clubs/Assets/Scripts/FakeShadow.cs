using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeShadow : MonoBehaviour
{
    [SerializeField] private AnimationCurve m_shadowDistanceScale;
    [SerializeField] private float m_initialShadowXScale = 1;
    [SerializeField] private float m_initialShadowZScale = 1;

    [SerializeField] private GameObject m_shadow;
    [SerializeField] private GameObject m_shadowSprite;
    [SerializeField] private LayerMask m_shadowHitMask;

    [SerializeField] private float m_raycastDistance = 99;
    [SerializeField] private Vector3 m_downVector = new Vector3(0, -1, 0);
    [SerializeField] private Vector3 m_offset;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + (m_downVector * m_raycastDistance));
    }
#endif

    private void Update()
    {
        RaycastHit shadowHit;
        bool isHit = Physics.Raycast(transform.position, m_downVector, out shadowHit, m_raycastDistance);        
        if (isHit)
        {            
            m_shadow.transform.position = shadowHit.point + m_offset;

            // adjust shadow size based on distance from hit point
            var cellCounts = SpawnManager.Instance.GetCellCounts();
            var maxCellPosition = SpawnManager.Instance.GetCellPosition((int)cellCounts.x, (int)cellCounts.y, (int)cellCounts.z);

            float yDistance = Mathf.Abs(maxCellPosition.y - (shadowHit.point + m_offset).y);
            float yCurrentDistance = Mathf.Abs(transform.position.y - (shadowHit.point + m_offset).y);

            var normalizedDistance = Mathf.InverseLerp(maxCellPosition.y, maxCellPosition.y + yDistance, maxCellPosition.y + yCurrentDistance);            
            var scaleRatio = m_initialShadowXScale / m_initialShadowZScale;
            var zScale = Mathf.Lerp(scaleRatio * 0.5f, scaleRatio, normalizedDistance);
            var xScale = zScale * m_initialShadowZScale;
            var yScale = m_shadowSprite.transform.localScale.y;

            m_shadowSprite.transform.localScale = new Vector3(xScale, yScale, zScale);
        }
    }
}
