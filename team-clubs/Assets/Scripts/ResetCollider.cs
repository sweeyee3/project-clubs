using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResetCollider : MonoBehaviour
{
    [SerializeField] private Vector3 size;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position, size);
    }
#endif

    private void Update()
    {
        var collided = Physics.OverlapBox(transform.position, size / 2);

        foreach(var collide in collided)
        {
            if (collide.tag == "Ball")
            {
                collide.GetComponentInParent<Ball>().Reset();
            }
        }
    }    
}
