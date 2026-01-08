using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Platformer
{
    public class AudioManager : MonoBehaviour
    {
        [field: Header("Game Volume")] 
        [Range(0, 1)] public float masterVolume = 1;
        [Range(0, 1)] public float musicVolume = 1;
        [Range(0, 1)] public float ambienceVolume = 1;
        [Range(0, 1)] public float SFXVolume = 1;
        
        private Bus masterBus;
        private Bus musicBus;
        private Bus ambienceBus;
        private Bus sfxBus;
        
        private List<EventInstance> eventInstances;
        private List<StudioEventEmitter> eventEmitters;
        
        private EventInstance ambienceEventInstance;
        private EventInstance musicEventInstance;
        public static AudioManager instance { get; private set; }
        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Found more than one AudioManager in the scene");
            }
            instance = this;
            eventInstances = new List<EventInstance>();
            eventEmitters = new List<StudioEventEmitter>();
            
            masterBus = RuntimeManager.GetBus("bus:/");
            musicBus = RuntimeManager.GetBus("bus:/Music");
            ambienceBus = RuntimeManager.GetBus("bus:/Ambience");
            sfxBus = RuntimeManager.GetBus("bus:/SFX");
        }

        private void Start()
        {
            LoadVolumeSettings();
            ApplyVolumeSettings();
            ApplyButtonPressed();
            
            InitiializeAmbience(FMODEvents.instance.ambience);
            IntializeMusic(FMODEvents.instance.music);
        }
        
        private void Update()
        {
            masterBus.setVolume(masterVolume);
            musicBus.setVolume(musicVolume);
            ambienceBus.setVolume(ambienceVolume);
            sfxBus.setVolume(SFXVolume);
        }

        private void InitiializeAmbience(EventReference ambienceEventReference)
        {
            ambienceEventInstance = CreateEventInstance(ambienceEventReference);
            ambienceEventInstance.start();
        }
        
        public void SetAmbienceParameter(string parameterName, float parameterValue)
        {
            ambienceEventInstance.setParameterByName(parameterName, parameterValue);
        }
        
        private void IntializeMusic(EventReference musicEventReference)
        {
            musicEventInstance = CreateEventInstance(musicEventReference);
            musicEventInstance.start();
        }
     
        public void SetMusicArea(MusicArea area)
        {
            musicEventInstance.setParameterByName("Area", (float)area);
        }
        
        public void SetMusicParameter(string parameterName, float parameterValue)
        {
            musicEventInstance.setParameterByName(parameterName, parameterValue);
        }
        public void PlayOneShot(EventReference sound, Vector3 worldPos)
        {
            RuntimeManager.PlayOneShot(sound, worldPos);
        }

        public EventInstance CreateEventInstance(EventReference eventReference)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
            eventInstances.Add(eventInstance);
            return eventInstance;   
        }

        public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject)
        {
            StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
            emitter.EventReference = eventReference;
            eventEmitters.Add(emitter);
            return emitter;
        }

        private void CleanUp()
        {
            // stop and release any created instance
            foreach (EventInstance eventInstance in eventInstances)
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release();
            }
            // stop all of the event emitters, because they may hang around in other scenes
            foreach (StudioEventEmitter emitter in eventEmitters)
            {
                emitter.Stop();
            }
        }
        private void OnDestroy()
        {
            CleanUp();
        }
        
        // Method to save volume settings to PlayerPrefs
        public void SaveVolumeSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("AmbienceVolume", ambienceVolume);
            PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
        
            PlayerPrefs.Save(); // Ensures data is written to disk
        }

        // Method to load volume settings from PlayerPrefs
        public void LoadVolumeSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1); // Default to 1
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1);
            ambienceVolume = PlayerPrefs.GetFloat("AmbienceVolume", 1);
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1);
            ApplyVolumeSettings(); // Apply the settings to your audio system
        }

        // Apply loaded or updated volume settings (implementation depends on your setup)
        private void ApplyVolumeSettings()
        {
            //Debug.Log("Applying volume settings...");
            // Example: Call relevant methods to update the audio system with new values
            // Adjust the master, music, ambience, SFX, and UI volumes in your system here
        }
        
        public void UpdateVolumeLevels(float newMasterVolume, float newMusicVolume)
        {
            masterVolume = newMasterVolume;
            musicVolume = newMusicVolume;
            ambienceVolume = newMusicVolume;
            SFXVolume = newMusicVolume;
    
            // Save and apply updated values
            SaveVolumeSettings();
        }
        
        public void ApplyButtonPressed()
        {
           // Debug.Log("Apply button pressed, saving and applying settings...");
            SaveVolumeSettings();
            ApplyVolumeSettings();
        }
        private void OnApplicationQuit()
        {
           // Debug.Log("Application is quitting, saving volume settings...");
            SaveVolumeSettings();
        }
        
        public void StopMusic()
        {
            if (musicEventInstance.isValid())
            {
                musicEventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }
        }
    }
}