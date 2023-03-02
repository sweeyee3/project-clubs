using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingHoop : MonoBehaviour
{
    //Store spawn position
    //Get left cell position and right cell position
    //Move instantiated hoop from spawn point to other cell position
    //Lerp between the 2 positions

    [SerializeField] private Hoop m_hoop;
    [SerializeField] private float m_posThreshold = 0.1f;
    
    [SerializeField] private int m_moveUnits = 1;
    [SerializeField] private float m_moveSpeed = 2.0f;

    private Vector3 m_positionTarget;
    private Vector3 m_positionOriginal;    

    private bool m_isMovingInPositiveDirection = true;

    public Vector3 PositionTarget
    {
        get
        {
            return m_positionTarget;
        }
    }

    private void Start()
    {
        Vector3 gridIndex = m_hoop.CellIndex;
        Vector3 cellCounts = SpawnManager.Instance.GetCellCounts();

        int targetTargetX = (int)m_hoop.CellIndex.x;
        int targetTargetY = (int)m_hoop.CellIndex.y;
        int targetTargetZ = (int)m_hoop.CellIndex.z;

        int targetOriginalX = (int)m_hoop.CellIndex.x;
        int targetOriginalY = (int)m_hoop.CellIndex.y;
        int targetOriginalZ = (int)m_hoop.CellIndex.z;

        int moveDirection = Random.Range(0, 2) == 1 ? -1 : 1;

        switch (m_hoop.HoopType)
        {
            case Hoop.EHoopType.MOVE_X:
                var targetX = targetTargetX + (moveDirection * m_moveUnits);
                Debug.Log(targetX + ", " + cellCounts.x);
                moveDirection = (targetX < 0 || targetX >= cellCounts.x) ? -moveDirection : moveDirection;

                targetTargetX = Mathf.Clamp(targetTargetX + (moveDirection * m_moveUnits), 0, (int)cellCounts.x);                
                break;
            case Hoop.EHoopType.MOVE_Y:
                var targetY = targetTargetY + (moveDirection * m_moveUnits);
                moveDirection = (targetY < 0 || targetY >= cellCounts.x) ? -moveDirection : moveDirection;
                targetTargetY = Mathf.Clamp(targetTargetY + (moveDirection * m_moveUnits), 0, (int)cellCounts.x);                
                break;
            case Hoop.EHoopType.MOVE_Z:
                var targetZ = targetTargetZ + (moveDirection * m_moveUnits);
                moveDirection = (targetZ < 0 || targetZ >= cellCounts.x) ? -moveDirection : moveDirection;
                targetTargetZ = Mathf.Clamp(targetTargetZ + (moveDirection * m_moveUnits), 0, (int)cellCounts.x);
                break;
        }
        
        m_positionTarget = SpawnManager.Instance.GetCellPosition(targetTargetX, targetTargetY, targetTargetZ);
        m_positionOriginal = SpawnManager.Instance.GetCellPosition(targetOriginalX, targetOriginalY, targetOriginalZ);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position != m_positionTarget && m_isMovingInPositiveDirection)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_positionTarget, Time.deltaTime * m_moveSpeed);
            
        }            

        if (transform.position != m_positionOriginal && !m_isMovingInPositiveDirection)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_positionOriginal, Time.deltaTime * m_moveSpeed);
        }

        if ( ((transform.position - m_positionTarget).magnitude < m_posThreshold && m_isMovingInPositiveDirection) ||
            ((transform.position - m_positionOriginal).magnitude < m_posThreshold && !m_isMovingInPositiveDirection))
        {
            m_isMovingInPositiveDirection = !m_isMovingInPositiveDirection;
        }        
    }
}
