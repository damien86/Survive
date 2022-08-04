using UnityEngine;

public class HealthPlayer : Health
{
    [SerializeField] private AudioClip[] _playerHitClips = null;

    public override void TakeDamage(float damage) {
        base.TakeDamage(damage);
        if(_playerHitClips != null) {
            AudioClip spawnClip = _playerHitClips[Random.Range(0, _playerHitClips.Length)];
            AudioManager.Instance.PlayEffect(spawnClip, AudioType.SFX, false, null, false);
        }

        if(_currentHealth <= 0) {
            Events.OnDeath?.Invoke();
        }
        else {
            Events.OnHealthChange?.Invoke(GetCurrentHealthPercentage());
        }        
    }

    public override void RecoverHealth(float health) {
        base.RecoverHealth(health);
        Events.OnHealthChange?.Invoke(GetCurrentHealthPercentage());
    }
}
