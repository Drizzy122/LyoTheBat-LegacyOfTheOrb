using System.Collections.Generic;
using Platformer;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AnimationController))]
[RequireComponent(typeof(PlayerDetector))]
public class VillagerAgent : GoapAgent {

    [Header("Sensors")]
    [SerializeField] Sensor enemySensor;

    [Header("Known Locations")]
    [SerializeField] Transform housePosition;
    [SerializeField] Transform wellPosition;
    [SerializeField] Transform safePoint;

    [Header("Dialogue")]
    [SerializeField] string dialogueKnotName;

    PlayerDetector playerDetector;

    protected override void Awake() {
        base.Awake();
        playerDetector = GetComponent<PlayerDetector>();
    }

    void OnEnable() {
        GameEventsManager.instance.inputEvents.onSubmitPressed += OnSubmitPressed;
    }

    void OnDisable() {
        GameEventsManager.instance.inputEvents.onSubmitPressed -= OnSubmitPressed;
    }

    void OnSubmitPressed(InputEventContext context) {
        if (!playerDetector.CanDetectPlayer() || !context.Equals(InputEventContext.DEFAULT)) return;
        if (!string.IsNullOrEmpty(dialogueKnotName))
            GameEventsManager.instance.dialogueEvents.EnterDialogue(dialogueKnotName);
    }

    protected override void Update() {
        if (playerDetector.CanDetectPlayer()) {
            if (currentGoal == null || currentGoal.Name != "Interact") {
                currentAction?.Stop();
                currentAction = null;
                currentGoal = null;
            }
        }
        base.Update();
    }

    protected override void SetupBeliefs() {
        beliefs = new Dictionary<string, AgentBelief>();
        BeliefFactory factory = new BeliefFactory(this, beliefs);

        factory.AddBelief("Nothing", () => false);
        factory.AddBelief("AgentMoving", () => navMeshAgent.hasPath);
        factory.AddBelief("PlayerNearby", () => playerDetector.CanDetectPlayer());
        factory.AddBelief("EnemyNearby", () => enemySensor.IsTargetInRange);
        factory.AddBelief("IsInteracting", () => false);
        factory.AddBelief("AgentIsResting", () => false);

        factory.AddLocationBelief("AgentAtHouse", 3f, housePosition);
        factory.AddLocationBelief("AgentAtWell", 3f, wellPosition);
        factory.AddLocationBelief("AgentAtSafePoint", 3f, safePoint);
    }

    protected override void SetupActions() {
        actions = new HashSet<AgentAction>();

        actions.Add(new AgentAction.Builder("Move To House")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => housePosition.position))
            .AddEffect(beliefs["AgentAtHouse"])
            .Build());

        actions.Add(new AgentAction.Builder("Move To Well")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => wellPosition.position))
            .AddEffect(beliefs["AgentAtWell"])
            .Build());

        actions.Add(new AgentAction.Builder("Rest At House")
            .WithStrategy(new IdleStrategy(5))
            .AddPrecondition(beliefs["AgentAtHouse"])
            .AddEffect(beliefs["AgentIsResting"])
            .Build());

        actions.Add(new AgentAction.Builder("Rest At Well")
            .WithStrategy(new IdleStrategy(5))
            .AddPrecondition(beliefs["AgentAtWell"])
            .AddEffect(beliefs["AgentIsResting"])
            .Build());

        actions.Add(new AgentAction.Builder("Interact With Player")
            .WithStrategy(new IdleStrategy(10))
            .AddPrecondition(beliefs["PlayerNearby"])
            .AddEffect(beliefs["IsInteracting"])
            .Build());

        actions.Add(new AgentAction.Builder("Flee From Enemy")
            .WithStrategy(new MoveStrategy(navMeshAgent, () => safePoint.position))
            .AddPrecondition(beliefs["EnemyNearby"])
            .AddEffect(beliefs["AgentAtSafePoint"])
            .Build());
    }

    protected override void SetupGoals() {
        goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder("Go Home")
            .WithPriority(1)
            .WithDesiredEffect(beliefs["AgentAtHouse"])
            .Build());

        goals.Add(new AgentGoal.Builder("Go To Well")
            .WithPriority(1)
            .WithDesiredEffect(beliefs["AgentAtWell"])
            .Build());

        goals.Add(new AgentGoal.Builder("Rest")
            .WithPriority(1)
            .WithDesiredEffect(beliefs["AgentIsResting"])
            .Build());

        goals.Add(new AgentGoal.Builder("Interact")
            .WithPriority(2)
            .WithDesiredEffect(beliefs["IsInteracting"])
            .Build());

        goals.Add(new AgentGoal.Builder("Flee From Enemy")
            .WithPriority(3)
            .WithDesiredEffect(beliefs["AgentAtSafePoint"])
            .Build());
    }
}