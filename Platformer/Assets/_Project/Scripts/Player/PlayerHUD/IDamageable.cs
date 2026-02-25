public interface IDamageable
{
    void TakeDamage(float damage, float knockBackTime = 0f);
}

public interface IWeapon
{
    void Fire(IDamageable target);
}