using System.Collections.Generic;
using Platformer;
using UnityEngine;
using UnityEngine.AI;

// NavMeshAgent stays required because the GoapAgent base class depends on it
// (animation speed, path resets) — this villager simply never asks it to move.
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AnimationController))]
[RequireComponent(typeof(PlayerDetector))]
public class VillagerAgent : GoapAgent {

    [Header("Dialogue")]
    [SerializeField] DialogueConversationSO dialogue;

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
        if (dialogue != null)
            GameEventsManager.instance.dialogueEvents.EnterDialogue(dialogue);
    }

    protected override void Update() {
        // Interrupt whatever the agent is doing so the Interact goal can win
        // as soon as the player walks up.
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
        factory.AddBelief("PlayerNearby", () => playerDetector.CanDetectPlayer());
        factory.AddBelief("IsInteracting", () => false);
        factory.AddBelief("AgentIsIdle", () => false);
    }

    protected override void SetupActions() {
        actions = new HashSet<AgentAction>();

        // Stationary NPC: the only routine is standing in place...
        actions.Add(new AgentAction.Builder("Stand Idle")
            .WithStrategy(new IdleStrategy(5))
            .AddEffect(beliefs["AgentIsIdle"])
            .Build());

        // ...and giving the player attention when they're close.
        actions.Add(new AgentAction.Builder("Interact With Player")
            .WithStrategy(new IdleStrategy(10))
            .AddPrecondition(beliefs["PlayerNearby"])
            .AddEffect(beliefs["IsInteracting"])
            .Build());
    }

    protected override void SetupGoals() {
        goals = new HashSet<AgentGoal>();

        goals.Add(new AgentGoal.Builder("Idle")
            .WithPriority(1)
            .WithDesiredEffect(beliefs["AgentIsIdle"])
            .Build());

        goals.Add(new AgentGoal.Builder("Interact")
            .WithPriority(2)
            .WithDesiredEffect(beliefs["IsInteracting"])
            .Build());
    }
}

