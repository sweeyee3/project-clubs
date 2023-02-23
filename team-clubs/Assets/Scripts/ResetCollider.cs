using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResetCollider : MonoBehaviour
{
    [SerializeField] private CurveHandler m_curveHandler;
    [SerializeField] private Vector3 m_size;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position, m_size);
    }
#endif

    private void Update()
    {
        var collided = Physics.OverlapBox(transform.position, m_size / 2);

        foreach(var collide in collided)
        {
            if (collide.tag == "Ball")
            {
                Debug.Log("death");
                m_curveHandler.Reset();
            }
        }
    }    
}
