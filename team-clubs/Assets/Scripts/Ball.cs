using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Ball : MonoBehaviour
{
    [Header("Mobile Control Settings")]
    [SerializeField] private Camera m_camera;
    [SerializeField] private Vector3 m_swipeRange = new Vector3(2, 2, 0);

    [Header("Keyboard / Mouse Settings")]
    [SerializeField] private float m_verticalAngleSpeed = 0.01f;
    [SerializeField] private float m_horizontalAngleSpeed = 0.01f;

    [Header("Ball Settings")]
    [SerializeField][Range(0, 1)] private float m_speedThrottleFactor;
    [SerializeField] private AnimationCurve m_speedCurve;

    [SerializeField] private Vector2 m_horizontalAngleRange = new Vector2(0.1f, 0.9f);
    [SerializeField] [Range(0, 1)] private float m_horizontalAngle = 0;

    [SerializeField] private Vector2 m_verticalAngleRange = new Vector2(0.1f, 0.9f);
    [SerializeField][Range(0, 1)] private float m_verticalAngle = 0;        

    [SerializeField] private Vector3 m_gravity = new Vector3(0, -5, 0);

    [SerializeField] private Vector3 m_initialPosition = Vector3.zero;

    [Header("Line settings")]
    [SerializeField] private float m_lineDotInterval = 0.5f;
    [SerializeField] private GameObject m_lineDotPrefab;
    [SerializeField] private Transform m_lineParent;
    [SerializeField] private int m_lineMaxDots = 50;

    [Header("Debug Settings")]
    [SerializeField] private float m_debugIntervalTime = 0.01f;
    [SerializeField] private float m_debugAccumTime = 0.0f;

    [Header("Fixed Settings")]
    [SerializeField] private float m_ballDistance = 1;
    [SerializeField] private float m_ballHeight = 1;

    [Header("Calculated Settings")]            
    [SerializeField] private float m_accmulatedBallTime = 0;
    [SerializeField] private float m_accumulatedVertAngleTime = 0;
    [SerializeField] private float m_accumulatedHorizontalAngleTime = 0;

    [SerializeField] private float m_totalBallTime; 

    [SerializeField] private Vector3 m_throwUp = new Vector3(0, 1, 0);


    private Vector3 m_touchStartPos;
    private Vector3 m_touchCurrentPos;
    private bool m_isTapped;

    private List<GameObject> m_line;

    public Vector3 InitialPosition
    {
        get
        {
            return m_initialPosition;
        }
    }

    public Vector3 InitialVelocity
    {
        get
        {
            var horizontalAngle = Mathf.Lerp(m_horizontalAngleRange.x, Mathf.Clamp(m_horizontalAngleRange.y, m_horizontalAngleRange.x, m_horizontalAngleRange.y), m_horizontalAngle);
            var forward = -Vector3.Slerp(Vector3.right, -Vector3.right, horizontalAngle).normalized;

            // calculate initial velocity given speed and angle instead
            var verticalAngle = Mathf.Lerp(m_verticalAngleRange.x, Mathf.Clamp(m_verticalAngleRange.y, m_verticalAngleRange.x, m_verticalAngleRange.y), m_verticalAngle);
            var v = Vector3.Slerp(forward, m_throwUp, verticalAngle);

            return v * m_speedCurve.Evaluate(verticalAngle);
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

    public float BallHeight
    {
        get
        {
            return m_ballHeight;
        }
    }

    public float BallDistance
    {
        get
        {
            return m_ballDistance;
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
        // calculate speed based on angle
        var initialVel = InitialVelocity;

        var vf = GetVelocity(initialVel, m_gravity, new Vector3(0, -m_ballHeight, m_ballDistance));        
        var tx = GetTime(initialVel.z, vf.z, m_ballDistance);
        //var ty = GetTime(m_initialVelocity.y, vf.y, m_ballHeight);       

        var t = tx;

        m_totalBallTime = t;        

        var tempPos = m_initialPosition;
        var tempTime = 0.0f;
        while (t > 0)
        {
            var intermediateVelocity = GetVelocity(initialVel, m_gravity, tempTime);
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(tempPos, tempPos + intermediateVelocity);

            tempPos += intermediateVelocity;
            tempTime += m_debugIntervalTime;
            t -= m_debugIntervalTime;
        }
        
        transform.position = m_initialPosition;
        m_debugAccumTime = 0;

        while (m_debugAccumTime < m_accmulatedBallTime)
        {
            var vel = GetVelocity(initialVel, m_gravity, m_debugAccumTime);
            transform.position = transform.position + vel;

            m_debugAccumTime += m_debugIntervalTime;
        }        
        Gizmos.color = Color.red;
        Gizmos.DrawLine(m_initialPosition, (m_initialPosition + (initialVel)));                    
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
                            Debug.Log("tapped: " + info.collider.transform.parent.name);
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
                            m_horizontalAngle = dx2;

                            // move vertical axis
                            Debug.Log(m_touchCurrentPos.y);
                            var dy1 = Mathf.InverseLerp(m_touchStartPos.y, 0, m_touchCurrentPos.y);
                            var dy2 = Mathf.InverseLerp(0, 0.5f, dy1);
                            m_verticalAngle = dy2;                            
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
                var totalVertAngleTime = 1 / m_verticalAngleSpeed;
                m_accumulatedVertAngleTime += Time.deltaTime;

                m_verticalAngle = Mathf.Lerp(0, 1, m_accumulatedVertAngleTime / totalVertAngleTime);
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                // release ball
                m_isMove = true;                           
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                m_accumulatedHorizontalAngleTime = 0;
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {            
                m_horizontalAngle = Mathf.Clamp(m_horizontalAngle - m_horizontalAngleSpeed * Time.deltaTime, 0, 1);
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {            
                m_horizontalAngle = Mathf.Clamp(m_horizontalAngle + m_horizontalAngleSpeed * Time.deltaTime, 0, 1);
            }               
        }

        if (m_isMove)
        {
            var initialVel = InitialVelocity;

            var vf = GetVelocity(initialVel, m_gravity, new Vector3(0, -m_ballHeight, m_ballDistance));
            var tx = GetTime(initialVel.z, vf.z, m_ballDistance);
            var ty = GetTime(initialVel.y, vf.y, m_ballHeight);

            var t = tx;

            m_totalBallTime = t;

            transform.position = m_initialPosition;
            var debugAccumTime = 0.0f;

            while (debugAccumTime < m_accmulatedBallTime)
            {
                var vel = GetVelocity(initialVel, m_gravity, debugAccumTime);
                transform.position = transform.position + vel;

                debugAccumTime += m_debugIntervalTime;
            }
            m_accmulatedBallTime += (Time.deltaTime);
        }

        RenderLine();
    }

    public void Reset()
    {
        transform.position = m_initialPosition;        
        m_isMove = false;

        m_accmulatedBallTime = 0;
        m_accumulatedVertAngleTime = 0;
        m_verticalAngle = 0;
    }

    void RenderLine()
    {
        // calculate speed based on angle
        var initialVel = InitialVelocity;

        var tempPos = m_initialPosition;
        var tempTime = 0.0f;
        
        var vf = GetVelocity(initialVel, m_gravity, new Vector3(0, -m_ballHeight, m_ballDistance));
        var t = GetTime(initialVel.z, vf.z, m_ballDistance);

        var i = 0;
        while (t > 0)
        {
            var intermediateVelocity = GetVelocity(initialVel, m_gravity, tempTime);            
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

    public float GetTime(float u, float v, float s)
    {
        float t = (2*s) / (u + v);

        return t;
    }

    public Vector3 GetTargetCalculatedVelocity(float inHorizontal, float inVertical)
    {
        var horizontalAngle = Mathf.Lerp(m_horizontalAngleRange.x, Mathf.Clamp(m_horizontalAngleRange.y, m_horizontalAngleRange.x, m_horizontalAngleRange.y), inHorizontal);
        var forward = -Vector3.Slerp(Vector3.right, -Vector3.right, horizontalAngle).normalized;

        // calculate initial velocity given speed and angle instead
        var verticalAngle = Mathf.Lerp(m_verticalAngleRange.x, Mathf.Clamp(m_verticalAngleRange.y, m_verticalAngleRange.x, m_verticalAngleRange.y), inVertical);
        var v = Vector3.Slerp(forward, m_throwUp, verticalAngle);

        return v * m_speedCurve.Evaluate(verticalAngle);
    }

    public Vector3 GetVelocity(Vector3 u, Vector3 a, Vector3 s, bool isZ = true)
    {
        var ux = isZ ? u.z : u.x;
        var sx = isZ ? s.z : s.x;
        var ax = isZ ? a.z : a.x;

        var uz = isZ ? u.x : u.z;
        var sz = isZ ? s.x : s.z;
        var az = isZ ? a.x : a.z;

        var vx = Mathf.Sqrt(Mathf.Pow(ux, 2) + (2*ax*sx));
        var vy = Mathf.Sqrt(Mathf.Pow(u.y, 2) + (2*a.y*s.y));

        var vz = Mathf.Sqrt(Mathf.Pow(uz, 2) + (2 * az * sz));

        return isZ ? new Vector3(vz, vy, vx) : new Vector3(vx, vy, vz);
    }

    public Vector3 GetVelocity(Vector3 u, Vector3 a, float t, bool isZ = true)
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
