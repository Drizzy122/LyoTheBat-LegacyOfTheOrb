using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

namespace Platformer
{
    public class SettingsManager : Menu
    {
        public static SettingsManager Instance { get; private set; }


        [Header("VSync")] [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private TMP_Text vsyncText;

        [Header("Window Mode")] [SerializeField]
        private TMP_Text windowModeText;

        private string[] windowModes = { "Fullscreen", "Borderless", "Maximized", "Windowed" };
        private int currentWindowModeIndex;

        [Header("Resolution")] [SerializeField]
        private TMP_Text resolutionText;

        private Resolution[] resolutions;
        private int currentResolutionIndex;

        [Header("Quality")] [SerializeField] private TMP_Text qualityText;
        private string[] qualityLevels = { "PC", "Mobile" }; // force both to appear
        private int currentQualityIndex = 0;

        [Header("Motion Blur")] [SerializeField]
        private Toggle motionBlurToggle;

        [SerializeField] private TMP_Text motionBlurText;
        private MotionBlur motionBlur;

        [Header("Gamma")] [SerializeField] private Slider gammaSlider;
        [SerializeField] private TMP_Text gammaText;
        private ColorAdjustments colorAdjustments;



        [Header("Post Processing Volume")] [SerializeField]
        private Volume postProcessingVolume;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null)
            {
                Debug.LogError("Found more than one SettingsManager in the scene.");
            }

            Instance = this;


        }


        private void Start()
        {
            // --- VSync setup ---
            if (vsyncToggle != null)
            {
                vsyncToggle.isOn = QualitySettings.vSyncCount > 0;
                vsyncToggle.onValueChanged.AddListener(SetVSync);
            }

            UpdateVSyncUI();

            // --- Window Mode setup ---
            currentWindowModeIndex = GetIndexFromMode(Screen.fullScreenMode);
            UpdateWindowModeUI();

            // --- Resolution setup ---
            resolutions = Screen.resolutions;
            currentResolutionIndex = GetClosestResolutionIndex(Screen.currentResolution);
            UpdateResolutionUI();

            // --- Quality setup ---
            currentQualityIndex = QualitySettings.GetQualityLevel();
            UpdateQualityUI();

            // --- Motion Blur setup ---
            if (postProcessingVolume != null && postProcessingVolume.profile.TryGet(out motionBlur))
            {
                motionBlurToggle.isOn = motionBlur.active;
                motionBlurToggle.onValueChanged.AddListener(SetMotionBlur);
            }

            UpdateMotionBlurUI();

            // --- Gamma setup
            if (postProcessingVolume != null && postProcessingVolume.profile.TryGet(out colorAdjustments))
            {
                colorAdjustments.postExposure.overrideState = true;
                gammaSlider.minValue = 0f; // darker
                gammaSlider.maxValue = 100f; // brighter
                gammaSlider.wholeNumbers = true;
                gammaSlider.value = 50f; // neutral
                gammaSlider.onValueChanged.AddListener(SetGamma);
            }

            UpdateGammaUI();

        }





        // --- VSync ---
        public void SetVSync(bool enabled)
        {
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            UpdateVSyncUI();
            Debug.Log("VSync " + (enabled ? "Enabled" : "Disabled"));
        }

        private void UpdateVSyncUI()
        {
            if (vsyncText != null)
                vsyncText.text = QualitySettings.vSyncCount > 0 ? "Enabled" : "Disabled";
        }

        // --- Window Mode ---
        public void NextWindowMode()
        {
            currentWindowModeIndex = (currentWindowModeIndex + 1) % windowModes.Length;
            ApplyWindowMode();
        }

        public void PreviousWindowMode()
        {
            currentWindowModeIndex--;
            if (currentWindowModeIndex < 0)
                currentWindowModeIndex = windowModes.Length - 1;

            ApplyWindowMode();
        }

        private void ApplyWindowMode()
        {
            switch (currentWindowModeIndex)
            {
                case 0: Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen; break;
                case 1: Screen.fullScreenMode = FullScreenMode.FullScreenWindow; break;
                case 2: Screen.fullScreenMode = FullScreenMode.MaximizedWindow; break;
                case 3: Screen.fullScreenMode = FullScreenMode.Windowed; break;
            }

            UpdateWindowModeUI();
            Debug.Log("Window Mode set to: " + windowModes[currentWindowModeIndex]);
        }

        private void UpdateWindowModeUI()
        {
            if (windowModeText != null)
                windowModeText.text = windowModes[currentWindowModeIndex];
        }

        private int GetIndexFromMode(FullScreenMode mode)
        {
            switch (mode)
            {
                case FullScreenMode.ExclusiveFullScreen: return 0;
                case FullScreenMode.FullScreenWindow: return 1;
                case FullScreenMode.MaximizedWindow: return 2;
                case FullScreenMode.Windowed: return 3;
                default: return 3; // fallback to windowed
            }
        }

        // --- Resolution ---
        public void NextResolution()
        {
            currentResolutionIndex = (currentResolutionIndex + 1) % resolutions.Length;
            ApplyResolution();
        }

        public void PreviousResolution()
        {
            currentResolutionIndex--;
            if (currentResolutionIndex < 0)
                currentResolutionIndex = resolutions.Length - 1;

            ApplyResolution();
        }

        private void ApplyResolution()
        {
            Resolution res = resolutions[currentResolutionIndex];
            Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
            UpdateResolutionUI();
            Debug.Log("Resolution set to: " + res.width + "x" + res.height);
        }

        private void UpdateResolutionUI()
        {
            if (resolutionText != null)
            {
                Resolution res = resolutions[currentResolutionIndex];
                resolutionText.text = res.width + " x " + res.height;
            }
        }

        private int GetClosestResolutionIndex(Resolution current)
        {
            for (int i = 0; i < resolutions.Length; i++)
            {
                if (resolutions[i].width == current.width &&
                    resolutions[i].height == current.height)
                {
                    return i;
                }
            }

            return resolutions.Length - 1; // fallback to last
        }

        // --- Quality ---
        public void NextQuality()
        {
            currentQualityIndex = (currentQualityIndex + 1) % qualityLevels.Length;
            ApplyQuality();
        }

        public void PreviousQuality()
        {
            currentQualityIndex--;
            if (currentQualityIndex < 0)
                currentQualityIndex = qualityLevels.Length - 1;

            ApplyQuality();
        }

        private void ApplyQuality()
        {
            string selectedQuality = qualityLevels[currentQualityIndex];

            // Find the index in QualitySettings.names
            int index = System.Array.IndexOf(QualitySettings.names, selectedQuality);
            if (index >= 0)
                QualitySettings.SetQualityLevel(index);

            UpdateQualityUI();
            Debug.Log("Graphics Quality set to: " + selectedQuality);
        }

        private void UpdateQualityUI()
        {
            if (qualityText != null)
                qualityText.text = qualityLevels[currentQualityIndex];
        }

        // --- MotionBlur  ---
        public void SetMotionBlur(bool enabled)
        {
            if (motionBlur != null)
            {
                motionBlur.active = enabled;
                UpdateMotionBlurUI();
                Debug.Log("Motion Blur " + (enabled ? "Enabled" : "Disabled"));
            }
        }

        private void UpdateMotionBlurUI()
        {
            if (motionBlurText != null)
                motionBlurText.text = motionBlur != null && motionBlur.active ? "Enabled" : "Disabled";
        }

        // --- Gamma ---

        public void SetGamma(float sliderValue)
        {
            if (colorAdjustments != null)
            {
                // Map 0 → 100 slider to -2 → 2 exposure
                float exposureValue = Mathf.Lerp(-2f, 2f, sliderValue / 100f);

                colorAdjustments.postExposure.value = exposureValue;

                UpdateGammaUI(); // updates UI text
                //Debug.Log($"Slider: {sliderValue}, Exposure applied: {exposureValue}");
            }
        }

        private void UpdateGammaUI()
        {
            if (gammaText != null && gammaSlider != null)
                gammaText.text = Mathf.RoundToInt(gammaSlider.value).ToString(); // shows 0 → 100
        }
    }
}