using UnityEngine;

public class AnimalAnimationController : AnimationController {
    protected override void SetLocomotionClip() {
        locomotionClip = Animator.StringToHash("Locomotion");
    }

    protected override void SetAttackClip() {
        attackClip = Animator.StringToHash("ChomperAttack");
    }

    protected override void SetSpeedHash() {
        speedHash = Animator.StringToHash("Speed");
    }

    protected override void SetKnockBackHash()
    {
        knockBackClip = Animator.StringToHash("ChomperHit4");
    }
    
    protected override void SetDeathClip() {
        deathClip = Animator.StringToHash("ChomperHit3");
    }
}