using UnityEngine;

public class ProjectileBuilder
{
    private GameObject _projectilePrefab;
    private float _speed;
    private float _duration;
    private Transform _caster;

    public ProjectileBuilder WithProjectilePrefab(GameObject projectilePrefab)
    {
        _projectilePrefab = projectilePrefab;
        return this;
    }

    public ProjectileBuilder WithSpeed(float speed)
    {
        _speed = speed;
        return this;
    }

    public ProjectileBuilder WithDuration(float duration)
    {
        _duration = duration;
        return this;
    }

    public ProjectileBuilder WithCaster(Transform caster)
    {
        _caster = caster;
        return this;
    }

    public GameObject Build(Transform origin)
    {
        GameObject blastProjectile = Object.Instantiate(_projectilePrefab, origin.position, origin.rotation);

        // Ignore all caster colliders immediately — before any physics step
        if (_caster != null && blastProjectile.TryGetComponent<Collider>(out var projectileCollider))
        {
            foreach (var casterCollider in _caster.GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(projectileCollider, casterCollider);
        }

        if (blastProjectile.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = origin.forward * _speed;

        Object.Destroy(blastProjectile, _duration);

        return blastProjectile;
    }
}
