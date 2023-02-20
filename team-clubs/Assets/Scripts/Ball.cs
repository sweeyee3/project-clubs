using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Ball : MonoBehaviour
{
    [SerializeField] private float m_ballSpeed = 10;
    [SerializeField] [Range(0, 1)] private float m_angleSpeed = 0.5f;

    [SerializeField] private float m_gravity = -5;    

    [SerializeField] private Vector3 m_initialVelocityDirection;
    [SerializeField] private Vector3 m_currentVelocity;
    [SerializeField][Range(0, 1)] private float m_currentAngle;    

    [SerializeField] private float m_accumulatedAngleTime = 0;
    [SerializeField] private float m_accmulatedBallTime = 0;
    
    [SerializeField] private float m_totalBallTime;
    [SerializeField] private float m_totalAngleTime;
 
    [SerializeField] private Vector3 m_throwForward = new Vector3(0, 0, 1);

    [Header("Debug Settings")]
    [SerializeField] private float m_debugIntervalTime = 0.01f;    

    Coroutine m_routine;

    private void Awake()
    {
        m_throwForward = transform.forward;
        m_currentAngle = 1;
    }

    private void OnDrawGizmos()
    {
        m_totalBallTime = -m_ballSpeed / m_gravity;
        var speed = m_ballSpeed + m_gravity * Mathf.Lerp(0, m_totalBallTime, m_accmulatedBallTime / m_totalBallTime);

        m_initialVelocityDirection = Vector3.Lerp(transform.up, m_throwForward, m_currentAngle);

        var tTime = m_accmulatedBallTime;
        var vTime = 0.0f;        
        var tempPos = transform.position;

        while (tTime > 0)
        {
            var ss = m_ballSpeed + m_gravity * Mathf.Lerp(0, m_totalBallTime / 2, vTime / m_totalBallTime / 2);
            var vv = Vector3.Lerp(m_initialVelocityDirection.normalized, m_throwForward, vTime / (m_totalBallTime / 2));

            Gizmos.color = Color.white;
            Gizmos.DrawLine(tempPos, tempPos + (vv.normalized * ss));

            tempPos += (vv.normalized * ss);
            tTime -= m_debugIntervalTime;
            vTime += m_debugIntervalTime;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, (transform.position + (m_initialVelocityDirection.normalized * speed)));                           
    }

    private void Update()
    {        
        m_totalBallTime = -m_ballSpeed / m_gravity;
        m_totalAngleTime = 1 / m_angleSpeed;

        m_accmulatedBallTime += Time.deltaTime;

        if (Input.GetKey(KeyCode.Space))
        {
            m_accumulatedAngleTime += Time.deltaTime;
            m_currentAngle = Mathf.Lerp(1, 0, m_accumulatedAngleTime / m_totalAngleTime);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            // release ball
            if (m_routine != null) StopCoroutine(m_routine);
            m_routine = StartCoroutine(MoveBall());
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // rotate anti clockwise
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            // rotate clockwise
        }
        
        //if (Physics.OverlapSphere(transform.position, m_collider)
    }

    IEnumerator MoveBall()
    {
        m_accumulatedAngleTime = 0;        
        m_accmulatedBallTime = 0;
        var tempPos = transform.position;

        while (m_accmulatedBallTime < (m_totalBallTime / 2))
        {            
            var ss = m_ballSpeed + m_gravity * Mathf.Lerp(0, m_totalBallTime / 2, m_accmulatedBallTime / m_totalBallTime / 2);
            var vv = Vector3.Lerp(m_initialVelocityDirection.normalized, m_throwForward, m_accmulatedBallTime / (m_totalBallTime / 2));
            
            transform.Translate(vv * ss);

            yield return null;
        }        
    }
}
