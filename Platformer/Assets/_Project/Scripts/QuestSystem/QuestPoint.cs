using System;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    /// <summary>
    /// NPC / world quest hotspot. Holds a chain of quests handled in order:
    /// the active entry is the first quest in the list that isn't FINISHED
    /// (once everything is done, the last entry stays active for post-quest
    /// flavor dialogue). Each entry can carry its own conversation; without
    /// one, submit starts/finishes the quest directly.
    /// </summary>
    public class QuestPoint : MonoBehaviour
    {
        [Serializable]
        public class QuestChainEntry
        {
            public QuestInfoSO quest;

            [Tooltip("Optional. Conversation for this quest — its sections already follow the quest state (canStart/inProgress/canFinish/finished).")]
            public DialogueConversationSO dialogue;
        }

        [Header("Quest Chain (handled in order)")]
        [SerializeField] private QuestChainEntry[] quests;

        [Header("Config")]
        [SerializeField] private bool startPoint = true;
        [SerializeField] private bool finishPoint = true;

        private bool playerIsNear = false;
        private QuestIcon questIcon;

        // Latest known state of every quest in the chain (QuestManager broadcasts
        // all states on startup and on every change).
        private readonly Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();

        private void Awake()
        {
            questIcon = GetComponentInChildren<QuestIcon>();
        }

        private void OnEnable()
        {
            GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
            GameEventsManager.instance.inputEvents.onSubmitPressed += SubmitPressed;
        }

        private void OnDisable()
        {
            if (GameEventsManager.instance == null) return;
            GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
            GameEventsManager.instance.inputEvents.onSubmitPressed -= SubmitPressed;
        }

        private QuestChainEntry ActiveEntry
        {
            get
            {
                if (quests == null || quests.Length == 0) return null;

                foreach (QuestChainEntry entry in quests)
                {
                    if (entry == null || entry.quest == null) continue;
                    if (GetState(entry.quest.id) != QuestState.FINISHED) return entry;
                }

                // whole chain done — keep the last quest active for its 'finished' dialogue
                return quests[quests.Length - 1];
            }
        }

        private QuestState GetState(string questId)
        {
            return questStates.TryGetValue(questId, out QuestState state)
                ? state
                : QuestState.REQUIREMENTS_NOT_MET;
        }

        private void SubmitPressed(InputEventContext inputEventContext)
        {
            if (!playerIsNear || !inputEventContext.Equals(InputEventContext.DEFAULT))
            {
                return;
            }

            QuestChainEntry entry = ActiveEntry;
            if (entry == null || entry.quest == null) return;

            // conversation-driven: the dialogue's quest actions start/finish the quest
            if (entry.dialogue != null)
            {
                GameEventsManager.instance.dialogueEvents.EnterDialogue(entry.dialogue);
            }
            // no dialogue: start or finish the quest immediately
            else
            {
                QuestState state = GetState(entry.quest.id);
                if (state.Equals(QuestState.CAN_START) && startPoint)
                {
                    GameEventsManager.instance.questEvents.StartQuest(entry.quest.id);
                }
                else if (state.Equals(QuestState.CAN_FINISH) && finishPoint)
                {
                    GameEventsManager.instance.questEvents.FinishQuest(entry.quest.id);
                }
            }
        }

        private void QuestStateChange(Quest quest)
        {
            bool tracked = false;
            if (quests != null)
            {
                foreach (QuestChainEntry entry in quests)
                {
                    if (entry != null && entry.quest != null && entry.quest.id.Equals(quest.info.id))
                    {
                        tracked = true;
                        break;
                    }
                }
            }
            if (!tracked) return;

            questStates[quest.info.id] = quest.state;
            RefreshIcon();
        }

        private void RefreshIcon()
        {
            QuestChainEntry entry = ActiveEntry;
            if (questIcon == null || entry == null || entry.quest == null) return;
            questIcon.SetState(GetState(entry.quest.id), startPoint, finishPoint);
        }

        private void OnTriggerEnter(Collider otherCollider)
        {
            if (otherCollider.CompareTag("Player"))
            {
                playerIsNear = true;
            }
        }

        private void OnTriggerExit(Collider otherCollider)
        {
            if (otherCollider.CompareTag("Player"))
            {
                playerIsNear = false;
            }
        }
    }
}
