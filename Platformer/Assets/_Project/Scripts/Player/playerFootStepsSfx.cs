using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Platformer
{
    public class playerFootStepsSfx : MonoBehaviour
    {
        private int materialValue;
        private RaycastHit rh;
        private float distance = 1.5f;

        private LayerMask lm;
        private Rigidbody rb;

        private void Start()
        {
            lm = LayerMask.GetMask("Ground");
            rb = GetComponent<Rigidbody>();
        }

        // Called by animation event on run frames
        void PlayRunEvent()
        {
            PlayFootstep(1);
        }

        // Called by animation event on walk frames
        void PlayWalkEvent()
        {
            PlayFootstep(0);
        }

        private void PlayFootstep(int walkRunValue)
        {
            MaterialCheck();
            AudioManager.instance.PlayFootstep(
                FMODEvents.instance.playerFootsteps,
                transform.position,
                materialValue,
                walkRunValue
            );
        }

        void MaterialCheck()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out rh, distance, lm))
            {
                switch (rh.collider.tag)
                {
                    case "Stone":  materialValue = 0; break;
                    case "Metal":  materialValue = 1; break;
                    case "Grass":  materialValue = 2; break;
                    case "Gravel": materialValue = 3; break;
                    case "Wood":   materialValue = 4; break;
                    case "Water":  materialValue = 5; break;
                    default:       materialValue = 0; break;
                }
            }
        }
    }
}