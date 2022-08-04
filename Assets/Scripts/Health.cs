using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] protected float _maxHealth = 100f;

    protected float _currentHealth = 1f;
    private bool _isDead = false;

    private void Awake() {
        _currentHealth = _maxHealth;
        _isDead = false;
    }

    public virtual void TakeDamage(float damage) {
        if (_isDead) {
            return;
        }

        _currentHealth -= damage;

        if (_currentHealth <= 0) {
            _isDead = true;
            _currentHealth = 0f;

            if (!this.gameObject.CompareTag("Player")) {
                AudioManager.Instance.ZombieDeath(this.gameObject);
                GameManager.Instance.RemoveAgent(this.gameObject);
            }
        }
    }

    public virtual void RecoverHealth(float health) {
        _currentHealth += health;

        if (_currentHealth > _maxHealth) {
            _currentHealth = _maxHealth;
        }
    }

    public float GetCurrentHealthPercentage() {
        return _currentHealth / _maxHealth;
    }
}
