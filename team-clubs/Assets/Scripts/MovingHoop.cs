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

    [SerializeField] public Vector3 m_cellIndexTarget;
    [SerializeField] public Vector3 m_cellIndexOriginal;

    private Vector3 m_positionTarget;
    private Vector3 m_positionOriginal;    

    private bool m_isMovingInPositiveDirection = true;

    public int MoveUnits
    {
        get
        {
            return m_moveUnits;
        }
    }   

    private void Start()
    {        
        int targetOriginalX = (int)m_hoop.CellIndex.x;
        int targetOriginalY = (int)m_hoop.CellIndex.y;
        int targetOriginalZ = (int)m_hoop.CellIndex.z;        

        m_cellIndexOriginal = m_hoop.CellIndex;
        m_cellIndexTarget = GetTargetCellIndex(m_hoop.HoopType, m_hoop.CellIndex, m_moveUnits);

        m_positionTarget = SpawnManager.Instance.GetCellPosition((int)m_cellIndexTarget.x, (int)m_cellIndexTarget.y, (int)m_cellIndexTarget.z);
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

    public static Vector3 GetTargetCellIndex(Hoop.EHoopType type, Vector3 cellIndex, int moveUnits)
    {
        Vector3 gridIndex = cellIndex;
        Vector3 cellCounts = SpawnManager.Instance.GetCellCounts();

        int targetTargetX = (int)cellIndex.x;
        int targetTargetY = (int)cellIndex.y;
        int targetTargetZ = (int)cellIndex.z;       

        int moveDirection = Random.Range(0, 2) == 1 ? -1 : 1;

        switch (type)
        {
            case Hoop.EHoopType.MOVE_X:
                var targetX = targetTargetX + (moveDirection * moveUnits);
                moveDirection = (targetX < 0 || targetX >= cellCounts.x) ? -moveDirection : moveDirection;
                targetTargetX = Mathf.Clamp(targetTargetX + (moveDirection * moveUnits), 0, (int)cellCounts.x);
                break;
            case Hoop.EHoopType.MOVE_Y:
                var targetY = targetTargetY + (moveDirection * moveUnits);
                moveDirection = (targetY < 0 || targetY >= cellCounts.x) ? -moveDirection : moveDirection;
                targetTargetY = Mathf.Clamp(targetTargetY + (moveDirection * moveUnits), 0, (int)cellCounts.x);
                break;
            case Hoop.EHoopType.MOVE_Z:
                var targetZ = targetTargetZ + (moveDirection * moveUnits);
                moveDirection = (targetZ < 0 || targetZ >= cellCounts.x) ? -moveDirection : moveDirection;
                targetTargetZ = Mathf.Clamp(targetTargetZ + (moveDirection * moveUnits), 0, (int)cellCounts.x);
                break;
        }

        return new Vector3(targetTargetX, targetTargetY, targetTargetZ);
    }    
}
