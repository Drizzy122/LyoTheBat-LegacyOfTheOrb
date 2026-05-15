using UnityEngine;

public abstract class SpellStragedy : ScriptableObject
{
    public abstract void CastSpell(Transform origin, Transform caster = null);
}