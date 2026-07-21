namespace Platformer
{
    public class KillEnemiesQuestStep : QuestStep
    {
        public int enemiesToKill = 5;   // target — set on the quest step prefab
        private int enemiesKilled = 0;  // progress

        private void Start()
        {
            UpdateState();
        }

        private void OnEnable()
        {
            GameEventsManager.instance.enemyEvents.onEnemyDeath += EnemyDeath;
        }

        private void OnDisable()
        {
            GameEventsManager.instance.enemyEvents.onEnemyDeath -= EnemyDeath;
        }

        private void EnemyDeath()
        {
            if (enemiesKilled >= enemiesToKill) return;

            enemiesKilled++;
            UpdateState();

            if (enemiesKilled >= enemiesToKill)
            {
                FinishQuestStep();
            }
        }

        private void UpdateState()
        {
            string state = enemiesKilled.ToString();
            string status = $"Defeated {enemiesKilled} / {enemiesToKill} enemies.";
            ChangeState(state, status);
        }

        protected override void SetQuestStepState(string state)
        {
            this.enemiesKilled = System.Int32.Parse(state);
            UpdateState();
        }
    }
}
