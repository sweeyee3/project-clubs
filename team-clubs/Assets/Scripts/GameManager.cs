using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public enum EGameState
    {
        NONE,
        START,
        GAME,
        WIN,
        LOSE
    }

    [Header("Difficulty setttings")]
    [SerializeField] private AnimationCurve m_scoreDifficulty;
    [SerializeField] private float m_startTime = 30;

    [Header("UI settings")]

    [Header("Debug settings")]
    [SerializeField] private EGameState m_currentGameState;
    [SerializeField] private float m_currentTime;
    [SerializeField] private int m_currentScore;
    [SerializeField] private int m_currentRound;
    [SerializeField] private int m_targetScore;

    private static GameManager m_instance;    

    public int CurrentScore
    {
        get
        {
            return m_currentScore;
        }
        set
        {
            m_currentScore = value;
        }
    }

    public int CurrentRound
    {
        get
        {
            return m_currentRound;
        }
        set
        {
            m_currentRound = value;
        }
    }

    public EGameState CurrentGameState
    {
        get
        {
            return m_currentGameState;
        }
        set
        {
            // do an on enter check here?
            m_currentGameState = value;
        }
    }

    public static GameManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                var gObj = new GameObject("GameManager");
                m_instance = gObj.AddComponent<GameManager>();
            }
            return m_instance;
        }
    }

    private void Awake()
    {
        m_instance = this;
        CurrentGameState = EGameState.GAME;
    }

    private void Update()
    {
        m_currentTime -= Time.deltaTime;
        if (m_currentTime <= 0)
        {
            // check score and target score. set win or lose depending on score
            if (m_currentScore >= m_targetScore) CurrentGameState = EGameState.WIN;
            else CurrentGameState = EGameState.LOSE;
        }
    }

    private void NextRound()
    {
        m_currentTime = m_startTime;
        m_currentScore = 0;
        m_currentRound++;

        m_targetScore = (int) m_scoreDifficulty.Evaluate(m_currentRound);

        CurrentGameState = EGameState.GAME;
    }

    private void Restart()
    {
        m_currentTime = m_startTime;
        m_currentScore = 0;
        m_currentRound = 0;

        m_targetScore = (int)m_scoreDifficulty.Evaluate(m_currentRound);
        
        CurrentGameState = EGameState.GAME;
    }
}
