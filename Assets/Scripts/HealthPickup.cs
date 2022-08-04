using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [SerializeField] private float _health = 100f;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            IDamageable health = other.GetComponent<IDamageable>();
            health?.RecoverHealth(_health);
            Destroy(this.gameObject);
        }
    }
}
