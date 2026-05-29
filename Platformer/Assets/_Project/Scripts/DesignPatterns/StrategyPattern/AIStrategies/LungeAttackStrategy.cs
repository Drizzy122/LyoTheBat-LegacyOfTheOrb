using System;
using DG.Tweening;
using Platformer;
using UnityEngine;
using UnityEngine.AI;

public class LungeAttackStrategy : IActionStrategy {
    public bool CanPerform => !Complete;
    public bool Complete { get; private set; }

    readonly NavMeshAgent agent;
    readonly PlayerDetector playerDetector;
    readonly ParticleSystem counterWarning;
    readonly AnimationController animations;
    readonly Transform transform;
    readonly float damage;
    readonly Action onComplete; // optional callback fired when the attack finishes

    Sequence sequence;

    const float warningDuration = 0.4f;
    const float lungeDuration   = 0.3f;
    public LungeAttackStrategy(NavMeshAgent agent, PlayerDetector playerDetector,
                               ParticleSystem counterWarning, AnimationController animations,
                               Transform transform, float damage = 10f, Action onComplete = null) {
        this.agent          = agent;
        this.playerDetector = playerDetector;
        this.counterWarning = counterWarning;
        this.animations     = animations;
        this.transform      = transform;
        this.damage         = damage;
        this.onComplete     = onComplete;
    }

    public void Start() {
        Complete = false;

        if (playerDetector.Player == null) {
            Finish();
            return;
        }

        Vector3 direction = (playerDetector.Player.position - transform.position).normalized;
        direction.y = 0f;
        if (direction != Vector3.zero) transform.rotation = Quaternion.LookRotation(direction);

        ShowCounterWarning(true);
        agent.enabled = false;

        Vector3 stopPosition = playerDetector.Player.position - direction * 1.5f;

        sequence = DOTween.Sequence();
        sequence
            .AppendInterval(warningDuration)
            .AppendCallback(() => {
                ShowCounterWarning(false);
                animations.Attack();
            })
            .Append(transform.DOMove(stopPosition, lungeDuration).SetEase(Ease.OutQuad))
            .OnComplete(() => {
                if (playerDetector.PlayerHealth != null && !playerDetector.PlayerHealth.IsInvulnerable)
                    playerDetector.PlayerHealth.TakeDamage(damage);
                Finish();
            });
    }

    public void Stop() {
        sequence?.Kill();
        ShowCounterWarning(false);
        Finish();
    }

    void Finish() {
        agent.enabled = true;
        onComplete?.Invoke();
        Complete = true;
    }

    void ShowCounterWarning(bool active) {
        if (counterWarning == null) return;
        if (active) counterWarning.Play();
        else { counterWarning.Clear(); counterWarning.Stop(); }
    }
}
