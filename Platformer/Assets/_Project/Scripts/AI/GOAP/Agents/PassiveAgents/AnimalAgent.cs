using System.Collections.Generic;
using DG.Tweening;
using Platformer;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AnimationController))]
[RequireComponent(typeof(PlayerDetector))]
public class AnimalAgent : GoapAgent {

    [Header("Sensors")]
    [SerializeField] Sensor chaseSensor;
    [SerializeField] Sensor attackSensor;

    [Header("Known Locations")]
    [SerializeField] Transform safePoint;

    [Header("Combat")]
    [SerializeField] ParticleSystem counterWarning;

    [Header("Knockback Settings")]
    [SerializeField] float knockbackDistance = 2f;
    [SerializeField] Ease knockbackEase = Ease.OutQuad;

    Health health;
    PlayerDetector playerDetector;
    bool isHostile;

    protected override void Awake() {
        base.Awake();
        health = GetComponent<Health>();
        playerDetector = GetComponent<PlayerDetector>();
    }

    void OnEnable() {
        if (health != null) health.OnHit += OnHit;
        if (health != null) health.OnDeath += OnDeath;
        if (chaseSensor != null) chaseSensor.OnTargetChanged += HandleTargetChanged;
        AgentManager.instance?.RegisterAgent(this);
    }

    void OnDisable() {
        if (health != null) health.OnHit -= OnHit;
        if (health != null) health.OnDeath -= OnDeath;
        if (chaseSensor != null) chaseSensor.OnTargetChanged -= HandleTargetChanged;
        AgentManager.instance?.UnregisterAgent(this);
    }

    void OnHit(float knockBackTime) {
        BecomeHostile();
        AgentManager.instance?.AlertNearbyAgents(this);

        if (playerDetector.Player != null) {
            navMeshAgent.enabled = false;
            Vector3 knockbackDir = (transform.position - playerDetector.Player.position).normalized;
            knockbackDir.y = 0;
            Vector3 knockbackTarget = transform.position + knockbackDir * knockbackDistance;

            transform.DOMove(knockbackTarget, knockBackTime)
                .SetEase(knockbackEase)
                .OnComplete(() => navMeshAgent.enabled = true);
        }
    }

    public void BecomeHostile() {
        isHostile = true;
        currentAction = null;
        currentGoal = null;
        animations.KnockBack();
    }

    void OnDeath() {
        AgentManager.instance?.UnregisterAgent(this);
        navMeshAgent.enabled = false;
        enabled = false;
        animations.Death();
        float deathLength = animations.GetAnimationLength(animations.deathClip);
        Destroy(gameObject, deathLength);
    }

    void HandleTargetChanged() {
        currentAction = null;
        currentGoal = null;
    }

    protected override void SetupBeliefs() {
        beliefs = new Dictionary<string, AgentBelief>();
        BeliefFactory factory = new BeliefFactory(this, beliefs);

        factory.AddBelief("Nothing", () => false);
        factory.AddBelief("AgentMoving", () => navMeshAgent.hasPath);
        factory.AddBelief("IsHostile", () => isHostile);
        factory.AddBelief("AgentHealthLow", () => health.currentHealth < 30f && AgentManager.instance.ShouldFlee());
        factory.AddBelief("AgentIsHealthy", () => health.currentHealth >= 50f);
        factory.AddBelief("AttackingPlayer", () => false);

        factory.AddLocationBelief("AgentAtSafePoint", 3f, safePoint);

        factory.AddSensorBelief("PlayerInChaseRange", chaseSensor);
        factory.AddSensorBelief("PlayerInAttackRange", attackSensor);
    }

    protected override void SetupActions() {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder("Wander Around")
            .WithStrategy(new WanderStrategy(navMeshAgent, 10))
            .AddEffect(beliefs["AgentMoving"])
            .Build());

        actions.Add(new AgentAction.Builder("Move To Safe Point")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => safePoint.position))
            .AddPrecondition(beliefs["IsHostile"])
            .AddPrecondition(beliefs["AgentHealthLow"])
            .AddEffect(beliefs["AgentAtSafePoint"])
            .Build());

        actions.Add(new AgentAction.Builder("Chase Player")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => beliefs["PlayerInChaseRange"].Location))
            .AddPrecondition(beliefs["IsHostile"])
            .AddPrecondition(beliefs["PlayerInChaseRange"])
            .AddEffect(beliefs["PlayerInAttackRange"])
            .Build());

        actions.Add(new AgentAction.Builder("Attack Player")
            .WithStrategy(new LungeAttackStrategy(navMeshAgent, playerDetector, counterWarning, animations, transform))
            .AddPrecondition(beliefs["IsHostile"])
            .AddPrecondition(beliefs["PlayerInAttackRange"])
            .AddEffect(beliefs["AttackingPlayer"])
            .Build());
    }

    protected override void SetupGoals() {
        goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder("Wander")
            .WithPriority(1)
            .WithDesiredEffect(beliefs["AgentMoving"])
            .Build());

        goals.Add(new AgentGoal.Builder("Retaliate")
            .WithPriority(2)
            .WithDesiredEffect(beliefs["AttackingPlayer"])
            .Build());

        goals.Add(new AgentGoal.Builder("Flee To Safety")
            .WithPriority(3)
            .WithDesiredEffect(beliefs["AgentAtSafePoint"])
            .Build());
    }
}