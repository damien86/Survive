using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private int _startingWaveAmount;
    [SerializeField] private float _waveDelay = 5f;
    [SerializeField] private DialogueScriptableObject _introDialogue = null;

    private AgentManager _agentManager;
    private int _currentWave = 0;
    private int _highScore = 0;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (Instance != null) {
            Destroy(this.gameObject);
        }
    }

    private void Start() {
        _highScore = PreferenceManager.Instance.GetIntPref("HighScore");
        UIManager.Instance.UpdateHighScore(_highScore);
    }

    private IEnumerator SpawnAgents(int currentWave) {
        yield return new WaitForSeconds(_waveDelay);

        UIManager.Instance.RevealWaveText(currentWave);
        _agentManager.SpawnAgent((int)(currentWave * (1 + (currentWave + _startingWaveAmount) / 100f)) + + _startingWaveAmount);
    }

    public void SetAgentManager(AgentManager manager) {
        if (manager!= null) {
            _agentManager = manager;
        }        
    }

    public void RemoveAgent(GameObject ai) {
        _agentManager.RemoveAgent(ai);
    }

    public void StartWave() {
        _currentWave++;

        if(_currentWave == 1 && !PreferenceManager.Instance.GetBoolPref("Intro")) {
            AudioManager.Instance.SpeakVoiceLine(_introDialogue);
            PreferenceManager.Instance.UpdatePreferences("Intro_", true);
        }

        StartCoroutine(SpawnAgents(_currentWave));        
    }

    public void RestartWave() {
        _currentWave = 0;

        UIManager.Instance.RevealWaveText(_currentWave);
        StartWave();
    }

    public void HighScore() {
        if (_currentWave > _highScore) {
            _highScore = _currentWave;

            PreferenceManager.Instance.UpdatePreferences("HighScore_", _highScore);
            UIManager.Instance.UpdateHighScore(_highScore);
        }      
    }
}
