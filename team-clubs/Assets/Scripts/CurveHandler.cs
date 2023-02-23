using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CurveHandler : MonoBehaviour
{
    [Header("Mobile Control Settings")]
    [SerializeField] private Camera m_camera;   

    [Header("Keyboard / Mouse Settings")]
    [Tooltip("Distance refers to the x-direction distance of projectile motion")] [SerializeField] private float m_distanceAdjustmentSpeed = 0.01f;
    [Tooltip("Forward refers to the x-direction vector of projectile motion")] [SerializeField] private float m_forwardAdjustmentSpeed = 0.01f;

    [Header("Curve Settings")]
    [SerializeField] private AnimationCurve m_distanceCurve;
    [SerializeField] private AnimationCurve m_speedCurve;

    [Tooltip("Forward refers to the x-direction vector of projectile motion")] [SerializeField] private Vector2 m_forwardAdjustmentRange = new Vector2(0.1f, 0.9f);
    [Tooltip("Forward refers to the x-direction vector of projectile motion")] [SerializeField] [Range(0, 1)] private float m_forwardAdjustment = 0;

    [Tooltip("Distance refers to the forward distance of projectile motion")] [SerializeField] private Vector2 m_normalizedDistanceAdjustmentRange = new Vector2(0.1f, 0.9f);
    [Tooltip("Distance refers to the forward distance of projectile motion")] [SerializeField] [Range(0, 1)] private float m_normalizedDistanceAdjustment = 0;

    [SerializeField][Range(0, 1)] private float m_distanceRange = 0.5f;
    [SerializeField] [Range(0, 5)] private float m_xVelRange = 0.5f;

    [SerializeField] private Vector3 m_gravity = new Vector3(0, -0.01f, 0);

    [Header("Ball settings")]
    [SerializeField] private GameObject m_ball;
    [SerializeField] private Vector3 m_initialBallPosition = Vector3.zero;

    [Header("Line settings")]
    [SerializeField] private float m_lineDotInterval = 0.5f;
    [SerializeField] private GameObject m_lineDotPrefab;
    [SerializeField] private Transform m_lineParent;
    [SerializeField] private int m_lineMaxDots = 50;

    [Header("Debug Settings")]
    [SerializeField] private float m_debugIntervalTime = 0.01f;
    [SerializeField] private float m_debugAccumTime = 0.0f;

    [Header("Fixed Settings")]    
    [SerializeField] private Vector3 m_throwUp = new Vector3(0, 1, 0);

    [Header("Calculated Settings")]    
    [SerializeField] private float m_accmulatedBallTime = 0;
    [SerializeField] private float m_accumulatedVertAngleTime = 0;             

    private Vector3 m_touchStartPos;
    private Vector3 m_touchCurrentPos;
    private bool m_isTapped;

    private List<GameObject> m_line;

    public Vector3 InitialPosition
    {
        get
        {
            return m_initialBallPosition;
        }
    }
    
    public Vector3 InitialVelocity
    {
        get
        {
            var range = EvaluateCurveValue(m_distanceCurve, m_distanceRange, m_normalizedDistanceAdjustmentRange.x, m_normalizedDistanceAdjustmentRange.y, m_normalizedDistanceAdjustment);
            var velocity = EvaluateCurveValue(m_speedCurve, m_xVelRange, m_normalizedDistanceAdjustmentRange.x, m_normalizedDistanceAdjustmentRange.y, m_normalizedDistanceAdjustment);
            var time = GetTime(velocity, range);            

            var s = new Vector3(0, 0, range);
            var a = m_gravity;
            var t = time;
            var isZ = true;

            var sx = isZ ? s.z : s.x;
            var ax = isZ ? a.z : a.x;

            var sy = s.y;
            var ay = a.y;

            var sz = isZ ? s.x : s.z;
            var az = isZ ? a.x : a.z;

            float vx = (sx - 0.5f * ax * Mathf.Pow(t, 2)) / t;
            float vy = (sy - 0.5f * ay * Mathf.Pow(t, 2)) / t;
            float vz = (sz - 0.5f * az * Mathf.Pow(t, 2)) / t;

            return isZ ? new Vector3(vz, vy, vx) : new Vector3(vx, vy, vz);

            //return m_initialVelocity;
        }
    }

    public Vector3 CurrentVelocity
    {
        get
        {
            return GetVelocity(InitialVelocity, m_gravity, m_accmulatedBallTime);
        }
    }
    
    public Vector3 Gravity
    {
        get
        {
            return m_gravity;
        }
    }    

    bool m_isMove;

    private void Awake()
    {
        m_line = new List<GameObject>();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {                                                                          
        var tempPos = transform.position;
        var tempTime = 0.0f;
        var dist = EvaluateCurveValue(m_distanceCurve, m_distanceRange, m_normalizedDistanceAdjustmentRange.x, m_normalizedDistanceAdjustmentRange.y, m_normalizedDistanceAdjustment);
        var velocity = EvaluateCurveValue(m_speedCurve, m_xVelRange, m_normalizedDistanceAdjustmentRange.x, m_normalizedDistanceAdjustmentRange.y, m_normalizedDistanceAdjustment);
        var t = GetTime(velocity, dist);

        while (t > 0)
        {
            var intermediateVelocity = GetVelocity(InitialVelocity, m_gravity, tempTime);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(tempPos, tempPos + intermediateVelocity);

            tempPos += intermediateVelocity;
            tempTime += m_debugIntervalTime;
            t -= m_debugIntervalTime;
        }
        
        m_ball.transform.position = m_initialBallPosition;
        m_debugAccumTime = 0;

        while (m_debugAccumTime < m_accmulatedBallTime)
        {
            var vel = GetVelocity(InitialVelocity, m_gravity, m_debugAccumTime);
            m_ball.transform.position = m_ball.transform.position + vel;

            m_debugAccumTime += m_debugIntervalTime;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawLine(m_initialBallPosition, (m_initialBallPosition + (InitialVelocity)));                    
    }
#endif

    private void Update()
    {        
        if (Input.GetKeyUp(KeyCode.R))
        {
            Reset();
        }
        
        if (!m_isMove && GameManager.Instance.CurrentGameState == GameManager.EGameState.GAME)
            //if ( m_moveRoutine == null && GameManager.Instance.CurrentGameState == GameManager.EGameState.GAME )
        {
            #if UNITY_STANDALONE_WIN

            if (Input.GetMouseButtonDown(0))
            {
                // store position
            }

            if (Input.GetMouseButtonUp(0))
            {

            }

            #elif UNITY_ANDROID

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Stationary:
                    case TouchPhase.Began:
                        RaycastHit info;
                        Debug.Log(m_isTapped);
                        if (!m_isTapped && Physics.Raycast(m_camera.ScreenPointToRay(touch.position), out info, 999, 1 << LayerMask.NameToLayer("BallTap")))
                        {
                            m_touchStartPos = touch.position;
                            m_isTapped = true;                            
                        }

                        break;
                    case TouchPhase.Moved:
                        if (m_isTapped)
                        {
                            m_touchCurrentPos = touch.position;

                            // TODO: smooth dx2 and dy2

                            // move horizontal axis

                            var dx1 = Mathf.InverseLerp(0, Screen.width, m_touchStartPos.x) - Mathf.InverseLerp(0, Screen.width, m_touchCurrentPos.x);
                            var dx2 = Mathf.InverseLerp(-0.25f, 0.25f, dx1);
                            m_forwardAdjustment = dx2;

                            // move vertical axis                            
                            var dy1 = Mathf.InverseLerp(m_touchStartPos.y, 0, m_touchCurrentPos.y);
                            var dy2 = Mathf.InverseLerp(0, 0.5f, dy1);
                            m_normalizedDistanceAdjustment = dy2;                            
                        }

                        break;                    
                    case TouchPhase.Ended:
                        if (m_isTapped)
                        {
                            m_isMove = true;
                            m_isTapped = false;
                        }
                        break;
                }
            }

            #endif

            if (Input.GetKey(KeyCode.Space))
            {
                var totalVertAngleTime = 1 / m_distanceAdjustmentSpeed;
                m_accumulatedVertAngleTime += Time.deltaTime;

                m_normalizedDistanceAdjustment = Mathf.Lerp(0, 1, m_accumulatedVertAngleTime / totalVertAngleTime);
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                // release ball
                m_isMove = true;                           
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {            
                m_forwardAdjustment = Mathf.Clamp(m_forwardAdjustment - m_forwardAdjustmentSpeed * Time.deltaTime, 0, 1);
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {            
                m_forwardAdjustment = Mathf.Clamp(m_forwardAdjustment + m_forwardAdjustmentSpeed * Time.deltaTime, 0, 1);
            }               
        }

        if (m_isMove)
        {
            m_ball.transform.position = m_initialBallPosition;
            var debugAccumTime = 0.0f;

            while (debugAccumTime < m_accmulatedBallTime)
            {
                var vel = GetVelocity(InitialVelocity, m_gravity, debugAccumTime);
                m_ball.transform.position = m_ball.transform.position + vel;

                debugAccumTime += m_debugIntervalTime;
            }
            m_accmulatedBallTime += (Time.deltaTime);
        }

        RenderLine();
    }

    public void Reset()
    {
        m_ball.transform.position = m_initialBallPosition;        
        m_isMove = false;

        m_accmulatedBallTime = 0;
        m_accumulatedVertAngleTime = 0;
        m_normalizedDistanceAdjustment = 0;
    }

    void RenderLine()
    {                
        var tempPos = transform.position;
        var tempTime = 0.0f;

        var dist = EvaluateCurveValue(m_distanceCurve, m_distanceRange, m_normalizedDistanceAdjustmentRange.x, m_normalizedDistanceAdjustmentRange.y, m_normalizedDistanceAdjustment);
        var vel = EvaluateCurveValue(m_speedCurve, m_xVelRange, m_normalizedDistanceAdjustmentRange.x, m_normalizedDistanceAdjustmentRange.y, m_normalizedDistanceAdjustment);
        var t = GetTime(vel, dist);

        var i = 0;
        while (t > 0)
        {
            var intermediateVelocity = GetVelocity(InitialVelocity, m_gravity, tempTime);            
            tempPos += intermediateVelocity;
            
            if (i <= m_line.Count && m_line.Count < m_lineMaxDots)
            {
                var dot = Instantiate(m_lineDotPrefab);
                m_line.Add(dot);

                dot.transform.parent = m_lineParent;
                dot.transform.position = tempPos;
            }
            else
            {
                m_line[i].SetActive(true);
                m_line[i].transform.position = tempPos;
            }            

            tempTime += m_lineDotInterval;
            t -= m_lineDotInterval;

            i++;

            if (i >= m_lineMaxDots) break;
        }        

        while (i < m_line.Count)
        {
            m_line[i].SetActive(false);
            i++;
        }
    }

    float EvaluateCurveValue(AnimationCurve curve, float maxRange, float minNormalized, float maxNormalized, float valueNormalized)
    {
        float cnd = Mathf.Lerp(minNormalized, maxNormalized, valueNormalized);
        return curve.Evaluate(cnd) * maxRange;
    }
    
    float GetTime(float velocity, float distance)
    {
        return (velocity / distance);
    }

    Vector3 GetVelocity(Vector3 u, Vector3 a, float t, bool isZ = true)
    {
        var ux = isZ ? u.z : u.x;        
        var ax = isZ ? a.z : a.x;

        var uz = isZ ? u.x : u.z;
        var az = isZ ? a.x : a.z;

        var vx = ux + ax* t;
        var vy = u.y + a.y * t;
        var vz = uz + az * t;

        return isZ ? new Vector3(vz, vy, vx) : new Vector3(vx, vy, vz);
    }       
}
