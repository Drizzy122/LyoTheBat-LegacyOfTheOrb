using UnityEngine;

namespace Platformer
{
    public class TutorialQuest : MonoBehaviour
    {
        
        [Header("Quest")]
        [SerializeField] private QuestInfoSO questInfoForPoint;
        
        [Header("Config")]
        [SerializeField] private bool startPoint = true;
        [SerializeField] private bool finishPoint = true;
        
    
        private string questId;
        private QuestState currentQuestState;
        private QuestIcon questIcon;
        
        

        private void Awake()
        {
            questId = questInfoForPoint.id;
            questIcon = GetComponentInChildren<QuestIcon>();
        }

        private void OnEnable()
        {
            GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
       
        }

        private void OnDisable()
        {
            GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
           
        }
        
        private void QuestStateChange(Quest quest)
        {
            if (quest.info.id.Equals(questId))
            {
                currentQuestState = quest.state;
                questIcon.SetState(currentQuestState, startPoint, finishPoint);
            }
        }
        private void OnTriggerEnter(Collider otherCollider)
        {
            if (otherCollider.CompareTag("Player"))
            {
         
                
                // start or finish quest
                if (currentQuestState.Equals(QuestState.CAN_START) && startPoint)
                {
                    GameEventsManager.instance.questEvents.StartQuest(questId);
                }
                else if (currentQuestState.Equals(QuestState.CAN_FINISH) && finishPoint)
                {
                    GameEventsManager.instance.questEvents.FinishQuest(questId);
                }
            }
        }

        private void OnTriggerExit(Collider otherCollider)
        {
            if (otherCollider.CompareTag("Player"))
            {
           
                
            }
        }
    }
}