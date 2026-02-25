using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Platformer
{
    public class playerFootStepsSfx : MonoBehaviour
    {
        private int MaterialValue;
        private RaycastHit rh;
        private float distance = 0.3f;

        // REMOVED: hardcoded string EventPath
        // REMOVED: ParamID and ParamID2 initialization via raw numbers (optional but cleaner)

        private LayerMask lm;
        private Rigidbody rb;

        private void Start()
        {
            lm = LayerMask.GetMask("Ground");
            rb = GetComponent<Rigidbody>();
        }

        void PlayRunEvent()
        {
            // Pass 1 for the 'WalkRun' parameter
            PlayFootstep(1);
        }

        void PlayWalkEvent()
        {
            // Pass 0 for the 'WalkRun' parameter (assuming 0 is walk, 1 is run)
            PlayFootstep(0);
        }

        private void PlayFootstep(int walkRunValue)
        {
            MaterialCheck();

            // Use your AudioManager to create the instance using the EventReference
            // This pulls from: FMODEvents.instance.playerFootsteps
            EventInstance footstep = AudioManager.instance.CreateEventInstance(FMODEvents.instance.playerFootsteps);

            RuntimeManager.AttachInstanceToGameObject(footstep, transform, rb);

            // Setting parameters by Name is much cleaner than using those long ID numbers
            footstep.setParameterByName("Terrain", MaterialValue);
            footstep.setParameterByName("WalkRun", walkRunValue);

            footstep.start();
            footstep.release();
        }

        void MaterialCheck()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out rh, distance, lm))
            {
                switch (rh.collider.tag)
                {
                    case "Stone": MaterialValue = 0; break;
                    case "Metal": MaterialValue = 1; break;
                    case "Grass": MaterialValue = 2; break;
                    case "Gravel": MaterialValue = 3; break;
                    case "Wood": MaterialValue = 4; break;
                    case "Water": MaterialValue = 5; break;
                    default: MaterialValue = 0; break;
                }
            }
        }
    }
}