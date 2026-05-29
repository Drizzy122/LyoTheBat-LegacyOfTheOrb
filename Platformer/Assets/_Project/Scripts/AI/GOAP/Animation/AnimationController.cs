using ImprovedTimers;
using UnityEngine;

public abstract class AnimationController : MonoBehaviour {
    const float k_crossfadeDuration = 0.1f;
    
    Animator animator;
    CountdownTimer timer;
    
    readonly System.Collections.Generic.Dictionary<int, float> animationLengths = new();
    
    [HideInInspector] public int locomotionClip = Animator.StringToHash("Locomotion");
    [HideInInspector] public int speedHash = Animator.StringToHash("Speed");
    [HideInInspector] public int attackClip = Animator.StringToHash("Attack");
    [HideInInspector] public int knockBackClip = Animator.StringToHash("KnockBack");
    [HideInInspector] public int deathClip = Animator.StringToHash("Death");
    
    void Awake() {
        animator = GetComponentInChildren<Animator>();
        SetLocomotionClip();
        SetAttackClip();
        SetSpeedHash();
        SetKnockBackHash();
        SetDeathClip();
    }

    public void SetSpeed(float speed) => animator.SetFloat(speedHash, speed);
    public void Attack() => PlayAnimationUsingTimer(attackClip);
    public void KnockBack() => PlayAnimationUsingTimer(knockBackClip);
    public void Death() => animator.CrossFade(deathClip, k_crossfadeDuration);
    
    void Update() => timer?.Tick();

    void PlayAnimationUsingTimer(int clipHash) {
        timer = new CountdownTimer(GetAnimationLength(clipHash));
        timer.OnTimerStart += () => animator.CrossFade(clipHash, k_crossfadeDuration);
        timer.OnTimerStop += () => animator.CrossFade(locomotionClip, k_crossfadeDuration);
        timer.Start();
    }

    public float GetAnimationLength(int hash) {
        if (animationLengths.TryGetValue(hash, out float length))
            return length;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips) {
            if (Animator.StringToHash(clip.name) == hash) {
                animationLengths[hash] = clip.length;
                return clip.length;
            }
        }

        return -1f;
    }

    protected abstract void SetLocomotionClip();
    protected abstract void SetAttackClip();
    protected abstract void SetSpeedHash();
    protected abstract void SetKnockBackHash();
    protected abstract void SetDeathClip();
}