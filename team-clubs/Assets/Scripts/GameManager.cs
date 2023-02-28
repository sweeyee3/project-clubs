using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
        LOSE,
        PAUSE
    }

    [Header("Managers")]
    [SerializeField] private CurveHandler m_curveHandler;
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
    [SerializeField] private GameObject m_pauseMenu;
    [SerializeField] private GameObject m_startMenu;
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

                // TODO: display score
                string scoreText = (m_currentScore < 10) ? "0" + m_currentScore.ToString() : m_currentScore.ToString();
                string targetScore = (m_targetScore < 10) ? "0" + m_targetScore.ToString() : m_targetScore.ToString();

                m_gameScore.text = scoreText + "/" + targetScore;
                break;
        }
    }

    public void StartGame()
    {
        m_currentScore = 0;
        m_currentRound = 0;

        Restart();

        CurrentGameState = EGameState.GAME;
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
