using UnityEngine;

namespace Platformer
{
    public class FootstepParticleController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _leftFootParticleSystem;
        [SerializeField] private ParticleSystem _rightFootParticleSystem;

        [SerializeField] private FootstepController footstepController;

        private void Awake()
        {
            footstepController = GetComponent<FootstepController>();
        }

        private void Start()
        {
            footstepController.OnLeftFootDown += PlayLeftFootParticleSystem;
            footstepController.OnRightFootDown += PlayRightFootParticleSystem;
        }

        private void PlayLeftFootParticleSystem()
        {
            if (enabled)
            {
                _leftFootParticleSystem.Play();
            }
        }

        private void PlayRightFootParticleSystem()
        {
            if (enabled)
            {
                _rightFootParticleSystem.Play();
            }
        }

        private void OnDisable()
        {
            _leftFootParticleSystem.Stop();
            _rightFootParticleSystem.Stop();
        }
    }
}