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
    [Tooltip("Forward refers to the x-direction vector of projectile motion")] [SerializeField] private float m_forwardAdjustmentSpeed = 0.01f;
    [Tooltip("Distance refers to the x-direction distance of projectile motion")] [SerializeField] private float m_forwardSpeedAdjustmentSpeed = 0.01f;

    [Header("Curve Settings")]
    [SerializeField] private AnimationCurve m_speedCurve;

    [Tooltip("Forward refers to the x-direction vector of projectile motion")] [SerializeField] private Vector2 m_forwardAdjustmentRange = new Vector2(0.1f, 0.9f);
    [Tooltip("Forward refers to the x-direction vector of projectile motion")] [SerializeField] [Range(0, 1)] private float m_forwardAdjustment = 0;

    [Tooltip("Distance refers to the forward distance of projectile motion")] [SerializeField] private Vector2 m_normalizedForwardSpeedAdjustmentRange = new Vector2(0.1f, 0.9f);
    [Tooltip("Distance refers to the forward distance of projectile motion")] [SerializeField] [Range(0, 1)] private float m_normalizedForwardSpeedAdjustment = 0;

    [SerializeField] [Range(0, 10)] private float m_maxForwardSpeed = 0.5f;

    [SerializeField] private Vector3 m_gravity = new Vector3(0, -0.01f, 0);

    [Header("Ball settings")]
    [SerializeField] private GameObject m_ball;
    [SerializeField] private Vector3 m_initialBallPosition = Vector3.zero;

    [Header("Bounce settings")]
    [SerializeField] private int m_bounceCount;
    [SerializeField][Range(0, 1)] private float m_normalizedVelocityReductionFactor = 1;
    [SerializeField] [Range(0, 1)] private float m_normalizedGravityModulation = 1;
    [SerializeField] private LayerMask m_bounceLayerMask;

    [Header("Line settings")]
    [SerializeField] private bool m_isDisplayLine;
    [SerializeField] private float m_lineDotInterval = 0.5f;
    [SerializeField] private GameObject m_lineDotPrefab;
    [SerializeField] private Transform m_lineParent;
    [SerializeField] private int m_lineMaxDots = 50;

    [Header("Arrow settings")]
    [SerializeField] private bool m_isDisplayArrow;
    [SerializeField] private Vector2 m_initalArrowSize = new Vector2(1, 1);
    [SerializeField] private Vector2 m_maxArrowSize = new Vector2(3, 1);
    [SerializeField] private GameObject m_arrowParent;
    [SerializeField] private SpriteRenderer m_arrowSprite;

    [Header("Debug Settings")]
    [SerializeField] private float m_debugIntervalTime = 0.01f;    
    [SerializeField] private float m_debugTotalTime;

    [Header("Fixed Settings")]
    [SerializeField] private bool m_isXDirectionForward = true;
    [SerializeField] private float m_forwardDegAngle = 45;
    [SerializeField] private float m_timeAcceleration = 5;
    [SerializeField] private Vector3 m_throwUp = new Vector3(0, 1, 0);

    [Header("Calculated Settings")]
    [SerializeField] private float m_accmulatedBallTime = 0;
    [SerializeField] private float m_accumulatedVertAngleTime = 0;
    [SerializeField] private Vector3 m_currentVelocity;
    [SerializeField] private int m_currentBounceCount;

    [Header("Runtime Settings")]
    [SerializeField] bool m_isMove;
    
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

    public Vector3 InitialInputSpeed
    {
        get
        {
            float cnd = Mathf.Lerp(m_normalizedForwardSpeedAdjustmentRange.x, m_normalizedForwardSpeedAdjustmentRange.y, m_normalizedForwardSpeedAdjustment);           
            var vx = m_speedCurve.Evaluate(cnd) * m_maxForwardSpeed;
            var vy = Mathf.Tan(m_forwardDegAngle * Mathf.Deg2Rad) * vx;
            var vz = 0;
            
            var initialInputSpeed = m_isXDirectionForward ? new Vector3(vz, vy, vx) : new Vector3(vx, vy, vz);

            return initialInputSpeed;
        }
    }

    public Vector3 InitialProjectileVelocity
    {
        get
        {
            var initialVel = InitialInputSpeed;

            var forward = -Vector3.Slerp(Vector3.right, -Vector3.right, Mathf.Lerp(m_forwardAdjustmentRange.x, m_forwardAdjustmentRange.y, m_forwardAdjustment)).normalized;
            float iv = UtilityExtension.InverseLerp(transform.forward, transform.up, initialVel.normalized);
            var velocityDirection = Vector3.Slerp(forward, transform.up, iv);

            return velocityDirection * initialVel.magnitude;
        }
    }
    
    public Vector3 CurrentVelocity
    {
        get
        {
            return m_currentVelocity;
        }
    }

    public int CurrentBounceCount
    {
        get
        {
            return m_currentBounceCount;
        }
    }

    public int BounceCount
    {
        get
        {
            return m_bounceCount;
        }
    }

    public Vector3 Gravity
    {
        get
        {
            return m_gravity;
        }
    }

    private void Awake()
    {
        m_line = new List<GameObject>();
        //m_currentVelocity = InitialProjectileVelocity;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawProjectilePath();

        MoveBall();

        var forward = -Vector3.Slerp(Vector3.right, -Vector3.right, m_forwardAdjustment).normalized;
        var velocityDirection = Vector3.Slerp(forward, m_throwUp, m_normalizedForwardSpeedAdjustment);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(m_initialBallPosition, m_initialBallPosition + velocityDirection);

        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(m_initialBallPosition, (m_initialBallPosition + (m_currentVelocity)));
    }

    void DrawProjectilePath()
    {
        var tempPos = transform.position;
        var tempTime = 0.0f;
        var tempBounceCount = m_bounceCount;
        var isProjectile = true;

        var currentVelocity = InitialProjectileVelocity;        
        var t = CalculateProjectileTime(currentVelocity);
        var additionalTime = t;

        while (t > 0)
        {
            var intermediateVelocity = (isProjectile) ? CalculateProjectileVelocity(currentVelocity, m_gravity, tempTime) : currentVelocity; // TODO: might need to calculate lerped velocity           
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(tempPos, tempPos + intermediateVelocity);

            RaycastHit info;
            Vector3 hitPos = tempPos;
            Vector3 hitReflection = intermediateVelocity;

            bool isHit = Physics.Raycast(hitPos, hitReflection.normalized, out info, hitReflection.magnitude, m_bounceLayerMask);

            Gizmos.DrawWireSphere(hitPos, 0.1f);            
            if (isHit)
            {
                hitPos = info.point;
                hitReflection = Vector3.Reflect(hitReflection.normalized, info.normal);                

                Gizmos.color = Color.yellow;                
                Gizmos.DrawSphere(hitPos, 0.25f);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(info.point, info.point + (info.normal * 2));                

                // check dot product
                var dot = Vector3.Dot(info.normal, Vector3.up);                
                if (dot >= 0.8f)
                {                    
                    // calculate new projectile velocity
                    // set new time
                    var prevAdditionalTime = additionalTime;
                    currentVelocity = hitReflection.normalized * (InitialInputSpeed.magnitude * m_normalizedVelocityReductionFactor);
                    additionalTime = CalculateProjectileTime(currentVelocity);
                    t += additionalTime - (prevAdditionalTime - tempTime);

                    tempPos = info.point;
                    isProjectile = true;

                    //Gizmos.color = Color.black;
                    //Gizmos.DrawLine(tempPos, tempPos + currentVelocity.normalized * 2);
                }
                else
                {
                    // do another raycast, find new velocity
                    // set new time
                    RaycastHit newHit;
                    hitReflection += (m_gravity * m_normalizedGravityModulation);
                    hitReflection *= 99;
                    bool isHitNext = Physics.Raycast(hitPos, hitReflection.normalized, out newHit, hitReflection.magnitude, m_bounceLayerMask);
                    if (isHitNext)
                    {
                        var prevAdditionalTime = additionalTime;
                        currentVelocity = InitialInputSpeed.magnitude * hitReflection.normalized;
                        additionalTime = (newHit.point - hitPos).magnitude / InitialInputSpeed.magnitude;                                                

                        t += additionalTime - (prevAdditionalTime - tempTime); // TODO: remove remaining time from previous motion
                        
                        tempPos = info.point;
                        isProjectile = false;                        
                    }
                }
                tempTime = 0;
                tempBounceCount--;
            }
            else
            {
                tempPos += intermediateVelocity;
            }

            tempTime += m_debugIntervalTime;
            t -= m_debugIntervalTime;
            
            if (tempBounceCount < 0) break;
        }
    }
#endif

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            Reset();
        }

        if (!m_isMove && GameManager.Instance.CurrentGameState == GameManager.EGameState.GAME)        
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
                            m_normalizedForwardSpeedAdjustment = dy2;
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
                var totalVertAngleTime = 1 / m_forwardSpeedAdjustmentSpeed;
                m_accumulatedVertAngleTime += Time.deltaTime;

                m_normalizedForwardSpeedAdjustment = Mathf.Lerp(0, 1, m_accumulatedVertAngleTime / totalVertAngleTime);
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                // release ball
                m_isMove = true;               
                m_debugTotalTime = CalculateProjectileTime(InitialProjectileVelocity);
                //m_currentVelocity = InitialProjectileVelocity;
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

        if (m_isDisplayLine) RenderLine();
        if (m_isDisplayArrow) RenderArrow();
    }

    private void FixedUpdate()
    {
        if (m_isMove)
        {
            MoveBall();
            m_accmulatedBallTime += (Time.fixedDeltaTime * m_timeAcceleration);
        }
    }

    public void Reset()
    {
        m_ball.transform.position = m_initialBallPosition;
        m_isMove = false;

        m_accmulatedBallTime = 0;
        m_accumulatedVertAngleTime = 0;
        m_normalizedForwardSpeedAdjustment = 0;
    }    

    void MoveBall()
    {
        // Translate ball on path
        var diff = m_initialBallPosition - transform.position;
        
        var debugAccumTime = 0.0f;

        var isProjectile = true;
        var tempBounceCount = m_bounceCount;
        var tempPos = m_accmulatedBallTime <= 0 ? m_initialBallPosition : m_initialBallPosition - diff;
        var tempVel = InitialProjectileVelocity;

        float tempTrajectoryChangeTime = 0;
        //if (m_accmulatedBallTime <= 0) m_currentVelocity = InitialProjectileVelocity;

        while (debugAccumTime < m_accmulatedBallTime)
        {
            var cVel = (isProjectile) ? CalculateProjectileVelocity(tempVel, m_gravity, debugAccumTime - tempTrajectoryChangeTime) : tempVel;            
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
                    tempVel = hitReflection.normalized * (InitialInputSpeed.magnitude * m_normalizedVelocityReductionFactor);                    
                    isProjectile = true;
                    tempTrajectoryChangeTime = debugAccumTime;                                      
                }
                else
                {
                    // do another raycast, find new velocity
                    // set new time
                    
                    hitReflection = (hitReflection.normalized + (m_gravity * m_normalizedGravityModulation));
                    tempVel = hitReflection;                                       
                    
                    isProjectile = false;                                       
                }                
                tempBounceCount--;                
            }
            else
            {                
                tempPos += cVel; // TODO: find out why ball constantly toggles between 2 point when raycast hit                
            }

            debugAccumTime += m_debugIntervalTime;            
        }

        m_currentBounceCount = tempBounceCount;
        m_currentVelocity = tempVel;
        m_ball.transform.position = tempPos;
    }

    void RenderArrow()
    {
        m_arrowParent.SetActive(true);
        var forward = -Vector3.Slerp(Vector3.right, -Vector3.right, Mathf.Lerp(m_forwardAdjustmentRange.x, m_forwardAdjustmentRange.y, m_forwardAdjustment)).normalized;
        m_arrowParent.transform.position = m_initialBallPosition;
        m_arrowParent.transform.forward = forward;
        m_arrowSprite.size = new Vector2(m_initalArrowSize.x + Mathf.Lerp(0, m_maxArrowSize.x, m_normalizedForwardSpeedAdjustment), m_initalArrowSize.y);        
    }

    void RenderLine()
    {
        
    }    

    float CalculateProjectileTime(Vector3 v)
    {       
        var h = -Mathf.Pow(v.y, 2) / (2 * m_gravity.y);
        var a = m_gravity.y;
        var b = 2 * v.y;
        var c = -2 * h;
        var b2m4ac = (Mathf.Pow(b, 2) - 4 * a * c);
        b2m4ac = b2m4ac < 0 ? 0 : b2m4ac;

        var t1 = (-b + Mathf.Sqrt(b2m4ac)) / (2 * a);
        var t2 = (-b - Mathf.Sqrt(b2m4ac)) / (2 * a);

        var t = (t1 > 0) ? t1 : (t2 > 0) ? t2 : 0;
        t *= 2;

        return t;
    }

    Vector3 CalculateProjectileVelocity(Vector3 u, Vector3 a, float t, bool isZ = true)
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
