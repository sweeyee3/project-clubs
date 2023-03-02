using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public enum EGameState
    {
        NONE,
        START,
        GAME,
        WIN,
        LOSE,
        PAUSE
    }

    [Header("Managers")]
    [SerializeField] private CurveHandler m_curveHandler;
    [SerializeField] private SpawnManager m_spawnManager;

    [Header("Score setttings")]
    [SerializeField][Tooltip("x-axis: Number of rounds, y-axis: target score to pass round")] private AnimationCurve m_scoreDifficulty;
    [SerializeField][Tooltip("x-axis: Number of rounds, y-axis: time increment for every set of hoops cleared")] private AnimationCurve m_setTimeIncrement;
    [SerializeField][Tooltip("x-axis: Hoop distance from player, y-axis: score player receives")] private AnimationCurve m_hoopScoreIncrement;
    [SerializeField][Tooltip("Initial start time of every round")] private float m_startTime = 30;

    [Header("Spawn difficulty settings")]
    [SerializeField][Tooltip("x-axis: Number of rounds, y-axis: minimum number of boards")] private AnimationCurve m_minSpawnCount;
    [SerializeField][Tooltip("x-axis: Number of rounds, y-axis: maximum number of boards")] private AnimationCurve m_maxSpawnCount;

    [Header("(NOT IN USE) Distance difficulty settings")]
    [SerializeField] private AnimationCurve m_distanceSpawnProbabilityMin;
    [SerializeField] private AnimationCurve m_distanceSpawnProbabilityMax;
    [SerializeField] private AnimationCurve m_distanceRoundSpawnProbability;

    [Header("Spawn type difficulty settings")]
    [SerializeField][Tooltip("x-axis: number of rounds, y-axis: probability of hoop type spawning")] private List<AnimationCurve> m_hoopSpawnProbability;

    [Header("UI settings")]
    [SerializeField] private TextMeshProUGUI m_gameScore;
    [SerializeField] private TextMeshProUGUI m_gameTimer;
    [SerializeField] private TextMeshProUGUI m_winScore;
    [SerializeField] private TextMeshProUGUI m_loseScore;
    [SerializeField] private TextMeshProUGUI m_addedTimer;
    [SerializeField] private TextMeshProUGUI m_roundText;
    [SerializeField] private TextMeshProUGUI m_clearedText;
    [SerializeField] private TextMeshProUGUI m_finalStage;
    [SerializeField] private GameObject m_game;
    [SerializeField] private GameObject m_win;
    [SerializeField] private GameObject m_lose;
    [SerializeField] private GameObject m_pauseMenu;
    [SerializeField] private GameObject m_startMenu;
    [SerializeField] private GameObject m_tutorial;
    [SerializeField] private Button m_pauseButton;
    [SerializeField] private Button m_playButton;

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

    public float CurrentTime
    {
        get
        {
            return m_currentTime;
        }
        set
        {
            m_currentTime = value;
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
                // entering state
                switch (value)
                {
                    case EGameState.START:
                        m_game.SetActive(false);
                        m_win.SetActive(false);
                        m_lose.SetActive(false);
                        m_pauseMenu.SetActive(false);
                        m_startMenu.SetActive(true);

                        AudioManager.Instance.Play("startBGM", AudioManager.EAudioType.BGM);
                        AudioManager.Instance.Stop("gameBGM", AudioManager.EAudioType.BGM);
                        AudioManager.Instance.Stop("gameTimeUp", AudioManager.EAudioType.SFX);
                        break;

                    case EGameState.GAME:                                                
                        m_game.SetActive(true);
                        m_win.SetActive(false);
                        m_lose.SetActive(false);
                        m_pauseMenu.SetActive(false);
                        m_startMenu.SetActive(false);

                        m_pauseButton.gameObject.SetActive(true);
                        m_playButton.gameObject.SetActive(false);

                        m_tutorial.SetActive(!PlayerPrefs.HasKey("tutorial"));                        

                        AudioManager.Instance.Play("gameBGM", AudioManager.EAudioType.BGM);
                        AudioManager.Instance.Stop("startBGM", AudioManager.EAudioType.BGM);
                        AudioManager.Instance.Stop("gameTimeUp", AudioManager.EAudioType.SFX);
                        break;
                    case EGameState.WIN:
                        m_game.SetActive(false);
                        m_win.SetActive(true);
                        m_lose.SetActive(false);
                        m_pauseMenu.SetActive(false);
                        m_startMenu.SetActive(false);

                        //string winScoreText = (m_currentScore < 10) ? "0" + m_currentScore.ToString() : m_currentScore.ToString();
                        string winScoreText = m_currentScore.ToString();
                        m_clearedText.text = "stage " + (m_currentRound+1).ToString() + " cleared!";  
                        m_winScore.text = winScoreText + " points";

                        AudioManager.Instance.Play("gameTimeUp", AudioManager.EAudioType.SFX);
                        break;
                    case EGameState.LOSE:
                        m_game.SetActive(false);
                        m_win.SetActive(false);
                        m_lose.SetActive(true);
                        m_pauseMenu.SetActive(false);
                        m_startMenu.SetActive(false);

                        //string loseScoreText = (m_currentScore < 10) ? "0" + m_currentScore.ToString() : m_currentScore.ToString();
                        string loseScoreText = m_currentScore.ToString();
                        m_loseScore.text = loseScoreText + " points";
                        m_finalStage.text = "stage " + (m_currentRound + 1).ToString();

                        AudioManager.Instance.Play("gameTimeUp", AudioManager.EAudioType.SFX);
                        break;
                    case EGameState.PAUSE:                        
                        m_pauseMenu.SetActive(true);
                        m_pauseButton.gameObject.SetActive(false);
                        m_playButton.gameObject.SetActive(true);

                        AudioManager.Instance.Stop("gameBGM", AudioManager.EAudioType.BGM);
                        AudioManager.Instance.Play("startBGM", AudioManager.EAudioType.BGM);
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
        CurrentGameState = EGameState.START;
    }

    private void Update()
    {
        switch (CurrentGameState)
        {
            case EGameState.GAME:
                m_currentTime -= Time.deltaTime;
                if (m_currentTime <= 0)
                {
                    // check score and target score. set win or lose depending on score
                    if (m_currentScore >= m_targetScore) CurrentGameState = EGameState.WIN;
                    else CurrentGameState = EGameState.LOSE;
                }
                
                m_gameScore.text = m_currentScore.ToString();

                int minute = Mathf.FloorToInt(m_currentTime / 60);
                int seconds = Mathf.FloorToInt(m_currentTime % 60);

                string minuteText = (minute < 10) ? "0" + minute.ToString() : minute.ToString();
                string secondsText = (seconds < 10) ? "0" + seconds.ToString() : seconds.ToString();
                m_gameTimer.text = minuteText + ":" + secondsText;

                //Display current round
                m_roundText.text = "stage " + (m_currentRound+1).ToString();

                // TODO: display score
                string scoreText = (m_currentScore < 10) ? "0" + m_currentScore.ToString() : m_currentScore.ToString();
                string targetScore = (m_targetScore < 10) ? "0" + m_targetScore.ToString() : m_targetScore.ToString();

                m_gameScore.text = scoreText + "/" + targetScore;
                break;
        }
    }

    public void TriggerTimerText(string text, bool isActivate)
    {
        m_addedTimer.text = "+" + text + "s";
        m_addedTimer.gameObject.SetActive(isActivate);
    }

    public float GetTimeIncrement()
    {
        return m_setTimeIncrement.Evaluate(CurrentRound);
    }

    public int GetScore(int distance)
    {
        return (int)m_hoopScoreIncrement.Evaluate(distance);
    }

    public int GetMinSpawnCount()
    {
        int minSpawn = (int)m_minSpawnCount.Evaluate(CurrentRound);

        return minSpawn;
    }

    public int GetMaxSpawnCount()
    {
        int maxSpawn = (int)m_maxSpawnCount.Evaluate(CurrentRound);

        return maxSpawn;
    }

    public int GetDistanceSpawnProbability(int maxDistance, float probability)
    {
        int z = 0;
        float prevProbability = 0;
        for (int i = 0; i< maxDistance; i++)
        {
            var min = m_distanceSpawnProbabilityMin.Evaluate(i);
            var max = m_distanceSpawnProbabilityMax.Evaluate(i);
            var interpolater = m_distanceRoundSpawnProbability.Evaluate(CurrentRound);

            var p = Mathf.Lerp(min, max, interpolater);
            if (probability > prevProbability && probability <= p)
            {
                z = i;
                break;
            }
            prevProbability = p;
        }

        return z;
    }
    
    public Hoop.EHoopType GetSpawnedHoop(float probability)
    {
        // pick static first, then pick x then so on and so forth
        Dictionary<Hoop.EHoopType, float> hoopSpawnProbabilities = new Dictionary<Hoop.EHoopType, float>();
        float totalProbability = 1;
        foreach (var e in Enum.GetValues(typeof(Hoop.EHoopType)))
        {
            var eHoopType = (Hoop.EHoopType)e;
            var eValue = m_hoopSpawnProbability[(int)e].Evaluate(CurrentRound);

            if (totalProbability > eValue) hoopSpawnProbabilities[eHoopType] = eValue;
            else hoopSpawnProbabilities[eHoopType] = totalProbability;
            totalProbability = Mathf.Clamp(totalProbability - hoopSpawnProbabilities[eHoopType], 0, 1);
        }

        float prevP = 0;
        var outKey = Hoop.EHoopType.STATIC;
        foreach(var k in hoopSpawnProbabilities.Keys)
        {
            var p = hoopSpawnProbabilities[k];
            if (probability > prevP && probability <= p)
            {
                outKey = k;
                break;
            }
            prevP = p;
        }

        return outKey;
    }

    public void StartGame()
    {
        m_currentScore = 0;
        m_currentRound = 0;

        Restart();

        CurrentGameState = EGameState.GAME;
    }

    public void DisableTutorial()
    {
        m_tutorial.SetActive(false);
        PlayerPrefs.SetInt("tutorial", 1);
    }

    public void NextRound()
    {        
        m_currentScore = 0;
        m_currentRound++;

        m_curveHandler.ResetAll();
        m_spawnManager.Reset();

        m_currentTime = m_startTime;
        m_targetScore = (int)m_scoreDifficulty.Evaluate(m_currentRound);

        CurrentGameState = EGameState.GAME;
    }

    public void Restart()
    {        
        m_currentScore = 0;
        m_currentRound = 0;

        m_curveHandler.ResetAll();
        m_spawnManager.Reset();

        m_currentTime = m_startTime;
        m_targetScore = (int)m_scoreDifficulty.Evaluate(m_currentRound);

        CurrentGameState = EGameState.GAME;
    }

    public void Pause()
    {
        CurrentGameState = EGameState.PAUSE;                       
    }

    public void UnPause()
    {
        CurrentGameState = EGameState.GAME;
    }

    public void BackToStart()
    {
        CurrentGameState = EGameState.START;
    }
}
