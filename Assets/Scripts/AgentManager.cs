using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField] private GameObject _zombiePrefab = null;
    [SerializeField] private Transform[] _spawnpoints = null;

    private List<EnemyAI> _agents = null;
    private GameObject _player = null;

    private void Awake() {
        _agents = new List<EnemyAI>();
        _player = GameObject.FindGameObjectWithTag("Player");

        GameManager.Instance.SetAgentManager(this);
    }

    void Update() {
        foreach(EnemyAI agent in _agents) {
            agent.SetDestination(_player);
        }
    }

    public void SpawnAgent(int howManyToSpawn) {
        for (int i = 0; i < howManyToSpawn; i++) {
            if (_spawnpoints == null) {
                return;
            }
            Transform randomSpawnPoint = _spawnpoints[Random.Range(0, _spawnpoints.Length)];            
            
            GameObject clone = Instantiate(_zombiePrefab, randomSpawnPoint.position, Quaternion.identity);
            if (clone == null) {
                return;
            }

            EnemyAI ai = clone.GetComponent<EnemyAI>();
            if (ai == null) {
                Destroy(clone);
                Debug.LogError("Clone didn't survive");
                return;
            }

            ai.ChooseSkin();
            _agents.Add(ai);
        }        
    }

    public void RemoveAgent(GameObject agent) {
        EnemyAI ai = agent.GetComponent<EnemyAI>();

        if (ai != null) {
            _agents.Remove(ai);
            if (_agents.Count <= 0) {
                GameManager.Instance.StartWave();
            }
        }

        Destroy(agent);
    }
}
