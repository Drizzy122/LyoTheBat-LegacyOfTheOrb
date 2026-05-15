using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    public float damage;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;

        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
            damageable.TakeDamage(damage);

        Destroy(gameObject);
    }
}
