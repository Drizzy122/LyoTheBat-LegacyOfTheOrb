using UnityEngine;

namespace Platformer
{
    public class GameEventsManager : MonoBehaviour
    {
        public static GameEventsManager instance { get; private set; }
        
        public MiscEvents miscEvents;
        public PlayerEvents playerEvents;
        public QuestEvents questEvents;
        public InputEvents inputEvents;
        public EnemyEvents enemyEvents;
        public DialogueEvents dialogueEvents;
        public InventoryEvents inventoryEvents;
        

      
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Found more than one Game Events Manager in the scene.");
            }

            instance = this;
            
            miscEvents = new MiscEvents();
            playerEvents = new PlayerEvents();
            questEvents = new QuestEvents();
            inputEvents = new InputEvents();
            enemyEvents = new EnemyEvents();
            dialogueEvents = new DialogueEvents();
            inventoryEvents = new InventoryEvents();
        }
    }
}