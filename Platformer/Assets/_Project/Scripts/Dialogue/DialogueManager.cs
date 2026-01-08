using System;
using UnityEngine;
using Ink.Runtime;

namespace Platformer
{
   public class DialogueManager : MonoBehaviour
   {
      [Header("Ink Story")] 
      [SerializeField] private TextAsset inkjson;

      private Story story;

      private int currentChoiceIndex = -1;
      private bool dialoguePlaying = false;

      private PlayerController playerController;
      private CameraManager cameraManager;
      
      private InkExternalFunctions inkExternalFunctions;
      private InkDialogueVariables inkDialogueVariables;
      [Obsolete("Obsolete")]
      private void Awake()
      {
         story = new Story(inkjson.text);
         inkExternalFunctions = new InkExternalFunctions();
         inkDialogueVariables = new InkDialogueVariables(story);
         inkExternalFunctions.Bind(story);
         playerController = FindObjectOfType<PlayerController>();
         cameraManager = FindObjectOfType<CameraManager>();
      }

      private void OnDestroy()
      {
         inkExternalFunctions.Unbind(story);
      }

      private void OnEnable()
      {
         GameEventsManager.instance.dialogueEvents.onEnterDialogue += EnterDialogue;
         GameEventsManager.instance.inputEvents.onSubmitPressed += SubmitPressed;
         GameEventsManager.instance.dialogueEvents.onUpdateChoiceIndex += UpdateChoiceIndex;
         GameEventsManager.instance.dialogueEvents.onUpdateInkDialogueVariable += UpdateInkDialogueVariable;
         GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
      }

      private void OnDisable()
      {
         GameEventsManager.instance.dialogueEvents.onEnterDialogue -= EnterDialogue;
         GameEventsManager.instance.inputEvents.onSubmitPressed -= SubmitPressed;
         GameEventsManager.instance.dialogueEvents.onUpdateChoiceIndex -= UpdateChoiceIndex;
         GameEventsManager.instance.dialogueEvents.onUpdateInkDialogueVariable -= UpdateInkDialogueVariable;
         GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;

      }

      private void QuestStateChange(Quest quest)
      {
         GameEventsManager.instance.dialogueEvents.UpdateInkDialogueVariable(
            quest.info.id + "State",
             new StringValue(quest.state.ToString())
            );
      }
      private void UpdateInkDialogueVariable(string name, Ink.Runtime.Object value)
      {
         inkDialogueVariables.UpdateVariableState(name, value);
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
         ContinueOrExitStory();
      }

      private void EnterDialogue(string knotName)
      {
         if (dialoguePlaying)
         {
            return;
         }
         
         dialoguePlaying = true;

         // inform other parts of our system that we've started dialogue
         GameEventsManager.instance.dialogueEvents.DialogueStarted();

         // input event context
         GameEventsManager.instance.inputEvents.ChangeInputEventContext(InputEventContext.DIALOGUE);

         // freeze player movement
         if (playerController != null)
         {
            playerController.enabled = false;
         }

         // disable camera
         if (cameraManager != null)
         {
            cameraManager.enabled = false;
         }


         // jump to the knot
         if (!knotName.Equals(""))
         {
            story.ChoosePathString(knotName);
         }
         else
         {
            Debug.LogWarning("Knot name was the empty string when entering dialogue.");
         }
         
         inkDialogueVariables.SyncVariablesAndStartListening(story);
         // kick off the story
         ContinueOrExitStory();
      }

      private void ContinueOrExitStory()
      {
         if (story.currentChoices.Count > 0 && currentChoiceIndex != -1)
         {
            story.ChooseChoiceIndex(currentChoiceIndex);
            currentChoiceIndex = -1;
         }

         if (story.canContinue)
         {
            string dialogueLine = story.Continue();
            
            // handle the case where there's an empty line of dialogue
            // by continuing untill we get a line with content
            while (IsLineBlank(dialogueLine) && story.canContinue)
            {
               dialogueLine = story.Continue();
            }
            // handle the case where the last line of dialogue is blank
            if (IsLineBlank(dialogueLine) && !story.canContinue)
            {
               ExitDialogue();
            }
            else
            {
               GameEventsManager.instance.dialogueEvents.DisplayDialogue(dialogueLine, story.currentChoices);

            }
         }
         else if (story.currentChoices.Count == 0)
         {
            ExitDialogue();
         }
      }

      private void ExitDialogue()
      {

         dialoguePlaying = false;

         // inform other parts of my system that we've finished the dialogue
         GameEventsManager.instance.dialogueEvents.DialogueFinished();

         if (playerController != null)
         {
            playerController.enabled = true;
         }

         if (cameraManager != null)
         {
            cameraManager.enabled = true;
         }

         GameEventsManager.instance.inputEvents.ChangeInputEventContext(InputEventContext.DEFAULT);
         
         inkDialogueVariables.StopListening(story);
         // reset story state
         story.ResetState();
      }

      private bool IsLineBlank(string dialogueLine)
      {
         return dialogueLine.Trim().Equals("") || dialogueLine.Trim().Equals("\n");
      }
   }
}