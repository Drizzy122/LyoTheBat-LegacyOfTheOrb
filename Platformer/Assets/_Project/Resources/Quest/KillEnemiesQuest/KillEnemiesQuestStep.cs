namespace Platformer
{
    public class KillEnemiesQuestStep : QuestStep
    {
        public int enemiesToKill = 0;
        public int enemiesKilled = 0;
        
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
            if (enemiesToKill < enemiesKilled)
            {
                enemiesToKill++;
                UpdateState();
            }

            if (enemiesToKill >= enemiesKilled)
            {
                FinishQuestStep();
            }
        }

        private void UpdateState()
        {
            string state = enemiesToKill.ToString();
            string status = "Killed" + enemiesToKill + " / " + enemiesKilled + " enemies.";
            ChangeState(state,status);
        }
        protected override void SetQuestStepState(string state)
        {
            this.enemiesToKill = System.Int32.Parse(state);
            UpdateState();
        }
    }
}