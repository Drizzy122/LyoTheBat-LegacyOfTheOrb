using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace Platformer
{
    public class SaveSlot
    {
        private string profileId;
        private VisualElement rootSlotElement;
        
        private VisualElement noDataContent;
        private VisualElement hasDataContent;
        private Label percentageCompleteText;
        private Button clearButton;
        private Label saveDateText;
        private VisualElement thumbnailImage;
        
        public Button saveSlotButton { get; private set; }
        public Button getClearButton { get { return clearButton; } } // Expose for SaveSlotsMenu to bind to
        
        public bool hasData { get; private set; } = false;

        public SaveSlot(VisualElement slotElement, string profileId) 
        {
            this.rootSlotElement = slotElement;
            this.profileId = profileId;

            // Query the specific parts inside this slot
            saveSlotButton = rootSlotElement.Q<Button>("SaveSlotButton");
            noDataContent = rootSlotElement.Q<VisualElement>("NoDataContent");
            hasDataContent = rootSlotElement.Q<VisualElement>("HasDataContent");
            percentageCompleteText = rootSlotElement.Q<Label>("PercentageCompleteText");
            saveDateText = rootSlotElement.Q<Label>("SaveDateText");
            clearButton = rootSlotElement.Q<Button>("ClearButton");
            thumbnailImage = hasDataContent.Q<VisualElement>("ThumbnailImage");
            
        }

        public void SetData(GameData data) 
        {
            if (data == null) 
            {
                hasData = false;
                noDataContent.style.display = DisplayStyle.Flex;
                hasDataContent.style.display = DisplayStyle.None;
                clearButton.style.display = DisplayStyle.None;
                
                
            }
            else 
            {
                hasData = true;
                noDataContent.style.display = DisplayStyle.None;
                hasDataContent.style.display = DisplayStyle.Flex;
                clearButton.style.display = DisplayStyle.Flex;

                percentageCompleteText.text = data.GetPercentageComplete() + "% COMPLETE";
                
                System.DateTime saveTime = System.DateTime.FromBinary(data.lastUpdated);
                saveDateText.text = saveTime.ToString("MM/dd/yyyy HH:mm");
                LoadDynamicThumbnail();
            }
        }
        
        private void LoadDynamicThumbnail()
        {
            
            string thumbnailPath = $"Thumbnails/{profileId}";
            Texture2D loadedTexture = Resources.Load<Texture2D>(thumbnailPath);

            if (loadedTexture != null)
            {
                // Apply the texture to the element.
                thumbnailImage.style.backgroundImage = new StyleBackground(loadedTexture); // Use Texture2D

                // Ensure the background scales correctly within the element
                thumbnailImage.style.backgroundSize = new BackgroundSize(BackgroundSizeType.Contain);
            }
            else
            {
                // Safety: If the texture couldn't be loaded, reset the background.
                Debug.LogWarning($"Thumbnail image not found at path: {thumbnailPath}");
                // thumbnailImage.style.backgroundImage = null;
            }
        }

        public string GetProfileId() 
        {
            return this.profileId;
        }

        public void SetInteractable(bool interactable)
        {
            saveSlotButton.SetEnabled(interactable);
            clearButton.SetEnabled(interactable);
        }
    }
}