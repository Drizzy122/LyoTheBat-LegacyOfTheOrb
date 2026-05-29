using System.Collections.Generic;
using UnityEngine;

public class AgentManager : MonoBehaviour {
    public static AgentManager instance;

    [Header("Alert Settings")]
    [SerializeField] float alertRadius = 15f;

    readonly List<AnimalAgent> animalAgents = new();

    void Awake() {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void OnDestroy() {
        if (instance == this) instance = null;
    }

    public void RegisterAgent(AnimalAgent agent) {
        if (!animalAgents.Contains(agent)) animalAgents.Add(agent);
    }

    public void UnregisterAgent(AnimalAgent agent) {
        animalAgents.Remove(agent);
    }

    public int AliveAgentCount() => animalAgents.Count;
    public bool ShouldFlee()     => animalAgents.Count <= 2;

    public void AlertNearbyAgents(AnimalAgent sender) {
        foreach (var agent in animalAgents) {
            if (agent == sender) continue;
            if (Vector3.Distance(sender.transform.position, agent.transform.position) <= alertRadius)
                agent.BecomeHostile();
        }
    }
}
