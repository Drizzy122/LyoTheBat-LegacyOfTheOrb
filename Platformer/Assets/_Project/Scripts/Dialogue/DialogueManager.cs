using System.Collections.Generic;
using UnityEngine;

namespace Platformer
{
   /// <summary>
   /// Runs DialogueConversationSO assets (replaces the old ink Story runner).
   /// Picks the conversation section matching the linked quest's current state,
   /// plays its lines one submit-press at a time, offers choices with the last
   /// line, and fires the chosen quest action (start/advance/finish).
   /// </summary>
   public class DialogueManager : MonoBehaviour
   {
      [Tooltip("Optional. When empty, it's taken from the CameraManager at runtime. Used to lock gameplay input (aim, attacks, look) during dialogue.")]
      [SerializeField] private InputReader inputReader;

      // Current quest states, kept fresh by QuestManager's broadcasts
      // (all states are broadcast on startup and on every change).
      private readonly Dictionary<string, QuestState> questStates = new Dictionary<string, QuestState>();

      // Sections the player has already heard this session — they play their
      // shorter repeatLines (when authored) instead of the full intro.
      private readonly HashSet<DialogueSection> seenSections = new HashSet<DialogueSection>();

      private DialogueConversationSO currentConversation;
      private List<DialogueLine> pendingLines;
      private List<DialogueChoice> pendingChoices;
      private int lineIndex;
      private bool awaitingChoice;
      private int currentChoiceIndex = -1;
      private bool dialoguePlaying = false;

      private PlayerMovement _playerMovement;
      private CameraManager cameraManager;

      private void Awake()
      {
         _playerMovement = FindAnyObjectByType<PlayerMovement>();
         cameraManager = FindAnyObjectByType<CameraManager>();
         if (inputReader == null && cameraManager != null)
         {
            inputReader = cameraManager.InputReader;
         }
      }

      private void OnEnable()
      {
         GameEventsManager.instance.dialogueEvents.onEnterDialogue += EnterDialogue;
         GameEventsManager.instance.inputEvents.onSubmitPressed += SubmitPressed;
         GameEventsManager.instance.dialogueEvents.onUpdateChoiceIndex += UpdateChoiceIndex;
         GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
      }

      private void OnDisable()
      {
         if (GameEventsManager.instance == null) return;
         GameEventsManager.instance.dialogueEvents.onEnterDialogue -= EnterDialogue;
         GameEventsManager.instance.inputEvents.onSubmitPressed -= SubmitPressed;
         GameEventsManager.instance.dialogueEvents.onUpdateChoiceIndex -= UpdateChoiceIndex;
         GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
      }

      private void QuestStateChange(Quest quest)
      {
         questStates[quest.info.id] = quest.state;
      }

      private void UpdateChoiceIndex(int choiceIndex)
      {
         this.currentChoiceIndex = choiceIndex;
      }

      private void SubmitPressed(InputEventContext inputEventContext)
      {
         if (!inputEventContext.Equals(InputEventContext.DIALOGUE))
         {
            return;
         }
         ContinueOrExitDialogue();
      }

      private void EnterDialogue(DialogueConversationSO conversation)
      {
         if (dialoguePlaying || conversation == null)
         {
            return;
         }

         DialogueSection section = conversation.GetSection(GetQuestState(conversation));
         if (section == null || !section.HasContent)
         {
            Debug.LogWarning($"Conversation '{conversation.name}' has nothing to say in the current quest state.");
            return;
         }

         dialoguePlaying = true;
         currentConversation = conversation;

         // Revisited sections play their shorter repeat lines when authored
         bool seenBefore = !seenSections.Add(section);
         bool useRepeat = seenBefore && section.repeatLines != null && section.repeatLines.Count > 0;
         pendingLines = useRepeat ? section.repeatLines : (section.lines ?? new List<DialogueLine>());

         pendingChoices = section.choices ?? new List<DialogueChoice>();
         lineIndex = 0;
         awaitingChoice = false;
         currentChoiceIndex = -1;

         // inform other parts of our system that we've started dialogue
         GameEventsManager.instance.dialogueEvents.DialogueStarted();

         // input event context
         GameEventsManager.instance.inputEvents.ChangeInputEventContext(InputEventContext.DIALOGUE);

         // freeze player movement
         if (_playerMovement != null)
         {
            _playerMovement.enabled = false;
         }

         // lock gameplay input (aim, attacks, look events) — dialogue submit
         // comes through the separate InputManager/PlayerInput path
         inputReader?.DisablePlayerMap();

         // disable camera control (Cinemachine reads the Look action directly,
         // so the orbit axis has to be frozen too, not just the manager)
         if (cameraManager != null)
         {
            cameraManager.SetLookInputEnabled(false);
            cameraManager.enabled = false;
         }

         // kick off the conversation
         ContinueOrExitDialogue();
      }

      private QuestState? GetQuestState(DialogueConversationSO conversation)
      {
         if (conversation.quest != null && questStates.TryGetValue(conversation.quest.id, out QuestState state))
         {
            return state;
         }
         return null;
      }

      private void ContinueOrExitDialogue()
      {
         // A choice is on screen → resolve the picked one first
         if (awaitingChoice)
         {
            if (currentChoiceIndex < 0 || currentChoiceIndex >= pendingChoices.Count)
            {
               return; // nothing selected yet — wait
            }

            DialogueChoice choice = pendingChoices[currentChoiceIndex];
            currentChoiceIndex = -1;
            awaitingChoice = false;

            FireQuestAction(choice);

            // the choice's response lines finish the conversation
            pendingLines = choice.responseLines ?? new List<DialogueLine>();
            pendingChoices = new List<DialogueChoice>();
            lineIndex = 0;
         }

         // skip blank lines
         while (lineIndex < pendingLines.Count && IsLineBlank(pendingLines[lineIndex]))
         {
            lineIndex++;
         }

         bool hasLine = lineIndex < pendingLines.Count;
         bool isLastLine = lineIndex == pendingLines.Count - 1;
         bool offerChoices = pendingChoices.Count > 0 && (!hasLine || isLastLine);

         if (!hasLine && !offerChoices)
         {
            ExitDialogue();
            return;
         }

         string dialogueLine = hasLine ? FormatLine(pendingLines[lineIndex]) : "";
         if (hasLine) lineIndex++;

         List<string> choiceTexts = new List<string>();
         if (offerChoices)
         {
            awaitingChoice = true;
            foreach (DialogueChoice choice in pendingChoices)
            {
               choiceTexts.Add(choice.text);
            }
         }

         GameEventsManager.instance.dialogueEvents.DisplayDialogue(dialogueLine, choiceTexts);
      }

      private void FireQuestAction(DialogueChoice choice)
      {
         if (choice.action == DialogueQuestAction.None)
         {
            return;
         }

         QuestInfoSO quest = currentConversation != null ? currentConversation.quest : null;
         if (quest == null)
         {
            Debug.LogWarning($"Dialogue choice '{choice.text}' has a quest action but the conversation has no quest linked.");
            return;
         }

         QuestEvents questEvents = GameEventsManager.instance.questEvents;
         switch (choice.action)
         {
            case DialogueQuestAction.StartQuest:
               questEvents.StartQuest(quest.id);
               break;
            case DialogueQuestAction.AdvanceQuest:
               questEvents.AdvanceQuest(quest.id);
               break;
            case DialogueQuestAction.FinishQuest:
               questEvents.FinishQuest(quest.id);
               break;
         }
      }

      private void ExitDialogue()
      {
         dialoguePlaying = false;
         currentConversation = null;
         pendingLines = null;
         pendingChoices = null;
         awaitingChoice = false;
         currentChoiceIndex = -1;

         // inform other parts of my system that we've finished the dialogue
         GameEventsManager.instance.dialogueEvents.DialogueFinished();

         if (_playerMovement != null)
         {
            _playerMovement.enabled = true;
         }

         // restore gameplay input and camera control
         inputReader?.EnablePlayerMap();

         if (cameraManager != null)
         {
            cameraManager.SetLookInputEnabled(true);
            cameraManager.enabled = true;
         }

         GameEventsManager.instance.inputEvents.ChangeInputEventContext(InputEventContext.DEFAULT);
      }

      private string FormatLine(DialogueLine line)
      {
         return string.IsNullOrWhiteSpace(line.speaker) ? line.text : $"{line.speaker}: {line.text}";
      }

      private bool IsLineBlank(DialogueLine line)
      {
         return line == null || string.IsNullOrWhiteSpace(line.text);
      }
   }
}
