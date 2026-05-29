using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AnimationController))]
public abstract class GoapAgent : MonoBehaviour {

    protected NavMeshAgent navMeshAgent;
    protected AnimationController animations;
    protected Rigidbody rb;

    public AgentGoal lastGoal;
    public AgentGoal currentGoal;
    public ActionPlan actionPlan;
    public AgentAction currentAction;

    public Dictionary<string, AgentBelief> beliefs;
    public HashSet<AgentAction> actions;
    public HashSet<AgentGoal> goals;

    protected IGoapPlanner gPlanner;

    protected virtual void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        animations = GetComponent<AnimationController>();
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.freezeRotation = true;
    }

    protected virtual void Start() {
        gPlanner = new GoapPlanner();
        SetupBeliefs();
        SetupActions();
        SetupGoals();
    }

    protected abstract void SetupBeliefs();
    protected abstract void SetupActions();
    protected abstract void SetupGoals();

    protected virtual void Update() {
        animations.SetSpeed(navMeshAgent.velocity.magnitude);

        if (currentAction == null) {
            CalculatePlan();

            if (actionPlan != null && actionPlan.Actions.Count > 0) {
                if (!navMeshAgent.enabled) return; // wait until NavMeshAgent is re-enabled
                navMeshAgent.ResetPath();

                currentGoal = actionPlan.AgentGoal;
                currentAction = actionPlan.Actions.Pop();

                if (currentAction.Preconditions.All(b => b.Evaluate())) {
                    currentAction.Start();
                } else {
                    currentAction = null;
                    currentGoal = null;
                }
            }
        }


        if (actionPlan != null && currentAction != null) {
            currentAction.Update(Time.deltaTime);

            if (currentAction.Complete) {
                currentAction.Stop();
                currentAction = null;

                if (actionPlan.Actions.Count == 0) {
                    lastGoal = currentGoal;
                    currentGoal = null;
                }
            }
        }
    }

    void CalculatePlan() {
        var priorityLevel = currentGoal?.Priority ?? 0;

        HashSet<AgentGoal> goalsToCheck = goals;

        if (currentGoal != null) {
            goalsToCheck = new HashSet<AgentGoal>(goals.Where(g => g.Priority > priorityLevel));
        }

        var potentialPlan = gPlanner.Plan(this, goalsToCheck, lastGoal);
        if (potentialPlan != null) {
            actionPlan = potentialPlan;
        }
    }
}