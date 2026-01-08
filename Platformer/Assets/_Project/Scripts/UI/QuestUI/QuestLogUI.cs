using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace Platformer
{
    public class QuestLogUI : MonoBehaviour
    {
        [Header("Components")] [SerializeField]
        private GameObject contentParent;
        
        private PlayerController playerController;
        private CameraManager cameraManager;
        [SerializeField] private QuestLogScrollingList scrollingList;
        [SerializeField] private TextMeshProUGUI questDisplayNameText;
        [SerializeField] private TextMeshProUGUI questStatusText;
        [SerializeField] private TextMeshProUGUI goldRewardsText;
        [SerializeField] private TextMeshProUGUI experienceRewardsText;
        [SerializeField] private TextMeshProUGUI levelRequirementsText;
        [SerializeField] private TextMeshProUGUI questRequirementsText;

        [Obsolete("Obsolete")]
        private void Awake()
        {
            playerController = FindObjectOfType<PlayerController>();
            cameraManager = FindObjectOfType<CameraManager>();
        }

        private Button firstSelectedButton;
        private void OnEnable()
        {
            GameEventsManager.instance.questEvents.onQuestStateChange += QuestStateChange;
            GameEventsManager.instance.inputEvents.onQuestLogTogglePressed += QuestLogTogglePressed;
            
        }
        
        private void OnDisable()
        {
            GameEventsManager.instance.questEvents.onQuestStateChange -= QuestStateChange;
            GameEventsManager.instance.inputEvents.onQuestLogTogglePressed -= QuestLogTogglePressed;
        }

        private void QuestLogTogglePressed()
        {
            if (contentParent.activeInHierarchy)
            {
                HideUI();
            }
            else
            {
                ShowUI();
            }
        }
        
        private void ShowUI()
        {
            contentParent.SetActive(true);
            //GameEventsManager.instance.playerEvents.DisablePlayerMovement();
            // disable the playerController
            if (playerController != null)
            {
                playerController.enabled = false;
            }

            if (cameraManager != null)
            {
                cameraManager.enabled = false;
            }
            
            // this needs to happen after the content parent is set active,
            // or else the onSelectAction wont work as expected
            if (firstSelectedButton != null)
            {
                firstSelectedButton.Select();
            }
            else
            {
                Debug.LogWarning("No first selected button was set.");
            }
            Time.timeScale = 0;
        }
        private void HideUI()
        {
            contentParent.SetActive(false);
            //GameEventsManager.instance.playerEvents.EnablePlayerMovement();
            if (playerController != null)
            {
                playerController.enabled = true;
            }

            if (cameraManager != null)
            {
                cameraManager.enabled = true;
            }

            EventSystem.current.SetSelectedGameObject(null);
            Time.timeScale = 1;
        }
        
        private void QuestStateChange(Quest quest)
        {
            // add the button to the scrolling list if not already added
            QuestLogButton questLogButton = scrollingList.CreateButtonIfNotExists(quest, () =>
            {
                SetQuestLogInfo(quest);
            });

            // initialize the first selected button if not already so that it's always the top button
            if (firstSelectedButton == null)
            {
                firstSelectedButton = questLogButton.button;
                
            }
            
            questLogButton.SetState(quest.state);
            
        }
        private void SetQuestLogInfo(Quest quest)
        {
            // quest name
            questDisplayNameText.text = quest.info.displayName;
            
            // todo
            questStatusText.text = quest.GetFullStatusText();
            // requirements
            levelRequirementsText.text = "Level " + quest.info.levelRequirement;
            questRequirementsText.text = "";
            foreach (QuestInfoSO prerequiQuestInfoSo in quest.info.questPrerequisites)
            {
                questRequirementsText.text += prerequiQuestInfoSo.displayName + "\n";
            }
            
            // rewards
            goldRewardsText.text = "Gold: " + quest.info.goldReward;
            experienceRewardsText.text = "Experience: " + quest.info.experienceReward;
        }
    }
}