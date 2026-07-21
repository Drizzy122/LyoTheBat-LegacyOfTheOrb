using System;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
    public enum DialogueQuestAction
    {
        None,
        StartQuest,
        AdvanceQuest,
        FinishQuest
    }

    [Serializable]
    public class DialogueLine
    {
        [Tooltip("Optional. Shown as \"Speaker: text\" when set.")]
        public string speaker;

        [TextArea(2, 5)]
        public string text;
    }

    [Serializable]
    public class DialogueChoice
    {
        [Tooltip("Text shown on the choice button.")]
        public string text;

        [Tooltip("Quest action fired on the conversation's linked quest when this choice is picked.")]
        public DialogueQuestAction action = DialogueQuestAction.None;

        [Tooltip("Lines the NPC says after this choice is picked. The conversation ends after them.")]
        public List<DialogueLine> responseLines = new List<DialogueLine>();
    }

    [Serializable]
    public class DialogueSection
    {
        [Tooltip("Said in order, one per submit press.")]
        public List<DialogueLine> lines = new List<DialogueLine>();

        [Tooltip("Optional. Played instead of Lines when the player has already heard this section this session — a short re-greeting instead of the full intro. Leave empty to replay Lines.")]
        public List<DialogueLine> repeatLines = new List<DialogueLine>();

        [Tooltip("Optional. Offered together with the last line.")]
        public List<DialogueChoice> choices = new List<DialogueChoice>();

        public bool HasContent =>
            (lines != null && lines.Count > 0) ||
            (choices != null && choices.Count > 0);
    }

    /// <summary>
    /// One NPC conversation, authored entirely in the inspector (replaces the
    /// old ink knots). When a quest is linked, the section matching the quest's
    /// current state plays; otherwise — or when that section is empty — the
    /// fallback section plays.
    /// </summary>
    [CreateAssetMenu(fileName = "New Conversation", menuName = "Dialogue/Conversation")]
    public class DialogueConversationSO : ScriptableObject
    {
        [Header("Quest link (optional)")]
        [Tooltip("When set, the section matching this quest's current state is played.")]
        public QuestInfoSO quest;

        [Header("Sections per quest state")]
        public DialogueSection requirementsNotMet = new DialogueSection();
        public DialogueSection canStart = new DialogueSection();
        public DialogueSection inProgress = new DialogueSection();
        public DialogueSection canFinish = new DialogueSection();
        public DialogueSection finished = new DialogueSection();

        [Header("Fallback (no quest linked, or the state's section is empty)")]
        public DialogueSection fallback = new DialogueSection();

        public DialogueSection GetSection(QuestState? questState)
        {
            if (quest == null || questState == null) return fallback;

            DialogueSection section = questState.Value switch
            {
                QuestState.REQUIREMENTS_NOT_MET => requirementsNotMet,
                QuestState.CAN_START => canStart,
                QuestState.IN_PROGRESS => inProgress,
                QuestState.CAN_FINISH => canFinish,
                QuestState.FINISHED => finished,
                _ => fallback
            };

            return section != null && section.HasContent ? section : fallback;
        }
    }
}
