using UnityEngine;

public abstract class AbilityStrategy : ScriptableObject
{
    [Header("Ability Info")]
    public string abilityName;
    public float cooldownTime = 2f;

    // We pass the player GameObject so the ability can access the player's position and components
    public abstract void Execute(GameObject player);
}