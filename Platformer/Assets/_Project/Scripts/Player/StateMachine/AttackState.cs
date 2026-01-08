using UnityEngine;

namespace Platformer
{
    public class AttackState : BaseState
    {
        private int comboIndex = 0; // Tracks the current attack animation
        private float lastAttackTime; // Tracks when the last attack happened
        private readonly float comboResetTime = 0.5f; // Time allowed between attacks to maintain the combo

        public AttackState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter()
        {
            // Play the correct combo animation based on the comboIndex
            PlayComboAnimation();

            // Register the time of this attack to manage combo resets
            lastAttackTime = Time.time;

            // Execute the player's attack logic (damage/hitbox/etc.)
            player.Attack();
            
        }

        public override void Update()
        {
            // During the attack, you can add logic if movement or combo reset logic is needed.
            if (Time.time - lastAttackTime > comboResetTime)
            {
                ResetCombo();
            }
        }

        private void PlayComboAnimation()
        {
            // Select animation to play based on the combo index
            switch (comboIndex)
            {
                case 0:
                    animator.CrossFade(BaseState.AttackHash, crossFadeDuration); // Attack1
                    break;
                case 1:
                    animator.CrossFade(Animator.StringToHash("Attack2"), crossFadeDuration); // Attack2
                    break;
                case 2:
                    animator.CrossFade(Animator.StringToHash("Attack3"), crossFadeDuration); // Attack3
                    break;
                default:
                    animator.CrossFade(BaseState.AttackHash, crossFadeDuration); // Reset to Attack1
                    break;
            }

            // Increment the combo index, and loop back to the first attack if maximum is reached
            comboIndex = (comboIndex + 1) % 3; // Assuming you have 3 combo animations
        }

        private void ResetCombo()
        {
            comboIndex = 0; // Reset combo index if time runs out
        }
    }
}