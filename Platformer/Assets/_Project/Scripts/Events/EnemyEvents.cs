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
    }
}