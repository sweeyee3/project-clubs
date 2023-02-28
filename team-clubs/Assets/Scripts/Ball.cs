using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{        
    [Header("Other settings")]
    [SerializeField] private float m_timeAcceleration = 0.75f;

    [Header("Calculated Settings")]
    [SerializeField] private float m_accmulatedBallTime;
    [SerializeField] private float m_timeStep = 0.01f;
    [SerializeField] private bool m_isMoveBall;
    [SerializeField] private int m_currentBounceCount;
    [SerializeField] private Vector3 m_currentVelocity;

    Vector3 m_initialBallPosition;
    Vector3 m_initialProjectileVelocity;
    Vector3 m_initialSpeed;
    Vector3 m_gravity;

    int m_bounceCount;
    float m_normalizedVelocityReductionFactor;
    float m_normalizedGravityModulation;
    LayerMask m_bounceLayerMask;

    public Vector3 CurrentVelocity
    {
        get
        {
            return m_currentVelocity;
        }
    }

    public bool isMoving
    {
        get
        {
            return m_isMoveBall;
        }
    }

#if UNITY_EDITOR
    
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            if (m_accmulatedBallTime <= 0) m_isMoveBall = false;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawLine(m_initialBallPosition, (m_initialBallPosition + (m_currentVelocity * 2)));
    }

#endif

    private void FixedUpdate()
    {
        if (m_isMoveBall)
        {
            Move(m_initialBallPosition, m_initialProjectileVelocity, m_initialSpeed, m_gravity);
            m_accmulatedBallTime += (Time.fixedDeltaTime * m_timeAcceleration);
        }
    }

    public void Set(Vector3 initialBallPosition, Vector3 initialProjectileVelocity, Vector3 initialSpeed, Vector3 gravity, int bounceCount, float normalizedVelocityReductionFactor, float normalizedGravityModulation, LayerMask bounceMask)
    {
        m_initialBallPosition = initialBallPosition;
        m_initialProjectileVelocity = initialProjectileVelocity;
        m_initialSpeed = initialSpeed;
        m_gravity = gravity;

        m_bounceCount = bounceCount;
        m_normalizedVelocityReductionFactor = normalizedVelocityReductionFactor;
        m_normalizedGravityModulation = normalizedGravityModulation;
        m_bounceLayerMask = bounceMask;

        m_isMoveBall = true;
    }

    public void Move(Vector3 initialBallPosition, Vector3 initialProjectileVelocity, Vector3 initialSpeed, Vector3 gravity)
    {
        // Translate ball on path        
        var debugAccumTime = 0.0f;

        var isProjectile = true;
        var tempBounceCount = m_bounceCount;
        //var tempPos = m_accmulatedBallTime <= 0 ? initialBallPosition : initialBallPosition;
        var tempPos = m_initialBallPosition;
        var tempVel = initialProjectileVelocity;        

        float tempTrajectoryChangeTime = 0;
        //if (m_accmulatedBallTime <= 0) m_currentVelocity = InitialProjectileVelocity;

        while (debugAccumTime < m_accmulatedBallTime)
        {
            var cVel = (isProjectile) ? CustomUtility.CalculateProjectileVelocity(tempVel, gravity, debugAccumTime - tempTrajectoryChangeTime) : tempVel;            
            RaycastHit info;
            Vector3 hitPos = tempPos;
            Vector3 hitReflection = cVel;

            if (tempBounceCount < 0) isProjectile = false;            

            bool isHit = Physics.Raycast(hitPos, hitReflection.normalized, out info, hitReflection.magnitude, m_bounceLayerMask);
            if (isHit && tempBounceCount > 0)
            {                
                tempPos = info.point;
                hitReflection = Vector3.Reflect(hitReflection.normalized, info.normal) * 99;

                // check dot product
                var dot = Vector3.Dot(info.normal, Vector3.up);
                if (dot >= 0.8f)
                {
                    // calculate new projectile velocity
                    // set new time                    
                    tempVel = hitReflection.normalized * (initialSpeed.magnitude * m_normalizedVelocityReductionFactor);
                    isProjectile = true;
                    tempTrajectoryChangeTime = debugAccumTime;

                    AudioManager.Instance.Play("ballBounce", AudioManager.EAudioType.SFX);
                }
                else
                {
                    // do another raycast, find new velocity
                    // set new time

                    hitReflection = (hitReflection.normalized + (gravity * m_normalizedGravityModulation));
                    tempVel = hitReflection;

                    isProjectile = false;
                    AudioManager.Instance.Play("ballHitBoard", AudioManager.EAudioType.SFX);
                }
                tempBounceCount--;                
            }
            else
            {
                tempPos += cVel;               
            }

            debugAccumTime += m_timeStep;
        }

        m_currentBounceCount = tempBounceCount;
        m_currentVelocity = tempVel;
        transform.position = tempPos;        
    }
}
