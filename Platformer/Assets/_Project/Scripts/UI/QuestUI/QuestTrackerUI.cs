using UnityEngine;
using UnityEngine.UIElements;

namespace Platformer
{
    public class QuestTrackerUI : MonoBehaviour
    {
        [Header("UI Toolkit")] [SerializeField]
        private UIDocument uiDocument;

        [Header("UXML Element Names")] [SerializeField]
        private string rootContainerName = "MissionBoxContainer";

        [SerializeField] private string questTitleName = "QuestTitle";
        [SerializeField] private string questStatusName = "QuestStatus";

        private VisualElement rootContainer;
        private Label questTitleLabel;
        private Label questStatusLabel;

        // Keep track of the currently displayed quest
        private Quest trackedQuest;

        private void Awake()
        {
            var root = uiDocument.rootVisualElement;
            rootContainer = root.Q<VisualElement>(rootContainerName);
            questTitleLabel = root.Q<Label>(questTitleName);
            questStatusLabel = root.Q<Label>(questStatusName);

            // 1. Hide the mission box when the game starts
            rootContainer.style.display = DisplayStyle.None;
        }

        private void OnEnable()
        {
            GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
            GameEventsManager.instance.questEvents.onQuestStepStateChange += QuestStepStateChange;
        }

        private void OnDisable()
        {
            GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
            GameEventsManager.instance.questEvents.onQuestStepStateChange -= QuestStepStateChange;
        }

        private void QuestStateChange(Quest quest)
        {
            // 2. If a quest is active or ready to turn in, track it and show the UI
            if (quest.state == QuestState.IN_PROGRESS || quest.state == QuestState.CAN_FINISH)
            {
                trackedQuest = quest;
                UpdateTrackerUI();
            }
            // 3. If the quest we are tracking finishes, hide the UI
            else if (quest.state == QuestState.FINISHED && trackedQuest != null &&
                     quest.info.id == trackedQuest.info.id)
            {
                rootContainer.style.display = DisplayStyle.None;
                trackedQuest = null;
            }
        }

        private void QuestStepStateChange(string questId, int stepIndex, QuestStepState questStepState)
        {
            // 4. If the step advances on the quest we are currently tracking, update the text
            if (trackedQuest != null && trackedQuest.info.id == questId)
            {
                UpdateTrackerUI();
            }
        }

        private void UpdateTrackerUI()
        {
            // Make the box visible
            rootContainer.style.display = DisplayStyle.Flex;

            // Update the text using the data from the Quest object
            questTitleLabel.text = trackedQuest.info.displayName;
            questStatusLabel.text = trackedQuest.GetFullStatusText();
        }
    }
}