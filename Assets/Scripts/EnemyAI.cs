using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("Model")]
    [SerializeField] private GameObject[] _skins = null;

    [Header("Combat")]
    [SerializeField] private float _attackCooldown = 3f;
    [SerializeField] private float _attackStrength = 3f;    
    private float _currentCooldownTimer = 0f;
    private NavMeshAgent _agent = null;
    private Animator _animator = null;

    private void Awake() {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _currentCooldownTimer = _attackCooldown;
    }

    private void Start() {       
        AudioManager.Instance.ZombieSpawn(this.gameObject);
    }

    private void OnDestroy() {
        if (!AudioManager.IsDestroyed) {
            AudioSource[] sources = gameObject.GetComponentsInChildren<AudioSource>();
            if (sources != null) {
                AudioManager.Instance.ReturnSourceToParent(sources);
            }
        }                
    }    

    private void OnTriggerStay(Collider other) {
        if (UIManager.Instance.GetCurrentType() != UIType.HUD) {
            return;
        }

        if (_currentCooldownTimer >= _attackCooldown) {
            if (other.CompareTag("Player")) {
                _animator.Play("Zombie_Attack");
                _currentCooldownTimer = 0f;
                IDamageable target = other.GetComponent<IDamageable>();
                target?.TakeDamage(_attackStrength);
                AudioManager.Instance.ZombieAttack(this.gameObject);
            }
        }
        else {
            _currentCooldownTimer += Time.deltaTime;
        }        
    }

    public void SetDestination(GameObject target) {
        _agent.SetDestination(target.transform.position);
    }

    public void ChooseSkin() {
        int randomSkin = Random.Range(0, _skins.Length);
        _skins[randomSkin].SetActive(true);
    }
}
