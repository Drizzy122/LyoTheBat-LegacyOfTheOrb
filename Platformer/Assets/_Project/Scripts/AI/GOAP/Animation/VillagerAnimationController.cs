using UnityEngine;

public class VillagerAnimationController : AnimationController {
    protected override void SetLocomotionClip() {
        locomotionClip = Animator.StringToHash("Locomotion");
    }

    protected override void SetAttackClip() {
        attackClip = locomotionClip; // Villager has no attack
    }

    protected override void SetSpeedHash() {
        speedHash = Animator.StringToHash("Speed");
    }

    protected override void SetKnockBackHash() {
        knockBackClip = locomotionClip; // Villager has no knockback
    }

    protected override void SetDeathClip() {
        deathClip = locomotionClip; // Villager has no death
    }
}