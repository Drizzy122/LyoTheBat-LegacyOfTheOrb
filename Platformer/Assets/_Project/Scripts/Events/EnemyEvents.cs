using System;

namespace Platformer
{
    public class EnemyEvents
    {
        public event Action onEnemyDeath;
        public void EnemyDeath()
        {
            if (onEnemyDeath != null)
            {
                onEnemyDeath();
            }
        }

        public event Action<float> onEnemyHit;
        public void EnemyHit(float knockback)
        {
            if (onEnemyHit != null)
            {
                onEnemyHit?.Invoke(knockback);
            }
        }
    }
}