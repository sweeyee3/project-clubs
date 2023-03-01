using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class SpawnManager : MonoBehaviour
{
    public struct SpawnPair
    {
        public Vector3 CellIndex;
        public GameObject Fab;        
    }

    [Header("Spawn Setting")]
    [SerializeField] private float m_spawnHeight = 5;
    [SerializeField] private float m_spawnLength = 10;
    [SerializeField] private float m_spawnWidth = 5;

    [SerializeField] private Vector3 m_offset;

    [SerializeField] private int m_minSpawnCount = 1;
    [SerializeField] private int m_maxSpawnCount = 5;

    [SerializeField] private GameObject m_effectPrefab;    

    [Header("Grid Settings")]
    [SerializeField] private Vector3 m_cellSize;

    [SerializeField] private CurveHandler m_ball;
    [SerializeField] public List<GameObject> m_hoopPrefabs;

    [Header("Debug Settings")]
    [SerializeField] private bool m_isDisplaySpawnGrid = true;

    private List<Hoop> m_hoopInstances;
    private List<Vector3> m_cellIndices;

    private Coroutine m_spawnRoutine;

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
            var spawnCount = Random.Range(GameManager.Instance.GetMinSpawnCount(), GameManager.Instance.GetMaxSpawnCount());

            List<SpawnPair> pairs = Generate(spawnCount);
            foreach (var pair in pairs)
            {
                Spawn(pair.Fab, pair.CellIndex);
            }

            var timer = Mathf.FloorToInt(GameManager.Instance.GetTimeIncrement());
            GameManager.Instance.CurrentTime += timer;
            if (m_spawnRoutine != null) StopCoroutine(m_spawnRoutine);
            m_spawnRoutine = StartCoroutine(SpawnTimerIncrement(timer));            
        }
    }

    List<SpawnPair> Generate(int count)
    {
        var spawns = new List<SpawnPair>();
        var tempSpawnPair = new List<SpawnPair>();
        if (m_cellIndices.Count <= 0) return spawns;

        var indices = new List<int>();
        var fabs = new List<GameObject>();
        
        while (count >= 0)
        {
            // pick probabilistic z poisition but randomise y and x
            var distanceProbability = Random.Range(0.0f, 1.0f);
            //Vector3 cellCount = GetCellCounts();
            //int z = GameManager.Instance.GetDistanceSpawnProbability((int)cellCount.z, distanceProbability);
            //int z = Random.Range(0, (int)cellCount.z);
            //int y = Random.Range(0, (int)cellCount.y);
            //int x = Random.Range(0, (int)cellCount.x);
            var shuffledPositions = CustomUtility.Shuffle(m_cellIndices);
            var cellIdx = shuffledPositions[0];

            // pick probabilisitc hoop            
            var hoopProbability = Random.Range(0.0f, 1.0f);
            var hoopType = GameManager.Instance.GetSpawnedHoop(hoopProbability);
           
            //Vector3 cellIdx = new Vector3(x, y, z);            
            var fab = m_hoopPrefabs.Find(x => x.GetComponentInChildren<Hoop>().HoopType == hoopType);

            SpawnPair sp;
            sp.CellIndex = cellIdx;
            sp.Fab = fab;
            tempSpawnPair.Add(sp);
            
            m_cellIndices.Remove(cellIdx);
            
            count--;
        }

        // spawn static stuff first, then spawn other types
        tempSpawnPair = tempSpawnPair.OrderBy(x => x.Fab.GetComponentInChildren<Hoop>().HoopType).ToList();

        for (int i=0; i<tempSpawnPair.Count; i++)
        {            
            // check prefab type to see if can spawn. if cannot, change to static
            switch (tempSpawnPair[i].Fab.GetComponentInChildren<Hoop>().HoopType)
            {
                case Hoop.EHoopType.STATIC:
                    SpawnPair pair;
                    pair.Fab = tempSpawnPair[i].Fab;
                    pair.CellIndex = tempSpawnPair[i].CellIndex;
                    spawns.Add(pair);                                       
                    break;
                case Hoop.EHoopType.MOVE_X:                    
                    var item = spawns.Find(x => ((x.CellIndex.z == tempSpawnPair[i].CellIndex.z) && (x.CellIndex.y == tempSpawnPair[i].CellIndex.y) && (x.CellIndex.x == tempSpawnPair[i].CellIndex.x - 1 || x.CellIndex.x == tempSpawnPair[i].CellIndex.x + 1)));                    

                    if (item.Fab == null)
                    {                        
                        pair.Fab = tempSpawnPair[i].Fab;
                        pair.CellIndex = tempSpawnPair[i].CellIndex;
                        spawns.Add(pair);
                    }                    
                    break;
            }
            
            //count--;
        }    

        return spawns;
    }

    void Spawn(GameObject fab, Vector3 cellIndex)
    {                    
        // instantiate prefab
        var hoopObj = Instantiate(fab);
        var hoopInstance = hoopObj.GetComponentInChildren<Hoop>();       

        hoopObj.transform.position = GetCellPosition((int)cellIndex.x, (int)cellIndex.y, (int)cellIndex.z);

        hoopInstance.CellIndex = cellIndex;        
        m_hoopInstances.Add(hoopInstance);
    }

    public void Remove(Hoop hoop, int score)
    {
        // spawn hoop effect here
        StartCoroutine(SpawnEffect(m_effectPrefab, hoop.transform.parent.position, score));
        AudioManager.Instance.Play("ballSuccess", AudioManager.EAudioType.SFX);
        CustomUtility.Vibrate();
        m_cellIndices.Add(hoop.CellIndex);
        m_hoopInstances.Remove(hoop);
        Destroy(hoop.transform.parent.gameObject);
    }

    IEnumerator SpawnEffect(GameObject effect, Vector3 position, int score)
    {
        var effectObj = Instantiate(effect);
        effectObj.transform.position = position;
        effectObj.GetComponentInChildren<TextMeshPro>().text = "+" + score.ToString();
        yield return new WaitForSeconds(0.5f);
        Debug.Log("effects destroyed");
        Destroy(effectObj);
    }

    IEnumerator SpawnTimerIncrement(int timer)
    {
        if (timer >= 1)
        {            
            GameManager.Instance.TriggerTimerText(timer.ToString(), true);
            yield return new WaitForSeconds(0.5f);
            GameManager.Instance.TriggerTimerText(timer.ToString(), false);
        }
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

        // spawn again
        var spawnCount = Random.Range(GameManager.Instance.GetMinSpawnCount(), GameManager.Instance.GetMaxSpawnCount());
        List<SpawnPair> pairs = Generate(spawnCount);
        foreach (var pair in pairs)
        {
            Spawn(pair.Fab, pair.CellIndex);
        }

        if (m_cellIndices != null)
        {
            var gridSize = GetCellCounts();
            m_cellIndices.Clear();
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
    }

    public Vector3 GetCellCounts()
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

        var startPos = Vector3.zero;
        if (m_ball != null) startPos = m_ball.InitialPosition;

        return startPos + m_offset + m_cellSize / 2 - (new Vector3(m_cellSize.x * (nx / 2), m_cellSize.y * (ny / 2), 0));
    }
    
    public Vector3 GetCellPosition(int x, int y, int z)
    {                   
        return GetStartPosition() + new Vector3(m_cellSize.x * x, m_cellSize.y * y, m_cellSize.z * z);
    }
}
