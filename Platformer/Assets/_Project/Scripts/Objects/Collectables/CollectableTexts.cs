using KBCore.Refs;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

namespace Platformer
{
    public class CollectableTexts : ValidatedMonoBehaviour, IDataPersistence
    {
        [field: Header("Settings")] 
        [field: SerializeField] CollectableType collectableType;
        [SerializeField] private int totalAmount = 0;
        
        private int currentCollectedCount = 0; 
        [field: SerializeField, Anywhere] TextMeshProUGUI textComponent;
        
        private void Awake()
        {
            textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            switch (collectableType)
            {
                case CollectableType.Ecliptium:
                    GameEventsManager.instance.miscEvents.onEcliptiumCollected += OnItemCollected;
                    break;
                case CollectableType.LuminOrb:
                    GameEventsManager.instance.miscEvents.onLuminCollected += OnItemCollected;
                    break;
                case CollectableType.Coin:
                    GameEventsManager.instance.miscEvents.onCoinCollected += OnItemCollected;
                    break;
            }
            RefreshUI();
        }

        private void OnDestroy()
        {
            switch (collectableType)
            {
                case CollectableType.Ecliptium:
                    GameEventsManager.instance.miscEvents.onEcliptiumCollected -= OnItemCollected;
                    break;
                case CollectableType.LuminOrb:
                    GameEventsManager.instance.miscEvents.onLuminCollected -= OnItemCollected;
                    break;
                case CollectableType.Coin:
                    GameEventsManager.instance.miscEvents.onCoinCollected -= OnItemCollected;
                    break;
            }
        }
        
        private void OnItemCollected()
        {
            currentCollectedCount++;
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (textComponent != null)
            {
                textComponent.text = currentCollectedCount + " / " + totalAmount;
            }
        }
        public void LoadData(GameData data)
        {
            switch (collectableType)
            {
               case CollectableType.Ecliptium:
                   foreach (KeyValuePair<string, bool> pair in data.ecliptiumCollected)
                   {
                       if (pair.Value) currentCollectedCount++;
                   }
                   break;
               case CollectableType.LuminOrb:
                   foreach (KeyValuePair<string, bool> pair in data.luminCollected)
                   {
                       if (pair.Value) currentCollectedCount++;
                   }
                   break;
               case CollectableType.Coin:
                   foreach (KeyValuePair<string, bool> pair in data.coinsCollected)
                   {
                       if (pair.Value) currentCollectedCount++;
                   }
                   break;
            }
            RefreshUI();
        }
        public void SaveData(GameData data)
        {
            // Noop
        }
    }
}