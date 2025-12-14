using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARGameManager : MonoBehaviour
{
    public static ARGameManager I { get; private set; }

    [Header("Game Rule")]
    [Tooltip("Seconds per round")]
    public float roundSeconds = 60f;

    [Header("UI (Optional)")]
    public Text timeText;      // Unity UI Text (TMP 사용해도 됨 - 아래 주석 참고)
    public Text scoreText;
    public GameObject startHintPanel;   // "Tap plane to start" 같은 안내 패널
    public GameObject resultPanel;      // 결과 패널
    public Text resultText;             // 결과 텍스트
    public Button restartButton;        // 재시작 버튼

    private float _timeLeft;
    private int _score;
    private bool _isRunning;

    private readonly List<GameObject> _spawned = new List<GameObject>();

    public bool IsRunning => _isRunning;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;

        // DontDestroyOnLoad(gameObject); // 단일 씬이면 필요 없음
    }

    private void Start()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartRound);

        ResetToWaitingState();
        RefreshUI();
    }

    private void Update()
    {
        if (!_isRunning) return;

        _timeLeft -= Time.deltaTime;
        if (_timeLeft <= 0f)
        {
            _timeLeft = 0f;
            EndRound();
        }

        RefreshUI();
    }

    public void BeginRound()
    {
        if (_isRunning) return;

        _score = 0;
        _timeLeft = roundSeconds;
        _isRunning = true;

        if (startHintPanel != null) startHintPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);

        RefreshUI();
    }

    public void EndRound()
    {
        if (!_isRunning) return;

        _isRunning = false;

        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultText != null) resultText.text = $"Collected: {_score}";
    }

    public void AddScore(int amount)
    {
        _score += amount;
        if (_score < 0) _score = 0;
        RefreshUI();
    }

    public void RegisterSpawned(GameObject go)
    {
        if (go != null) _spawned.Add(go);
    }

    public void ClearSpawned()
    {
        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            if (_spawned[i] != null) Destroy(_spawned[i]);
        }
        _spawned.Clear();
    }

    public void RestartRound()
    {
        // 라운드 종료/대기 상태로 리셋 + 오브젝트 삭제
        ClearSpawned();
        ResetToWaitingState();
        RefreshUI();
    }

    private void ResetToWaitingState()
    {
        _isRunning = false;
        _score = 0;
        _timeLeft = roundSeconds;

        if (startHintPanel != null) startHintPanel.SetActive(true);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    private void RefreshUI()
    {
        if (timeText != null)
        {
            int sec = Mathf.CeilToInt(_timeLeft);
            timeText.text = $"Time: {sec}";
        }

        if (scoreText != null)
            scoreText.text = $"Relics: {_score}";
    }
}