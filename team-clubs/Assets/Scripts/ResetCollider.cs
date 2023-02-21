using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ResetCollider : MonoBehaviour
{
    [SerializeField] private Vector3 size;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(transform.position, size);
    }

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
