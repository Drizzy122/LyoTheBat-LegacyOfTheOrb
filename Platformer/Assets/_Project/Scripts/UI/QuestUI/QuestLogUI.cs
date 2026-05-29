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

        private PlayerMovement _playerMovement;
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
            _playerMovement = FindObjectOfType<PlayerMovement>();
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
            if (_playerMovement != null)
            {
                _playerMovement.enabled = false;
            }
            if (cameraManager != null)
            {
                cameraManager.enabled = false;
            }
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
            if (_playerMovement != null)
            {
                _playerMovement.enabled = true;
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
            QuestLogButton questLogButton = scrollingList.CreateButtonIfNotExists(quest, () =>
            {
                SetQuestLogInfo(quest);
            });

            if (firstSelectedButton == null)
            {
                firstSelectedButton = questLogButton.button;
            }

            questLogButton.SetState(quest.state);
        }

        private void SetQuestLogInfo(Quest quest)
        {
            questDisplayNameText.text = quest.info.displayName;
            questStatusText.text = quest.GetFullStatusText();
            levelRequirementsText.text = "Level " + quest.info.levelRequirement;
            questRequirementsText.text = "";
            foreach (QuestInfoSO prerequiQuestInfoSo in quest.info.questPrerequisites)
            {
                questRequirementsText.text += prerequiQuestInfoSo.displayName + "\n";
            }
            goldRewardsText.text = "Gold: " + quest.info.goldReward;
            experienceRewardsText.text = "Experience: " + quest.info.experienceReward;
        }
    }
}
