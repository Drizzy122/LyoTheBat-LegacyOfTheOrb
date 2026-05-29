using System;
using ImprovedTimers;
using UnityEngine;
using UnityEngine.AI;
using UnityUtils;

// TODO Migrate Strategies, Beliefs, Actions and Goals to Scriptable Objects and create Node Editor for them
public interface IActionStrategy {
    bool CanPerform { get; }
    bool Complete { get; }
    
    void Start() {
        // noop
    }
    
    void Update(float deltaTime) {
        // noop
    }
    
    void Stop() {
        // noop
    }
}

public class AttackStrategy : IActionStrategy {
    public bool CanPerform => true; // Agent can always attack
    public bool Complete { get; private set; }
    
    readonly CountdownTimer timer;
    readonly AnimationController animations;

    public AttackStrategy(AnimationController animations) {
        this.animations = animations;
        timer = new CountdownTimer(animations.GetAnimationLength(animations.attackClip));
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }
    
    public void Start() {
        timer.Start();
        animations.Attack();
    }
    
    public void Update(float deltaTime) => timer.Tick();
}

public class MoveStrategy : IActionStrategy {
    readonly NavMeshAgent agent;
    readonly Func<Vector3> destination;
    readonly Func<bool> completionCondition; // optional override — defaults to remainingDistance check

    public bool CanPerform => !Complete;
    public bool Complete => completionCondition != null
        ? completionCondition()
        : agent.hasPath && agent.remainingDistance <= 2f && !agent.pathPending;

    /// <param name="completionCondition">
    ///   When provided, the action is considered complete as soon as this returns true.
    ///   Useful when the destination is a moving target (e.g. "am I within attack range?")
    ///   and remainingDistance would never settle to a stable value.
    /// </param>
    public MoveStrategy(NavMeshAgent agent, Func<Vector3> destination, Func<bool> completionCondition = null) {
        this.agent               = agent;
        this.destination         = destination;
        this.completionCondition = completionCondition;
    }

    public void Start() => agent.SetDestination(destination());
    public void Update(float deltaTime) => agent.SetDestination(destination());
    public void Stop() => agent.ResetPath();
}

public class WanderStrategy : IActionStrategy {
    readonly NavMeshAgent agent;
    readonly float wanderRadius;
    readonly float minIdleTime;
    readonly float maxIdleTime;

    bool isIdle;
    float idleTimer;

    public bool CanPerform => !Complete;
    public bool Complete => isIdle && idleTimer <= 0f;

    public WanderStrategy(NavMeshAgent agent, float wanderRadius, float minIdleTime = 1f, float maxIdleTime = 3f) {
        this.agent = agent;
        this.wanderRadius = wanderRadius;
        this.minIdleTime = minIdleTime;
        this.maxIdleTime = maxIdleTime;
    }

    public void Start() {
        isIdle = false;
        idleTimer = 0f;

        for (int i = 0; i < 5; i++) {
            Vector3 randomDirection = (UnityEngine.Random.insideUnitSphere * wanderRadius).With(y: 0);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(agent.transform.position + randomDirection, out hit, wanderRadius, 1)) {
                agent.SetDestination(hit.position);
                return;
            }
        }
    }

    public void Update(float deltaTime) {
        if (!isIdle) {
            if (agent.hasPath && agent.remainingDistance <= 2f && !agent.pathPending) {
                isIdle = true;
                idleTimer = UnityEngine.Random.Range(minIdleTime, maxIdleTime);
                agent.ResetPath();
            }
        } else {
            idleTimer -= deltaTime;
        }
    }

    public void Stop() => agent.ResetPath();
}

public class IdleStrategy : IActionStrategy {
    public bool CanPerform => true; // Agent can always Idle
    public bool Complete { get; private set; }

    readonly CountdownTimer timer;

    public IdleStrategy(float duration) {
        timer = new CountdownTimer(duration);
        timer.OnTimerStart += () => Complete = false;
        timer.OnTimerStop += () => Complete = true;
    }

    public void Start() => timer.Start();
    public void Update(float deltaTime) => timer.Tick();
}

