using System;
using System.Collections.Generic;

namespace Platformer
{
    public class DialogueEvents
    {
        public event Action<DialogueConversationSO> onEnterDialogue;

        public void EnterDialogue(DialogueConversationSO conversation)
        {
            if (onEnterDialogue != null)
            {
                onEnterDialogue(conversation);
            }
        }

        public event Action onDialogueStarted;
        public void DialogueStarted()
        {
            if (onDialogueStarted != null)
            {
                onDialogueStarted();
            }
        }

        public event Action onDialogueFinished;
        public void DialogueFinished()
        {
            if (onDialogueFinished != null)
            {
                onDialogueFinished();
            }
        }

        public event Action<string, List<string>> onDisplayDialogue;
        public void DisplayDialogue(string dialogueLine, List<string> dialogueChoices)
        {
            if (onDisplayDialogue != null)
            {
                onDisplayDialogue(dialogueLine, dialogueChoices);
            }
        }

        public event Action<int> onUpdateChoiceIndex;
        public void UpdateChoiceIndex(int choiceIndex)
        {
            if (onUpdateChoiceIndex != null)
            {
                onUpdateChoiceIndex(choiceIndex);
            }
        }
    }
}
