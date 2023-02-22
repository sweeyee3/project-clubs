using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Setting")]
    [SerializeField] private float m_spawnHeight = 5;
    [SerializeField] private float m_spawnLength = 10;
    [SerializeField] private float m_spawnWidth = 5;

    [SerializeField] private Vector3 m_offset;

    [SerializeField] private int m_minSpawnCount = 1;
    [SerializeField] private int m_maxSpawnCount = 5;

    [Header("Grid Settings")]
    [SerializeField] private Vector3 m_cellSize;

    [SerializeField] private Ball m_ball;
    [SerializeField] private List<GameObject> m_hoopPrefabs;

    [Header("Debug Settings")]
    [SerializeField] private bool m_isDisplaySpawnGrid = true;

    private List<Hoop> m_hoopInstances;
    private List<Vector3> m_cellIndices;

    private static SpawnManager m_instance;
    public static SpawnManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                var gObj = new GameObject("SpawnManager");
                m_instance = gObj.AddComponent<SpawnManager>();
            }
            return m_instance;
        }
    }

    private void Awake()
    {
        m_instance = this;
        m_cellIndices = new List<Vector3>();
        m_hoopInstances = new List<Hoop>();

        var gridSize = GetCellCounts();

        for (var x = 0; x < gridSize.x; x++)
        {
            for (var y = 0; y < gridSize.y; y++)
            {
                for (var z = 0; z < gridSize.z; z++)
                {
                    m_cellIndices.Add(new Vector3(x, y, z));
                }
            }
        }
    }

    Vector3 MaxSpawnBound
    {
        get
        {
            // TODO: find the maximum  range given most expanded velocity

            var vLargest = m_ball.GetTargetCalculatedVelocity(0.5f, 0.5f);            

            var u = m_ball.GetVelocity(vLargest, m_ball.Gravity, new Vector3(0, -m_ball.BallHeight, m_ball.BallDistance));
            var t = m_ball.GetTime(u.z, u.z, m_ball.BallDistance);                                   

            var sz = (u.z*t) + (0.5f*m_ball.Gravity.z*Mathf.Pow(t, 2));
            var sx = m_spawnWidth;            

            return new Vector3(m_ball.InitialPosition.x + sx, 0, m_ball.transform.position.z + sz);
        }
    }

    Vector3 MinSpawnBound
    {
        get
        {
            // TODO: find the minimum range given most retracted velocity

            var vSmallest = m_ball.GetTargetCalculatedVelocity(0.5f, 1);

            var u = m_ball.GetVelocity(vSmallest, m_ball.Gravity, new Vector3(0, -m_ball.BallHeight, m_ball.BallDistance));
            var t = m_ball.GetTime(u.z, u.z, m_ball.BallDistance);

            var sz = (u.z * t) + (0.5f * m_ball.Gravity.z * Mathf.Pow(t, 2));
            var sx = 0;            

            return new Vector3(m_ball.InitialPosition.x + sx, 0, m_ball.transform.position.z + sz);            
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        var size = new Vector3(m_spawnWidth, m_spawnHeight, m_spawnLength);
        if (m_ball != null)
        {
            //var d = MaxSpawnBound - MinSpawnBound;        
            //var spawnCentreZ = m_ball.transform.position.z + MinSpawnBound.z + d.z/2;


            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(m_ball.InitialPosition + m_offset + new Vector3(0, 0, size.z / 2), size);
        }


        if (m_isDisplaySpawnGrid)
        {
            var cellCounts = GetCellCounts();
            for (var x = 0; x < cellCounts.x; x++)
            {
                for (var y = 0; y < cellCounts.y; y++)
                {
                    for (var z = 0; z < cellCounts.z; z++)
                    {
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawWireCube(GetCellPosition(x, y, z), m_cellSize);
                    }
                }
            }
        }
    }
#endif
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            Reset();
        }

        if (m_hoopInstances.Count <= 0)
        {
            var spawnCount = Random.Range(m_minSpawnCount, m_maxSpawnCount);
            while (spawnCount > 0)
            {
                Spawn();
                spawnCount--;
            }
        }
    }

    void Spawn()
    {
        // pick random prefab
        var shuffledFab = UtilityExtension.Shuffle(m_hoopPrefabs);

        // instantiate prefab
        var hoopObj = Instantiate(shuffledFab[0]);
        var hoopInstance = hoopObj.GetComponentInChildren<Hoop>();

        // pick random position
        var shuffledPositions = UtilityExtension.Shuffle(m_cellIndices);

        hoopObj.transform.position = GetCellPosition((int)shuffledPositions[0].x, (int)shuffledPositions[0].y, (int)shuffledPositions[0].z);

        hoopInstance.HoopGridIndex = shuffledPositions[0];
        shuffledPositions.RemoveAt(0);
        
        m_cellIndices = shuffledPositions;
        m_hoopInstances.Add(hoopInstance);
    }

    public void Remove(Hoop hoop)
    {
        Debug.Log("remove!");
        m_cellIndices.Add(hoop.HoopGridIndex);
        m_hoopInstances.Remove(hoop);
        Destroy(hoop.transform.parent.gameObject);
    }

    public void Reset()
    {
        if (m_hoopInstances != null)
        {
            foreach (var hoop in m_hoopInstances)
            {
                Destroy(hoop.transform.parent.gameObject);
            }
            m_hoopInstances.Clear();
        }
    }

    Vector3 GetCellCounts()
    {
        var size = new Vector3(m_spawnWidth, m_spawnHeight, m_spawnLength);

        var nx = Mathf.RoundToInt(m_cellSize.x > 0 ? size.x / m_cellSize.x : 5);
        var ny = Mathf.RoundToInt(m_cellSize.y > 0 ? size.y / m_cellSize.y : 5);
        var nz = Mathf.RoundToInt(m_cellSize.z > 0 ? size.z / m_cellSize.z : 5);

        return new Vector3(nx, ny, nz);
    }

    Vector3 GetStartPosition()
    {
        var size = new Vector3(m_spawnWidth, m_spawnHeight, m_spawnLength);

        var nx = Mathf.RoundToInt(m_cellSize.x > 0 ? size.x / m_cellSize.x : 5);
        var ny = Mathf.RoundToInt(m_cellSize.y > 0 ? size.y / m_cellSize.y : 5);
        var nz = Mathf.RoundToInt(m_cellSize.z > 0 ? size.z / m_cellSize.z : 5);

        return m_ball.InitialPosition + m_offset + m_cellSize / 2 - (new Vector3(m_cellSize.x * (nx / 2), m_cellSize.y * (ny / 2), 0));
    }
    
    Vector3 GetCellPosition(int x, int y, int z)
    {                   
        return GetStartPosition() + new Vector3(m_cellSize.x * x, m_cellSize.y * y, m_cellSize.z * z);
    }
}
