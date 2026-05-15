using UnityEngine;


[CreateAssetMenu(fileName = "ProjectileSpellStrategy", menuName ="spells/ProjectileSpawnerStrategy")]
public class ProjectileSpellStrategy : SpellStragedy
{
    public GameObject projectilePrefab;
    public float speed = 10f;
    public float duration = 10f;

    public override void CastSpell(Transform origin, Transform caster = null)
    {
        if (projectilePrefab == null) return;

        new ProjectileBuilder()
            .WithProjectilePrefab(projectilePrefab)
            .WithSpeed(speed)
            .WithDuration(duration)
            .WithCaster(caster)
            .Build(origin);
    }
}
