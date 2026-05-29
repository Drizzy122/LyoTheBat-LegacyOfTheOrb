using System;
using KBCore.Refs;
using UnityEngine;

namespace Platformer
{
    public class FootstepController : ValidatedMonoBehaviour
    {
        [SerializeField, Self] Animator animator;
        [SerializeField] GroundChecker groundChecker;

        [Header("Trigger Settings")]
        [SerializeField] string locomotionStateName = "Locomotion";
        [SerializeField] float speedThreshold = 0.1f;

        static readonly int SpeedHash = Animator.StringToHash("Speed");

        public event Action OnLeftFootDown;
        public event Action OnRightFootDown;

        void Update()
        {
            if (animator == null || groundChecker == null) return;
            if (!groundChecker.IsGrounded) return;
            if (animator.GetFloat(SpeedHash) <= speedThreshold) return;

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName(locomotionStateName)) return;

            if (stateInfo.normalizedTime % 1f < 0.5f)
                LeftFootDown();
            else
                RightFootDown();
        }

        public void LeftFootDown()
        {
            if (enabled) OnLeftFootDown?.Invoke();
        }

        public void RightFootDown()
        {
            if (enabled) OnRightFootDown?.Invoke();
        }

        public void BothFeetDown()
        {
            LeftFootDown();
            RightFootDown();
        }
    }
}
