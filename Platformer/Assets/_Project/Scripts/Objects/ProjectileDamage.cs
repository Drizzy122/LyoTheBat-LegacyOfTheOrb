using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
    public float damage;

    [Tooltip("Stagger duration applied to whatever this hits — drives the enemy's knockback state and hit animation, same as melee's 0.5s.")]
    public float knockbackTime = 0.5f;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;

        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageable))
            damageable.TakeDamage(damage, knockbackTime);

        Destroy(gameObject);
    }
}
