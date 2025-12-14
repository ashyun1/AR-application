using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ARGameManager : MonoBehaviour
{
    public static ARGameManager I { get; private set; }

    [Header("Game Rule")]
    public float roundSeconds = 60f;

    [Header("UI")]
    public Text timeText;
    public Text scoreText;
    public GameObject startHintPanel;
    public GameObject resultPanel;
    public Text resultText;
    public Button restartButton;

    float _timeLeft;
    int _score;
    bool _isRunning;

    readonly List<GameObject> _spawned = new List<GameObject>();
    public bool IsRunning => _isRunning;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
           
        }

        ResetToWaiting();
        RefreshUI();
    }

    void Update()
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

        RefreshUI();
    }

    public void AddScore(int value)
    {
        _score += value;
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
        ClearSpawned();
        ResetToWaiting();
        RefreshUI();
    }

    void ResetToWaiting()
    {
        _isRunning = false;
        _score = 0;
        _timeLeft = roundSeconds;

        if (startHintPanel != null) startHintPanel.SetActive(true);
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    void RefreshUI()
    {
        if (timeText != null)
            timeText.text = $"Time: {Mathf.CeilToInt(_timeLeft)}";

        if (scoreText != null)
            scoreText.text = $"Relics: {_score}";
    }
}
