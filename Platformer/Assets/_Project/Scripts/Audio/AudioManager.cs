using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.UIElements;

namespace Platformer
{
    public class AudioManager : MonoBehaviour
    {
        [field: Header("Game Volume")] 
        [SerializeField] UIDocument settingsUIDocument;
        [field: Range(0, 1)] public float masterVolume = 1;
        [field: Range(0, 1)] public float musicVolume = 1;
        [field: Range(0, 1)] public float ambienceVolume = 1;
        [field: Range(0, 1)] public float SFXVolume = 1;
        
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
            InitiializeAmbience(FMODEvents.instance.ambience);
            IntializeMusic(FMODEvents.instance.music);
            SetupUIToolkit();
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
        
        private void SetupUIToolkit()
        {
            // Make sure we actually assigned the UI Document in the inspector
            if (settingsUIDocument == null) return;

            var root = settingsUIDocument.rootVisualElement;

            // 1. Find the sliders using the exact IDs from your UI Builder screenshot
            Slider masterSlider = root.Q<Slider>("MasterVolumeSlider");
            Slider musicSlider = root.Q<Slider>("MusicVolumeSlider");
            Slider sfxSlider = root.Q<Slider>("SFXVolumeSlider");
            Slider ambienceSlider = root.Q<Slider>("AmbienceVolumeSlider");

            // 2. Set the sliders to match your saved volumes on startup
            if (masterSlider != null) masterSlider.value = masterVolume;
            if (musicSlider != null) musicSlider.value = musicVolume;
            if (sfxSlider != null) sfxSlider.value = SFXVolume;
            if (ambienceSlider != null) ambienceSlider.value = ambienceVolume;

            // 3. Register Callbacks (This fires ONLY when the player moves the slider)
            masterSlider?.RegisterValueChangedCallback(evt => {
                masterVolume = evt.newValue;
                masterBus.setVolume(masterVolume); // Update FMOD immediately
            });

            musicSlider?.RegisterValueChangedCallback(evt => {
                musicVolume = evt.newValue;
                musicBus.setVolume(musicVolume);
            });

            sfxSlider?.RegisterValueChangedCallback(evt => {
                SFXVolume = evt.newValue;
                sfxBus.setVolume(SFXVolume);
            });

            ambienceSlider?.RegisterValueChangedCallback(evt => {
                ambienceVolume = evt.newValue;
                ambienceBus.setVolume(ambienceVolume);
            });
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