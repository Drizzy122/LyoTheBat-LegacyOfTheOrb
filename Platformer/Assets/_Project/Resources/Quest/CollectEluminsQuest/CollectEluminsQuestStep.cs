using UnityEngine.Serialization;

namespace Platformer
{
    public class CollectEluminsQuestStep : QuestStep
    {
        private int eluminsCollected = 0;
        [FormerlySerializedAs("coinsToComplete")] public int eluminsToComplete = 5;

        private void Start()
        {
            UpdateState();
        }
        private void OnEnable()
        {
            GameEventsManager.instance.miscEvents.onLuminCollected += EluminCollected;
          
        }

        private void OnDisable()
        {
            GameEventsManager.instance.miscEvents.onLuminCollected -= EluminCollected;
        }
        
        private void EluminCollected()
        {
            if (eluminsCollected < eluminsToComplete)
            {
                eluminsCollected++;
                UpdateState();
            }

            if (eluminsCollected >= eluminsToComplete)
            {
                FinishQuestStep();
            }
        }
        
        private void UpdateState()
        {
            string state = eluminsCollected.ToString();
            string status = "Collected" + eluminsCollected + " / " + eluminsToComplete + "coins.";
            ChangeState(state, status);
        }
        
        protected override void SetQuestStepState(string state)
        {
            this.eluminsCollected = System.Int32.Parse(state);
            UpdateState();
        }
    }
}