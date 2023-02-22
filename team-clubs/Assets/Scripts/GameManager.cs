using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

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

    [Header("Managers")]
    [SerializeField] private Ball m_ball;
    [SerializeField] private SpawnManager m_spawnManager;

    [Header("Difficulty setttings")]
    [SerializeField] private AnimationCurve m_scoreDifficulty;
    [SerializeField] private float m_startTime = 30;

    [Header("UI settings")]
    [SerializeField] private TextMeshProUGUI m_gameScore;
    [SerializeField] private TextMeshProUGUI m_gameTimer;
    [SerializeField] private TextMeshProUGUI m_winScore;
    [SerializeField] private TextMeshProUGUI m_loseScore;
    [SerializeField] private GameObject m_game;
    [SerializeField] private GameObject m_win;
    [SerializeField] private GameObject m_lose;

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
            if (m_currentGameState != value)
            {
                switch (value)
                {
                    case EGameState.GAME:
                        m_ball.Reset();
                        m_spawnManager.Reset();

                        m_currentTime = m_startTime;
                        m_targetScore = (int)m_scoreDifficulty.Evaluate(m_currentRound);

                        m_game.SetActive(true);
                        m_win.SetActive(false);
                        m_lose.SetActive(false);
                        break;
                    case EGameState.WIN:
                        m_game.SetActive(false);
                        m_win.SetActive(true);
                        m_lose.SetActive(false);

                        //string winScoreText = (m_currentScore < 10) ? "0" + m_currentScore.ToString() : m_currentScore.ToString();
                        string winScoreText = m_currentScore.ToString();
                        m_winScore.text = winScoreText + " points";
                        break;
                    case EGameState.LOSE:
                        m_game.SetActive(false);
                        m_win.SetActive(false);
                        m_lose.SetActive(true);

                        //string loseScoreText = (m_currentScore < 10) ? "0" + m_currentScore.ToString() : m_currentScore.ToString();
                        string loseScoreText = m_currentScore.ToString();
                        m_loseScore.text = loseScoreText + " points";
                        break;
                }
            }
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

        switch (CurrentGameState)
        {
            case EGameState.GAME:
                m_gameScore.text = m_currentScore.ToString();

                int minute = Mathf.FloorToInt(m_currentTime / 60);
                int seconds = Mathf.FloorToInt(m_currentTime % 60);

                string minuteText = (minute < 10) ? "0" + minute.ToString() : minute.ToString();
                string secondsText = (seconds < 10) ? "0" + seconds.ToString() : seconds.ToString();
                m_gameTimer.text = minuteText + ":" + secondsText;

                // TODO: display score
                string scoreText = (m_currentScore < 10) ? "0" + m_currentScore.ToString() : m_currentScore.ToString();
                string targetScore = (m_targetScore < 10) ? "0" + m_targetScore.ToString() : m_targetScore.ToString();

                m_gameScore.text = scoreText + "/" + targetScore;
                break;
        }
    }

    public void NextRound()
    {        
        m_currentScore = 0;
        m_currentRound++;        

        CurrentGameState = EGameState.GAME;
    }

    public void Restart()
    {        
        m_currentScore = 0;
        m_currentRound = 0;        
        
        CurrentGameState = EGameState.GAME;
    }
}
